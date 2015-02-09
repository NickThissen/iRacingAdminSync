using System.Collections.ObjectModel;
using System.Linq;

namespace iRacingAdmin.Models.Admins
{
   public  class UserContainerCollection : ObservableCollection<UserContainer>
    {
       public UserContainer FromId(int custid)
       {
           return this.SingleOrDefault(d => d.User.CustId == custid);
       }
    }
}
