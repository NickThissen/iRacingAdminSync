using System.Windows;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Models.Penalties;
using iRacingAdmin.Models.ViewModels;

namespace iRacingAdmin.Views.Penalties
{
    /// <summary>
    /// Interaction logic for DecidePenaltyWindow.xaml
    /// </summary>
    public partial class DecidePenaltyWindow : WindowBase
    {
        private readonly DecidePenaltyViewModel _model;

        public DecidePenaltyWindow(PenaltyContainer penalty, DriverContainer driver)
        {
            InitializeComponent();

            _model = new DecidePenaltyViewModel(penalty, driver);
            this.DataContext = _model;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            _model.SaveResult();
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _model.ResetResult();
            this.DialogResult = false;
            this.Close();
        }
    }
}
