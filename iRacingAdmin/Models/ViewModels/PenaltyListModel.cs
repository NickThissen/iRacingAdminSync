using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using iRacingAdmin.Classes;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Models.Penalties;
using iRacingAdmin.Sync;
using iRacingAdmin.Sync.Penalties;
using iRacingAdmin.Views.Penalties;

namespace iRacingAdmin.Models.ViewModels
{
    public class PenaltyListModel : ViewModelBase
    {
        private MainViewModel _mainModel;
        private string _selectedId;

        public PenaltyListModel(MainViewModel mainModel)
        {
            _mainModel = mainModel;

            _penalties = new ObservableCollection<PenaltyContainer>();
            _penaltiesView = CollectionViewSource.GetDefaultView(_penalties);
            _penaltiesView.SortDescriptions.Add(new SortDescription("Penalty.StartInvestigationTime", ListSortDirection.Descending));

            _decidedPenalties = new ObservableCollection<PenaltyContainer>();
            _decidedPenaltiesView = CollectionViewSource.GetDefaultView(_decidedPenalties);
            _decidedPenaltiesView.SortDescriptions.Add(new SortDescription("Penalty.StartInvestigationTime", ListSortDirection.Descending));
        }

        #region Properties

        private readonly ObservableCollection<PenaltyContainer> _penalties;
        public ObservableCollection<PenaltyContainer> Penalties { get { return _penalties; } }

        private readonly ObservableCollection<PenaltyContainer> _decidedPenalties;
        public ObservableCollection<PenaltyContainer> DecidedPenalties { get { return _decidedPenalties; } }

        private readonly ICollectionView _penaltiesView;
        public ICollectionView PenaltiesView { get { return _penaltiesView; } }

        private readonly ICollectionView _decidedPenaltiesView;
        public ICollectionView DecidedPenaltiesView{get { return _decidedPenaltiesView; }}

        private PenaltyContainer _selectedPenalty;
        public PenaltyContainer SelectedPenalty
        {
            get { return _selectedPenalty; }
            set
            {
                _selectedPenalty = value;
                this.IsPenaltySelected = false;
                this.IsPenaltySelected = (value != null);
                this.OnPropertyChanged();
            }
        }

