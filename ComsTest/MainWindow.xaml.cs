using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComsTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _model;
        private string _user = "User 1";

        public MainWindow()
        {
            InitializeComponent();

            _model = new MainViewModel();
            this.DataContext = _model;
        }
    }

    public class MainViewModel
    {
        public MainViewModel()
        {
            _drivers = new ObservableCollection<Driver>();
            _drivers.Add(new Driver { Id = 1, Name = "Nick" });
            _drivers.Add(new Driver { Id = 2, Name = "Joep" });
            _drivers.Add(new Driver { Id = 3, Name = "Egil" });
            _drivers.Add(new Driver { Id = 4, Name = "Niel" });
            _drivers.Add(new Driver { Id = 5, Name = "Robbert" });
        }

        private readonly ObservableCollection<Driver> _drivers;
        public ObservableCollection<Driver> Drivers { get { return _drivers; } }


    }
}
