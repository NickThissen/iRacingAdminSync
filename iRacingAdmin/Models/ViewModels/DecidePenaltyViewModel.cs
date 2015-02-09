using System.Collections.Generic;
using System.Windows.Input;
using iRacingAdmin.Classes;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Models.Penalties;
using iRacingAdmin.Sync;
using iRacingAdmin.Sync.Penalties;
using iRacingAdmin.Views;

namespace iRacingAdmin.Models.ViewModels
{
    public class DecidePenaltyViewModel : ViewModelBase
    {
        private PenaltyResult _resetResult;

        public DecidePenaltyViewModel(PenaltyContainer penalty, DriverContainer driver)
        {
            _penalty = penalty;
            _driver = driver;

            _possibleResults = PenaltyResult.AllResults();

            _resetResult = penalty.Penalty.Result;

            if (string.IsNullOrWhiteSpace(penalty.Penalty.Result.PenaltyMessage))
            {
                penalty.Penalty.Result.PenaltyMessage = penalty.Penalty.Reason;
            }

            this.Penalty.Penalty.Result.DriverId = driver.Driver.Id;
            this.Penalty.UpdateDrivers();
        }

        private readonly PenaltyContainer _penalty;
        public PenaltyContainer Penalty { get { return _penalty; } }

        private readonly DriverContainer _driver;
        public DriverContainer Driver { get { return _driver; } }

        private readonly List<PenaltyResult> _possibleResults;
        public List<PenaltyResult> PossibleResults { get { return _possibleResults; } }

        private PenaltyResult _selectedResult;
        public PenaltyResult SelectedResult
        {
            get { return _selectedResult; }
            set
            {
                _selectedResult = value;
                this.OnPropertyChanged();
                this.OnSelectedResultChanged();
            }
        }

        private void OnSelectedResultChanged()
        {
            var selected = this.SelectedResult;
            if (selected == null) return;

            var type = selected.Type;

            var newResult = PenaltyResult.FromType(type);
            this.ChangeResult(newResult);
        }

        public void ChangeResult(PenaltyResult result)
        {
            var currentResult = this.Penalty.Penalty.Result;
            result.PenaltyMessage = currentResult.PenaltyMessage;
            result.PenaltyValue = currentResult.PenaltyValue;
            result.DriverId = this.Driver.Driver.Id;
            result.CarNumber = this.Driver.Driver.CarNumber;

            this.Penalty.Penalty.Result = result;
            this.Penalty.UpdateDrivers();
        }

        public void SaveResult()
        {
            this.Penalty.Penalty.DecideResult(this.Penalty.Penalty.Result, this.Driver.Driver.Id, SyncManager.Instance.User);
            this.Penalty.Penalty.DecidedLap = this.Driver.Driver.Live.Lap;
            this.Penalty.UpdateDrivers();
        }

        public void ResetResult()
        {
            this.ChangeResult(_resetResult);
        }

        #region Commands

        private ICommand _useCommand;
        public ICommand UseCommand
        {
            get { return _useCommand ?? (_useCommand = new RelayCommand(ExecuteUseCommand, CanExecuteUseCommand)); }
        }

        private void ExecuteUseCommand(object param)
        {
            var dialog = new SendCommandWindow(this.Penalty.Penalty.Result.Command);
            dialog.ShowDialog();
        }

        private bool CanExecuteUseCommand(object param)
        {
            return !string.IsNullOrWhiteSpace(this.Penalty.Penalty.Result.Command);
        }

        #endregion
    }
}
