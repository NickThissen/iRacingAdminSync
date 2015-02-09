using System.Windows;
using iRacingSdkWrapper;

namespace iRacingAdmin.Models.ViewModels
{
    public abstract class SdkViewModel : ViewModelBase
    {
        protected SdkViewModel()
        {
            var app = (App) Application.Current;
            app.RegisterSdkModel(this);
        }

        public abstract void OnSessionInfoUpdated(SdkWrapper.SessionInfoUpdatedEventArgs e);
        public abstract void OnTelemetryUpdated(SdkWrapper.TelemetryUpdatedEventArgs e);
        public abstract void OnSyncStateUpdated();

        public void Close()
        {
            var app = (App)Application.Current;
            app.UnregisterSdkModel(this);
        }
    }
}
