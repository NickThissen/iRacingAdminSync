using System;
using System.Collections.Generic;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Sync.Penalties;

namespace iRacingAdmin.Sync
{
    [Serializable]
    public class SyncState
    {
        public SyncState(long ssid)
        {
            _watchedDrivers = new Dictionary<int, int>();
            _liveStatus = new Dictionary<int, bool>();
            _penalties = new List<Penalty>();
            _events = new List<ReplayEvent>();
            _offtrackHistories = new Dictionary<int, List<Offtrack>>();

            _users = new UserList();

            this.SubsessionId = ssid;
            this.OfftrackLimit = Properties.Settings.Default.OfftrackLimit;
        }

        public long SubsessionId { get; set; }

        public int OfftrackLimit { get; set; }

        private readonly UserList _users;
        public UserList Users { get { return _users; } } 

        private readonly Dictionary<int, int> _watchedDrivers;
        public Dictionary<int, int> WatchedDrivers
        {
            get { return _watchedDrivers; }
        }

        private readonly Dictionary<int, bool> _liveStatus;
        public Dictionary<int, bool> LiveStatus
        {
            get { return _liveStatus; }
        }

        private List<Penalty> _penalties;
        public List<Penalty> Penalties
        {
            get { return _penalties; }
        }

        private List<ReplayEvent> _events;
        public List<ReplayEvent> Events
        {
            get { return _events; }
        }

        private Dictionary<int, List<Offtrack>> _offtrackHistories;
        public Dictionary<int, List<Offtrack>> OfftrackHistories
        {
            get { return _offtrackHistories; }
        }

        //public void SetOfftrackHistory(IEnumerable<DriverContainer> drivers)
        //{
        //    this.OfftrackHistories.Clear();
        //    foreach (var driver in drivers)
        //    {
        //        this.OfftrackHistories.Add(driver.Driver.Id, driver.Driver.OfftrackHistory.Offtracks);
        //    }
        //}
    }
}
