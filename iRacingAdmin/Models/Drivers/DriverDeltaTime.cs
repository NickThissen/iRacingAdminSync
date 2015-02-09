using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iRacingAdmin.Models.ViewModels;
using iRacingSdkWrapper;
using PostSharp.Patterns.Model;

namespace iRacingAdmin.Models.Drivers
{
    public class DriverDeltaTime : ViewModelBase
    {
        private readonly Driver _driver;
        private DeltaTime _toNext;
        private DeltaTime _toLeader;

        public DriverDeltaTime(Driver driver)
        {
            _driver = driver;
            this.ToNext = DeltaTime.Zero;
            this.ToLeader = DeltaTime.Zero;
        }

        public DeltaTime ToNext
        {
            get { return _toNext; }
            set
            {
                _toNext = value;
                this.OnPropertyChanged();
            }
        }

        public DeltaTime ToLeader
        {
            get { return _toLeader; }
            set
            {
                _toLeader = value;
                this.OnPropertyChanged();
            }
        }
    }

    public class DeltaTime : ViewModelBase
    {
        private double? _time;
        private int _laps;
        private bool _isLapped;

        public double? Time
        {
            get { return _time; }
            set
            {
                _time = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("Display");
            }
        }

        public int Laps
        {
            get { return _laps; }
            set
            {
                _laps = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("Display");
            }
        }

        public bool IsLapped
        {
            get { return _isLapped; }
            set
            {
                _isLapped = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("Display");
            }
        }

        public static DeltaTime FromTime(double time)
        {
            return new DeltaTime {Time = time, Laps = 0, IsLapped = false};
        }

        public static DeltaTime FromLaps(int laps)
        {
            return new DeltaTime {Time = null, Laps = laps, IsLapped = true};
        }

        public string Display
        {
            get
            {
                if (this.IsLapped) return this.Laps + " L";
                if (this.Time == null) return "-";

                var seconds = (float) Math.Abs(this.Time.GetValueOrDefault());
                var laptime = new Laptime(seconds);

                if (this.Time.GetValueOrDefault() < 0)
                    return "-" + laptime.DisplayShort;
                return laptime.DisplayShort;
            }
        }

        public override string ToString()
        {
            return this.Display;
        }

        public static DeltaTime Zero { get { return new DeltaTime {Time = null, IsLapped = false, Laps = 0}; }}
    }
}
