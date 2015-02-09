// ****************************************************************************
// Copyright Swordfish Computing Australia 2006                              **
// http://www.swordfish.com.au/                                              **
//                                                                           **
// Filename: Swordfish\WinFX\Charts\CopyToClipboard.xaml.cs                  **
// Authored by: John Stewien of Swordfish Computing                          **
// Date: April 2006                                                          **
//                                                                           **
// - Change Log -                                                            **
//*****************************************************************************

using System;
using System.Collections.Generic;
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
using System.IO;

namespace Swordfish.NET.Charts {
  /// <summary>
  /// Interaction logic for UIElementToClipboard.xaml
  /// </summary>

  public partial class CopyToClipboard : UserControl {

    /// <summary>
    /// This has a default handler where you set the target element through
    /// the CopyTarget property
    /// 
    /// DoCopyToClipboard(FrameworkElement target, double width, double height)
    /// </summary>
    public Action<FrameworkElement, ChartControl, double, double> DoCopyToClipboard;
    /// <summary>
    /// DoSaveToFile(FrameworkElement target, double width, double height)
    /// </summary>
    public Action<FrameworkElement, double, double> DoSaveToFile;

    public CopyToClipboard() {
      InitializeComponent();
      _copyOptions.Visibility = Visibility.Collapsed;
      this.MouseEnter += new MouseEventHandler(UIElementToClipboard_MouseEnter);
      this.MouseLeave += new MouseEventHandler(UIElementToClipboard_MouseLeave);
      DoCopyToClipboard = ChartUtilities.CopyChartToClipboard;
    }

    // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CopyTargetProperty =
      DependencyProperty.Register("CopyTarget", typeof(FrameworkElement), typeof(CopyToClipboard), new UIPropertyMetadata(null));

    public FrameworkElement CopyTarget {
      get {
        return (FrameworkElement)GetValue(CopyTargetProperty);
      }
      set {
        SetValue(CopyTargetProperty, value);
      }
    }

    // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ChartControlProperty =
      DependencyProperty.Register("ChartControl", typeof(ChartControl), typeof(CopyToClipboard), new UIPropertyMetadata(null));

    public ChartControl ChartControl {
      get {
        return (ChartControl)GetValue(ChartControlProperty);
      }
      set {
        SetValue(ChartControlProperty, value);
      }
    }

    void OnCopyToClipboard(double width, double height) {
      if(_saveToFile.IsChecked == true) {
        if(DoSaveToFile != null) {
          DoSaveToFile(CopyTarget, width, height);
        }
      } else {
        if(DoCopyToClipboard != null) {
          DoCopyToClipboard(CopyTarget, ChartControl, width, height);
        }
      }
    }

    void UIElementToClipboard_MouseLeave(object sender, MouseEventArgs e) {
      if(!(tbHeight.IsFocused || tbWidth.IsFocused)) {
        _copyOptions.Visibility = Visibility.Collapsed;
        this.Margin = new Thickness(0, 0, 0, 0);
      }
    }

    void UIElementToClipboard_MouseEnter(object sender, MouseEventArgs e) {
      _copyOptions.Visibility = Visibility.Visible;
      this.Margin = new Thickness(0, 0, 0, 8);
    }

    void bCopy640x480_Click(object sender, RoutedEventArgs e) {
      OnCopyToClipboard(640, 480);
    }
    void bCopy800x600_Click(object sender, RoutedEventArgs e) {
      OnCopyToClipboard(800, 600);
    }
    void bCopy1024x768_Click(object sender, RoutedEventArgs e) {
      OnCopyToClipboard(1024, 768);
    }
    void bCopy1280x1024_Click(object sender, RoutedEventArgs e) {
      OnCopyToClipboard(1280, 1024);
    }
    void bCopyCustom_Click(object sender, RoutedEventArgs e) {
      double width = 640;
      double height = 480;
      Double.TryParse(tbWidth.Text, out width);
      Double.TryParse(tbHeight.Text, out height);
      OnCopyToClipboard(width, height);
    }
  }
}