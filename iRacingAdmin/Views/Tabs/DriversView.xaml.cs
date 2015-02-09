using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Models.ViewModels;

namespace iRacingAdmin.Views.Tabs
{
    /// <summary>
    /// Interaction logic for DriversView.xaml
    /// </summary>
    public partial class DriversView : UserControl
    {
        public DriversView()
        {
            InitializeComponent();
        }

        private void Drivers_OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var model = (DriverListModel)this.DataContext;
            model.SwitchToDriver(model.SelectedDriver);
        }

        private void Grid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var model = (DriverListModel)this.DataContext;
            var drivers = new List<DriverContainer>();
            foreach (object row in grid.SelectedItems)
            {
                var driver = row as DriverContainer;
                if (driver != null) drivers.Add(driver);
            }
            model.OnSelectedDriversChanged(drivers);
        }
    }
}
