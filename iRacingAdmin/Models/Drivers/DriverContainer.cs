using System.Collections.Specialized;
using iRacingAdmin.Models.Admins;
using iRacingAdmin.Models.ViewModels;

namespace iRacingAdmin.Models.Drivers
{
    public class DriverContainer : ViewModelBase
    {
        public DriverContainer(Driver driver)
        {
            _driver = driver;
            _userCameras = new UserCameraCollection(this);
            _userCameras.CollectionChanged += UserCamerasOnCollectionChanged;
        }

        private Driver _driver;
        public Driver Driver
        {
            get { return _driver; }
            set
            {
                _driver = value;
                this.OnPropertyChanged();
            }
        }

        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                this.OnPropertyChanged();
            }
        }

        private bool _watching;
        public bool Watching
        {
            get
            {
                return _watching;
            }
            set
            {
                _watching = value;
                this.OnPropertyChanged();
            }
        }

        private UserCameraCollection _userCameras;
        public UserCameraCollection UserCameras
        {
            get { return _userCameras; }
        }

        private void UserCamerasOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged("UserCameras");
        }
    }
}
