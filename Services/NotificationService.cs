using Microsoft.Toolkit.Uwp.Notifications; // For Toast notifications
using System;
using System.Linq;
using System.Threading;
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
            AdjustTimerForClosestTask(); // Initial start
        }

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
                }

                // Recalculate timer for the next closest task
                AdjustTimerForClosestTask();
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

        public void AdjustTimerForClosestTask()
        {
            lock (_lock)
            {
                using (var db = new TodoContext())
                {
                    var now = DateTime.Now;

                    // Find the closest upcoming task's notify time
                    _nextTask = db.TodoItems
                        .Where(t => t.DueTime.HasValue)
                        .OrderBy(t => t.DueTime.Value.AddMinutes(-t.NotificationMinutesBefore))
                        .FirstOrDefault(t => t.DueTime.Value.AddMinutes(-t.NotificationMinutesBefore) > now);

                    if (_nextTask != null)
                    {
                        // Update the next notify time
                        _nextNotifyTime = _nextTask.DueTime.Value.AddMinutes(-_nextTask.NotificationMinutesBefore);

                        var nextCheckInterval = _nextNotifyTime.Value - now;

                        // Set the timer for the closest task's notification
                        _timer ??= new Timer(CheckForUpcomingTasks, null, Timeout.Infinite, Timeout.Infinite);

                        // Convert nextCheckInterval to milliseconds and ensure it's non-negative
                        _timer.Change((int)Math.Max(nextCheckInterval.TotalMilliseconds, 0), Timeout.Infinite);

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

        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }
}
