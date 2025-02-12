using System.Windows;
using EasyLib.ViewModels;
using EasyLib.Services;
using EasySaveLog.Services;

namespace EasyWPF
{
    public partial class MainWindow : Window
    {
        private readonly BackupViewModel _viewModel; 

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new BackupViewModel(
                new DailyLogService(@"C:\Logs\Daily"),
                new BackupService(),
                new StateService(@"C:\Logs\States\Daily"));

            DataContext = _viewModel;
        }

        private void OpenCreateBackupWindow(object sender, RoutedEventArgs e)
        {
            CreateBackupWindow createWindow = new CreateBackupWindow(_viewModel);
            createWindow.ShowDialog();
        }
    }
}
