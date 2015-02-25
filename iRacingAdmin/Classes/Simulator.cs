using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using iRacingAdmin.Classes;
using iRacingAdmin.Extensions;
using iRacingAdmin.Models;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Sync;
using iRacingSdkWrapper;
using iRacingSimulator;

namespace iRacingAdmin
{
    public class Simulator
    {
        private const double LIVE_DIFF_MARGIN = 1.25d;

        private bool _isUpdatingDrivers;
        private TelemetryInfo _telemetry, _previousTelemetry;
        private SessionInfo _sessionInfo;
        private TimeDelta _timeDelta;

        private bool _mustUpdateSessionData = true;

        private Simulator()
        {
            _drivers = new DriverContainerCollection();
            _sessionData = new SessionData();

            Connection.Instance.SessionInfoUpdated += SdkOnSessionInfoUpdated;
            Connection.Instance.TelemetryUpdated += SdkOnTelemetryUpdated;
        }

        #region Properties

        private static Simulator _instance;
        public static Simulator Instance
        {
            get { return _instance; }
        }

        // Call from UI thread
        public static void Initialize()
        {
            _instance = new Simulator();
        }

        public SdkWrapper Sdk { get { return iRacingSimulator.Connection.Instance.Sdk; } }
        public int? CurrentSessionNumber { get { return iRacingSimulator.Connection.Instance.CurrentSessionNumber; } }

        public TelemetryInfo Telemetry { get { return _telemetry; } }
        public SessionInfo SessionInfo { get { return _sessionInfo; } }

        private SessionData _sessionData;
        public SessionData SessionData { get { return _sessionData; } }

        private readonly DriverContainerCollection _drivers;
        public DriverContainerCollection Drivers { get { return _drivers; } }

       // private DriverContainerCollection _threadDrivers;
        
        public DriverContainer WatchedDriver { get; set; }

        #endregion

        #region Methods

        #region Drivers

        private void UpdateDriverlist(SessionInfo sessionInfo)
        {
            if (_isUpdatingDrivers) return;

            new Thread(UpdateDriversThread).Start(sessionInfo);
        }

        private void UpdateDriversThread(object info)
        {
            var sessionInfo = (SessionInfo) info;

            var start = DateTime.Now;
            Debug.WriteLine(">> Start session info update.");

            _isUpdatingDrivers = true;

            this.GetDrivers(sessionInfo);

            _isUpdatingDrivers = false;

            this.GetResults(sessionInfo);

            var diff = DateTime.Now - start;
            Debug.WriteLine(">> End session info update: {0} ms.", diff.TotalMilliseconds);

            App.Instance.Dispatcher.Invoke(UpdateDriversFinished);
        }

        private void UpdateDriversFinished()
        {
            
        }

        private Task<DriverContainerCollection> GetDriversUpdate(SessionInfo sessionInfo)
        {
            var drivers = new DriverContainerCollection();
            for (int id = 0; id < 70; id++)
            {
                var driver = Driver.FromSessionInfo(sessionInfo, id);
                if (driver != null)
                {
                    drivers.Add(new DriverContainer(driver));
                }
                else
                {
                    // Found everyone
                    break;
                }
            }
            return Task.FromResult(drivers);
        }

        private void GetDrivers(SessionInfo sessionInfo)
        {
            for (int id = 0; id < 70; id++)
            {
                // Find existing driver
                Driver driver;
                var container = _drivers.FromId(id);
                if (container == null)
                {
                    // Create new driver from session info
                    driver = Driver.FromSessionInfo(sessionInfo, id);
                }
                else
                {
                    // Update existing driver
                    driver = container.Driver;
                    driver.ParseDynamicSessionInfo(sessionInfo);
                }

                // Check if this driver is already in the list
                if (driver != null)
                {
                    var existing = _drivers.FromId(id);
                    if (existing == null)
                    {
                        // Add him
                        App.Instance.Dispatcher.Invoke(() => _drivers.Add(new DriverContainer(driver)));
                    }
                    else
                    {
                        // Update only
                        existing.Driver = driver;
                    }
                }
            }
        }

        private void GetResults(SessionInfo sessionInfo)
        {
            if (_isUpdatingDrivers) return;
            if (this.CurrentSessionNumber == null) return;

            var query =
                sessionInfo["SessionInfo"]["Sessions"]["SessionNum", this.CurrentSessionNumber]["ResultsPositions"];

            for (int position = 1; position <= _drivers.Count; position++)
            {
                var positionQuery = query["Position", position];

                string idValue;
                if (!positionQuery["CarIdx"].TryGetValue(out idValue))
                {
                    // Driver not found
                    continue;
                }

                int id = int.Parse(idValue);                
                var driverContainer = _drivers.SingleOrDefault(d => d.Driver.Id == id);
                if (driverContainer != null)
                {
                    driverContainer.Driver.UpdateResultsInfo(this.CurrentSessionNumber.GetValueOrDefault(), positionQuery, position);
                }
            }
        }

