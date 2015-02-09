using System;
using System.Diagnostics;
using iRacingSdkWrapper;
using Newtonsoft.Json;

namespace iRacingAdmin.Models.Drivers
{
    [Serializable]
    public class DriverPitInfo
    {
        public DriverPitInfo(Driver driver)
        {
            _driver = driver;
        }

        private readonly Driver _driver;
        public int Pitstops { get; set; }

        public bool InPitLane { get; set; }
        public bool InPitStall { get; set; }

        [JsonIgnore]
        public DateTime? PitLaneEntryTime { get; set; }
        [JsonIgnore]
        public DateTime? PitStallEntryTime { get; set; }

        public double LastPitLaneTimeSeconds { get; set; }
        public double LastPitStallTimeSeconds { get; set; }

        public double CurrentPitLaneTimeSeconds { get; set; }
        public double CurrentPitStallTimeSeconds { get; set; }

        public int LastPitLap { get; set; }
        public int CurrentStint { get; set; }

        public string LastPitStallTimeDisplay
        {
            get
            {
                if (this.InPitStall)
                {
                    return string.Format("{0:0.0} s (L {1})", this.CurrentPitStallTimeSeconds, _driver.Live.Lap);
                }
                else
                {
                    if (this.LastPitLap == 0 && this.LastPitLaneTimeSeconds <= 0) return "";
                    return string.Format("{0:0.0} s (L {1})", this.LastPitStallTimeSeconds, this.LastPitLap);
                }
            }
        }

        public void CalculatePitInfo()
        {
            this.InPitLane = _driver.Live.TrackSurface == TrackSurfaces.AproachingPits ||
                        _driver.Live.TrackSurface == TrackSurfaces.InPitStall;
            this.InPitStall = _driver.Live.TrackSurface == TrackSurfaces.InPitStall;

            this.CurrentStint = _driver.Results.Current.LapsComplete - this.LastPitLap;

            // Are we already in pitlane?
            if (this.PitLaneEntryTime == null)
            {
                // We were not previously in pitlane

                if (this.InPitLane)
                {
                    // We have now entered pitlane
                    this.PitLaneEntryTime = DateTime.UtcNow;
                    this.CurrentPitLaneTimeSeconds = 0;
                }
            }
            else
            {
                // We were already in pitlane but have not exited yet
                this.CurrentPitLaneTimeSeconds = (DateTime.UtcNow - this.PitLaneEntryTime.Value).TotalSeconds;

                // Are we already in pit stall?
                if (this.PitStallEntryTime == null)
                {
                    // We were not previously in our pit stall yet

                    if (this.InPitStall)
                    {
                        // We have just entered our pit stall
                        this.PitStallEntryTime = DateTime.UtcNow;
                        this.CurrentPitStallTimeSeconds = 0;
                    }
                }
                else
                {
                    // We already were in our pit stall
                    this.CurrentPitStallTimeSeconds =
                            (DateTime.UtcNow - this.PitStallEntryTime.Value).TotalSeconds;

                    if (!this.InPitStall)
                    {
                        // We have now left our pit stall
                        this.LastPitStallTimeSeconds =
                                (DateTime.UtcNow - this.PitStallEntryTime.Value).TotalSeconds;

                        this.CurrentPitStallTimeSeconds = 0;

                        this.Pitstops += 1;
                        this.LastPitLap = _driver.Results.Current.LapsComplete;
                        this.CurrentStint = 0;

                        // Reset
                        this.PitStallEntryTime = null;
                    }
                }
                
                if (!this.InPitLane)
                {
                    // We have now left pitlane
                    this.LastPitLaneTimeSeconds =
                        (DateTime.UtcNow - this.PitLaneEntryTime.Value).TotalSeconds;
                    this.CurrentPitLaneTimeSeconds = 0;

                    // Reset
                    this.PitLaneEntryTime = null;
                }
            }
        }
    }
}
