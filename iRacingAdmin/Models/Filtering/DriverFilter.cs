using iRacingAdmin.Models.Drivers;

namespace iRacingAdmin.Models.Filtering
{
    public class DriverFilter : FilterManager<DriverContainer>
    {
        public DriverFilter()
        {
            this.AddFilter(this.DrivingFilter);
            this.AddFilter(this.NameFilter);
            this.AddFilter(this.TeamFilter);
            this.AddFilter(this.NumberFilter);
            this.AddFilter(this.ClassFilter);
        }

        private IsDrivingFilter _drivingFilter;
        public IsDrivingFilter DrivingFilter
        {
            get { return _drivingFilter ?? (_drivingFilter = new IsDrivingFilter()); }
        }

        private DriverNameFilter _nameFilter;
        public DriverNameFilter NameFilter
        {
            get { return _nameFilter ?? (_nameFilter = new DriverNameFilter()); }
        }

        private TeamNameFilter _teamFilter;
        public TeamNameFilter TeamFilter
        {
            get { return _teamFilter ?? (_teamFilter = new TeamNameFilter()); }
        }

        private CarNumberFilter _carNumberFilter;
        public CarNumberFilter NumberFilter
        {
            get { return _carNumberFilter ?? (_carNumberFilter = new CarNumberFilter()); }
        }

        private CarClassFilter _classFilter;
        public CarClassFilter ClassFilter
        {
            get { return _classFilter ?? (_classFilter = new CarClassFilter()); }
        }

        public class IsDrivingFilter : Filter<DriverContainer>
        {
            public override bool Execute(DriverContainer item)
            {
                return !item.Driver.IsPacecar && !item.Driver.IsSpectator; //&& !item.Driver.Results.Current.IsEmpty;
            }
        }

        public class DriverNameFilter : Filter<DriverContainer>
        {
            public override bool Execute(DriverContainer item)
            {
                var name = (string)this.Property;
                if (string.IsNullOrWhiteSpace(name)) return true;
                return item.Driver.Name.ToLower().Contains(name.ToLower());
            }
        }

        public class TeamNameFilter : Filter<DriverContainer>
        {
            public override bool Execute(DriverContainer item)
            {
                var name = (string)this.Property;
                if (string.IsNullOrWhiteSpace(name)) return true;
                return item.Driver.TeamName.ToLower().Contains(name.ToLower());
            }
        }

        public class CarNumberFilter : Filter<DriverContainer>
        {
            public override bool Execute(DriverContainer item)
            {
                var number = (string)this.Property;
                if (string.IsNullOrWhiteSpace(number)) return true;
                return item.Driver.CarNumber.Contains(number);
            }
        }

        public class CarClassFilter : Filter<DriverContainer>
        {
            public override bool Execute(DriverContainer item)
            {
                var carClass = (ClassFilterItem) this.Property;
                if (carClass == null || carClass.CarClassRelSpeed == -1) return true;
                return item.Driver.CarClassRelSpeed == carClass.CarClassRelSpeed;
            }
        }
    }

}
