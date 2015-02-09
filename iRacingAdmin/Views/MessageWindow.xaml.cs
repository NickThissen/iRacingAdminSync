using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using iRacingAdmin.Classes;

namespace iRacingAdmin.Views
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow
    {
        private MessageWindow()
        {
            InitializeComponent();

            linkPanel.Visibility = Visibility.Collapsed;
            progressPanel.Visibility = Visibility.Collapsed;
        }

        private MessageBoxResult _result;
        private BackgroundWorker _worker;

        public static MessageBoxResult Show(string title, string message, MessageBoxButton buttons, Brush titleColor = null)
        {
            var window = new MessageWindow();
            window.Title = title;
            window.lblText.Text = message;

            if (titleColor != null) window.Background = titleColor;

            window.SetButtons(buttons);

            App.Instance.MainModel.ShowDialog(window);

            return window._result;
        }

        public static void ShowException(string message)
        {
            App.Instance.Dispatcher.Invoke(() =>
                ShowLink(true, "Error", message, Log.Instance.Path, "View log file"));
        }

        public static void ShowLink(bool isError, string title, string message, string url, string linkText = null)
        {
            App.Instance.Dispatcher.Invoke(() =>
            {
                var window = new MessageWindow();
                window.Title = title;
                window.lblText.Text = message;

                if (isError) window.Background = Brushes.Tomato;

                if (string.IsNullOrWhiteSpace(linkText)) linkText = url;
                window.linkPanel.Visibility = Visibility.Visible;
                window.hyperlink.Inlines.Add(linkText);
                window.hyperlink.NavigateUri = new Uri(url);
                window.hyperlink.RequestNavigate += (s, e) =>
                {
                    Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                    e.Handled = true;
                };

                window.SetButtons(MessageBoxButton.OK);
                App.Instance.MainModel.ShowDialog(window);
            });
        }

        public static void ShowProgress(string title, string message, bool indeterminate, 
            Action<BackgroundWorker, object> workerMethod, object parameter = null)
        {
            App.Instance.Dispatcher.Invoke(() =>
            {
                var window = new MessageWindow();
                window.Title = title;
                window.lblText.Text = message;

                window._worker = new BackgroundWorker();
                window._worker.WorkerReportsProgress = true;
                window._worker.WorkerSupportsCancellation = false;
                window._worker.DoWork += (o, e) => workerMethod(window._worker, parameter);

                if (!indeterminate)
                {
                    window._worker.ProgressChanged += new ProgressChangedEventHandler(window.Worker_ProgressChanged);
                }
                else
                {
                    window.progress.IsIndeterminate = true;
                }

                window._worker.RunWorkerCompleted += (sender, args) => window.Close();


                window.progressPanel.Visibility = Visibility.Visible;
                window.buttonsPanel.Visibility = Visibility.Collapsed;
                App.Instance.MainModel.ShowDialog(window);
            });
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress.Value = e.ProgressPercentage;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_worker != null)
            {
                _worker.RunWorkerAsync();
            }
        }

        private void SetButtons(MessageBoxButton buttons)
        {
            switch (buttons)
            {
                case MessageBoxButton.OK:
                    this.btnOK.Visibility = Visibility.Visible;
                    this.btnCancel.Visibility = Visibility.Collapsed;
                    this.btnYes.Visibility = Visibility.Collapsed;
                    this.btnNo.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.OKCancel:
                    this.btnOK.Visibility = Visibility.Visible;
                    this.btnCancel.Visibility = Visibility.Visible;
                    this.btnYes.Visibility = Visibility.Collapsed;
                    this.btnNo.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNo:
                    this.btnOK.Visibility = Visibility.Collapsed;
                    this.btnCancel.Visibility = Visibility.Collapsed;
                    this.btnYes.Visibility = Visibility.Visible;
                    this.btnNo.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    this.btnOK.Visibility = Visibility.Collapsed;
                    this.btnCancel.Visibility = Visibility.Visible;
                    this.btnYes.Visibility = Visibility.Visible;
                    this.btnNo.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.Yes;
            this.Close();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.No;
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.Cancel;
            this.Close();
        }

        public class ProgressMethod
        {
            public ParameterizedThreadStart Method { get; set; }
            public Action OnFinished { get; set; }
        }

    }
}
