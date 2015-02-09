using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using iRacingAdmin.Classes;
using iRacingAdmin.Models.ViewModels;
using iRacingAdmin.Properties;
using iRacingSimulator;
using MahApps.Metro.Controls.Dialogs;

namespace iRacingAdmin.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        public MainWindow()
        {
            InitializeComponent();

            var clientVersion = Assembly.GetExecutingAssembly().GetName().Version;
            txtVersion.Text = string.Format("v{0}.{1}", clientVersion.Major, clientVersion.Minor);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}
