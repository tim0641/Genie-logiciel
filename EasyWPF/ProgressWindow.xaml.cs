using System;
using System.Windows;
using EasyLib.ViewModels;
using System.Collections.ObjectModel;  // Pour ObservableCollection
using EasyLib.Models;
using System.Drawing;
using System.Text;
using System.Diagnostics;
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
    public partial class ProgressWindow : Window
    {
        // private readonly BackupService _backupService;
        // private readonly DailyLogService _dailyLogService;
        // private readonly StateService _stateService;


        public ProgressWindow()
        {
            InitializeComponent();

        }


    }
}