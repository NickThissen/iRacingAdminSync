using System;
using PostSharp.Patterns.Model;

namespace iRacingAdmin.Sync
{
    [NotifyPropertyChanged]
    public class ReplayEvent
    {
        public ReplayEvent()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }

        public int AdminId { get; set; }
        public CameraDetails Camera { get; set; }
        public string Text { get; set; }

        public EventTypes Type { get; set; }

        public enum EventTypes
        {
            Incident = 0,
            Interesting
        }
    }
}
