using System.ComponentModel;
using iRacingAdmin.Models.Drivers;

namespace iRacingAdmin.Classes.Sorting
{
    public class ClassPositionSorter : ICustomSorter
    {
        public ListSortDirection SortDirection { get; set; }

        public int Compare(object x, object y)
        {
            var driver1 = x as DriverContainer;
            var driver2 = y as DriverContainer;

            if (driver1 == null || driver2 == null) return 0;
            
            // Sort by class first, then by position
            if (driver1.Driver.CarClassRelSpeed == driver2.Driver.CarClassRelSpeed)
            {
                // Same class, sort by position
                if (this.SortDirection == ListSortDirection.Descending)
                {
                    // Swap
                    var tmp = driver1;
                    driver1 = driver2;
                    driver2 = tmp;
                }

                return driver1.Driver.Results.Current.ClassPosition.CompareTo(
                    driver2.Driver.Results.Current.ClassPosition);
            }
            else
            {
                // Different class, sort by class
                return driver2.Driver.CarClassRelSpeed.CompareTo(driver1.Driver.CarClassRelSpeed);
            }
        }
    }
}
