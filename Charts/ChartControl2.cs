using System;
using System.Collections.Generic;
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

namespace Swordfish.NET.Charts {

  /// <summary>
  /// Do the chart control properly as a custom control when I get time.
  /// The display tree is in Themes\Generic.xaml
  /// </summary>
  public class ChartControl2 : Control {
    static ChartControl2() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(ChartControl2), new FrameworkPropertyMetadata(typeof(ChartControl2)));
    }
  }
}
