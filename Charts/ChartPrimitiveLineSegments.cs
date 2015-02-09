using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Swordfish.NET.Charts {
  public class ChartPrimitiveLineSegments  : ChartPrimitive, IChartRendererWPF {

    protected List<Color> _lineColors = new List<Color>();

    protected internal ChartPrimitiveLineSegments() {
    }

    protected ChartPrimitiveLineSegments(ChartPrimitiveLineSegments chartPrimitiveLineSegments)
      : base(chartPrimitiveLineSegments) {
    }

    public virtual ChartPrimitiveLineSegments Clone() {
      return new ChartPrimitiveLineSegments(this);
    }

    /// <summary>
    /// Adds a point to the end
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void AddSegment(double x1, double y1, double x2, double y2, Color color) {
      AddSegment(new Point(x1, y1), new Point(x2, y2), color);
    }

    /// <summary>
    /// Adds a point to the end
    /// </summary>
    /// <param name="point"></param>
    public void AddSegment(Point point1, Point point2, Color color) {
      Points.Add(point1);
      Points.Add(point2);
      _lineColors.Add(color);
    }

    /// <summary>
    /// Gets the UIElement that can be added to the plot
    /// </summary>
    /// <returns></returns>
    public void RenderFilledElements(DrawingContext ctx, Rect chartArea, Transform transform) {
    }

    public void RenderUnfilledElements(DrawingContext ctx, Rect chartArea, Transform transform) {

      for(int segmentIndex = 0; segmentIndex < _lineColors.Count; ++segmentIndex) {
        SolidColorBrush brush = new SolidColorBrush(_lineColors[segmentIndex]);
        Pen pen = new Pen(brush, LineThickness);
        pen.LineJoin = PenLineJoin.Bevel;
        if(IsDashed) {
          pen.DashStyle = new DashStyle(new double[] { 2, 2 }, 0);
        }
        ctx.DrawLine(pen, transform.Transform(Points[segmentIndex*2]), transform.Transform(Points[segmentIndex*2+1]));
      }
    }

  }
}
