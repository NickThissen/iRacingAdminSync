using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Views;

namespace iRacingAdmin.Models.ViewModels
{
    public class OfftrackLimitsViewModel : ViewModelBase
    {
        private OfftracksViewModel _model;
        private OfftrackLimitsView _view;

        public OfftrackLimitsViewModel(OfftracksViewModel model)
        {
            _model = model;

            _driverLimits = new ObservableCollection<DriverOfftrackLimit>();
            _view = new OfftrackLimitsView();
            _view.DataContext = this;
        }

        private ObservableCollection<DriverOfftrackLimit> _driverLimits;
        public ObservableCollection<DriverOfftrackLimit> DriverLimits { get { return _driverLimits; } } 

        public void Show()
        {
            _view.Show();
        }

        public void CheckLimits()
        {
            foreach (var driver in Simulator.Instance.Drivers)
            {
                // Offtrack count rounded to limit
                var count = driver.Driver.OfftrackPenaltyCount * OfftrackHistory.OfftrackLimit;
                if (count > 0)
                {
                    var limit = AddLimit(driver, count);
                    if (limit != null)
                    {
                        // New limit reached
                        _model.NotifyOfftrackLimit(limit);
                    }
                }
            }
        }

        public DriverOfftrackLimit AddLimit(DriverContainer driver, int count)
        {
            if (this.DriverLimits.Any(
                d => d.Driver.Driver.Id == driver.Driver.Id
                     && d.OfftrackCount == count))
            {
                // Already added
                return null;
            }

            var limit = new DriverOfftrackLimit(driver, count);
            this.DriverLimits.Add(limit);

            return limit;
        }
    }
}
