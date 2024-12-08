using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Todo_List_WPF.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                if (Wpf.Ui.Appearance.ApplicationThemeManager.GetSystemTheme().ToString() == "Dark") {
                    Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Dark, Wpf.Ui.Controls.WindowBackdropType.Auto, true);
                }
                else
                {
                    Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Light, Wpf.Ui.Controls.WindowBackdropType.Auto, true);
                }
                
                Wpf.Ui.Appearance.SystemThemeWatcher.Watch(
                    this,                                    // Window class
                    Wpf.Ui.Controls.WindowBackdropType.Auto,  // Background type
                    true                                     // Whether to change accents automatically
                );
            };
        }

        public void CloseAddTaskDialog()
        {
            foreach (var window in Application.Current.Windows)
            {
                if (window is AddTaskDialog dialog)
                {
                    dialog.Close();
                    break;
                }
            }
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            HideAppToTray();
        }

        private void OnNotifyIconLeftClick(object sender, RoutedEventArgs e)
        {
            ShowAppFromTray(sender, e);
        }

        private void ShowAppFromTray(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void HideAppToTray()
        {
            this.Visibility = Visibility.Hidden;
        }
    }
}
