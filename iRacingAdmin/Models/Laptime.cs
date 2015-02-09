using System;
using System.Collections.Generic;
using System.Linq;

namespace iRacingAdmin.Models
{
    public class Laptime
    {
        public Laptime() : this(0) { }

        public Laptime(int value)
        {
            this.Value = value;
            this.Time = TimeSpan.FromMilliseconds(value);
        }

        public Laptime(float seconds)
            : this((int)(seconds * 1000f))
        {
        }

        public int Value { get; set; }
        public TimeSpan Time { get; set; }
        public int LapNumber { get; set; }

        public string Display
        {
            get
            {
                if (this.Value <= 0) return "-:--";
                if (this.Time.Minutes > 0)
                    return string.Format("{0:0}:{1:00}.{2:000}", this.Time.Minutes, this.Time.Seconds, this.Time.Milliseconds);
                return string.Format("{0:00}.{1:000}", this.Time.Seconds, this.Time.Milliseconds);
            }
        }

        public string DisplayShort
        {
            get
            {
                if (this.Value <= 0) return "-:--";

                int precision = 1;
                const int TIMESPAN_SIZE = 7;
                int factor = (int)Math.Pow(10, (TIMESPAN_SIZE - precision));
                var rounded = new TimeSpan(((long)Math.Round((1.0 * this.Time.Ticks / factor)) * factor));

                if (rounded.Minutes > 0)
                {
                    var min = rounded.Minutes;
                    var sec = rounded.TotalSeconds - 60 * min;
                    return string.Format("{0}:{1:00.0}", min, sec);
                }
                else
                {
                    var sec = rounded.TotalSeconds;
                    return string.Format("{0:0.0}", sec);
                }
            }
        }

        public static Laptime Empty
        {
            get
            {
                return new Laptime(0);
            }
        }
    }

    public class LaptimeCollection : List<Laptime>
    {
        public Laptime Average()
        {
            var validLaps = this.Where(l => l.Value > 0).ToList();
            if (validLaps.Count == 0) return Laptime.Empty;
            var averageMs = (int)validLaps.Average(l => l.Value);
            return new Laptime(averageMs);
        }
    }
}