        public PenaltyContainer SelectedDecidedPenalty
        {
            get { return _selectedDecidedPenalty; }
            set
            {
                _selectedDecidedPenalty = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isPenaltySelected;
        public bool IsPenaltySelected
        {
            get { return _isPenaltySelected; }
            set
            {
                _isPenaltySelected = value;
                this.OnPropertyChanged();
            }
        }


        #endregion

        #region Methods

        public void OnSyncStateUpdated()
        {
            //SyncManager.Instance.Dispatcher.Invoke(this.UpdatePenalties);
            this.UpdatePenalties();
        }

        public void AddDriver(DriverContainer driver, PenaltyContainer parentPenalty = null)
        {
            Penalty penalty;
            if (parentPenalty == null)
            {
                penalty = Penalty.Create();
                penalty.Camera = CameraControl.GetCurrentCameraSessionTime();
                penalty.Lap = driver.Driver.Live.Lap.ToString();
                SyncManager.Instance.State.Penalties.Add(penalty);

                // Create incident event
                var @event = new ReplayEvent();
                @event.Type = ReplayEvent.EventTypes.Incident;
                @event.AdminId = SyncManager.Instance.UserId;
                @event.Camera = penalty.Camera;
                //_mainModel.ReplayBarModel.AddEvent(@event);
            }
            else
            {
                penalty = parentPenalty.Penalty;
            }

            penalty.StartInvestigation(driver.Driver.Id, SyncManager.Instance.User);

            if (parentPenalty == null)
            {
                SyncManager.Instance.SendStateUpdate(SyncCommandHelper.AddPenalty(penalty));
                EditPenalty(penalty);
            }
            else
            {
                SyncManager.Instance.SendPenaltyUpdate(penalty);
            }

            this.UpdatePenalties();
        }

        public void AddOfftrackPenalty(DriverOfftrackLimit limit)
        {
            Penalty penalty;
            penalty = Penalty.Create();
            penalty.Reason = "Ignoring track limits";
            penalty.Camera = CameraControl.GetCurrentCameraSessionTime();
            penalty.Lap = limit.Driver.Driver.Live.Lap.ToString();
            SyncManager.Instance.State.Penalties.Add(penalty);

            // Create incident event
            var @event = new ReplayEvent();
            @event.Type = ReplayEvent.EventTypes.Incident;
            @event.AdminId = SyncManager.Instance.UserId;
            @event.Camera = penalty.Camera;
           // _mainModel.ReplayBarModel.AddEvent(@event);

            penalty.StartInvestigation(limit.Driver.Driver.Id, SyncManager.Instance.User);

            SyncManager.Instance.SendStateUpdate(SyncCommandHelper.AddPenalty(penalty));
        }

        private void UpdatePenalties()
        {
            // Store selected penalty id
            _selectedId = this.SelectedPenalty == null ? null : this.SelectedPenalty.Penalty.Id;

            this.ClearPenalties();

            foreach (var penalty in SyncManager.Instance.State.Penalties.ToList())
            {
                var container = new PenaltyContainer(penalty);
                container.UpdateDrivers();

                container.Penalty.PropertyChanged += OnPenaltyChanged;

                if (penalty.IsUnderInvestigation)
                {
                    this.Penalties.Add(container);
                }
                else
                {
                    this.DecidedPenalties.Add(container);
                }
            }

            // Find selected penalty again
            var selected = this.Penalties.FirstOrDefault(p => p.Penalty.Id == _selectedId);
            this.SelectedPenalty = selected;
        }

        public void JoinPenalty(Penalty penalty)
        {
            var user = SyncManager.Instance.User;

            // If penalty is already decided - ignore
            if (!penalty.IsUnderInvestigation) return;

            if (penalty.Users.Contains(user))
            {
                // If we are already investigating - leave
                penalty.Users.Remove(user);
            }
            else
            {
                // If we are not yet investigating - join
                penalty.JoinUser(user);

                // Sync camera
                CameraControl.ChangeCamera(penalty.Camera);
            }

            SyncManager.Instance.SendPenaltyUpdate(penalty);
            this.PenaltiesView.Refresh();
        }

        public void EditPenalty(Penalty penalty)
        {
            var selected = this.SelectedPenalty;
            this.SelectedPenalty = null;

            // Edit penalty details 
            var dialog = new EditPenaltyWindow(penalty);
            App.Instance.MainModel.ShowDialog(dialog);

            SyncManager.Instance.SendPenaltyUpdate(penalty);

            this.SelectedPenalty = selected;
        }

        public void DecidePenaltyResult(PenaltyContainer penalty)
        {
            this.SelectedPenalty = null;

            // If already locked then ignore
            if (penalty.Penalty.IsLocked) return;

            // Lock this penalty
            penalty.Penalty.LockUser = SyncManager.Instance.User;
            SyncManager.Instance.SendPenaltyUpdate(penalty.Penalty);

            // Get offending driver
            DriverContainer driver = null;

            if (penalty.Drivers.Count == 1)
            {
                driver = penalty.Drivers[0];
            }
            else if (penalty.Drivers.Count > 1)
            {
                // Select driver from list
                var selectDriverWindow = new SelectUserWindow(penalty.Drivers);
                if (App.Instance.MainModel.ShowDialog(selectDriverWindow).GetValueOrDefault())
                {
                    driver = selectDriverWindow.SelectedDriver;
                }
            }

            if (driver != null)
            {
                var wasUnderInvestigation = penalty.Penalty.IsUnderInvestigation;

                // Decide result window
                var dialog = new DecidePenaltyWindow(penalty, driver);
                App.Instance.MainModel.ShowDialog(dialog);

                if (wasUnderInvestigation && !penalty.Penalty.IsUnderInvestigation && this.Penalties.Contains(penalty))
                {
                    // No longer under investigation
                    this.Penalties.Remove(penalty);
                    this.DecidedPenalties.Add(penalty);
                }
                else if (!wasUnderInvestigation && penalty.Penalty.IsUnderInvestigation && this.DecidedPenalties.Contains(penalty))
                {
                    // Under investigation again
                    this.DecidedPenalties.Remove(penalty);
                    this.Penalties.Add(penalty);
                }
            }

            // Unlock again
            penalty.Penalty.LockUser = null;
            SyncManager.Instance.SendPenaltyUpdate(penalty.Penalty);
        }

        private void ClearPenalties()
        {
            foreach (var penalty in this.Penalties)
                penalty.Penalty.PropertyChanged -= OnPenaltyChanged;
            foreach (var penalty in this.DecidedPenalties)
                penalty.Penalty.PropertyChanged -= OnPenaltyChanged;

            this.Penalties.Clear();
            this.DecidedPenalties.Clear();
        }

        private void OnPenaltyChanged(object sender, EventArgs e)
        {
            var penalty = sender as Penalty;
            if (penalty != null) SyncManager.Instance.SendPenaltyUpdate(penalty);
        }

        #endregion

        #region Commands

        private ICommand _joinCommand;
        public ICommand JoinCommand
        {
            get { return _joinCommand ?? (_joinCommand = new RelayCommand(ExecuteJoin, CanExecuteJoin)); }
        }

        private ICommand _editCommand;
        public ICommand EditCommand
        {
            get { return _editCommand ?? (_editCommand = new RelayCommand(ExecuteEdit, CanExecuteEdit)); }
        }

        private ICommand _decideResultCommand;
        private PenaltyContainer _selectedDecidedPenalty;

        public ICommand DecideResultCommand
        {
            get
            {
                return _decideResultCommand ??
                       (_decideResultCommand = new RelayCommand(ExecuteDecideResult, CanExecuteDecideResult));
            }
        }

        private ICommand _deletePenaltyCommand;
        public ICommand DeletePenaltyCommand
        {
            get
            {
                return _deletePenaltyCommand ??
                       (_deletePenaltyCommand = new RelayCommand(ExecuteDeletePenalty, CanExecuteDeletePenalty));
            }
        }

        private ICommand _deleteDecidedPenaltyCommand;
        public ICommand DeleteDecidedPenaltyCommand
        {
            get
            {
                return _deleteDecidedPenaltyCommand ??
                       (_deleteDecidedPenaltyCommand = new RelayCommand(ExecuteDeleteDecidedPenalty, CanExecuteDeleteDecidedPenalty));
            }
        }

        private void ExecuteJoin(object param)
        {
            var penalty = this.SelectedPenalty;
            if (penalty != null)
            {
                this.JoinPenalty(penalty.Penalty);
            }
        }

        private bool CanExecuteJoin(object param)
        {
            var penalty = this.SelectedPenalty;
            return penalty != null;
        }

        private void ExecuteEdit(object param)
        {
            var penalty = this.SelectedPenalty;
            if (penalty != null)
            {
                this.EditPenalty(penalty.Penalty);
            }
        }

        private bool CanExecuteEdit(object param)
        {
            var penalty = this.SelectedPenalty;
            return penalty != null;
        }

        private void ExecuteDecideResult(object param)
        {
            var penalty = this.SelectedPenalty;
            if (penalty != null)
            {
                this.DecidePenaltyResult(penalty);
            }
        }

        private bool CanExecuteDecideResult(object param)
        {
            var penalty = this.SelectedPenalty;
            return penalty != null && penalty.Penalty.IsUnderInvestigation && !penalty.Penalty.IsLocked;
        }

        private void ExecuteDeletePenalty(object o)
        {
            if (this.SelectedPenalty != null && this.SelectedPenalty.Penalty != null)
            {
                SyncManager.Instance.SendPenaltyDelete(this.SelectedPenalty.Penalty);
                this.Penalties.Remove(this.SelectedPenalty);
            }
        }

        private void ExecuteDeleteDecidedPenalty(object o)
        {
            if (this.SelectedDecidedPenalty != null && this.SelectedDecidedPenalty.Penalty != null)
            {
                SyncManager.Instance.SendPenaltyDelete(this.SelectedDecidedPenalty.Penalty);
                this.DecidedPenalties.Remove(this.SelectedDecidedPenalty);
            }
        }

        private bool CanExecuteDeletePenalty(object o)
        {
            return this.SelectedPenalty != null;
        }

        private bool CanExecuteDeleteDecidedPenalty(object o)
        {
            return this.SelectedDecidedPenalty != null;
        }
        #endregion

    }
}
