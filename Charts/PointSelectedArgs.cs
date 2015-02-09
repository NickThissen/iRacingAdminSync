using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Swordfish.NET.Charts {
  public class PointSelectedArgs : EventArgs {
    private Point _selectedPoint;
    public PointSelectedArgs(Point selectedPoint) {
      _selectedPoint = selectedPoint;
    }
    public Point SelectedPoint {
      get {
        return _selectedPoint;
      }
    }
  }
}
