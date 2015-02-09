using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using iRacingAdmin.Classes;
using iRacingAdmin.Models.Admins;
using iRacingAdmin.Sync;

namespace iRacingAdmin.Models.ViewModels
{
    public class AdminListModel : ViewModelBase
    {
        private MainViewModel _mainModel;

        public AdminListModel(MainViewModel mainModel)
        {
            _mainModel = mainModel;

            _adminsView = CollectionViewSource.GetDefaultView(SyncManager.Instance.Users);
            _adminsView.Filter += delegate(object o)
            {
                var user = (UserContainer) o;
                return user.User.IsConnected;
            };
        }

        #region Properties

        private ICollectionView _adminsView;
        public ICollectionView AdminsView { get { return _adminsView; } }

        private UserContainer _selectedAdmin;
        public UserContainer SelectedAdmin
        {
            get { return _selectedAdmin; }
            set
            {
                _selectedAdmin = value;
                this.IsAdminSelected = false;
                this.IsAdminSelected = (value != null);
                this.OnPropertyChanged();
            }
        }

        private bool _isAdminSelected;
        public bool IsAdminSelected
        {
            get { return _isAdminSelected; }
            set
            {
                _isAdminSelected = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        public void OnSyncStateUpdated()
        {
            
        }

        #endregion
        
        #region Commands

        private ICommand _proposeSyncCameraCommand;
        public ICommand ProposeSyncCameraCommand
        {
            get { return _proposeSyncCameraCommand ?? (_proposeSyncCameraCommand = new RelayCommand(ExecuteProposeSyncCamera)); }
        }

        private ICommand _syncToCommand;

        public ICommand SyncToCommand
        {
            get { return _syncToCommand ?? (_syncToCommand = new RelayCommand(ExecuteSyncToCommand)); }
        }

        private void ExecuteProposeSyncCamera(object o)
        {
            // Propose OTHER admin to sync to YOUR camera
            var admin = this.SelectedAdmin;
            if (admin != null)
            {
                CameraControl.ProposeSyncCamera(admin.User);
            }
        }

        private void ExecuteSyncToCommand(object o)
        {
            // Sync YOUR camera to match OTHER admin camera
            var admin = this.SelectedAdmin;
            if (admin != null)
            {
                CameraControl.SyncCamera(admin.User);
            }
        }

        #endregion
    }
}
