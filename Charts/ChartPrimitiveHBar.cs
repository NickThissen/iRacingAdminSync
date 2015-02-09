using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Swordfish.NET.Charts {
  /// <summary>
  /// Creates a Horizontal Bar Chart
  /// </summary>
  public class ChartPrimitiveHBar : ChartPrimitive, IChartRendererWPF {

    protected double _centerPoint;
    protected double _height;
    protected List<Tuple<Color, Color>> _colors;

    /// <summary>
    /// Geometry to render
    /// </summary>
    private GeometryGroup _filledGeometry;
    private GeometryGroup _unfilledGeometry;

    protected internal ChartPrimitiveHBar(double centerPoint, double height) {
      this._centerPoint = centerPoint;
      this._height = height;
      this._colors = new List<Tuple<Color, Color>>();
    }

    public void AddSegment(Range<double> range, Color lineColor, Color fillColor) {
      AddSegment(range.Start, range.End, lineColor, fillColor);
    }

    public void AddSegment(double start, double end, Color lineColor, Color fillColor) {
      double top = _centerPoint - _height * 0.5;
      double bottom = _centerPoint + _height * 0.5;
      Points.Add(new Point(start, top));
      Points.Add(new Point(end, top));
      Points.Add(new Point(end, bottom));
      Points.Add(new Point(start, bottom));
      _colors.Add(new Tuple<Color, Color>(lineColor, fillColor));
    }


    /// <summary>
    /// Does a one off calculation of the geometry to be rendered
    /// </summary>
    private void CalculateGeometry() {

      if(_recalcGeometry) {
        Func<bool, int, StreamGeometry> buildGeometry = (bool isFilled, int pointIndex) => {
          StreamGeometry childGeometry = new StreamGeometry();
          using(StreamGeometryContext ctx = childGeometry.Open()) {
            // Break up into groups of 4
            ctx.BeginFigure(Points[pointIndex], isFilled, isFilled);
            for(int j = 0; j < 4; ++j) {
              ctx.LineTo(Points[pointIndex + j], !isFilled, true);
            }
            if(!isFilled) {
              ctx.LineTo(Points[pointIndex], !isFilled, true);
            }
          }
          return childGeometry;
        };

        _filledGeometry = _filledGeometry ?? new GeometryGroup();
        _unfilledGeometry = _unfilledGeometry ?? new GeometryGroup();

        _filledGeometry.Children.Clear();
        _unfilledGeometry.Children.Clear();
        if(Points.Count > 0) {
          for(int pointIndex = 0, colorIndex = 0; pointIndex < (Points.Count - 3); pointIndex += 4, colorIndex += 1) {
            _unfilledGeometry.Children.Add(buildGeometry(false, pointIndex));
            _filledGeometry.Children.Add(buildGeometry(true, pointIndex));
          }
        }
        _recalcGeometry = false;
      }
    }

    /// <summary>
    /// Gets the UIElement that can be added to the plot
    /// </summary>
    /// <returns></returns>
    public void RenderFilledElements(DrawingContext ctx, Rect chartArea, Transform transform) {
      CalculateGeometry();
      int colorIndex = 0;
      foreach(var childGeometry in _filledGeometry.Children) {
        Color fillColor = _colors[colorIndex].Item2;
        if(fillColor != Colors.Transparent) {
          Brush brush = IsDashed ? (Brush)(ChartUtilities.CreateHatch50(fillColor, new Size(2, 2))) : (Brush)(new SolidColorBrush(fillColor));
          childGeometry.Transform = transform;
          ctx.DrawGeometry(brush, null, childGeometry);
        }
        ++colorIndex;
      }
    }

    public void RenderUnfilledElements(DrawingContext ctx, Rect chartArea, Transform transform) {
      CalculateGeometry();
      int colorIndex = 0;
      foreach(var childGeometry in _unfilledGeometry.Children) {
        Color lineColor = _colors[colorIndex].Item1;
        if(lineColor != Colors.Transparent) {

          Pen pen = new Pen(new SolidColorBrush(lineColor), LineThickness);
          pen.LineJoin = PenLineJoin.Bevel;
          if(IsDashed) {
            pen.DashStyle = new DashStyle(new double[] { 2, 2 }, 0);
          }
          childGeometry.Transform = transform;
          ctx.DrawGeometry(null, pen, childGeometry);
        }
        ++colorIndex;
      }
    }
  }
}
