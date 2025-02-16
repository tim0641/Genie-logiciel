using System.Drawing;
using System.Text;
using System.Windows;
using EasyLib.ViewModels;
using EasyLib.Services;
using EasySaveLog.Services;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

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
    EncryptionCheckBox.Checked += (s, e) => DecryptionCheckBox.IsEnabled = false;
    EncryptionCheckBox.Unchecked += (s, e) => DecryptionCheckBox.IsEnabled = true;

    DecryptionCheckBox.Checked += (s, e) => EncryptionCheckBox.IsEnabled = false;
    DecryptionCheckBox.Unchecked += (s, e) => EncryptionCheckBox.IsEnabled = true;
        }
        private System.Windows.Shapes.Rectangle MainRectangle;
        private System.Windows.Shapes.Rectangle SecondRectangle;
        private void OpenCreateBackupWindow(object sender, RoutedEventArgs e)
        {
            CreateBackupWindow createWindow = new CreateBackupWindow(_viewModel);
            createWindow.ShowDialog();
        }

        public void CreateBackupWindow(BackupViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

     
        private void Buttoncreateclick(object sender, RoutedEventArgs e)
        {

            foreach (var child in MainGrid.Children)
            {
                if (child is Button button)
                {
                    button.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 106, 30, 85));
                }
            }

            buttoncreate.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(166, 77, 121));
            buttonrunaction.Opacity = 0;
            Panel.SetZIndex(buttonrunaction, 0);
            buttondelaction.Opacity = 0;
            Panel.SetZIndex(buttondelaction, 0);
            Datagrid.Opacity = 0;
            Panel.SetZIndex(Datagrid, 0);
            DatagridList.Opacity = 0;
            Panel.SetZIndex(DatagridList, 0);
            Texte.Opacity = 1;
            Panel.SetZIndex(Texte, 10);
            Formulairecreate.Opacity = 1;
            Panel.SetZIndex(Formulairecreate, 10);



            var rectanglesToRemove = new[] { MainRectangle, SecondRectangle };
            foreach (var rectangle in rectanglesToRemove)
            {
                if (rectangle != null)
                {
                    MainGrid.Children.Remove(rectangle);
                }
                }
                

            MainRectangle = new System.Windows.Shapes.Rectangle
            {
                Width = 523,
                Height = 285,
                Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6A1E55")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            MainGrid.Children.Add(MainRectangle);
            MainGrid.UpdateLayout();

            SecondRectangle = new System.Windows.Shapes.Rectangle
            {
                Width = 180,
                Height = 60,
                Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6A1E55")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 530, 175) 
            };
            MainGrid.Children.Add(SecondRectangle);
            MainGrid.UpdateLayout();
            Panel.SetZIndex(SecondRectangle,1);

            this.SizeChanged += (s, ev) =>
            {
                CenterRectangle();
            };
            CenterRectangle();
        }
        private void Buttonlistclick(object sender, RoutedEventArgs e)
        {
            
            foreach (var child in MainGrid.Children)
            {
                if (child is Button button)
                {
                    button.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 106, 30, 85));
                }
            }
            buttonlist.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(166, 77, 121));
            buttonrunaction.Opacity = 0;
            Panel.SetZIndex(buttonrunaction, 0);
            buttondelaction.Opacity = 0;
            Panel.SetZIndex(buttondelaction, 0);
            Datagrid.Opacity = 0;
            Panel.SetZIndex(Datagrid, 0);
            DatagridList.Opacity = 1;
            Panel.SetZIndex(DatagridList, 10);
            Texte.Opacity = 1;
            Panel.SetZIndex(Texte, 10);
            Formulairecreate.Opacity = 0;
            Panel.SetZIndex(Formulairecreate, 0);

            var rectanglesToRemove = new[] { MainRectangle, SecondRectangle };
            foreach (var rectangle in rectanglesToRemove)
            {
                if (rectangle != null)
                {
                    MainGrid.Children.Remove(rectangle);
                }
            }

            MainRectangle = new System.Windows.Shapes.Rectangle
            {
                Width = 523,
                Height = 285,
                Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6A1E55")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,




            };
            MainGrid.Children.Add(MainRectangle);
            MainGrid.UpdateLayout();
            SecondRectangle = new System.Windows.Shapes.Rectangle
            {
                Width = 180,
                Height = 60,
                Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6A1E55")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 530, 25)
            };
            MainGrid.Children.Add(SecondRectangle);
            MainGrid.UpdateLayout();
            Panel.SetZIndex(SecondRectangle, 1);

            this.SizeChanged += (s, ev) =>
            {
                CenterRectangle();
            };
            CenterRectangle();
        }
        private void Buttonrunclick(object sender, RoutedEventArgs e)
        {

            foreach (var child in MainGrid.Children)
            {
                if (child is Button button)
                {
                    button.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 106, 30, 85));
                }
            }

            buttonrun.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(166, 77, 121));
            Panel.SetZIndex(buttonrunaction, 10);
            Panel.SetZIndex(buttondelaction, 0);
            buttonrunaction.Opacity = 1;
            buttondelaction.Opacity = 0;
            Datagrid.Opacity = 1;
            Panel.SetZIndex(Datagrid, 10);
            DatagridList.Opacity = 0;
            Panel.SetZIndex(DatagridList, 0);
            Texte.Opacity = 1;
            Panel.SetZIndex(Texte, 10);
            Formulairecreate.Opacity = 0;
            Panel.SetZIndex(Formulairecreate, 0);

            var rectanglesToRemove = new[] { MainRectangle, SecondRectangle };
            foreach (var rectangle in rectanglesToRemove)
            {
                if (rectangle != null)
                {
                    MainGrid.Children.Remove(rectangle);
                }
            }

            MainRectangle = new System.Windows.Shapes.Rectangle
            {
                Width = 523,
                Height = 285,
                Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6A1E55")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            MainGrid.Children.Add(MainRectangle);
            MainGrid.UpdateLayout();
            SecondRectangle = new System.Windows.Shapes.Rectangle
            {
                Width = 180,
                Height = 60,
                Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6A1E55")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 125, 530, 0)
            };
            MainGrid.Children.Add(SecondRectangle);
            MainGrid.UpdateLayout();
            Panel.SetZIndex(SecondRectangle, 1);

            this.SizeChanged += (s, ev) =>
            {
                CenterRectangle();
            };
            CenterRectangle();
        }
        private void Buttondeleteclick(object sender, RoutedEventArgs e)
        {

            foreach (var child in MainGrid.Children)
            {
                if (child is Button button)
                {
                    button.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 106, 30, 85));
                }
            }
            buttondel.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(166, 77, 121));
            Panel.SetZIndex(buttonrunaction, 0);
            Panel.SetZIndex(buttondelaction, 10);
            buttonrunaction.Opacity = 0;
            buttondelaction.Opacity = 1;
            Datagrid.Opacity = 1;
            Panel.SetZIndex(Datagrid, 10);
            Texte.Opacity = 1;
            Panel.SetZIndex(Texte, 10);
            Formulairecreate.Opacity = 0;
            Panel.SetZIndex(Formulairecreate, 0);
            var rectanglesToRemove = new[] { MainRectangle, SecondRectangle };
            foreach (var rectangle in rectanglesToRemove)
            {
                if (rectangle != null)
                {
                    MainGrid.Children.Remove(rectangle);
                }
            }

            MainRectangle = new System.Windows.Shapes.Rectangle
            {
                Width = 523,
                Height = 285,
                Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6A1E55")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            MainGrid.Children.Add(MainRectangle);
            MainGrid.UpdateLayout();
            SecondRectangle = new System.Windows.Shapes.Rectangle
            {
                Width = 180,
                Height = 60,
                Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6A1E55")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 275, 530, 0)
            };
            MainGrid.Children.Add(SecondRectangle);
            MainGrid.UpdateLayout();
            Panel.SetZIndex(SecondRectangle, 1);

            this.SizeChanged += (s, ev) =>
            {
                CenterRectangle();
            };
            CenterRectangle();
        }
        private void CenterRectangle()
        {
            if (MainRectangle != null)
            {
                MainRectangle.Margin = new Thickness(160, 50, 0, 0);
            }
        }

    }
}