// ****************************************************************************
// Copyright Swordfish Computing Australia 2006                              **
// http://www.swordfish.com.au/                                              **
//                                                                           **
// Filename: Swordfish\WinFX\Charts\TestPage.xaml.cs                         **
// Authored by: John Stewien of Swordfish Computing                          **
// Date: April 2006                                                          **
//                                                                           **
// - Change Log -                                                            **
//*****************************************************************************

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Swordfish.NET.Charts {
  /// <summary>
  /// Interaction logic for Page1.xaml
  /// </summary>
  public partial class TestPage : Window {
    public TestPage() {
      InitializeComponent();
      ChartUtilities.AddTestLines(xyLineChart);
      xyLineChart.SubNotes = new string[] { "Right or Middle Mouse Button To Zoom, Left Mouse Button To Pan, Right Double-Click To Reset" };
      copyToClipboard.DoCopyToClipboard = ChartUtilities.CopyChartToClipboard;
    }

    public ChartControl XYLineChart {
      get {
        return xyLineChart;
      }
    }

  }
}