using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Swordfish.NET.Charts {
  public class ChartPrimitiveEventLine : ChartPrimitive, IChartRendererWPF {

    protected List<string> _labels = new List<string>();

    /// <summary>
    /// Geometry to render
    /// </summary>
    private GeometryGroup _unfilledGeometry;
    private Rect _lastRect = new Rect(0, 0, 0, 0);

    protected internal ChartPrimitiveEventLine() {
    }

    public void AddPoint(double time, string label) {
      this.Points.Add(new Point(time, 0));
      _labels.Add(label);
    }

    public void RenderFilledElements(DrawingContext ctx, Rect chartArea, Transform transform) {
    }

    public void RenderUnfilledElements(DrawingContext ctx, Rect chartArea, Transform transform) {
      CalculateGeometry(chartArea);
      if(LineColor != Colors.Transparent && LineThickness > 0) {
        Pen pen = new Pen(new SolidColorBrush(LineColor), LineThickness);
        pen.LineJoin = PenLineJoin.Bevel;
        if(IsDashed) {
          pen.DashStyle = new DashStyle(new double[] { 2, 2 }, 0);
        }
        _unfilledGeometry.Transform = transform;
        ctx.DrawGeometry(null, pen, _unfilledGeometry);
      }
    }

    private void CalculateGeometry(Rect rect) {
      if(_recalcGeometry || _lastRect != rect) {

        Action<bool, GeometryGroup> buildGeometry = (isFilled, geometry) => {
          geometry.Children.Clear();
          foreach(Point point in Points) {
            StreamGeometry childGeometry = new StreamGeometry();
            using(StreamGeometryContext ctx = childGeometry.Open()) {
              ctx.BeginFigure(new Point(point.X, rect.Bottom), isFilled, isFilled);
              ctx.LineTo(new Point(point.X, rect.Top), !isFilled, true);
            }
            geometry.Children.Add(childGeometry);
          }
        };
        _unfilledGeometry = _unfilledGeometry ?? new GeometryGroup();
        buildGeometry(false, _unfilledGeometry);

        _lastRect = rect;
        _recalcGeometry = false;
      }
    }

    public Color LineColor { get; set; }
    public Orientation Orientation { get; set; }
  }
}
