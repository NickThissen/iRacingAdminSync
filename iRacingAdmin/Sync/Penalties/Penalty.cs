using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using iRacingAdmin.Classes;
using PostSharp.Patterns.Model;

namespace iRacingAdmin.Sync.Penalties
{
    [NotifyPropertyChanged]
    public class Penalty : INotifyPropertyChanged
    {
        public Penalty()
        {
            _driverIds = new ObservableCollection<int>();
            _users = new UserList(true);
        }

        public static Penalty Create()
        {
            var penalty = new Penalty();

            var guid = Guid.NewGuid();
            penalty.Id = new ShortGuid(guid).ToString();
            return penalty;
        }

        public string Id { get; set; }

        private readonly ObservableCollection<int> _driverIds;
        public ObservableCollection<int> DriverIds { get { return _driverIds; } }

        private readonly UserList _users;
        private PenaltyResult _result;
        public UserList Users { get { return _users; } }

        public CameraDetails Camera { get; set; }

        public string Lap { get; set; }
        public string Turn { get; set; }
        public string Reason { get; set; }

        public PenaltyResult Result
        {
            get { return _result; }
            set
            {
                //if (_result != null) _result.PropertyChanged -= OnResultChanged;

                _result = value;

                //if (_result != null) _result.PropertyChanged += OnResultChanged;
            }
        }

        public User LockUser { get; set; }
        public bool IsLocked { get { return this.LockUser != null; } }
        
        public DateTime? StartInvestigationTime { get; set; }
        public DateTime? DecidedTime { get; set; }
        public int DecidedLap { get; set; }

        public bool IsUnderInvestigation { get; set; }

        private void OnResultChanged(object sender, EventArgs e)
        {
            this.OnPropertyChanged("");
        }

        public void StartInvestigation(int driverId, User user)
        {
            if (!this.IsUnderInvestigation)
            {
                this.IsUnderInvestigation = true;
                this.StartInvestigationTime = DateTime.Now.ToUniversalTime();
                this.Result = PenaltyResult.Investigation();
            }
            if (!this.DriverIds.Contains(driverId)) this.DriverIds.Add(driverId);
           
            this.JoinUser(user);
        }

        public void JoinUser(User user)
        {
            this.Users.Add(user);
        }

        public void DecideResult(PenaltyResult result, int driverId, User user)
        {
            this.DriverIds.Clear();
            this.DriverIds.Add(driverId);

            this.Result = result;
            if (this.Result.Type != PenaltyResult.PenaltyResultTypes.Investigation)
            {
                this.DecidedTime = DateTime.Now.ToUniversalTime();
                this.IsUnderInvestigation = false;

                this.Users.Clear();
                this.Users.Add(user);
            }
            else
            {
                this.DecidedTime = null;
                this.IsUnderInvestigation = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
