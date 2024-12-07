using System;
using System.Windows;
using Todo_List_WPF.Models;

namespace Todo_List_WPF
{
    public partial class App : Application
    {
        // Override OnStartup to ensure database is created/migrated
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Ensure the database is created and migrations are applied
            TodoContext.EnsureDatabaseCreated();

        }
    }
}
