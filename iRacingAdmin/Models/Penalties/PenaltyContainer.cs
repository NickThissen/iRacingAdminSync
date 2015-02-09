using System;
using System.Collections.ObjectModel;
using System.Linq;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Models.ViewModels;
using iRacingAdmin.Sync.Penalties;
using PostSharp.Patterns.Model;

namespace iRacingAdmin.Models.Penalties
{
    [NotifyPropertyChanged]
    public class PenaltyContainer : ViewModelBase
    {
        public PenaltyContainer(Penalty penalty)
        {
            _penalty = penalty;
            _drivers = new ObservableCollection<DriverContainer>();
        }

        private readonly Penalty _penalty;
        public Penalty Penalty { get { return _penalty; } }

        private ObservableCollection<DriverContainer> _drivers;
        public ObservableCollection<DriverContainer> Drivers { get { return _drivers; }}

        [IgnoreAutoChangeNotification]
        public string DriversDisplay
        {
            get
            {
                return string.Join(Environment.NewLine, this.Drivers.Select(d => d.Driver.NameNumber));
            }
        }

        public DriverContainer ResultDriver { get; set; }

        public void UpdateDrivers()
        {
            this.Drivers.Clear();
            foreach (var id in this.Penalty.DriverIds)
            {
                var driver = Simulator.Instance.Drivers.FirstOrDefault(d => d.Driver.Id == id);
                if (driver != null)
                {
                    this.Drivers.Add(driver);
                }
            }

            if (this.Penalty.Result != null)
            {
                this.ResultDriver =
                    Simulator.Instance.Drivers.FirstOrDefault(d => d.Driver.Id == this.Penalty.Result.DriverId);
                if (this.ResultDriver != null)
                {
                    this.Penalty.Result.CarNumber = this.ResultDriver.Driver.CarNumber;
                }
                else
                {
                    this.Penalty.Result.CarNumber = "";
                    this.Penalty.Result.DriverId = -1;
                }
            }

            this.OnPropertyChanged("DriversDisplay");
        }
    }
}
