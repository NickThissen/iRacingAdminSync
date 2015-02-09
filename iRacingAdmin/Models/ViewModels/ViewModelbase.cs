using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace iRacingAdmin.Models.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public ViewModelBase Model { get { return this; } }

        protected virtual void OnPropertyChanged([CallerMemberName] string member = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(member));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
