using iRacingAdmin.Properties;
using PostSharp.Patterns.Model;

namespace iRacingAdmin.Models.ViewModels
{
    [NotifyPropertyChanged]
    internal class SettingsViewModel : ViewModelBase
    {
        public Settings Settings { get; set; }

        public bool IsChanged { get; set; }

        public void Load()
        {
            this.Settings = Settings.Default;
            this.IsChanged = false;
        }

        public void Save()
        {
            if (this.Settings.LiveAdminInterval < 1) this.Settings.LiveAdminInterval = 1;
            if (this.Settings.DeltaTimeColorLimit < 0) this.Settings.DeltaTimeColorLimit = 0;

            this.Settings.Save();
            this.IsChanged = false;
        }
    }
}
