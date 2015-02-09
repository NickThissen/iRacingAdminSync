using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using PostSharp.Patterns.Model;

namespace iRacingAdmin.Sync.Penalties
{
    [NotifyPropertyChanged]
    public class PenaltyResult
    {
        public PenaltyResult()
            : this(PenaltyResultTypes.Investigation)
        {
        }

        public PenaltyResult(PenaltyResultTypes type)
        {
            this.Type = type;
            this.DriverId = -1;
            this.PenaltyMessage = "";
            this.PenaltyValue = 0;
            this.CommandFormat = "";
        }

        public int DriverId { get; set; }

        public string CarNumber
        {
            get { return _carNumber; }
            set
            {
                _carNumber = value;
                this.OnPropertyChanged();
                this.UpdateCommand();
            }
        }

        public PenaltyResultTypes Type { get; set; }
        public string DisplayLong { get; set; }
        public string DisplayShort { get; set; }

        // Commands
        public string CommandFormat
        {
            get { return _commandFormat; }
            set
            {
                _commandFormat = value;
                this.OnPropertyChanged();
                this.UpdateCommand();
            }
        }

        private int _penaltyValue;

        public int PenaltyValue
        {
            get { return _penaltyValue; }
            set
            {
                _penaltyValue = value;
                this.OnPropertyChanged();
                this.UpdateCommand();
            }
        }

        private string _penaltyMessage;
        private string _carNumber;
        private string _commandFormat;
        private string _command;

        public string PenaltyMessage
        {
            get { return _penaltyMessage; }
            set
            {
                _penaltyMessage = value;
                this.OnPropertyChanged();
                this.UpdateCommand();
            }
        }

        public bool HasValue { get; set; }

        public bool CanServe { get; set; }
        public bool Served { get; set; }

        public int ServedLap { get; set; }

        public string Command
        {
            get { return _command; }
            set
            {
                _command = value;
                this.OnPropertyChanged();
            }
        }

        private void UpdateCommand()
        {
            if (string.IsNullOrWhiteSpace(this.CommandFormat)
                    || this.CarNumber == null
                    || this.PenaltyValue < 0)
            {
                Debug.WriteLine(">>> Command returnin empty.");
                this.Command = "";
            }
            else
            {
                var command = string.Format(this.CommandFormat, this.CarNumber, this.PenaltyValue, this.PenaltyMessage);
                Debug.WriteLine(">>> Command changed: " + command);
                this.Command = command;
            }
        }

        //[JsonIgnore]
        //public string Command
        //{
        //    get
        //    {
        //        if (string.IsNullOrWhiteSpace(this.CommandFormat)
        //            || this.CarNumber == null
        //            || this.PenaltyValue < 0)
        //        {
        //            Debug.WriteLine(">>> Command returnin empty.");
        //            return "";
        //        }
        //        var command = string.Format(this.CommandFormat, this.CarNumber, this.PenaltyValue, this.PenaltyMessage);
        //        Debug.WriteLine(">>> Command changed: " + command);
        //        return command;
        //    }
        //}

