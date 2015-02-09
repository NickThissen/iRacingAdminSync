using System.Windows.Input;
using iRacingAdmin.Sync.Penalties;

namespace iRacingAdmin.Views.Penalties
{
    /// <summary>
    /// Interaction logic for EditPenaltyWindow.xaml
    /// </summary>
    public partial class EditPenaltyWindow
    {
        private readonly Penalty _penalty;

        public EditPenaltyWindow(Penalty penalty)
        {
            InitializeComponent();

            _penalty = penalty;
            this.DataContext = penalty;

            this.KeyUp += OnKeyUp;

            cboReason.ItemsSource = new[]
                                    {
                                        "Avoidable contact",
                                        "Dangerous driving",
                                        "Ignoring track limits",
                                        "Incorrectly served penalty",
                                        "Ignoring admin warning"
                                    };
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
