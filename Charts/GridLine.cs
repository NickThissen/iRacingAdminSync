using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Swordfish.NET.Charts {
  public class GridLine {
    public static Pen _defaultPen = new Pen(new SolidColorBrush(Colors.Silver), 1);

    public GridLine(double location, Pen pen = null) {
      Location = location;
      Pen = pen ?? _defaultPen;
      Extended = false;
    }

    public Pen Pen { get; set; }
    public double Location { get; set; }
    /// <summary>
    /// Flag indicating that the grid line is rendered into the label area
    /// </summary>
    public bool Extended { get; set; }
  }
}
