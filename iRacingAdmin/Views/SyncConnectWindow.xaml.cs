using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using iRacingAdmin.Extensions;
using iRacingAdmin.Models.Server;
using iRacingAdmin.Sync;
using iRacingSimulator;

namespace iRacingAdmin.Views
{
    /// <summary>
    /// Interaction logic for SyncConnectWindow.xaml
    /// </summary>
    public partial class SyncConnectWindow 
    {
        public SyncConnectWindow()
        {
            InitializeComponent();

            this.KeyUp += OnKeyUp;

            _bookmarks = new ObservableCollection<ServerInfo>();
            cboBookmarks.ItemsSource = _bookmarks;
            this.LoadBookmarks();

#if !DEBUG
            txtCustid.Visibility = Visibility.Collapsed;
            txtUsername.Visibility = Visibility.Collapsed;
#endif

            txtIpAddress.Text = "";
            txtPort.Text = "8081";
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                this.Connect();
            }
        }
        
        public async static void ConnectLocal()
        {
            int custid = Connection.Instance.Driver.Id;
            string name = Connection.Instance.Driver.Username;
            string shortname = Connection.Instance.Driver.Initials;

#if DEBUG
            custid = 0;
            name = "Local";
            shortname = name;
#endif

            var serverInfo = Server.Server.Instance.LocalServerInfo;
            var result = await SyncManager.Instance.Connect(serverInfo.Ip, serverInfo.Port, custid, name, shortname, serverInfo.Password);
            if (result.Success)
            {
            }
        }

        private async void Connect()
        {
            IPAddress ip = null;
            int port;

            if (string.IsNullOrWhiteSpace(txtIpAddress.Text)
                || !IPAddress.TryParse(txtIpAddress.Text.Trim(), out ip))
            {
                MessageBox.Show("Invalid IP address.");
                return;
            }

            if (!int.TryParse(txtPort.Text, out port))
            {
                MessageBox.Show("Invalid port.");
                return;
            }

            int custid = Connection.Instance.Driver.Id;
            string name = Connection.Instance.Driver.Username;
            string shortname = Connection.Instance.Driver.Initials;
            
#if DEBUG
            custid = int.Parse(txtCustid.Text);
            name = txtUsername.Text;
            shortname = name;
#endif

            var result = await SyncManager.Instance.Connect(ip.ToString(), port, custid, name, shortname, txtPassword.Text);
            if (result.Success)
            {
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show(result.Message);
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            this.Connect();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        #region Bookmarks

        private readonly ObservableCollection<ServerInfo> _bookmarks;
        public ObservableCollection<ServerInfo> Bookmarks { get { return _bookmarks; } } 

        private void LoadBookmarks()
        {
            this.Bookmarks.Clear();
            this.Bookmarks.AddRange(ServerInfo.Load());
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var server = cboBookmarks.SelectedItem as ServerInfo;
            if (server != null)
            {
                txtIpAddress.Text = server.Ip;
                txtPort.Text = server.Port.ToString();
                txtPassword.Text = server.Password;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var window = new BookmarksView();
            window.ShowDialog();
            this.LoadBookmarks();
        }

        #endregion

    }
}
