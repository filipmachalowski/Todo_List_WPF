using System;
using System.Windows.Input;
using Todo_List_WPF.Models;
using Todo_List_WPF.Views;

namespace Todo_List_WPF.ViewModels
{
    public class AddTaskDialogViewModel : ViewModelBase
    {
        private string _title;
        private string _description;
        private DateTime? _dueTime;
        private int _notificationMinutesBefore;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public DateTime? DueTime
        {
            get => _dueTime;
            set => SetProperty(ref _dueTime, value);
        }

        public int NotificationMinutesBefore
        {
            get => _notificationMinutesBefore;
            set => SetProperty(ref _notificationMinutesBefore, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public Action<TodoItem> OnTaskSaved { get; set; }

        public AddTaskDialogViewModel()
        {
            SaveCommand = new RelayCommand(SaveTask);
            CancelCommand = new RelayCommand(CancelTask);
        }

        private void SaveTask()
        {
            var newTask = new TodoItem
            {
                Title = Title,
                Description = Description,
                DueTime = DueTime,
                NotificationMinutesBefore = NotificationMinutesBefore
            };

            // Trigger the save action (passing the task)
            OnTaskSaved?.Invoke(newTask);

            // Close the dialog after saving
            (App.Current.MainWindow as MainView)?.CloseAddTaskDialog();
        }

        private void CancelTask()
        {
            // Close the dialog without saving
            (App.Current.MainWindow as MainView)?.CloseAddTaskDialog();
        }
    }
}
