using System.Windows;
using Todo_List_WPF.ViewModels;

namespace Todo_List_WPF.Views
{
    public partial class CustomErrorDialog : Window
    {
        public string ErrorMessage { get; set; }
        public RelayCommand OkCommand { get; set; }

        public CustomErrorDialog(string errorMessage)
        {
            InitializeComponent();
            ErrorMessage = errorMessage;
            DataContext = this;
            OkCommand = new RelayCommand(CloseDialog);
        }

        private void CloseDialog()
        {
            this.Close();
        }
    }
}