using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using iRacingAdmin.Models.Admins;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Sync;

namespace iRacingAdmin.Views
{
    /// <summary>
    /// Interaction logic for ConfirmCameraSyncWindow.xaml
    /// </summary>
    public partial class ConfirmCameraSyncWindow
    {
        public ConfirmCameraSyncWindow(UserContainer admin, CameraDetails camera)
        {
            InitializeComponent();

            var driver = Simulator.Instance.Drivers.FirstOrDefault(d => d.Driver.CarNumberRaw == camera.CarNumber);

            var sync = new CameraSync(admin, driver, camera);
            this.DataContext = sync;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnDecline_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        internal class CameraSync
        {
            public CameraSync(UserContainer admin, DriverContainer driver, CameraDetails camera)
            {
                this.Admin = admin;
                this.Driver = driver;
                this.Camera = camera;

                if (camera.ReplayChangeType == CameraDetails.ReplayChangeTypes.SetReplayFrame)
                {
                    var seconds = Simulator.Instance.Telemetry.SessionTime.Value - (camera.Frame/60f);
                    var time = TimeSpan.FromSeconds(seconds);
                    this.SessionTime = string.Format("{0:0}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);
                }
                else if (camera.ReplayChangeType == CameraDetails.ReplayChangeTypes.SetSessionTime)
                {
                    var time = TimeSpan.FromSeconds(camera.SessionTime);
                    this.SessionTime = string.Format("{0:0}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);
                }
            }

            public UserContainer Admin { get; set; }
            public CameraDetails Camera { get; set; }
            public DriverContainer Driver { get; set; }
            public string SessionTime { get; set; }
        }
    }
}