        private void UpdateDriverTelemetry(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            if (_isUpdatingDrivers) return;

            var offtrackUpdates = new List<Offtrack>();
            foreach (var driver in _drivers)
            {
                driver.Driver.UpdateLiveInfo(e.TelemetryInfo);

               // driver.Driver.Live.CalculateSpeed(_previousTelemetry, e.TelemetryInfo, _trackLength);

                //var offtrack = driver.Driver.OfftrackHistory.RegisterTrackStatus(driver.Driver.Live.TrackSurface, 
                //    e.TelemetryInfo.SessionTime.Value);
                //if (offtrack != null) offtrackUpdates.Add(offtrack);
            }

            this.CalculateLivePositions();
            this.UpdateTimeDelta();
            
            bool isLive = IsReplayLive(e.TelemetryInfo.SessionTime.Value, e.TelemetryInfo.ReplaySessionTime.Value);

            this.UpdateLiveStatus(isLive);
            this.UpdateWatchedDriver(e.TelemetryInfo.CamCarIdx.Value);
            //if (offtrackUpdates.Count > 0) this.UpdateOfftrackHistory(offtrackUpdates);
        }

        private void CalculateLivePositions()
        {
            Driver leader = null;

            // Determine live position from lapdistance
            int pos = 1;
            foreach (var driver in _drivers.OrderByDescending(d => d.Driver.Live.TotalLapDistance))
            {
                if (pos == 1) leader = driver.Driver;
                driver.Driver.Live.Position = pos++;
            }

            if (leader != null) _sessionData.LeaderLap = leader.Results.Current.LapsComplete + 1;

            // Determine live class position from live positions and class
            // Group drivers in dictionary with key = classid and value = list of all drivers in that class
            var dict = (from driver in _drivers
                        group driver by driver.Driver.CarClassId)
                .ToDictionary(d => d.Key, d => d.ToList());

            // Set class position
            foreach (var drivers in dict.Values)
            {
                pos = 1;
                foreach (var driver in drivers.OrderBy(d => d.Driver.Live.Position))
                {
                    driver.Driver.Live.ClassPosition = pos++;
                }
            }
        }
        
        private void UpdateTimeDelta()
        {
            if (_timeDelta == null) return;

            // Update the positions of all cars
            _timeDelta.Update(_telemetry.SessionTime.Value, _telemetry.CarIdxLapDistPct.Value);

            // Order drivers by live position
            var drivers = _drivers.OrderBy(d => d.Driver.Live.Position).ToList();
            if (drivers.Count > 0)
            {
                // Get leader
                var leader = drivers[0];
                leader.Driver.Delta.ToLeader = DeltaTime.Zero;
                leader.Driver.Delta.ToNext = DeltaTime.Zero;

                // Loop through drivers
                for (int i = 1; i < drivers.Count; i++)
                {
                    var behind = drivers[i];
                    var ahead = drivers[i - 1];

                    // Lapped?
                    var leaderLapDiff = Math.Abs(leader.Driver.Live.TotalLapDistance - behind.Driver.Live.TotalLapDistance);
                    var nextLapDiff = Math.Abs(ahead.Driver.Live.TotalLapDistance - behind.Driver.Live.TotalLapDistance);

                    if (leaderLapDiff < 1)
                    {
                        var leaderDelta = _timeDelta.GetDelta(behind.Driver.Id, leader.Driver.Id);
                        behind.Driver.Delta.ToLeader = DeltaTime.FromTime(leaderDelta.TotalSeconds);
                    }
                    else
                    {
                        behind.Driver.Delta.ToLeader = DeltaTime.FromLaps((int)Math.Floor(leaderLapDiff));
                    }

                    if (nextLapDiff < 1)
                    {
                        var nextDelta = _timeDelta.GetDelta(behind.Driver.Id, ahead.Driver.Id);
                        behind.Driver.Delta.ToNext = DeltaTime.FromTime(nextDelta.TotalSeconds);
                    }
                    else
                    {
                        behind.Driver.Delta.ToNext = DeltaTime.FromLaps((int)Math.Floor(nextLapDiff));
                    }
                }
            }
        }

        private bool IsReplayLive(double sessionTime, double replayTime)
        {
            return Math.Abs(sessionTime - replayTime) < LIVE_DIFF_MARGIN;
        }

