using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Swordfish.NET.Charts {
  public class ChartPrimitiveXY : ChartPrimitive, IChartRendererWPF {

    /// <summary>
    /// Geometry to render
    /// </summary>
    private GeometryGroup _filledGeometry;
    private GeometryGroup _unfilledGeometry;

    protected internal ChartPrimitiveXY() {
      // Colors to draw the primitive with
      LineColor = Colors.Transparent;
      FillColor = Colors.Transparent;
    }

    protected ChartPrimitiveXY(ChartPrimitiveXY chartPrimitiveXY)
      : base(chartPrimitiveXY) {
      // Colors to draw the primitive with
      LineColor = chartPrimitiveXY.LineColor;
      FillColor = chartPrimitiveXY.FillColor;
    }

    public virtual ChartPrimitiveXY Clone() {
      return new ChartPrimitiveXY(this);
    }

    /// <summary>
    /// Adds a point to the end
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void AddPoint(double x, double y) {
      AddPoint(new Point(x, y));
    }

    /// <summary>
    /// Adds a point to the end
    /// </summary>
    /// <param name="point"></param>
    public void AddPoint(Point point) {
      Points.Add(point);
    }

    /// <summary>
    /// Inserts a point
    /// </summary>
    /// <param name="point"></param>
    /// <param name="index"></param>
    public void InsertPoint(Point point, int index) {
      Points.Insert(index, point);
    }

    /// <summary>
    /// Adds a bezier curve point where it gives a little plateau at the point
    /// </summary>
    /// <param name="point"></param>
    public void AddSmoothHorizontalBar(Point point) {
      if(Points.Count > 0) {
        Point lastPoint = Points[Points.Count - 1];
        double xDiff = (point.X - lastPoint.X) * .5;
        double yDiff = (point.Y - lastPoint.Y) * .3;

        Point controlPoint1 = new Point(lastPoint.X + xDiff, lastPoint.Y);
        Point controlPoint2 = new Point(point.X - xDiff, point.Y);
        Points.Add(controlPoint1);
        Points.Add(controlPoint2);
        Points.Add(point);
      } else {
        AddPoint(point);
      }
    }

    /// <summary>
    /// Does a one off calculation of the geometry to be rendered
    /// </summary>
    private void CalculateGeometry() {
      if(_recalcGeometry) {
        Action<bool, GeometryGroup> buildGeometry = (isFilled, geometry) => {
          geometry.Children.Clear();
          if(Points.Count > 0) {

            StreamGeometry childGeometry = new StreamGeometry();

            using(StreamGeometryContext ctx = childGeometry.Open()) {
              ctx.BeginFigure(Points.First(), isFilled, isFilled);
              foreach(Point point in Points.Skip(1)) {
                ctx.LineTo(point, !isFilled, true);
              }
            }
            geometry.Children.Add(childGeometry);
          }
        };

        _filledGeometry = _filledGeometry ?? new GeometryGroup();
        _unfilledGeometry = _unfilledGeometry ?? new GeometryGroup();

        if(FillColor != Colors.Transparent) {
          buildGeometry(true, _filledGeometry);
        }
        if(LineColor != Colors.Transparent && LineThickness > 0) {
          buildGeometry(false, _unfilledGeometry);
        }
        _recalcGeometry = false;
      }
    }

    /// <summary>
    /// Gets the UIElement that can be added to the plot
    /// </summary>
    /// <returns></returns>
    public void RenderFilledElements(DrawingContext ctx, Rect chartArea, Transform transform) {
      if(this.FillColor == Colors.Transparent) {
        return;
      }
      CalculateGeometry();
      Brush brush = IsDashed ? (Brush)(ChartUtilities.CreateHatch50(this.FillColor, new Size(2, 2))) : (Brush)(new SolidColorBrush(this.FillColor));
      _filledGeometry.Transform = transform;
      ctx.DrawGeometry(brush, null, _filledGeometry);
    }

    public void RenderUnfilledElements(DrawingContext ctx, Rect chartArea, Transform transform) {
      if(this.LineColor == Colors.Transparent) {
        return;
      }
      CalculateGeometry();
      SolidColorBrush brush = new SolidColorBrush(LineColor);
      Pen pen = new Pen(brush, LineThickness);
      pen.LineJoin = PenLineJoin.Bevel;
      if(IsDashed) {
        pen.DashStyle = new DashStyle(new double[] { 2, 2 }, 0);
      }
      _unfilledGeometry.Transform = transform;
      ctx.DrawGeometry(null, pen, _unfilledGeometry);
    }

    /// <summary>
    /// Gets/Sets the color of the primitive
    /// </summary>
    public Color FillColor { get; set; }
    public Color LineColor { get; set; }

  }
}
