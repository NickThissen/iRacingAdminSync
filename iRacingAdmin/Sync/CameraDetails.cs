namespace iRacingAdmin.Sync
{
    public class CameraDetails
    {
        public CameraDetails()
            : this(-1)
        {
            
        }

        public CameraDetails(int carNumber)
        {
            this.CarNumber = carNumber;
            this.Frame = -1;
            this.SessionTime = -1;
            this.ReplayChangeType = ReplayChangeTypes.Unchanged;
        }

        public ReplayChangeTypes ReplayChangeType { get; set; }

        public int CarNumber { get; set; }
        public int Frame { get; set; }
        public double SessionTime { get; set; }

        public static CameraDetails ChangeFocus(int carnumber)
        {
            var cam = new CameraDetails(carnumber);
            return cam;
        }

        public static CameraDetails ChangeReplayFrame(int frame, int carnumber = -1)
        {
            var cam = new CameraDetails(carnumber);
            cam.SetReplayFrame(frame);
            return cam;
        }

        public static CameraDetails ChangeReplayTime(double sessionTime, int carnumber = -1)
        {
            var cam = new CameraDetails(carnumber);
            cam.SetSessionTime(sessionTime);
            return cam;
        }
        
        public void SetReplayFrame(int frame)
        {
            this.Frame = frame;
            this.SessionTime = -1;
            this.ReplayChangeType = ReplayChangeTypes.SetReplayFrame;
        }

        public void SetSessionTime(double time)
        {
            this.Frame = -1;
            this.SessionTime = time;
            this.ReplayChangeType = ReplayChangeTypes.SetSessionTime;
        }

        public enum ReplayChangeTypes
        {
            Unchanged,
            SetReplayFrame,
            SetSessionTime
        }
    }
}
