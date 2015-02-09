using System.Collections.Generic;
using System.Collections.ObjectModel;
using iRacingAdmin.Classes;
using iRacingAdmin.Models.Events;
using iRacingAdmin.Sync;
using iRacingSdkWrapper;

namespace iRacingAdmin.Models.ViewModels
{
    public class ReplayBarModel : SdkViewModel
    {
        private MainViewModel _mainModel;

        public ReplayBarModel(MainViewModel mainModel)
        {
            _mainModel = mainModel;

            _events = new ObservableCollection<ReplayEventContainer>();
            this.UpdateEvents();
        }

        #region Properties

        private double _sessionTime;
        public double SessionTime
        {
            get
            {
#if DEBUG
                return 1000;
#endif
                return _sessionTime;
            }
            set
            {
                _sessionTime = value;
                this.OnPropertyChanged();
            }
        }

        private double _replaySessionTime;
        public double ReplaySessionTime
        {
            get
            {
#if DEBUG
                return 800;
#endif
                return _replaySessionTime;
            }
            set
            {
                _replaySessionTime = value;
                this.OnPropertyChanged();
            }
        }

        private readonly ObservableCollection<ReplayEventContainer> _events;
        private double _barLength;
        public ObservableCollection<ReplayEventContainer> Events { get { return _events; } }

        public double BarLength
        {
            get { return _barLength; }
            set
            {
                _barLength = value;
                this.UpdateEventPositions();
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        public void MoveToLive()
        {
            CameraControl.ChangeReplayLive();
        }

        public void MoveToReplayTime(double replayTime)
        {
            CameraControl.ChangeReplayTime(replayTime);
        }

        public void MoveToReplayPercentage(double percentage)
        {
            var time = this.SessionTime*percentage;
            this.MoveToReplayTime(time);
        }

        public void AddCustomEvent()
        {
            var @event = new ReplayEvent();
            @event.AdminId = SyncManager.Instance.UserId;
            @event.Camera = CameraControl.GetCurrentCameraSessionTime();
            @event.Type = ReplayEvent.EventTypes.Interesting;

            this.AddEvent(@event);
        }

        public void AddEvent(ReplayEvent @event)
        {
            var container = new ReplayEventContainer(@event);
            container.RelativePosition = @event.Camera.SessionTime / this.SessionTime;
            this.Events.Add(container);
            this.UpdateEventPosition(container);

            var command = SyncCommandHelper.AddEvent(@event);
            SyncManager.Instance.SendStateUpdate(command);
        }

        public override void OnSessionInfoUpdated(SdkWrapper.SessionInfoUpdatedEventArgs e)
        {
        }

        public override void OnTelemetryUpdated(SdkWrapper.TelemetryUpdatedEventArgs e)
        {
#if !DEBUG
            this.SessionTime = e.TelemetryInfo.SessionTime.Value;
            this.ReplaySessionTime = e.TelemetryInfo.ReplaySessionTime.Value;
            this.UpdateEvents();
#endif
        }

        public override void OnSyncStateUpdated()
        {
        }

        private void UpdateEvents()
        {
            this.Events.Clear();

#if DEBUG
            var debugEvents = new List<ReplayEvent>();
            var ev = new ReplayEvent();
            ev.AdminId = SyncManager.Instance.UserId;
            ev.Camera = CameraDetails.ChangeReplayTime(400, 4);
            ev.Type = ReplayEvent.EventTypes.Interesting;
            ev.Text = "Test event 1";
            debugEvents.Add(ev);

            ev = new ReplayEvent();
            ev.AdminId = SyncManager.Instance.UserId;
            ev.Camera = CameraDetails.ChangeReplayTime(500, 7);
            ev.Type = ReplayEvent.EventTypes.Incident;
            ev.Text = "Test event 2";
            debugEvents.Add(ev);

            ev = new ReplayEvent();
            ev.AdminId = SyncManager.Instance.UserId;
            ev.Camera = CameraDetails.ChangeReplayTime(850, 7);
            ev.Type = ReplayEvent.EventTypes.Interesting;
            ev.Text = "Test event 3";
            debugEvents.Add(ev);

            foreach (var @event in debugEvents)
            {
#else
            foreach (var @event in SyncManager.Instance.State.Events)
            {
#endif
                var container = new ReplayEventContainer(@event);
                container.RelativePosition = @event.Camera.SessionTime/this.SessionTime;
                this.Events.Add(container);
            }
            this.UpdateEventPositions();
        }

        public void UpdateEventPositions()
        {
            foreach (var @event in this.Events)
            {
                this.UpdateEventPosition(@event);
            }
        }

        public void UpdateEventPosition(ReplayEventContainer @event)
        {
            @event.AbsolutePosition = this.BarLength*@event.RelativePosition - @event.MouseGridWidth;
        }

        #endregion

    }
}
