using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Sync;

namespace iRacingAdmin.Models.Admins
{
    public class UserCameraCollection : UserContainerCollection
    {
        public UserCameraCollection(DriverContainer driver)
        {
            _driver = driver;
        }

        private readonly DriverContainer _driver;
        public DriverContainer Driver { get { return _driver; } }

        public IEnumerable<User> Users
        {
            get
            {
                return this.Items.Select(i => i.User);
            }
        }

        protected override void InsertItem(int index, UserContainer item)
        {
            base.InsertItem(index, item);
            item.WatchedDriver = _driver.Driver;

            if (item.User.CustId == SyncManager.Instance.UserId)
            {
                _driver.Watching = true;
            }
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            item.WatchedDriver = null;

            if (item.User.CustId == SyncManager.Instance.UserId)
            {
                _driver.Watching = false;
            }

            base.RemoveItem(index);
        }
    }
}
