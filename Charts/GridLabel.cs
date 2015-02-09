using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace Swordfish.NET.Charts {
  public class GridLabel {
    public static Brush _defaultBrush = new SolidColorBrush(Colors.White);

    public GridLabel(string text, double location, Orientation orientation, Brush brush=null) {
      Text = text;
      Location = location;
      Orientation = orientation;
      Brush = brush ?? _defaultBrush;
      IsFloating = false;
    }

    public GridLabel(string text, Orientation orientation, Brush brush = null) : this (text, 0, orientation, brush) {
      IsFloating = true;
    }

    public static implicit operator GridLabel(string text) {
      return new GridLabel(text, Orientation.Vertical);
    }

    public string Text { get; set; }
    public double Location { get; set; }
    public Orientation Orientation { get; set; }
    // If the label is floating, then it is just rendered in the middle
    public bool IsFloating { get; set; }
    public Brush Brush { get; set; }
  }
}
