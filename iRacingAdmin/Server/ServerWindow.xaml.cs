using System;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using iRacingAdmin.Views;
using iRacingSimulator;
using MahApps.Metro.Controls;

namespace iRacingAdmin.Server
{
    /// <summary>
    /// Interaction logic for ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow
    {
        public ServerWindow()
        {
            InitializeComponent(); 

            Server.Instance.Log += ServerLog;
            Server.Instance.UpdateConnections += ServerUpdateConnections;
            Server.Instance.StatusChanged += ServerStatusChanged;
            Server.Instance.MessageQueueUpdated += ServerMessageQueueUpdated;

            this.GetIp();
            txtPort.Text = "8081";
        }

        private void ServerMessageQueueUpdated(object sender, EventArgs eventArgs)
        {
            var size = Server.Instance.MessageQueueSize;
            if (size >= 50) size = 50;
            this.Dispatcher.Invoke(() =>
                                   {
                                       progressQueue.Value = size;
                                       lblQueue.Text = size.ToString();
                                   });
        }

        private void ServerStatusChanged(object sender, EventArgs e)
        {
            switch (Server.Instance.Status)
            {
                case Server.ServerStatus.Active:
                    lblStatus.Text = "Server active!";
                    btnStart.Content = "Stop";
                    statusPanel.Background = Brushes.ForestGreen;
                    break;

                case Server.ServerStatus.Stopped:
                    lblStatus.Text = "Stopped.";
                    btnStart.Content = "Start";
                    statusPanel.Background = Brushes.Tomato;
                    break;
            }
        }

        private void GetIp()
        {
            var thread = new Thread(GetIpThread);
            thread.Start();
        }

        private void GetIpThread()
        {
            string ip = "";
            try
            {
                string externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
                externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
                    .Matches(externalIP)[0].ToString();

                ip = externalIP;
            }
            catch
            {
                ip = "";
            }

            this.Dispatcher.Invoke(() => GetIpFinished(ip));
        }

        private void GetIpFinished(string ip)
        {
            txtIpAddress.Text = ip;
            ipProgress.IsActive = false;
            ipProgress.Visibility = Visibility.Collapsed;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (Server.Instance.Status == Server.ServerStatus.Stopped)
            {
                this.StartServer();
            }
            else if (Server.Instance.Status == Server.ServerStatus.Active)
            {
                Server.Instance.Stop();
            }
        }

        private void StartServer()
        {
            int port;
            if (int.TryParse(txtPort.Text, out port))
            {
                var ssid = Connection.Instance.SubSessionId.GetValueOrDefault();
                Server.Instance.Initialize(port, txtPassword.Text, Connection.Instance.Driver.Id, ssid);
            }
            else
            {
                MessageBox.Show("Please submit a valid numeric port number.", "Invalid port",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void ServerUpdateConnections(object sender, EventArgs eventArgs)
        {
            lstUsers.Dispatcher.Invoke(() =>
            {
                lstUsers.ItemsSource = Server.Instance.Users;
            });
        }

        private void ServerLog(object sender, Server.LogEventArgs e)
        {
            txtLog.Dispatcher.Invoke(new Action<string>(LogThread), e.Message);
        }

        private void LogThread(string msg)
        {
            txtLog.AppendText(string.Format("> ({0}) {1}{2}", DateTime.Now.ToShortTimeString(), msg, Environment.NewLine));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Server.Instance.Status == Server.ServerStatus.Active)
            {
                if (MessageWindow.Show("Close server",
                    "The Server is still running, are you sure you want to stop the Server and close this window?",
                    MessageBoxButton.YesNoCancel) != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            Server.Instance.Stop();
            
            base.OnClosed(e);
        }


    }
}
