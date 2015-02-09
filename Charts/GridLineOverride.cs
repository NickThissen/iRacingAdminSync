using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Swordfish.NET.Charts {
  /// <summary>
  /// Grid line overrides define a region on the plot surface where custom grid lines
  /// and labels are drawn instead of the auto-generated lines and labels.
  /// </summary>
  public class GridLineOverride {

    private Collection<GridLine> _gridlines;
    private Collection<GridLabel> _gridLabels;

    public GridLineOverride(Orientation orientation) {
      _gridlines = new Collection<GridLine>();
      _gridLabels = new Collection<GridLabel>();
      Orientation = orientation;
    }

    public GridLabel AddLabel(string text, double location, Orientation orientation, Color? color = null) {
      Brush brush = null;
      if(color.HasValue) {
        brush = new SolidColorBrush(color.Value);
      }
      GridLabel label = new GridLabel(text, location, orientation, brush);
      _gridLabels.Add(label);
      return label;
    }

    public GridLine AddLine(double location, double? thickness = null, Color? color = null) {
      Pen pen = null;
      if(thickness.HasValue || color.HasValue) {
        pen = new Pen(new SolidColorBrush(color ?? Colors.Silver), thickness ?? 1);
      }
      GridLine line = new GridLine(location, pen);
      _gridlines.Add(line);
      return line;
    }

    public void CalculateRange(double padding=0) {
      double rangeMin = _gridlines.First().Location;
      double rangeMax = rangeMin;
      foreach(GridLine gridLine in _gridlines) {
        rangeMin = Math.Min(rangeMin, gridLine.Location);
        rangeMax = Math.Max(rangeMax, gridLine.Location);
      }
      Range = new Range<double>(rangeMin-padding, rangeMax+padding);
    }

    public Range<double> Range { get; set; }
    public Orientation Orientation { get; set; }
    public IEnumerable<GridLine> GridLines {
      get {
        return _gridlines;
      }
    }
    public IEnumerable<GridLabel> GridLabels {
      get {
        return _gridLabels;
      }
    }
  }
}
