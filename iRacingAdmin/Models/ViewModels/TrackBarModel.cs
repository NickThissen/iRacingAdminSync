using System.Collections.ObjectModel;
using System.Linq;
using iRacingAdmin.Models.Drivers;
using iRacingSdkWrapper;

namespace iRacingAdmin.Models.ViewModels
{
    public class TrackBarModel : SdkViewModel
    {
        private MainViewModel _mainModel;

        public TrackBarModel(MainViewModel mainModel)
        {
            _mainModel = mainModel;
            _drivers = new ObservableCollection<TrackBarDriver>();
        }

        private readonly ObservableCollection<TrackBarDriver> _drivers;
        public ObservableCollection<TrackBarDriver> Drivers { get { return _drivers; } }

        private float _trackWidth;

        public float TrackWidth
        {
            get { return _trackWidth; }
            set
            {
                _trackWidth = value;
                this.OnPropertyChanged();
            }
        }

        private float _lineWidth;
        public float LineWidth
        {
            get { return _lineWidth; }
            set
            {
                _lineWidth = value;
                this.UpdateDrivers();
                this.OnPropertyChanged();
            }
        }

        public override void OnSyncStateUpdated()
        {
            //this.UpdateDrivers();
        }

        public override void OnSessionInfoUpdated(SdkWrapper.SessionInfoUpdatedEventArgs e)
        {
            //this.UpdateDrivers();
        }

        public override void OnTelemetryUpdated(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            this.UpdateDrivers();
        }

        private void UpdateDrivers()
        {
            bool top = true;

            _drivers.Clear();
            var sessionNumber = Simulator.Instance.CurrentSessionNumber;
            if (sessionNumber == null) return;

            // Add all drivers to the trackbar
            foreach (var driver in Simulator.Instance.Drivers.OrderBy(d => d.Driver.Results[sessionNumber.Value].Position))
            {
                var trackDriver = new TrackBarDriver();

                trackDriver.DriverContainer = driver;
                trackDriver.AbsoluteLapDistance = this.TrackWidth*driver.Driver.Live.LapDistance - 15;

                // Top or bottom row?
                if (top)
                {
                    trackDriver.Row = 0;
                    trackDriver.StickRow = 1;
                }
                else
                {
                    trackDriver.Row = 3;
                    trackDriver.StickRow = 2;
                }

                top = !top;

                _drivers.Add(trackDriver);
            }

            this.OnPropertyChanged("Drivers");
        }

        public class TrackBarDriver
        {
            public DriverContainer DriverContainer { get; set; }
            public float AbsoluteLapDistance { get; set; }
            public int Row { get; set; }
            public int StickRow { get; set; }
        }
    }
}
