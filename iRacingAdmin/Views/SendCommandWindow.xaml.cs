using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace iRacingAdmin.Views
{
    /// <summary>
    /// Interaction logic for SendCommandWindow.xaml
    /// </summary>
    public partial class SendCommandWindow
    {
        private const int MAX_LENGTH = 54;
        private string _originalCommand;

        public SendCommandWindow(string command)
        {
            InitializeComponent();

            _originalCommand = command;
            txtCommand.Text = command;
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtCommand.Text);
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtCommand_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtCommand.Text.Length > 54)
            {
                lblWarning.Text = "Warning: your command is too long to fit!";
                lblWarning.Visibility = Visibility.Visible;
            }
            else if (txtCommand.Text != _originalCommand)
            {
                lblWarning.Text = "(Command modified!)";
                lblWarning.Visibility = Visibility.Visible;
            }
            else
            {
                lblWarning.Visibility= Visibility.Hidden;
            }
        }
    }
}
