using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Todo_List_WPF.Models;

namespace Todo_List_WPF.Utils
{
    public class TaskNotificationVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var task = value as TodoItem;

            return task != null && IsNotificationPending(task)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private bool IsNotificationPending(TodoItem task)
        {
            return task.NotificationMinutesBefore > 0
                   && !task.IsCompleted
                   && task.DueTime.AddMinutes(-task.NotificationMinutesBefore) > DateTime.Now
                   && task.DueTime > DateTime.Now;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}