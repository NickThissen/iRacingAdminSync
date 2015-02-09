using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Swordfish.NET.Charts {
  public static class GridLineSpacings {

    public static Func<double, double> Monthly {
      get {
        return (requestedSpacing) => {
          if(requestedSpacing < 2) {
            return ChartUtilities.Closest_1_2_5_Pow10(requestedSpacing);
          } else {
            return ChartUtilities.ClosestValueInListTimesBaseToInteger(requestedSpacing, new double[] { 1, 3, 6 }, 12.0);
          }

        };
      }
    }

    public static Func<double, double> Base10 {
      get {
        return (requestedSpacing) => {
          return ChartUtilities.ClosestValueInListTimesBaseToInteger(requestedSpacing, new double[] { 1, 2, 5 }, 10.0);
        };
      }
    }

    public static Func<double, double> TimeSeconds {
      get {
        return (requestedSpacing) => {
          if(requestedSpacing < 2) {
            return ChartUtilities.Closest_1_2_5_Pow10(requestedSpacing);
          } else {
            return ChartUtilities.ClosestValueInListTimesBaseToInteger(requestedSpacing, new double[] { 1, 3, 6}, 10.0);
          }

        };
      }
    }
  }
}
