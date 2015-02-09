using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRacingAdmin.Models.Drivers
{
    public class DriverOfftrackLimit
    {
        public DriverOfftrackLimit(DriverContainer driver, int count)
        {
            this.Driver = driver;
            this.OfftrackCount = count;
            this.Time = DateTime.Now;
        }

        public DriverContainer Driver { get; set; }
        public int OfftrackCount { get; set; }
        public DateTime Time { get; set; }
    }
}