        public override string ToString()
        {
            return this.DisplayLong;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private static List<PenaltyResult> _allResults;

        public static List<PenaltyResult> AllResults()
        {
            if (_allResults == null)
            {
                _allResults = new List<PenaltyResult>(new[]
                {
                    Investigation(),
                    Warning(),
                    NoFurtherAction(),
                    StopAndGo(), 
                    StopAndHoldLaps(), 
                    StopAndHoldTime(), 
                    TimePenalty(), 
                    GridPenalty(),
                    Disqualify(),
                    Remove(),
                    Mute(),
                    Unmute(), 
                    Clear(), 
                    Other("Other", "Other")
                });
            }
            return _allResults;
        } 

        public static PenaltyResult FromType(PenaltyResultTypes type)
        {
            switch (type)
            {
                case PenaltyResultTypes.Clear:
                    return Clear();
                case PenaltyResultTypes.Disqualify:
                    return Disqualify();
                case PenaltyResultTypes.GridPenalty:
                    return GridPenalty();
                case PenaltyResultTypes.Mute:
                    return Mute();
                case PenaltyResultTypes.Investigation:
                    return Investigation();
                case PenaltyResultTypes.NoFurtherAction:
                    return NoFurtherAction();
                case PenaltyResultTypes.Remove:
                    return Remove();
                case PenaltyResultTypes.StopAndGo:
                    return StopAndGo();
                case PenaltyResultTypes.StopAndHoldLaps:
                    return StopAndHoldLaps();
                case PenaltyResultTypes.StopAndHoldTime:
                    return StopAndHoldTime();
                case PenaltyResultTypes.TimePenalty:
                    return TimePenalty();
                case PenaltyResultTypes.Unmute:
                    return Unmute();
                case PenaltyResultTypes.Warning:
                    return Warning();
            }
            return Other("", "");
        }

        public static PenaltyResult Investigation()
        {
            var result = new PenaltyResult(PenaltyResultTypes.Investigation);
            result.DisplayLong = "Under investigation";
            result.DisplayShort = "INV";

            result.CommandFormat = "/all {2}";

            return result;
        }

        public static PenaltyResult Clear()
        {
            var result = new PenaltyResult(PenaltyResultTypes.Clear);
            result.DisplayLong = "Cleared";
            result.DisplayShort = "CLR";

            result.CommandFormat = "!cl {0} {2}";

            return result;
        }

        public static PenaltyResult Warning()
        {
            var result = new PenaltyResult(PenaltyResultTypes.Warning);
            result.DisplayLong = "Warning";
            result.DisplayShort = "WARN";

            result.CommandFormat = "/{0} {2}";

            return result;
        }

        public static PenaltyResult NoFurtherAction()
        {
            var result = new PenaltyResult(PenaltyResultTypes.NoFurtherAction);
            result.DisplayLong = "No further action";
            result.DisplayShort = "NFA";
            return result;
        }

        public static PenaltyResult StopAndGo()
        {
            var result = new PenaltyResult(PenaltyResultTypes.StopAndGo);
            result.DisplayLong = "Stop & Go";
            result.DisplayShort = "S&G";
            result.CanServe = true;

            result.CommandFormat = "!bl {0} 0 - {2}";

            return result;
        }

        public static PenaltyResult StopAndHoldTime()
        {
            var result = new PenaltyResult(PenaltyResultTypes.StopAndHoldTime);
            result.DisplayLong = "Stop & Hold (time)";
            result.DisplayShort = "S&H";
            result.CanServe = true;
            result.HasValue = true;

            result.CommandFormat = "!bl {0} {1} - {2}";

            return result;
        }

        public static PenaltyResult StopAndHoldLaps()
        {
            var result = new PenaltyResult(PenaltyResultTypes.StopAndHoldLaps);
            result.DisplayLong = "Stop & Hold (laps)";
            result.DisplayShort = "S&H";
            result.CanServe = true;
            result.HasValue = true;

            result.CommandFormat = "!bl {0} L{1} - {2}";

            return result;
        }

        public static PenaltyResult Disqualify()
        {
            var result = new PenaltyResult(PenaltyResultTypes.Disqualify);
            result.DisplayLong = "Disqualified";
            result.DisplayShort = "DQ";

            result.CommandFormat = "!dq {0} {2}";

            return result;
        }

        public static PenaltyResult Remove()
        {
            var result = new PenaltyResult(PenaltyResultTypes.Remove);
            result.DisplayLong = "Removed";
            result.DisplayShort = "REM";

            result.CommandFormat = "!remove {0} {2}";

            return result;
        }

        public static PenaltyResult GridPenalty()
        {
            var result = new PenaltyResult(PenaltyResultTypes.GridPenalty);
            result.DisplayLong = "Grid penalty";
            result.DisplayShort = "GRID";
            result.HasValue = true;
            return result;
        }

        public static PenaltyResult TimePenalty()
        {
            var result = new PenaltyResult(PenaltyResultTypes.TimePenalty);
            result.DisplayLong = "Time penalty";
            result.DisplayShort = "TIME";
            result.HasValue = true;
            return result;
        }

        public static PenaltyResult Mute()
        {
            var result = new PenaltyResult(PenaltyResultTypes.Mute);
            result.DisplayLong = "Mute";
            result.DisplayShort = "MUTE";

            result.CommandFormat = "!nch {0} {2}";

            return result;
        }


        public static PenaltyResult Unmute()
        {
            var result = new PenaltyResult(PenaltyResultTypes.Unmute);
            result.DisplayLong = "Unmute";
            result.DisplayShort = "UNMT";

            result.CommandFormat = "!ch {0} {2}";

            return result;
        }

        public static PenaltyResult Other(string message, string display)
        {
            var result = new PenaltyResult(PenaltyResultTypes.Other);
            result.DisplayLong = message;
            result.DisplayShort = display;
            result.CanServe = true;
            result.HasValue = true;
            return result;
        }

        public enum PenaltyResultTypes
        {
            Investigation,
            Clear,
            Warning,
            NoFurtherAction,
            StopAndGo,
            StopAndHoldTime,
            StopAndHoldLaps,
            Disqualify,
            Remove,
            GridPenalty,
            TimePenalty,
            Mute,
            Unmute,
            Other
        }
    }
}
