using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace Todo_List_WPF.Models
{
    public class TodoContext : DbContext
    {
        public DbSet<TodoItem> TodoItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TodoList.db");
            options.UseSqlite($"Data Source={dbPath}");
        }

        public static void EnsureDatabaseCreated()
        {
            using (var context = new TodoContext())
            {
                context.Database.Migrate();  // Automatically applies any pending migrations
            }
        }

    }

    public class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime DueTime { get; set; }
        public int NotificationMinutesBefore { get; set; }
    }
}