        private void UpdateWatchedDriver(int carId)
        {
            if (SyncManager.Instance.Status == SyncManager.ConnectionStatus.Connected &&
                SyncManager.Instance.User != null)
            {
                var id = SyncManager.Instance.UserId;
                int? currentId = null;

                if (SyncManager.Instance.State.WatchedDrivers.ContainsKey(id))
                {
                    currentId = SyncManager.Instance.State.WatchedDrivers[id];
                }

                if (currentId == null || currentId.Value != carId)
                {
                    // Changed camera, update state
                    var user = SyncManager.Instance.FindUser(id);
                    if (user != null)
                    {
                        var driver = this.Drivers.FromId(carId);
                        if (driver != null)
                        {
                            driver.UserCameras.Add(user);
                        }
                    }
                    SyncManager.Instance.State.WatchedDrivers[id] = carId;

                    // Send to server
                    SyncManager.Instance.SendStateUpdate(SyncCommandHelper.UpdateWatchedDriver(carId));
                }
            }
        }

        private void UpdateLiveStatus(bool live)
        {
            if (SyncManager.Instance.Status == SyncManager.ConnectionStatus.Connected &&
                SyncManager.Instance.User != null)
            {
                var id = SyncManager.Instance.UserId;
                bool? currentlyLive = null;

                if (SyncManager.Instance.State.LiveStatus.ContainsKey(id))
                {
                    currentlyLive = SyncManager.Instance.State.LiveStatus[id];
                }

                if (currentlyLive == null || currentlyLive.Value != live)
                {
                    // Changed live status, update
                    var user = SyncManager.Instance.FindUser(id);
                    if (user != null)
                    {
                        user.IsLive = live;
                    }
                    SyncManager.Instance.State.LiveStatus[id] = live;

                    // Send to server
                    SyncManager.Instance.SendStateUpdate(SyncCommandHelper.UpdateLiveStatus(live));
                }
            }
        }

        //private void UpdateOfftrackHistory(List<Offtrack> offtracks)
        //{
        //    if (SyncManager.Instance.Status == SyncManager.ConnectionStatus.Connected &&
        //        SyncManager.Instance.User != null)
        //    {
        //         Only host sends offtracks
        //        if (SyncManager.Instance.User.IsHost)
        //        {
        //            SyncManager.Instance.State.SetOfftrackHistory(this.Drivers);

        //             Send to server
        //            SyncManager.Instance.SendStateUpdate(SyncCommandHelper.UpdateOfftracks(offtracks));
        //        }
        //    }
        //}

        #endregion

        #endregion

        #region Events

        private void SdkOnSessionInfoUpdated(object sender, SdkWrapper.SessionInfoUpdatedEventArgs e)
        {
            _sessionInfo = e.SessionInfo;
            
            try
            {
                if (_mustUpdateSessionData)
                {
                    _sessionData.Update(e.SessionInfo);
                    _timeDelta = new TimeDelta((float)_sessionData.Track.Length * 1000f, 20, 64);
                    _mustUpdateSessionData = false;

                    this.OnStaticInfoChanged();
                }

                App.Instance.Dispatcher.Invoke(() =>
                {
                    // Handle session info update
                    this.UpdateDriverlist(e.SessionInfo);

                    // Broadcast to windows
                    if (this.SessionInfoUpdated != null)
                    {
                        this.SessionInfoUpdated(sender, e);
                    }

                });
            }
            catch (Exception ex)
            {
                App.Instance.LogError("Updating session info.", ex);
            }
        }

        private void SdkOnTelemetryUpdated(object sender, SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            _telemetry = e.TelemetryInfo;

            try
            {
                App.Instance.Dispatcher.Invoke(() =>
                {
                    // Handle telemetry update
                    this.UpdateDriverTelemetry(e);

                    // Update session data
                    this.SessionData.Update(e.TelemetryInfo);

                    // Broadcast to windows
                    if (this.TelemetryUpdated != null)
                    {
                        this.TelemetryUpdated(sender, e);
                    }
                });
            }
            catch (Exception ex)
            {
                App.Instance.LogError("Updating telemetry.", ex);
            }

            _previousTelemetry = e.TelemetryInfo;
        }

        public event EventHandler StaticInfoChanged;
        public event EventHandler<SdkWrapper.SessionInfoUpdatedEventArgs> SessionInfoUpdated;
        public event EventHandler<SdkWrapper.TelemetryUpdatedEventArgs> TelemetryUpdated;

        protected virtual void OnStaticInfoChanged()
        {
            if (this.StaticInfoChanged != null) this.StaticInfoChanged(this, EventArgs.Empty);
        }

        #endregion

    }
}
