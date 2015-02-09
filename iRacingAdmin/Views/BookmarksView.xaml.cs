using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using iRacingAdmin.Extensions;
using iRacingAdmin.Models.Server;

namespace iRacingAdmin.Views
{
    /// <summary>
    /// Interaction logic for BookmarksView.xaml
    /// </summary>
    public partial class BookmarksView
    {
        public BookmarksView()
        {
            InitializeComponent();

            this.DataContext = this;

            _bookmarks = new ObservableCollection<ServerInfo>();
            this.LoadBookmarks();
        }

        private readonly ObservableCollection<ServerInfo> _bookmarks;
        public ObservableCollection<ServerInfo> Bookmarks { get { return _bookmarks; } }

        public ServerInfo SelectedBookmark { get; set; }

        private void LoadBookmarks()
        {
            _bookmarks.Clear();
            _bookmarks.AddRange(ServerInfo.Load());
        }

        private void SaveBookmarks()
        {
            ServerInfo.Save(_bookmarks.ToList());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _bookmarks.Add(ServerInfo.Create());
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedBookmark != null)
            {
                _bookmarks.Remove(this.SelectedBookmark);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            this.SaveBookmarks();
        }
    }

    public class BookmarkValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var server = (value as BindingGroup).Items[0] as ServerInfo;
            if (string.IsNullOrWhiteSpace(server.Name))
                return new ValidationResult(false, "Please submit a name.");

            IPAddress ip;
            if (!IPAddress.TryParse(server.Ip, out ip))
                return new ValidationResult(false, "Invalid IP address.");

            if (server.Port <= 0)
                return new ValidationResult(false, "Port must be larger than 0.");

            return ValidationResult.ValidResult;
        }
    }
}
