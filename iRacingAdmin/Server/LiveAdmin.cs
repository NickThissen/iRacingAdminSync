using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Sync;
using iRacingAdmin.Sync.Penalties;
using Newtonsoft.Json;

namespace iRacingAdmin.Server
{
    public static class LiveAdmin
    {
        public static string SendUpdate(SyncState state)
        {
            try
            {
                var settings = Properties.Settings.Default;

                var url = settings.LiveAdminUrl;
                var key = settings.LiveAdminKey;

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";

                var data = new LiveAdminData();
                data.Admins = state.Users.Select(LiveAdminData.LiveAdminUser.FromUser).ToList();
                data.Penalties = state.Penalties.Select(LiveAdminData.LiveAdminPenalty.FromPenalty)
                    .OrderByDescending(p => p.TimeGMT).ToList();

                var json = JsonConvert.SerializeObject(data);

                var post = string.Format("key={0}&ssid={1}&data={2}",
                    key,
                    state.SubsessionId,
                    System.Net.WebUtility.UrlEncode(json));
                var bytes = Encoding.UTF8.GetBytes(post);

                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = bytes.Length;

                using (var dataStream = request.GetRequestStream())
                {
                    dataStream.Write(bytes, 0, bytes.Length);
                }

                var response = request.GetResponse();
                using (var dataStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(dataStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [Serializable]
        public class LiveAdminData
        {
            public List<LiveAdminUser> Admins { get; set; }
            public List<LiveAdminPenalty> Penalties { get; set; }

            [Serializable]
            public class LiveAdminUser
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public string ShortName { get; set; }

                public static LiveAdminUser FromUser(User user)
                {
                    return new LiveAdminUser() { Id = user.CustId, Name = user.Name, ShortName = user.ShortName };
                }
            }

            [Serializable]
            public class LiveAdminPenalty
            {
                private static DateTime _epoch = new DateTime(1970, 1, 1);

                public string Id { get; set; }

                public int AdminId { get; set; }
                public string AdminName { get; set; }
                public string AdminShortName { get; set; }

                public int ProtestingTeamId { get; set; }
                public int OffendingTeamId { get; set; }

                public List<LiveAdminDriver> InvestigatedDrivers { get; set; }
                public string InvestigatedDriversDisplay { get; set; }

                public double TimeGMT { get; set; }

                public string Lap { get; set; }
                public string Turn { get; set; }

                public int RuleId { get; set; }
                public string Reason { get; set; }

                public bool Decided { get; set; }

                public LiveAdminDriver OffendingDriver { get; set; }

                public string Result { get; set; }
                public string ResultShort { get; set; }
                public int ResultDecidedLap { get; set; }

                public bool Served { get; set; }

                public static LiveAdminPenalty FromPenalty(Penalty penalty)
                {
                    var p = new LiveAdminPenalty();
                    p.Id = penalty.Id;

                    var admin = penalty.Users.FirstOrDefault();
                    if (admin != null)
                    {
                        p.AdminId = admin.CustId;
                        p.AdminName = admin.Name;
                        p.AdminShortName = admin.ShortName;
                    }

                    p.InvestigatedDrivers = new List<LiveAdminDriver>();
                    if (penalty.IsUnderInvestigation)
                    {
                        foreach (var id in penalty.DriverIds)
                        {
                            var driver = Simulator.Instance.Drivers.FromId(id);
                            if (driver != null)
                            {
                                p.InvestigatedDrivers.Add(LiveAdminDriver.FromDriver(driver.Driver));
                            }

                            p.InvestigatedDriversDisplay = string.Join("<br />",
                                p.InvestigatedDrivers.Select(d => d.Display));
                        }
                    }
                    else
                    {
                        var driver = Simulator.Instance.Drivers.FromId(penalty.Result.DriverId);
                        if (driver != null)
                        {
                            p.OffendingDriver = LiveAdminDriver.FromDriver(driver.Driver);
                        }
                        p.InvestigatedDriversDisplay = p.OffendingDriver.Display;
                    }

                    DateTime time = penalty.StartInvestigationTime.HasValue
                        ? penalty.StartInvestigationTime.Value
                        : _epoch;
                    p.TimeGMT = (time - _epoch).TotalMilliseconds;
                    p.Lap = penalty.Lap;
                    p.Turn = penalty.Turn;

                    //p.RuleId = ...
                    p.Decided = !penalty.IsUnderInvestigation;
                    p.Reason = penalty.Reason;
                    p.Result = penalty.Result.DisplayLong;
                    p.ResultShort = penalty.Result.DisplayShort;
                    p.ResultDecidedLap = penalty.DecidedLap;
                    p.Served = penalty.Result.Served;

                    return p;
                }
            }

            public class LiveAdminDriver
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public string ShortName { get; set; }
                public string Number { get; set; }

                public string Display
                {
                    get { return string.Format("#{0}. {1}", this.Number, this.Name); }
                }

                public static LiveAdminDriver FromDriver(Driver driver)
                {
                    return new LiveAdminDriver()
                    {
                        Id = driver.CustId,
                        Name = driver.Name,
                        ShortName = driver.ShortName,
                        Number = driver.CarNumber
                    };
                }
            }
        }
    }
}
