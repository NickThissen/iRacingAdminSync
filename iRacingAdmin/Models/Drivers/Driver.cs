using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Markup;
using System.Windows.Media;
using iRacingAdmin.Classes;
using iRacingAdmin.Sync;
using iRacingSdkWrapper;
using PostSharp.Patterns.Model;

namespace iRacingAdmin.Models.Drivers
{
    [NotifyPropertyChanged]
    public class Driver
    {
        private const string PACECAR_NAME = "safety pcfr500s";

        public Driver()
        {
            this.Results = new DriverResults(this);
            this.Live = new DriverLiveInfo(this);
            this.PitInfo = new DriverPitInfo(this);
            this.Delta = new DriverDeltaTime(this);
            //this.OfftrackHistory = new OfftrackHistory(this);
        }

        public int Id { get; set; }
        public int CustId { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }

        public string NameNumber
        {
            get
            {
                return string.Format("#{0}. {1}", this.CarNumber, this.Name);
            }
        }

        public int TeamId { get; set; }
        public string TeamName { get; set; }

        public string CarNumber { get; set; }
        public int CarNumberRaw { get; set; }
        public int CarId { get; set; }
        public string CarName { get; set; }
        public int CarClassId { get; set; }
        public int CarClassRelSpeed { get; set; }
        public Color CarClassColor { get; set; }

        public int IRating { get; set; }
        public License License { get; set; }

        public bool IsSpectator { get; set; }
        public bool IsPacecar { get; set; }

        public string HelmetDesign { get; set; }

        public string ClubName { get; set; }
        public string DivisionName { get; set; }

        public DriverResults Results { get; private set; }
        public DriverLiveInfo Live { get; private set; }
        public DriverPitInfo PitInfo { get; set; }
        public DriverDeltaTime Delta { get; set; }
        //public OfftrackHistory OfftrackHistory { get; private set; }

        private int _offtrackTotalCount;
        public int OfftrackTotalCount
        {
            get { return _offtrackTotalCount; }
            set
            {
                _offtrackTotalCount = value;

                //this.OnPropertyChanged();
                if (OfftrackHistory.OfftrackLimit > 0)
                {
                    this.OfftrackCurrentCount = value % OfftrackHistory.OfftrackLimit;
                    this.OfftrackPenaltyCount = (int)Math.Floor(value / (float)OfftrackHistory.OfftrackLimit);
                }
                else
                {
                    this.OfftrackCurrentCount = value;
                    this.OfftrackPenaltyCount = 0;
                }
            }
        }

        public int OfftrackCurrentCount { get; set; }
        public int OfftrackPenaltyCount { get; set; }

        public void ParseDynamicSessionInfo(SessionInfo info)
        {
            // Parse only session info that could have changed (driver dependent)
            var query = info["DriverInfo"]["Drivers"]["CarIdx", this.Id];

            this.Name = query["UserName"].GetValue("");
            this.CustId = Parser.ParseInt(query["UserID"].GetValue("0"));
            this.ShortName = query["AbbrevName"].GetValue();

            this.IRating = Parser.ParseInt(query["IRating"].GetValue());
            var licenseLevel = Parser.ParseInt(query["LicLevel"].GetValue());
            var licenseSublevel = Parser.ParseInt(query["LicSubLevel"].GetValue());
            var licenseColor = Parser.ParseColor(query["LicColor"].GetValue());

            this.License = new License(licenseLevel, licenseSublevel, licenseColor);
            this.IsSpectator = Parser.ParseInt(query["IsSpectator"].GetValue()) == 1;

            this.HelmetDesign = query["HelmetDesignStr"].GetValue();

            this.ClubName = query["ClubName"].GetValue();
            this.DivisionName = query["DivisionName"].GetValue();

            //this.OfftrackTotalCount = this.OfftrackHistory.Offtracks.Count;
        }

        public void ParseStaticSessionInfo(SessionInfo info)
        {
            var query = info["DriverInfo"]["Drivers"]["CarIdx", this.Id];

            this.TeamId = int.Parse(query["TeamID"].GetValue("0"));
            this.TeamName = query["TeamName"].GetValue();

            this.CarNumber = query["CarNumber"].GetValue();
            this.CarNumberRaw = int.Parse(query["CarNumberRaw"].GetValue("0"));
            if (this.CarNumber != null)
            {
                this.CarNumber = this.CarNumber.TrimStart('\"').TrimEnd('\"');
            }

            this.CarId = Parser.ParseInt(query["CarID"].GetValue());
            this.CarName = query["CarPath"].GetValue();
            this.CarClassId = Parser.ParseInt(query["CarClassID"].GetValue());
            this.CarClassRelSpeed = Parser.ParseInt(query["CarClassRelSpeed"].GetValue());
            this.CarClassColor =  Parser.ParseColor(query["CarClassColor"].GetValue());
            //this.CarClassColor.Freeze();

            this.IsPacecar = this.CustId == -1 || this.CarName == PACECAR_NAME;
        }

        public static Driver FromSessionInfo(SessionInfo info, int carIdx)
        {
            var query = info["DriverInfo"]["Drivers"]["CarIdx", carIdx];

            string name;
            if (!query["UserName"].TryGetValue(out name))
            {
                // Driver not found
                return null;
            }

            var driver = new Driver();
            driver.Id = carIdx;
            driver.ParseDynamicSessionInfo(info);
            driver.ParseStaticSessionInfo(info);

            return driver;
        }

        internal void UpdateResultsInfo(int sessionNumber, YamlQuery query, int position)
        {
            this.Results.SetResults(sessionNumber, query, position);
        }

        internal void UpdateLiveInfo(TelemetryInfo e)
        {
            this.Live.ParseTelemetry(e);
        }

        //internal void UpdateOfftrackHistory(List<Offtrack> list)
        //{
        //    this.OfftrackHistory.Offtracks.Clear();
        //    this.OfftrackHistory.Offtracks.AddRange(list);
        //    this.OfftrackTotalCount = this.OfftrackHistory.Offtracks.Count;
        //}
        
        //public event PropertyChangedEventHandler PropertyChanged;

        //protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        //}
    }
}
