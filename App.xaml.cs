using System;
using System.Diagnostics;
using System.Windows;
using Todo_List_WPF.Models;
using Todo_List_WPF.Services;

namespace Todo_List_WPF
{
    public partial class App : Application
    {
        public static NotificationService NotificationService { get; private set; }
        // Override OnStartup to ensure database is created/migrated
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize and start the notification service
            NotificationService = new NotificationService();
            NotificationService.Start();

            // Ensure the database is created and migrations are applied
            TodoContext.EnsureDatabaseCreated();

        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // Stop the notification service when the app exits
            NotificationService.Stop();
        }
    }
}
