using System;
using System.Linq; 
using System.Windows;
using EasyLib.ViewModels;
using System.Collections.ObjectModel;  // Pour ObservableCollection
using EasyLib.Models;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using EasyLib.Services;
using System.Collections.Generic;
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
    public partial class ProgressWindow : Window
    {

    private BackupViewModel _viewModel;
    public ProgressWindow(BackupViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

private void LanguageSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (LanguageSelectorComboBox.SelectedItem is ComboBoxItem selectedItem)
    {
        string language = selectedItem.Tag.ToString();
        EasyLib.Localization.SetLanguage(language);
        Header1.Header = EasyLib.Localization.Get("name");
        Header2.Header = EasyLib.Localization.Get("progression");
        Header3.Header = EasyLib.Localization.Get("etats");
        _viewModel.RefreshLocalization();
    }
}

    }


}
