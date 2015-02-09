using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swordfish.NET.Charts;

namespace iRacingAdmin.Models.Drivers
{
    public class DriverOfftrackLine
    {
        public DriverContainer Driver { get; set; }
        public ChartPrimitiveXY Line { get; set; }
        public bool Taken { get; set; }
    }
}
