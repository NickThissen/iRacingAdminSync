using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iRacingAdmin.Classes;
using Newtonsoft.Json;

namespace iRacingAdmin.Models.Penalties
{
    public class PenaltyReason
    {
        public PenaltyReason(string code, string display)
        {
            this.Code = code;
            this.Display = display;
        }

        public string Code { get; set; }
        public string Display { get; set; }

        public static void SaveReasons(List<PenaltyReason> reasons)
        {
            var path = GetPath();
            var json = JsonConvert.SerializeObject(reasons);
            File.WriteAllText(path, json);
        }

        public static List<PenaltyReason> LoadReasons()
        {
            try
            {
                var path = GetPath();
                var json = File.ReadAllText(path);
                var reasons = JsonConvert.DeserializeObject<List<PenaltyReason>>(json);
                return reasons;
            }
            catch (Exception)
            {
                return ResetDefaultReasons();
            }
        }

        private static List<PenaltyReason> ResetDefaultReasons()
        {
            var reasons = new List<PenaltyReason>();
            reasons.Add(new PenaltyReason("AVOID", "Avoidable contact"));
            reasons.Add(new PenaltyReason("DANGER", "Dangerous driving"));
            reasons.Add(new PenaltyReason("LIMITS", "Exceeding track limits"));
            reasons.Add(new PenaltyReason("PENAL", "Incorrectly served / ignored penalty"));
            reasons.Add(new PenaltyReason("WARN", "Ignoring admin warning"));
            SaveReasons(reasons);
            return reasons;
        }

        private static string GetPath()
        {
            return Path.Combine(Paths.ConfigPath, "reasons.json");
        }
    }
}
