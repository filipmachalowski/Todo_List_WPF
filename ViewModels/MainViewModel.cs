using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Todo_List_WPF.Models;
using Todo_List_WPF.Services;
using Todo_List_WPF.Views;

namespace Todo_List_WPF.ViewModels
{
    
    public class MainViewModel : ViewModelBase
    {
        private TodoItem _selectedItem;
        private DateTime _selectedDate;

        public ObservableCollection<TodoItem> Tasks { get; set; } = new ObservableCollection<TodoItem>();
        public TodoItem SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                    LoadTasksForDate();
            }
        }

        public ICommand AddTaskCommand { get; }
        public ICommand EditTaskCommand { get; }
        public ICommand RemoveTaskCommand { get; }

        public MainViewModel()
        {
            SelectedDate = DateTime.Today;

            AddTaskCommand = new RelayCommand(AddTask);
            EditTaskCommand = new RelayCommand(EditTask, () => SelectedItem != null);
            RemoveTaskCommand = new RelayCommand(RemoveTask, () => SelectedItem != null);
        }

        private void LoadTasksForDate()
        {
            using var db = new TodoContext();
            Tasks.Clear();
            var tasksForDate = db.TodoItems.Where(t => t.DueTime.Date == SelectedDate.Date).ToList();
            foreach (var task in tasksForDate)
                Tasks.Add(task);
        }

        private void AddTask()
        {
            var addTaskDialog = new AddTaskDialog();
            var dialogViewModel = addTaskDialog.DataContext as AddTaskDialogViewModel;
            dialogViewModel.OnTaskSaved = newTask =>
            {
                // Save to database
                using var db = new TodoContext();
                db.TodoItems.Add(newTask);
                db.SaveChanges();
                LoadTasksForDate();
            };

            addTaskDialog.ShowDialog();
            App.NotificationService.AdjustTimerForClosestTask();
        }

        private void EditTask()
        {
            Debug.WriteLine("Launched Edit");
            if (SelectedItem == null) return;

            var addTaskDialog = new AddTaskDialog();
            var dialogViewModel = addTaskDialog.DataContext as AddTaskDialogViewModel;

            // Pre-populate the dialog with existing task data, including IsCompleted
            dialogViewModel.Title = SelectedItem.Title;
            dialogViewModel.Description = SelectedItem.Description;
            dialogViewModel.DueTime = SelectedItem.DueTime;
            dialogViewModel.NotificationMinutesBefore = SelectedItem.NotificationMinutesBefore;
            dialogViewModel.IsCompleted = SelectedItem.IsCompleted;  // Add this line to bind IsCompleted

            dialogViewModel.OnTaskSaved = updatedTask =>
            {
                // Update the existing task in the database with the updated values
                using var db = new TodoContext();
                var taskToUpdate = db.TodoItems.FirstOrDefault(t => t.Id == SelectedItem.Id);

                if (taskToUpdate != null)
                {
                    taskToUpdate.Title = updatedTask.Title;
                    taskToUpdate.Description = updatedTask.Description;
                    taskToUpdate.DueTime = updatedTask.DueTime;
                    taskToUpdate.NotificationMinutesBefore = updatedTask.NotificationMinutesBefore;
                    taskToUpdate.IsCompleted = updatedTask.IsCompleted; // Save the updated completion status

                    db.SaveChanges();
                    Debug.WriteLine("Task updated in database");
                }
                else
                {
                   // Debug.WriteLine("Task not found in database");
                }

                LoadTasksForDate();
            };

            addTaskDialog.ShowDialog();
            App.NotificationService.AdjustTimerForClosestTask();
        }

        private void RemoveTask()
        {
            using var db = new TodoContext();
            db.TodoItems.Remove(SelectedItem);
            db.SaveChanges();
            LoadTasksForDate();
            App.NotificationService.AdjustTimerForClosestTask();
        }

        // This method will be called when the checkbox state changes
        public void UpdateTaskCompletionStatus(TodoItem task)
        {
            using (var db = new TodoContext())
            {
                var existingTask = db.TodoItems.SingleOrDefault(t => t.Id == task.Id);
                if (existingTask != null)
                {
                    existingTask.IsCompleted = task.IsCompleted;
                    db.SaveChanges();
                }
            }
        }
    }
}
