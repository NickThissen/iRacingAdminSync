using System;
using System.Collections.Generic;
using iRacingAdmin.Classes;
using iRacingSdkWrapper;
using PostSharp.Aspects.Internals;

namespace iRacingAdmin.Models.Drivers
{
    public class OfftrackHistory
    {
        private const double MIN_OFFTRACK_SPEED = 56.237;

        private Offtrack _previousOfftrack;
        private Offtrack _currentOfftrack;

        public OfftrackHistory(Driver driver)
        {
            _driver = driver;
            _offtracks = new List<Offtrack>();
        }
        
        private readonly Driver _driver;
        public Driver Driver { get { return _driver; } }
        
        private List<Offtrack> _offtracks;
        public List<Offtrack> Offtracks
        {
            get { return _offtracks; }
        }

        public static int OfftrackLimit { get; set; }

        public Offtrack RegisterTrackStatus(TrackSurfaces status, double sessionTime)
        {
            if (status == TrackSurfaces.OffTrack)
            {
                // Currently offtrack
                if (_currentOfftrack == null)
                {
                    // Was not offtrack before - check how long ago they went ontrack again

                    // Is the previous offtrack long enough ago?
                    if (_previousOfftrack != null && _previousOfftrack.EndTime.HasValue &&
                        _previousOfftrack.EndTime.Value <= sessionTime - Properties.Settings.Default.OfftrackTimeout * 1000)
                    {
                        // Previous offtrack was less than OfftrackTimeout ago: ignore
                        return null;
                    }

                    // Is the speed high enough? > 35 mph = 56.327
                    if (this.Driver.Live.Speed < MIN_OFFTRACK_SPEED)
                    {
                        // Too slow - ignore
                        return null;
                    }

                    this.StartOfftrack(sessionTime);
                    return _currentOfftrack;
                }
                else
                {
                    // Was already offtrack - ignore
                    return null;
                }
            }
            else
            {
                // Not currently offtrack
                if (_currentOfftrack != null)
                {
                    // Driver just returned to track - set end time
                    _currentOfftrack.EndTime = sessionTime;

                    var cur = _currentOfftrack;
                    _currentOfftrack = null;
                    return cur;
                }
            }
            return null;
        }

        private Offtrack StartOfftrack(double sessionTime)
        {
            _previousOfftrack = _currentOfftrack;

            _currentOfftrack = new Offtrack(this.Driver.Id, sessionTime);
            this.Offtracks.Add(_currentOfftrack);
            this.Driver.OfftrackTotalCount = this.Offtracks.Count;

            return _currentOfftrack;
        }

        public void Clear()
        {
            _previousOfftrack = null;
            _currentOfftrack = null;
            this.Offtracks.Clear();
        }
    }

    public class Offtrack
    {
        public Offtrack(int id, double start)
        {
            var guid = Guid.NewGuid();
            this.UniqueId = new ShortGuid(guid).ToString();
            this.DriverId = id;
            this.StartTime = start;
        }

        public string UniqueId { get; set; }
        public int DriverId { get; set; }
        public double StartTime { get; set; }
        public double? EndTime { get; set; }
        public bool IsMaxTime { get; set; }
    }
}
