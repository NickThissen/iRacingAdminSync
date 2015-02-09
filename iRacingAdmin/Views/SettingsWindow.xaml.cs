using System;
using System.ComponentModel;
using System.Windows;
using iRacingAdmin.Models.ViewModels;

namespace iRacingAdmin.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        private SettingsViewModel _model;

        public SettingsWindow()
        {
            InitializeComponent();
            _model = new SettingsViewModel();
            this.DataContext = _model;
            _model.Load();
        }

        public override void OnShown()
        {
            _model.IsChanged = false;
        }

        private void SettingsChanged(object sender, EventArgs e)
        {
            _model.IsChanged = true;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _model.Save();
            base.OnClosing(e);
        }

        private void btnMinOfftrackHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageWindow.Show("Help",
                "The offtrack timeout is the time (in seconds) during which a driver is immune for new offtrack penalties, after he returns on track an initial offtrack.\n\n" +
            "iRacing uses 14 seconds - which means a driver has 14 seconds to go offtrack a second time after an initial offtrack without penalty.\n" +
            "To prevent this, set this value relatively low (0.5 - 3 seconds).\n\n" +
            "Note: do not use values very close to 0, as that may cause drivers getting many offtracks for a single incident when their car hovers just on the border between "+
            "on and offtrack.", MessageBoxButton.OK);
        }

        private void btnMaxOfftrackHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageWindow.Show("Help",
                "A notification will be displayed to all admins once a driver reaches the set offtrack limit. Set to 0 to disable.", MessageBoxButton.OK);
        }
    }
}
