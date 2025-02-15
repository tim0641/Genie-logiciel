using System.Drawing;
using System.Text;
using System.Windows;
using EasyLib;
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
            LanguageComboBox.SelectedIndex = 0;
            EasyLib.Localization.SetLanguage("en");
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


        private void LanguageComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Vérifier si un élément est sélectionné
            if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string language = selectedItem.Tag.ToString();
                EasyLib.Localization.SetLanguage(language);

                buttoncreate.Content = EasyLib.Localization.Get("create");
                buttonlist.Content = EasyLib.Localization.Get("list");
                buttonrun.Content = EasyLib.Localization.Get("run");
                buttondel.Content = EasyLib.Localization.Get("delete");
                Textblock1.Text = EasyLib.Localization.Get("createbackup");
                Textblock2.Text = EasyLib.Localization.Get("createname");
                Textblock3.Text = EasyLib.Localization.Get("createsource");
                Textblock4.Text = EasyLib.Localization.Get("createdestination");
                Textblock5.Text = EasyLib.Localization.Get("createtype");
                DatagridBorder1Header1.Header = EasyLib.Localization.Get("name");
                DatagridBorder1Header2.Header = EasyLib.Localization.Get("source");
                DatagridBorder1Header3.Header = EasyLib.Localization.Get("destination");
                DatagridBorder1Header4.Header = EasyLib.Localization.Get("type");
                DatagridBorder2Header5.Header = EasyLib.Localization.Get("choice");
                DatagridBorder2Header1.Header = EasyLib.Localization.Get("name");
                DatagridBorder2Header2.Header = EasyLib.Localization.Get("source");
                DatagridBorder2Header3.Header = EasyLib.Localization.Get("destination");
                DatagridBorder2Header4.Header = EasyLib.Localization.Get("type");
                buttoncreateaction.Content = EasyLib.Localization.Get("create");
                buttonrunaction.Content = EasyLib.Localization.Get("run");
                buttondelaction.Content = EasyLib.Localization.Get("delete");

                
            }
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
                        buttoncreateaction.Opacity = 1;
            Panel.SetZIndex(buttoncreateaction, 1);
            Datagrid.Opacity = 0;
            Panel.SetZIndex(Datagrid, 0);
            DatagridList.Opacity = 0;
            Panel.SetZIndex(DatagridList, 0);
            Texte.Opacity = 1;
            Panel.SetZIndex(Texte, 10);
            Formulairecreate.Opacity = 1;
            Panel.SetZIndex(Formulairecreate, 10);
            DatagridBorder1.Opacity = 0;
            Panel.SetZIndex(DatagridBorder1, 0);
            DatagridBorder2.Opacity = 0;
            Panel.SetZIndex(DatagridBorder2, 0);
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

            RectangleGeometry clipGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, MainRectangle.Width, MainRectangle.Height),
                RadiusX = 80, // Arrondi en X (changez la valeur pour un effet plus ou moins arrondi)
                RadiusY = 80  // Arrondi en Y
            };

            // Application du Clip au Rectangle
            MainRectangle.Clip = clipGeometry;

            SecondRectangle = new System.Windows.Shapes.Rectangle
            {
                Width = 280,
                Height = 60,
                Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6A1E55")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 430, 175) 
            };

            MainGrid.Children.Add(SecondRectangle);
            MainGrid.UpdateLayout();
            Panel.SetZIndex(SecondRectangle,1);

            RectangleGeometry clipGeometry2 = new RectangleGeometry
            {
                Rect = new Rect(0, 0, SecondRectangle.ActualWidth, SecondRectangle.ActualHeight),
                RadiusX = 30, // Arrondi en X (changez la valeur pour un effet plus ou moins arrondi)
                RadiusY = 30  // Arrondi en Y
            };

            // Application du Clip au Rectangle
            SecondRectangle.Clip = clipGeometry2;





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
            buttoncreateaction.Opacity = 0;
            Panel.SetZIndex(buttoncreateaction, 0);
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
            DatagridBorder1.Opacity = 2;
            Panel.SetZIndex(DatagridBorder1, 2);
            DatagridBorder2.Opacity = 0;
            Panel.SetZIndex(DatagridBorder2, 0);
            
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

            RectangleGeometry clipGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, MainRectangle.Width, MainRectangle.Height),
                RadiusX = 50, // Arrondi en X (changez la valeur pour un effet plus ou moins arrondi)
                RadiusY = 50  // Arrondi en Y
            };

            // Application du Clip au Rectangle
            MainRectangle.Clip = clipGeometry;

            SecondRectangle = new System.Windows.Shapes.Rectangle
            {
                Width = 280,
                Height = 60,
                Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6A1E55")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 430, 25) 
            };

            MainGrid.Children.Add(SecondRectangle);
            MainGrid.UpdateLayout();
            Panel.SetZIndex(SecondRectangle,1);

            RectangleGeometry clipGeometry2 = new RectangleGeometry
            {
                Rect = new Rect(0, 0, SecondRectangle.ActualWidth, SecondRectangle.ActualHeight),
                RadiusX = 30, // Arrondi en X (changez la valeur pour un effet plus ou moins arrondi)
                RadiusY = 30  // Arrondi en Y
            };

            // Application du Clip au Rectangle
            SecondRectangle.Clip = clipGeometry2;

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
                        buttoncreateaction.Opacity = 0;
            Panel.SetZIndex(buttoncreateaction, 0);
            buttondelaction.Opacity = 0;
            Datagrid.Opacity = 1;
            Panel.SetZIndex(Datagrid, 10);
            DatagridList.Opacity = 0;
            Panel.SetZIndex(DatagridList, 0);
            Texte.Opacity = 1;
            Panel.SetZIndex(Texte, 10);
            Formulairecreate.Opacity = 0;
            Panel.SetZIndex(Formulairecreate, 0);
            DatagridBorder1.Opacity = 2;
            Panel.SetZIndex(DatagridBorder1, 2);
            DatagridBorder2.Opacity = 2;
            Panel.SetZIndex(DatagridBorder2, 2);
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

            RectangleGeometry clipGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, MainRectangle.Width, MainRectangle.Height),
                RadiusX = 50, // Arrondi en X (changez la valeur pour un effet plus ou moins arrondi)
                RadiusY = 50  // Arrondi en Y
            };

            // Application du Clip au Rectangle
            MainRectangle.Clip = clipGeometry;

            SecondRectangle = new System.Windows.Shapes.Rectangle
            {
                Width = 280,
                Height = 60,
                Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6A1E55")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 125, 430, 0) 
            };

            MainGrid.Children.Add(SecondRectangle);
            MainGrid.UpdateLayout();
            Panel.SetZIndex(SecondRectangle,1);

            RectangleGeometry clipGeometry2 = new RectangleGeometry
            {
                Rect = new Rect(0, 0, SecondRectangle.ActualWidth, SecondRectangle.ActualHeight),
                RadiusX = 30, // Arrondi en X (changez la valeur pour un effet plus ou moins arrondi)
                RadiusY = 30  // Arrondi en Y
            };

            // Application du Clip au Rectangle
            SecondRectangle.Clip = clipGeometry2;

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
                        buttoncreateaction.Opacity = 0;
            Panel.SetZIndex(buttoncreateaction, 0);
            Panel.SetZIndex(buttondelaction, 10);
            buttonrunaction.Opacity = 0;
            buttondelaction.Opacity = 1;
            Datagrid.Opacity = 1;
            Panel.SetZIndex(Datagrid, 10);
            Texte.Opacity = 1;
            Panel.SetZIndex(Texte, 10);
            Formulairecreate.Opacity = 0;
            Panel.SetZIndex(Formulairecreate, 0);
            DatagridBorder1.Opacity = 1;
            Panel.SetZIndex(DatagridBorder1, 1);
            DatagridBorder2.Opacity = 1;
            Panel.SetZIndex(DatagridBorder2, 1);
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

            RectangleGeometry clipGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, MainRectangle.Width, MainRectangle.Height),
                RadiusX = 50, // Arrondi en X (changez la valeur pour un effet plus ou moins arrondi)
                RadiusY = 50  // Arrondi en Y
            };

            // Application du Clip au Rectangle
            MainRectangle.Clip = clipGeometry;

            SecondRectangle = new System.Windows.Shapes.Rectangle
            {
                Width = 280,
                Height = 60,
                Fill = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6A1E55")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 275, 430, 0) 
            };

            MainGrid.Children.Add(SecondRectangle);
            MainGrid.UpdateLayout();
            Panel.SetZIndex(SecondRectangle,1);

            RectangleGeometry clipGeometry2 = new RectangleGeometry
            {
                Rect = new Rect(0, 0, SecondRectangle.ActualWidth, SecondRectangle.ActualHeight),
                RadiusX = 30, // Arrondi en X (changez la valeur pour un effet plus ou moins arrondi)
                RadiusY = 30  // Arrondi en Y
            };

            // Application du Clip au Rectangle
            SecondRectangle.Clip = clipGeometry2;

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