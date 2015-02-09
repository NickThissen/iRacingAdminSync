using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using iRacingAdmin.Classes;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Views;
using iRacingSdkWrapper;
using iRacingSimulator;

namespace iRacingAdmin.Models.ViewModels
{
    public class MainViewModel : SdkViewModel
    {
        public MainViewModel()
        {
            _view = new MainWindow();
            _view.DataContext = this;

            this.IsWaitingForConnection = true;
            this.SubSessionId = 0;

            this.SimConnectionStatusChanged();

            _driverList = new DriverListModel(this);
            _adminList = new AdminListModel(this);
            _penaltyList = new PenaltyListModel(this);
            //_offtracksModel = new OfftracksViewModel(this);
            //_replayBarModel = new ReplayBarModel(this);
            //_trackBarModel = new TrackBarModel(this);

        }

        #region Properties
        
        private readonly MainWindow _view;
        public MainWindow View { get { return _view; } }

        private bool _isWaitingForConnection;
        public bool IsWaitingForConnection
        {
            get { return _isWaitingForConnection; }
            set
            {
                _isWaitingForConnection = value;
                this.SimConnectionStatusChanged();
                this.OnPropertyChanged();
            }
        }

        private bool _isShowingOfftrackMessage;
        public bool IsShowingOfftrackMessage
        {
            get { return _isShowingOfftrackMessage; }
            set
            {
                _isShowingOfftrackMessage = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isShowingErrorPopup;
        public bool IsShowingErrorPopup
        {
            get { return _isShowingErrorPopup; }
            set
            {
                _isShowingErrorPopup = value;
                this.OnPropertyChanged();
            }
        }

        private string _errorPopupText;
        public string ErrorPopupText
        {
            get { return _errorPopupText; }
            set
            {
                _errorPopupText = value;
                this.OnPropertyChanged();
            }
        }

        private DriverOfftrackLimit _driverOfftrackLimit;
        public DriverOfftrackLimit DriverOfftrackLimit
        {
            get { return _driverOfftrackLimit; }
            set
            {
                _driverOfftrackLimit = value;
                this.OnPropertyChanged();
            }
        }

        private long _subSessionId;
        public long SubSessionId
        {
            get { return _subSessionId; }
            set
            {
                _subSessionId = value;
                this.OnPropertyChanged();
            }
        }

        private string _simConnectionStatus;
        public string SimConnectionStatus
        {
            get { return _simConnectionStatus; }
            set
            {
                _simConnectionStatus = value;
                this.OnPropertyChanged();
            }
        }

        private Brush _simConnectionStatusBrush;
        public Brush SimConnectionStatusBrush
        {
            get { return _simConnectionStatusBrush; }
            set
            {
                _simConnectionStatusBrush = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isDialogShowing;
        public bool IsDialogShowing
        {
            get { return _isDialogShowing; }
            set
            {
                _isDialogShowing = value;

                this.View.Effect = value ? new BlurEffect() {Radius = 2} : null;
                this.OnPropertyChanged();
            }
        }

        private readonly DriverListModel _driverList;
        public DriverListModel DriverList { get { return _driverList; } }

        private readonly AdminListModel _adminList;
        public AdminListModel AdminList { get { return _adminList; } }

        private readonly PenaltyListModel _penaltyList;
        public PenaltyListModel PenaltyList { get { return _penaltyList; } }

        //private readonly OfftracksViewModel _offtracksModel;
        //public OfftracksViewModel OfftracksModel { get { return _offtracksModel; } }

       // private readonly ReplayBarModel _replayBarModel;
       // public ReplayBarModel ReplayBarModel { get { return _replayBarModel; } }

        private readonly TrackBarModel _trackBarModel;
        public TrackBarModel TrackBarModel { get { return _trackBarModel; } }

        #endregion

        #region Methods

        public void OnSelectedDriversChanged()
        {
            //this.OfftracksModel.OnSelectedDriversChanged();
        }

        public bool? ShowDialog(Window window)
        {
            this.IsDialogShowing = true;
            var result = window.ShowDialog();
            this.IsDialogShowing = false;
            return result;
        }

        private DispatcherTimer _errorPopupTimer;

        public void ShowError(string action, Exception ex)
        {
            this.IsShowingErrorPopup = true;
            this.ErrorPopupText = action + "\n" + ex.Message;
            if (_errorPopupTimer == null) 
            {
                _errorPopupTimer = new DispatcherTimer(TimeSpan.FromSeconds(5), 
                    DispatcherPriority.Background,
                    (e,a) => HideError(), 
                    App.Instance.Dispatcher);
            }
            else
            {
                _errorPopupTimer.Stop();
            }
            _errorPopupTimer.Start();
        }

        private void HideError()
        {
            this.IsShowingErrorPopup = false;
        }

        #region Sync

        public override void OnSyncStateUpdated()
        {
            // The sync state has updated, send it to child viewmodels
            this.DriverList.OnSyncStateUpdated();
            this.AdminList.OnSyncStateUpdated();
            this.PenaltyList.OnSyncStateUpdated();
            //this.OfftracksModel.OnSyncStateUpdated();

            //DEBUG
            //this.OfftracksModel.NotifyOfftrackLimit(new DriverOfftrackLimit(Simulator.Instance.Drivers[3], 40));
        }

        public void NotifyOfftrackLimit(Drivers.DriverOfftrackLimit limit)
        {
            this.IsShowingOfftrackMessage = true;
            this.DriverOfftrackLimit = limit;
        }

        #endregion

        #region SDK

        private void SimConnectionStatusChanged()
        {
            var ssid = Connection.Instance.SubSessionId.GetValueOrDefault();
            if (ssid > 0)
            {
                if (Connection.Instance.IsSimulated)
                {
                    this.SimConnectionStatus = "SIMULATED connection to subsession " + ssid;
                    this.SimConnectionStatusBrush = Brushes.MediumPurple;
                }
                else
                {
                    this.SimConnectionStatus = "Connected to subsession " + ssid;
                    this.SimConnectionStatusBrush = Brushes.LimeGreen;
                }
            }
            else
            {
                this.SimConnectionStatus = "Waiting for iRacing...";
                this.SimConnectionStatusBrush = Brushes.Tomato;
            }
        }

        public override void OnSessionInfoUpdated(SdkWrapper.SessionInfoUpdatedEventArgs e)
        {
            //this.DriverList.Refresh();
            this.DriverList.UpdateClassFilterList();
        }

        public override void OnTelemetryUpdated(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
        }

        #endregion

        #region Commands

        private ICommand _closeMessageCommand;
        public ICommand CloseMessageCommand
        {
            get { return _closeMessageCommand ?? (_closeMessageCommand = new RelayCommand(CloseMessageExecute)); }
        }

        private ICommand _viewOfftrackLimitsCommand;
        public ICommand ViewOfftrackLimitsCommand
        {
            get { return _viewOfftrackLimitsCommand ?? (_viewOfftrackLimitsCommand = new RelayCommand(ViewLimitsExecute)); }
        }

        private ICommand _addOfftrackPenaltyCommand;
        public ICommand AddOfftrackPenaltyCommand
        {
            get { return _addOfftrackPenaltyCommand ?? (_addOfftrackPenaltyCommand = new RelayCommand(AddOfftrackPenaltyExecute)); }
        }

        private ICommand _settingsCommand;
        public ICommand SettingsCommand
        {
            get { return _settingsCommand ?? (_settingsCommand = new RelayCommand(SettingsExecute)); }
        }

        private ICommand _closeErrorPopupCommand;
        public ICommand CloseErrorPopupCommand
        {
            get
            {
                return _closeErrorPopupCommand ?? (_closeErrorPopupCommand = new RelayCommand(x => this.HideError()));
            }
        }

        private ICommand _showLogCommand;
        public ICommand ShowLogCommand
        {
            get { return _showLogCommand ?? (_showLogCommand = new RelayCommand(x => App.Instance.ShowLog())); }
        }

        private void CloseMessageExecute(object param)
        {
            this.IsShowingOfftrackMessage = false;
            this.DriverOfftrackLimit = null;
        }

        private void ViewLimitsExecute(object param)
        {
            //this.OfftracksModel.LimitsModel.Show();
            this.CloseMessageExecute(param);
        }

        private void AddOfftrackPenaltyExecute(object param)
        {
            this.PenaltyList.AddOfftrackPenalty(this.DriverOfftrackLimit);
            this.CloseMessageExecute(param);
        }

        private void SettingsExecute(object param)
        {
            var dialog = new SettingsWindow();
            this.ShowDialog(dialog);
        }

        #endregion

        #endregion
    }
}
