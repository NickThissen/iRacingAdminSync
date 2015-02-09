using System;
using System.IO;
using iRacingSdkWrapper;

namespace iRacingSimulator
{
    public class Connection
    {
        private Connection()
        {
            _sdk = new SdkWrapper();
            _sdk.TelemetryUpdateFrequency = 5;
            _sdk.EventRaiseType = SdkWrapper.EventRaiseTypes.BackgroundThread;

            _sdk.SessionInfoUpdated += SdkOnSessionInfoUpdated;
            _sdk.TelemetryUpdated += SdkOnTelemetryUpdated;
        }

        public void Start()
        {
            this.IsSimulated = false;
            this.SubSessionId = null;
            this.Sdk.Start();
            this.IsRunning = true;
        }

        public void Stop()
        {
            this.Sdk.Stop();
            this.IsRunning = false;
        }

        public void StartSimulate()
        {
            // Simulate session info update
            this.CurrentSessionNumber = 1;
            this.IsSimulated = true;
            this.IsRunning = true;

            this.Driver = new DriverInfo();
            this.Driver.Id = 0;
            this.Driver.Username = "Test user";
            this.Driver.Initials = "TU";

            var yaml = File.ReadAllText("sessioninfo_example.yaml");
            this.SdkOnSessionInfoUpdated(null, new SdkWrapper.SessionInfoUpdatedEventArgs(yaml, 0));
        }

        private void SdkOnTelemetryUpdated(object sender, SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            this.CurrentSessionNumber = e.TelemetryInfo.SessionNum.Value;

            if (this.TelemetryUpdated != null)
            {
                this.TelemetryUpdated(this, e);
            }
        }

        private void SdkOnSessionInfoUpdated(object sender, SdkWrapper.SessionInfoUpdatedEventArgs e)
        {
            var ssid = e.SessionInfo["WeekendInfo"]["SubSessionID"].GetValue();
            if (!string.IsNullOrWhiteSpace(ssid))
            {
                this.SubSessionId = long.Parse(ssid);
            }
            else
            {
                this.SubSessionId = null;
            }

            var query = e.SessionInfo["DriverInfo"]["Drivers"]["CarIdx", Sdk.DriverId];

            this.Driver = new DriverInfo();
            this.Driver.Id = int.Parse(query["UserID"].GetValue("0"));
            this.Driver.Username = query["UserName"].GetValue();
            this.Driver.Initials = query["Initials"].GetValue();

            if (this.SessionInfoUpdated != null)
            {
                this.SessionInfoUpdated(this, e);
            }
        }

        private static Connection _instance;
        public static Connection Instance
        {
            get { return _instance ?? (_instance = new Connection()); }
        }

        private SdkWrapper _sdk;
        public SdkWrapper Sdk { get { return _sdk; } }

        public int? CurrentSessionNumber { get; set; }

        private long? _subSessionId;
        public long? SubSessionId
        {
            get { return _subSessionId; }
            set
            {
                _subSessionId = value;
                if (this.SubSessionIdChanged != null)
                {
                    this.SubSessionIdChanged(this, EventArgs.Empty);
                }                
            }
        }

        public bool IsRunning { get; set; }
        public bool IsSimulated { get; set; }
        public DriverInfo Driver { get; set; }

        #region Events

        public event EventHandler SubSessionIdChanged;
        public event EventHandler<SdkWrapper.TelemetryUpdatedEventArgs> TelemetryUpdated;
        public event EventHandler<SdkWrapper.SessionInfoUpdatedEventArgs> SessionInfoUpdated;

        #endregion
    }

    public class DriverInfo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Initials { get; set; }
    }
}
