using System.Linq;

namespace iRacingAdmin.Sync
{
    public static class SyncStateUpdate
    {
        public static void Update(SyncState state)
        {
            SyncUserCameras(state);
            SyncLiveStatus(state);
            SyncPenalties(state);
        }

        private static void SyncUserCameras(SyncState state)
        {
            foreach (var driver in Simulator.Instance.Drivers.ToList())
            {
                driver.UserCameras.Clear();
            }

            foreach (var kvp in state.WatchedDrivers.ToList())
            {
                var address = kvp.Key;
                var carId = kvp.Value;

                var user = SyncManager.Instance.FindUser(address);
                if (user != null)
                {
                    var driver = Simulator.Instance.Drivers.FromId(carId);
                    if (driver != null)
                    {
                        driver.UserCameras.Add(user);
                    }
                }
            }
        }

        private static void SyncLiveStatus(SyncState state)
        {
            foreach (var kvp in state.LiveStatus.ToList())
            {
                var address = kvp.Key;
                var live = kvp.Value;

                var user = SyncManager.Instance.FindUser(address);
                if (user != null)
                {
                    user.IsLive = live;
                }
            }
        }

        private static void SyncPenalties(SyncState state)
        {

        }
    }
}
