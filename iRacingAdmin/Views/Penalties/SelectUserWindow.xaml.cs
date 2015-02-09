using System.Collections.Generic;
using System.Linq;
using System.Windows;
using iRacingAdmin.Models.Drivers;

namespace iRacingAdmin.Views.Penalties
{
    /// <summary>
    /// Interaction logic for SelectUserWindow.xaml
    /// </summary>
    public partial class SelectUserWindow 
    {
        public SelectUserWindow(IEnumerable<DriverContainer> drivers)
        {
            InitializeComponent();

            lst.ItemsSource = drivers.ToList();
        }

        private DriverContainer _selectedDriver;
        public DriverContainer SelectedDriver { get { return _selectedDriver; } }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            var driver = lst.SelectedItem as DriverContainer;
            if (driver != null)
            {
                _selectedDriver = driver;
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _selectedDriver = null;
            this.DialogResult = false;
            this.Close();
        }
    }
}
