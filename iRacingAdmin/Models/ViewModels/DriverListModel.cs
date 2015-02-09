using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using iRacingAdmin.Classes;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Models.Filtering;
using iRacingAdmin.Sync;

namespace iRacingAdmin.Models.ViewModels
{
    public class DriverListModel : ViewModelBase
    {
        private MainViewModel _mainModel;

        public DriverListModel(MainViewModel mainModel)
        {
            _mainModel = mainModel;

            // Initialize views and filters                      
            _driversView = (ListCollectionView) CollectionViewSource.GetDefaultView(Simulator.Instance.Drivers);
            _classFilterItems = new ObservableCollection<ClassFilterItem>();
            _classFilterItems.Add(ClassFilterItem.All());

            _driverFilter = new DriverFilter();
            _driverFilter.Attach(_driversView);
            _selectedDrivers = new ObservableCollection<DriverContainer>();

            
        }

        #region Properties
        
        private ListCollectionView _driversView;
        public ListCollectionView DriversView { get { return _driversView; } }

        private DriverContainer _selectedDriver;
        public DriverContainer SelectedDriver
        {
            get { return _selectedDriver; }
            set
            {
                _selectedDriver = value;
                _mainModel.OnSelectedDriversChanged();
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<DriverContainer> _selectedDrivers;
        public ObservableCollection<DriverContainer> SelectedDrivers
        {
            get { return _selectedDrivers; }
            set { _selectedDrivers = value; }
        }

        private DriverFilter _driverFilter;
        public DriverFilter DriverFilter { get { return _driverFilter; } }

        private ObservableCollection<ClassFilterItem> _classFilterItems;
        public ObservableCollection<ClassFilterItem> ClassFilterItems { get { return _classFilterItems; } }

        #endregion

        #region Methods

        public void OnSyncStateUpdated()
        {
            // Update offtracks
            //var state = SyncManager.Instance.State;
            //foreach (var driver in Simulator.Instance.Drivers.ToList())
            //{
            //    var id = driver.Driver.Id;
            //    if (state.OfftrackHistories.ContainsKey(id))
            //    {
            //        driver.Driver.UpdateOfftrackHistory(state.OfftrackHistories[id]);
            //    }
            //}
        }
        
        public void UpdateClassFilterList()
        {
            if (Simulator.Instance.Drivers == null) return;

            // When the session info update we might have to add or remove possible classes from the class filter dropdown
            var dict = _classFilterItems.ToDictionary(f => f.CarClassRelSpeed);

            foreach (var driver in Simulator.Instance.Drivers.OrderByDescending(d => d.Driver.CarClassRelSpeed))
            {
                if (driver.Driver.IsSpectator || driver.Driver.IsPacecar) continue;

                var speed = driver.Driver.CarClassRelSpeed;
                if (!dict.ContainsKey(speed))
                {
                    var item = new ClassFilterItem()
                    {
                        CarClassRelSpeed = speed,
                        Brush = new SolidColorBrush(driver.Driver.CarClassColor)
                    };
                    _classFilterItems.Add(item);
                    dict.Add(speed, item);
                }
            }
        }

        public void SwitchToDriver(DriverContainer driver)
        {
            if (driver == null) return;

            // Switch client camera to driver
            var camera = CameraDetails.ChangeFocus(driver.Driver.CarNumberRaw);
            CameraControl.ChangeCamera(camera);
        }

        internal void OnSelectedDriversChanged(List<DriverContainer> drivers)
        {
            this.SelectedDrivers.Clear();
            foreach (var driver in drivers) this.SelectedDrivers.Add(driver);

            //_mainModel.OfftracksModel.OnSelectedDriversChanged();
        }

        public void Refresh()
        {
            if (_driversView != null)
            {
                this.DriversView.Refresh();
            }
        }

        #endregion

        #region Commands

        private ICommand _investigateCommand;
        public ICommand InvestigateCommand
        {
            get { return _investigateCommand ?? (_investigateCommand = new RelayCommand(Investigate)); }
        }

        public void Investigate(object param)
        {
            var driver = this.SelectedDriver;
            if (driver != null)
            {
                _mainModel.PenaltyList.AddDriver(driver);
            }
        }

        #endregion


    }
}
