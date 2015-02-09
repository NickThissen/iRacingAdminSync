using System.Windows;
using System.Windows.Input;
using iRacingAdmin.Classes;
using iRacingAdmin.Models.Admins;
using iRacingAdmin.Sync;
using PostSharp.Patterns.Model;

namespace iRacingAdmin.Models.Events
{
    [NotifyPropertyChanged]
    public class ReplayEventContainer
    {
        private const int MOUSE_GRID_WIDTH = 20;

        public ReplayEventContainer(ReplayEvent @event)
        {
            _event = @event;
            this.Admin = SyncManager.Instance.Users.FromId(@event.AdminId);
        }

        private readonly ReplayEvent _event;
        public ReplayEvent Event { get { return _event; } }

        public UserContainer Admin { get; private set; }

        public double RelativePosition { get; set; }
        public double AbsolutePosition { get; set; }

        public int MouseGridWidth { get { return MOUSE_GRID_WIDTH; } }

        #region Commands

        private ICommand _moveToEventCommand;
        [IgnoreAutoChangeNotification]
        public ICommand MoveToEventCommand
        {
            get { return _moveToEventCommand ?? (_moveToEventCommand = new RelayCommand(MoveToEvent)); }
        }

        private void MoveToEvent(object param)
        {
            MessageBox.Show(this.Event.Text);
        }

        #endregion
    }
}
