using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Swordfish.NET.Charts {
  public class PlotRenderer {

    private DrawingImage _imageSource;
    private DrawingGroup _drawingGroup;

    public PlotRenderer() {
      _drawingGroup = new DrawingGroup();
      _imageSource = new DrawingImage(_drawingGroup);
    }

    public virtual void InvalidateVisual(Rect canvasRect) {
      _drawingGroup.ClipGeometry = new RectangleGeometry(canvasRect);
      DoRender();
    }

    private void DoRender() {
      if(PrimitiveTransform == null || PrimitiveList == null) {
        return;
      }

      _drawingGroup.Children.Clear();
      DrawingContext dc = _drawingGroup.Append();
      // Need to draw something on the whole rectangle, otherwise it doesn't align properly
      dc.DrawGeometry(new SolidColorBrush(Color.FromArgb(1,255,255,255)),null,_drawingGroup.ClipGeometry);

      foreach(IChartRendererWPF primitive in PrimitiveList) {
        primitive.RenderFilledElements(dc, ChartDataRange, PrimitiveTransform);
      }
      foreach(IChartRendererWPF primitive in PrimitiveList) {
        primitive.RenderUnfilledElements(dc, ChartDataRange, PrimitiveTransform);
      }

      // Now render the points

      Rect clipBounds = _drawingGroup.ClipGeometry.Bounds;
      Rect bounds = new Rect(-3, -3, clipBounds.Width + 6, clipBounds.Height + 6);

      foreach(ChartPrimitive primitive in PrimitiveList) {
        if(primitive.ShowPoints) {
          Brush pointBrush = new SolidColorBrush(primitive.PointColor);
          Pen pointPen = new Pen(pointBrush, 1);

          foreach(Point point in primitive.Points) {
            Point transformedPoint = PrimitiveTransform.Transform(point);
            if(bounds.Contains(transformedPoint)) {
              dc.DrawEllipse(null, pointPen, transformedPoint, 2, 2);
            }
          }
        }
      }

      dc.Close();
    }

    public virtual System.Windows.Media.ImageSource ImageSource {
      get {
        return _imageSource;
      }
    }

    /// <summary>
    /// A list of lines to draw on the chart
    /// </summary>
    public List<ChartPrimitive> PrimitiveList {
      get;
      set;
    }

    /// <summary>
    /// Gets sets the transform for the Geometry
    /// </summary>
    public MatrixTransform PrimitiveTransform {
      get;
      set;
    }

    public Rect ChartDataRange {
      get;
      set;
    }
  }
}
