using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Models.ViewModels;
using iRacingAdmin.Sync;

namespace iRacingAdmin.Models.Admins
{
    public class UserContainer : ViewModelBase
    {
        private User _user;
        private bool _isLive;
        private Driver _watchedDriver;

        public UserContainer(User user)
        {
            this.User = user;
        }

        public User User
        {
            get { return _user; }
            set
            {
                _user = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsLive
        {
            get { return _isLive; }
            set
            {
                _isLive = value;
                this.OnPropertyChanged();
            }
        }

        public Driver WatchedDriver
        {
            get { return _watchedDriver; }
            set
            {
                _watchedDriver = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("IsWatching");
            }
        }

        public bool IsWatching
        {
            get { return this.WatchedDriver != null; }
        }
    }
}
