using System.Collections.Generic;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Sync;
using iRacingSimulator;
using iRSDKSharp;

namespace iRacingAdmin.Classes
{
    public static class CameraControl
    {
        /// <summary>
        /// Get the driver you are currently following
        /// </summary>
        public static DriverContainer GetFollowedDriver()
        {
            if (Simulator.Instance.Drivers.Count == 0) return null;
            if (Simulator.Instance.Telemetry == null) return null;
            return Simulator.Instance.Drivers.FromId(Simulator.Instance.Telemetry.CamCarIdx.Value);
        }
        
        /// <summary>
        /// Get camera of followed driver at the current replay frame.
        /// </summary>
        public static CameraDetails GetCurrentCameraReplay()
        {
            if (Connection.Instance.IsSimulated)
            {
                return CameraDetails.ChangeReplayFrame(0, 0);
            }

            if (!Simulator.Instance.Sdk.IsConnected) return null;

            var driver = GetFollowedDriver();
            if (driver == null) return null;

            var carNumber = driver.Driver.CarNumberRaw;
            var replayFrame = Simulator.Instance.Telemetry.ReplayFrameNumEnd.Value;

            return CameraDetails.ChangeReplayFrame(replayFrame, carNumber);
        }

        /// <summary>
        /// Get camera of followed driver at the current replay session time.
        /// </summary>
        public static CameraDetails GetCurrentCameraSessionTime()
        {
            if (Connection.Instance.IsSimulated)
            {
                return CameraDetails.ChangeReplayTime(0, 0);
            }

            if (!Simulator.Instance.Sdk.IsConnected) return null;

            var driver = GetFollowedDriver();
            if (driver == null) return null;

            var carNumber = driver.Driver.CarNumberRaw;
            var time = Simulator.Instance.Telemetry.ReplaySessionTime.Value;

            return CameraDetails.ChangeReplayTime(time, carNumber);
        }

        public static void ProposeSyncCamera(User user)
        {
            // Sync this user's camera with me
            if (!Simulator.Instance.Sdk.IsConnected) return;

            //var camera = GetCurrentCameraReplay();
            var camera = GetCurrentCameraSessionTime();

            var users = new List<User>();
            users.Add(user);

            var command = SyncCommandHelper.ProposeSyncCameras(users, camera);
            SyncManager.Instance.SendCommand(command);
        }

        public static void SyncCamera(User user)
        {
            // Sync to other camera
            // Obtain camera details

        }

        public static void ChangeCamera(CameraDetails camera)
        {
            if (!Simulator.Instance.Sdk.IsConnected) return;
            if (Connection.Instance.IsSimulated) return;

            var driver = GetFollowedDriver();
            if (driver == null) return;

            if (camera.CarNumber < 0) camera.CarNumber = driver.Driver.CarNumberRaw;

            // Change camera
            Simulator.Instance.Sdk.Sdk.BroadcastMessage(BroadcastMessageTypes.CamSwitchNum, camera.CarNumber, -1);

            if (camera.ReplayChangeType == CameraDetails.ReplayChangeTypes.SetReplayFrame)
            {
                ChangeReplayFrame(camera.Frame);
            }
            else if (camera.ReplayChangeType == CameraDetails.ReplayChangeTypes.SetSessionTime)
            {
                ChangeReplayTime(camera.SessionTime);
            }
        }

        public static void ChangeReplayFrame(int frameToEnd)
        {
            if (Connection.Instance.IsSimulated) return;

            if (frameToEnd > 0)
            {
                // Change replay position
                Simulator.Instance.Sdk.Sdk.BroadcastMessage(BroadcastMessageTypes.ReplaySetPlayPosition,
                    (int)ReplayPositionModeTypes.End,
                    frameToEnd);
            }
            else
            {
                // Go to live
                Simulator.Instance.Sdk.Sdk.BroadcastMessage(BroadcastMessageTypes.ReplaySearch,
                    (int)ReplaySearchModeTypes.ToEnd, 0);
            }

            // Replay speed
            Simulator.Instance.Sdk.Sdk.BroadcastMessage(BroadcastMessageTypes.ReplaySetPlaySpeed, 1, 0);
        }

        public static void ChangeReplayTime(double replayTime)
        {
            if (Connection.Instance.IsSimulated) return;

            var currentTime = Simulator.Instance.Telemetry.SessionTime.Value;
            var diff = currentTime - replayTime;
            var frames = (int)(diff * 60);
            ChangeReplayFrame(frames);
        }

        public static void ChangeReplayLive()
        {
            ChangeReplayFrame(0);
        }
    }
}
