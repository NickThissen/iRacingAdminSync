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
using System.Windows.Navigation;
using System.Windows.Shapes;
using iRacingAdmin.Models.ViewModels;
using Swordfish.NET.Charts;

namespace iRacingAdmin.Views.Tabs
{
    /// <summary>
    /// Interaction logic for OfftracksView.xaml
    /// </summary>
    public partial class OfftracksView : UserControl
    {
        public OfftracksView()
        {
            InitializeComponent();

            this.DataContextChanged += (sender, args) =>
            {
                var model = (OfftracksViewModel) this.DataContext;
                model.SetChart(chart);
            };
        }
    }
}
