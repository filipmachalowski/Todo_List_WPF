﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Todo_List_WPF.Models;
using Todo_List_WPF.Views;

namespace Todo_List_WPF.ViewModels
{
    public class AddTaskDialogViewModel : ViewModelBase
    {
        private string _title;
        private string _description;
        private DateTime _dueTime;
        private int _notificationMinutesBefore;
        private bool _isCompleted;
        private bool _isNotifyON;

        public bool IsNotifyONEnabled { get; private set; } = true;
        public bool IsNotifyON
        {
            get => _isNotifyON;
            set => SetProperty(ref _isNotifyON, value);
        }
        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetProperty(ref _isCompleted, value);
        }
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

        public DateTime DueTime
        {
            get => _dueTime == default ? DateTime.Now : _dueTime;
            set
            {
                if (SetProperty(ref _dueTime, value))
                {
                    // Recalculate Hour and Minute when DueTime changes
                    OnPropertyChanged(nameof(Hour));
                    OnPropertyChanged(nameof(Minute));
                    PastCheckboxUpdate();
                }
            }
        }

        public void PastCheckboxUpdate()
        {
            if (DueTime < DateTime.Now.AddMinutes(1))
            {
                IsNotifyON = false;
                IsNotifyONEnabled = false;
                NotificationMinutesBefore = 0;

                // Trigger property changed for the new properties
                OnPropertyChanged(nameof(IsNotifyON));
                OnPropertyChanged(nameof(IsNotifyONEnabled));
                OnPropertyChanged(nameof(NotificationMinutesBefore));
            }
            else
            {
                IsNotifyONEnabled = true;
                OnPropertyChanged(nameof(IsNotifyONEnabled));
            }
        }

        public int Hour
        {
            get => DueTime.Hour;
            set
            {
                Debug.WriteLine("Set");
                if (value != DueTime.Hour)
                {
                    Debug.WriteLine($"Setting Hour to {value}");
                    DueTime = new DateTime(
                        DueTime.Year,
                        DueTime.Month,
                        DueTime.Day,
                        value,
                        DueTime.Minute,
                        0
                    );
                    OnPropertyChanged(nameof(DueTime));
                }
            }
        }
        public int Minute
        {
            get => DueTime.Minute;
            set
            {
                if (value != DueTime.Minute)
                {
                    DueTime = new DateTime(
                        DueTime.Year,
                        DueTime.Month,
                        DueTime.Day,
                        DueTime.Hour,
                        value,
                        0
                    );
                    OnPropertyChanged(nameof(DueTime));
                }
            }
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
            DueTime = DateTime.Now;
            IsNotifyON = NotificationMinutesBefore > 0;
            SaveCommand = new RelayCommand(SaveTask);
            CancelCommand = new RelayCommand(CancelTask);
        }

        private void SaveTask()
        {
            // Check if Title or Description is empty
            if (string.IsNullOrWhiteSpace(Title))
            {
                ShowErrorDialog("Title cannot be empty !");
                return; // Do not save if Title is empty
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                ShowErrorDialog("Description cannot be empty !");
                return; // Do not save if Description is empty
            }

            var newTask = new TodoItem
            {
                Title = Title,
                Description = Description,
                DueTime = DueTime,
                IsCompleted = IsCompleted
            };

            // Set NotificationMinutesBefore based on IsNotifyON
            if (IsNotifyON)
            {
                if (NotificationMinutesBefore == 0)
                {
                    newTask.NotificationMinutesBefore = 1;
                }
                else
                {
                    newTask.NotificationMinutesBefore = NotificationMinutesBefore;
                }
            }
            else
            {
                newTask.NotificationMinutesBefore = 0;
            }

            if (IsNotifyON && DueTime.AddMinutes(-newTask.NotificationMinutesBefore) <= DateTime.Now)
            {
                ShowErrorDialog("Notification cannot be in past !");
                return; // Do not save if Notification is in past
            }

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
        private void ShowErrorDialog(string errorMessage)
        {
            var errorDialog = new CustomErrorDialog(errorMessage);
            errorDialog.Owner = Application.Current.MainWindow; // To center the error dialog
            errorDialog.ShowDialog();
        }
    }
}
