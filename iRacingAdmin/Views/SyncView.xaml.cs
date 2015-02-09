using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using iRacingAdmin.Models.Admins;
using iRacingAdmin.Server;
using iRacingAdmin.Sync;

namespace iRacingAdmin.Views
{
    /// <summary>
    /// Interaction logic for SyncView.xaml
    /// </summary>
    public partial class SyncView : UserControl
    {
        private UserContainerCollection _users;
        private ICollectionView _usersView;

        public SyncView()
        {
            InitializeComponent();

            _usersView = CollectionViewSource.GetDefaultView(SyncManager.Instance.Users.Select(u => u.User));
            _usersView.SortDescriptions.Add(new SortDescription("User.IsHost", ListSortDirection.Ascending));
            lstUsers.ItemsSource = _usersView;

            SyncManager.Instance.UsersUpdated += SyncUsersUpdated;
            SyncManager.Instance.Connected += SyncConnected;
            SyncManager.Instance.Disconnected += SyncDisconnected;

            Server.Server.Instance.StatusChanged += ServerStatusChanged;

            this.UpdateStatus();
        }

        private void ServerStatusChanged(object sender, EventArgs e)
        {
            this.UpdateStatus();
        }

        private void SyncUsersUpdated(object sender, EventArgs eventArgs)
        {
            this.Dispatcher.Invoke(UpdateUsers);
        }

        private void SyncConnected(object sender, EventArgs eventArgs)
        {
            this.Dispatcher.Invoke(() =>
            {
                UpdateUsers();
                UpdateStatus();
            });
        }

        private void SyncDisconnected(object sender, EventArgs eventArgs)
        {
            this.Dispatcher.Invoke(() =>
                                   {
                                       UpdateUsers();
                                       UpdateStatus();
                                   });
        }

        private void UpdateUsers()
        {
            _usersView.Refresh();
        }

        private async void btnConnectLocal_Click(object sender, RoutedEventArgs e)
        {
            if (SyncManager.Instance.Status == SyncManager.ConnectionStatus.Disconnected)
            {
                SyncConnectWindow.ConnectLocal();
            }
            this.UpdateStatus();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (SyncManager.Instance.Status == SyncManager.ConnectionStatus.Disconnected)
            {
                // Connect
                var dialog = new SyncConnectWindow();
                App.Instance.MainModel.ShowDialog(dialog);
            }

            this.UpdateStatus();
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (SyncManager.Instance.Status == SyncManager.ConnectionStatus.Connected)
            {
                // Disconnect
                SyncManager.Instance.Disconnect();
            }

            this.UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (SyncManager.Instance.Status == SyncManager.ConnectionStatus.Connected)
            {
                connectedGrid.Visibility = Visibility.Visible;
                disconnectedGrid.Visibility = Visibility.Collapsed;
                hostingGrid.Visibility = Visibility.Collapsed;

                lblUser.Text = SyncManager.Instance.User.Name;
                userBorder.Background = new SolidColorBrush(SyncManager.Instance.User.Color);

                lblStatus.Text = string.Format("Connected to {0}.", SyncManager.Instance.Address);
            }
            else
            {
                if (Server.Server.Instance.Status == Server.Server.ServerStatus.Active)
                {
                    connectedGrid.Visibility = Visibility.Collapsed;
                    disconnectedGrid.Visibility = Visibility.Collapsed;
                    hostingGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    connectedGrid.Visibility = Visibility.Collapsed;
                    disconnectedGrid.Visibility = Visibility.Visible;
                    hostingGrid.Visibility = Visibility.Collapsed;

                    lblUser.Text = string.Empty;
                    userBorder.Background = Brushes.Transparent;
                    lblStatus.Text = "Not connected.";
                }
            }
        }

        private void btnHost_Click(object sender, RoutedEventArgs e)
        {
            var window = new ServerWindow();
            window.Show();
        }

        private string GetServerPath()
        {
            try
            {
                // Find server exe
                var exePath = Assembly.GetExecutingAssembly().Location; // iRAS/iRacingAdmin.exe
                var rootPath = System.IO.Path.GetDirectoryName(exePath); // iRAS

                // Installed path 2: iRAS/Server/iRacingAdminServer.exe
                var path = System.IO.Path.Combine(rootPath, "iRacingAdminServer.exe");
                if (System.IO.File.Exists(path)) return path;

                // Installed path 2: iRAS/Server/iRacingAdminServer.exe
                path = System.IO.Path.Combine(rootPath, "Server", "iRacingAdminServer.exe");
                if (System.IO.File.Exists(path)) return path;

                // Debug path: 
                var debugPath = System.IO.Path.GetDirectoryName(exePath);
                var binPath = System.IO.Path.GetDirectoryName(debugPath);
                var adminPath = System.IO.Path.GetDirectoryName(binPath);
                adminPath = System.IO.Path.GetDirectoryName(adminPath);

                path = System.IO.Path.Combine(adminPath, "iRacingAdminServer", "bin", "Debug",
                    "iRacingAdminServer.exe");
                if (System.IO.File.Exists(path)) return path;
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}
