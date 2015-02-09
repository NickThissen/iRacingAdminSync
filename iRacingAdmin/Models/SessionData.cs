using iRacingAdmin.Classes;
using iRacingSdkWrapper;

namespace iRacingAdmin.Models
{
    public class SessionData
    {
        public Track Track { get; set; }
        public string EventType { get; set; }
        public int SubsessionId { get; set; }

        public double SessionTime { get; set; }
        public double TimeRemaining { get; set; }
        public int LeaderLap { get; set; }

        public string RaceLaps { get; set; }
        public double RaceTime { get; set; }

        public void Update(SessionInfo info)
        {
            this.Track = Track.FromSessionInfo(info);
            this.EventType = info["WeekendInfo"]["EventType"].GetValue();
            this.SubsessionId = Parser.ParseInt(info["WeekendInfo"]["SubSessionID"].GetValue());

            var session = info["SessionInfo"]["Sessions"]["SessionNum", Simulator.Instance.CurrentSessionNumber];
            var laps = session["SessionLaps"].GetValue();
            var time = Parser.ParseSec(session["SessionTime"].GetValue());

            this.RaceLaps = laps;
            this.RaceTime = time;
        }

        public void Update(TelemetryInfo telemetry)
        {
            this.SessionTime = telemetry.SessionTime.Value;
            this.TimeRemaining = telemetry.SessionTimeRemain.Value;
        }
    }
}
