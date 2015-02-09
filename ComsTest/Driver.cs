using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ComsTest.Annotations;

namespace ComsTest
{
    public class Driver : INotifyPropertyChanged
    {
        private bool _isPenalizing;

        public int Id { get; set; }
        public string Name { get; set; }

        public bool IsPenalizing
        {
            get { return _isPenalizing; }
            set
            {
                if (value.Equals(_isPenalizing)) return;
                _isPenalizing = value;
                OnPropertyChanged("IsPenalizing");
                OnPropertyChanged("PenalizeText");
            }
        }

        public string PenalizeText
        {
            get
            {
                return this.IsPenalizing ? "In progress" : "Penalize";
            }
        }

        private ICommand _penalizeCommand;

        public ICommand PenalizeCommand
        {
            get { return _penalizeCommand ?? (_penalizeCommand = new RelayCommand(Penalize)); }
        }

        public void Penalize(object obj)
        {
            this.IsPenalizing = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
