using System.Windows;
using EasyLib.ViewModels;

namespace EasyWPF
{
    public partial class CreateBackupWindow : Window
    {
        public CreateBackupWindow(BackupViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
