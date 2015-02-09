using System.Collections.ObjectModel;
using System.Linq;
using System.Net;

namespace iRacingAdmin.Sync
{
    public class UserConnectionList : ObservableCollection<UserConnection>
    {
        public UserConnectionList() : this(true) { }

        public UserConnectionList(bool singlet)
        {
            _singlet = singlet;
        }

        private bool _singlet;

        protected override void InsertItem(int index, UserConnection item)
        {
            if (_singlet && this.Contains(item)) return;
            base.InsertItem(index, item);
        }

        public UserConnection FromAddress(string address)
        {
            return this.FirstOrDefault(u => u.ClientAddress == address);
        }

        public UserConnection FromAddress(IPAddress address)
        {
            return this.FromAddress(address.ToString());
        }

        public new bool Contains(UserConnection user)
        {
            var match = this.FromAddress(user.ClientAddress);
            return match != null;
        }

        public new void Remove(UserConnection user)
        {
            var match = this.FromAddress(user.ClientAddress);
            if (match != null) base.Remove(match);
        }
    }

    public class UserList : ObservableCollection<User>
    {
        public UserList() : this(true) { }

        public UserList(bool singlet)
        {
            _singlet = singlet;
        }

        private bool _singlet;

        protected override void InsertItem(int index, User item)
        {
            if (_singlet && this.Contains(item)) return;
            base.InsertItem(index, item);
        }

        public User FromId(int custid)
        {
            return this.FirstOrDefault(u => u.CustId == custid);
        }

        public new bool Contains(User user)
        {
            var match = this.FromId(user.CustId);
            return match != null;
        }

        public new void Remove(User user)
        {
            var match = this.FromId(user.CustId);
            if (match != null) base.Remove(match);
        }
    }
}
