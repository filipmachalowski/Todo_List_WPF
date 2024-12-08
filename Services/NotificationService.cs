using Microsoft.Toolkit.Uwp.Notifications; // For Toast notifications
using Todo_List_WPF.Models;

namespace Todo_List_WPF.Services
{
    public class NotificationService
    {
        private Timer _timer;
        private TodoItem _nextTask; // Cached next task
        private DateTime? _nextNotifyTime; // Cached notify time
        private readonly object _lock = new object(); // Thread safety

        public void Start()
        {
            // Initial start, adjust timer for the closest task
            AdjustTimerForClosestTask();
        }

        // Recalculate the closest upcoming task's notification time only if the task list changes
        public void AdjustTimerForClosestTask()
        {
            lock (_lock)
            {
                using (var db = new TodoContext())
                {
                    var now = DateTime.Now;

                    // Get the next upcoming task with a notification
                    _nextTask = db.TodoItems
                        .Where(t => t.NotificationMinutesBefore > 0)
                        .OrderBy(t => t.DueTime.AddMinutes(-t.NotificationMinutesBefore))
                        .FirstOrDefault(t => t.DueTime.AddMinutes(-t.NotificationMinutesBefore) > now);

                    if (_nextTask != null)
                    {
                        // Calculate the next notify time for the closest task
                        _nextNotifyTime = _nextTask.DueTime.AddMinutes(-_nextTask.NotificationMinutesBefore);

                        var nextCheckInterval = _nextNotifyTime.Value - now;

                        // If we have a valid next task, set the timer for it
                        if (_nextNotifyTime.HasValue)
                        {
                            _timer ??= new Timer(CheckForUpcomingTasks, null, Timeout.Infinite, Timeout.Infinite);

                            // Convert nextCheckInterval to milliseconds, ensure it's non-negative
                            _timer.Change((int)Math.Max(nextCheckInterval.TotalMilliseconds, 0), Timeout.Infinite);
                        }
                    }
                    else
                    {
                        // No upcoming tasks, stop the timer
                        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                        _timer = null; // Dispose timer if no tasks are available
                    }
                }
            }
        }

        // This function will be called periodically by the timer to check for upcoming tasks
        private void CheckForUpcomingTasks(object state)
        {
            lock (_lock)
            {
                var now = DateTime.Now;

                if (_nextTask != null && _nextNotifyTime.HasValue && _nextNotifyTime.Value <= now)
                {
                    // Time to show notification
                    ShowTaskNotification(_nextTask);

                    // Clear cached task after notification
                    _nextTask = null;
                    _nextNotifyTime = null;

                    // Recalculate timer for the next closest task
                    AdjustTimerForClosestTask();
                }
            }
        }

        private void ShowTaskNotification(TodoItem task)
        {
            var title = task.Title;
            var message = $"{task.Description}\nDue at: {task.DueTime}";

            new ToastContentBuilder()
                .AddArgument("action", "viewTask")
                .AddArgument("taskId", task.Id)
                .AddText(title)
                .AddText(message)
                .Show();
        }

        // Stop the timer when no longer needed ( App Exit )
        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }
}
