// ****************************************************************************
// Copyright Swordfish Computing Australia 2006                              **
// http://www.swordfish.com.au/                                              **
//                                                                           **
// Filename: Swordfish\WinFX\Charts\AdornerCursorCoordinateDrawer.cs         **
// Authored by: John Stewien of Swordfish Computing                          **
// Date: April 2006                                                          **
//                                                                           **
// - Change Log -                                                            **
//*****************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Globalization;
using System.Windows.Controls;

namespace Swordfish.NET.Charts {
  public class CursorCoordinateCanvas : Canvas {
    // ********************************************************************
    // Private Fields
    // ********************************************************************
    #region Private Fields
    /// <summary>
    /// Flag indicating if the coordinates are locked to the closest point or or not.
    /// </summary>
    private bool _locked;
    /// <summary>
    /// The transformed coordinate of the lock point
    /// </summary>
    private Point _lockPoint;
    /// <summary>
    /// The cursor position. Used for calculating the relative position of the text.
    /// </summary>
    private Point _mousePoint;

    private bool _drawWholePlotCursor = false;

    private ClosestPointPicker _closestPointPicker;

    private Dictionary<Point, string> _pointToText;

    private Pen textBoxPen = new Pen(new SolidColorBrush(Color.FromArgb(144, 255, 255, 255)), 1);

    #endregion Private Fields

    // ********************************************************************
    // Public Methods
    // ********************************************************************
    #region Public Methods

    public CursorCoordinateCanvas() {
      _pointToText = new Dictionary<Point, string>();
      _closestPointPicker = new ClosestPointPicker(new Size(13, 13), Dispatcher);
      _closestPointPicker.ClosestPointChanged += new EventHandler<ClosestPointArgs>(closestPointPicker_ClosestPointChanged);
    }

    /// <summary>
    /// Handles when the closest point to the mouse cursor changes. Hides
    /// or shows the closest point, and changes the mouse cursor accordingly.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void closestPointPicker_ClosestPointChanged(object sender, ClosestPointArgs e) {
      Locked = _closestPointPicker.Locked;
      LockPoint = _closestPointPicker.ClosestPoint;
    }

    public void Reinitialize() {
      _closestPointPicker.ClearPoints();
      foreach(ChartPrimitive primitive in PrimitiveList) {
        if(primitive.IsHitTest) {
          _closestPointPicker.AddPoints(primitive.Points, primitive);
        }
      }
    }

    public Point SelectedPoint(Point mousePos) {
      GeneralTransform inverse = PrimitiveTransform.Inverse;
      if(inverse == null) {
        return new Point(0, 0);
      }
      Point selectedPoint = _closestPointPicker.MouseMoved(mousePos, inverse);
      return selectedPoint;
    }

    /// <summary>
    /// Draws a mouse cursor on the adorened element
    /// </summary>
    /// <param name="drawingContext"></param>
    protected override void OnRender(DrawingContext drawingContext) {
      GeneralTransform inverse = PrimitiveTransform.Inverse;
      if(inverse == null)
        return;

      Func<double, string> DefaultLabelGenerator = x => x.ToString();

      // Make sure these functions are assigned
      XAxisLabelGenerator = XAxisLabelGenerator ?? DefaultLabelGenerator;
      YAxisLabelGenerator = YAxisLabelGenerator ?? DefaultLabelGenerator;

      //_closestPointPicker.MouseMoved(MousePoint, inverse);

      Brush blackBrush = new SolidColorBrush(Colors.Black);
      Pen thinBlackPen = new Pen(blackBrush, 0.5);

      float radius = 15;
      if(_locked) {
        // Draw the little circle around the lock point
        Pen blackPen = new Pen(blackBrush, 3);

        Point point = PrimitiveTransform.Transform(_lockPoint);
        drawingContext.DrawEllipse(null, blackPen, point, 2.5, 2.5);
        drawingContext.DrawEllipse(null, new Pen(new SolidColorBrush(Colors.White), 2), point, 2.5, 2.5);

        // Draw the big yellow circle

        Pen yellowPen = new Pen(new SolidColorBrush(Colors.Yellow), 2);
        drawingContext.DrawEllipse(null, blackPen, _mousePoint, radius, radius);
        drawingContext.DrawEllipse(null, yellowPen, _mousePoint, radius, radius);

        if(_drawWholePlotCursor) {
          drawingContext.DrawLine(thinBlackPen, new Point(point.X, 0), new Point(point.X, point.Y - 4));
          drawingContext.DrawLine(thinBlackPen, new Point(point.X, point.Y + 4), new Point(point.X, this.ActualHeight));
          drawingContext.DrawLine(thinBlackPen, new Point(0, point.Y), new Point(point.X - 4, point.Y));
          drawingContext.DrawLine(thinBlackPen, new Point(point.X + 4, point.Y), new Point(this.ActualWidth, point.Y));
        }

      } else {
        // Draw the target symbol

        drawingContext.DrawEllipse(null, thinBlackPen, _mousePoint, radius, radius);

        if(_drawWholePlotCursor) {
          drawingContext.DrawLine(thinBlackPen, new Point(_mousePoint.X, 0), new Point(_mousePoint.X, _mousePoint.Y - 2));
          drawingContext.DrawLine(thinBlackPen, new Point(_mousePoint.X, _mousePoint.Y + 2), new Point(_mousePoint.X, this.ActualHeight));
          drawingContext.DrawLine(thinBlackPen, new Point(0, _mousePoint.Y), new Point(_mousePoint.X - 2, _mousePoint.Y));
          drawingContext.DrawLine(thinBlackPen, new Point(_mousePoint.X + 2, _mousePoint.Y), new Point(this.ActualWidth, _mousePoint.Y));
        } else {
          drawingContext.DrawLine(thinBlackPen, new Point(_mousePoint.X - radius * 1.6, _mousePoint.Y), new Point(_mousePoint.X - 2, _mousePoint.Y));
          drawingContext.DrawLine(thinBlackPen, new Point(_mousePoint.X + radius * 1.6, _mousePoint.Y), new Point(_mousePoint.X + 2, _mousePoint.Y));
          drawingContext.DrawLine(thinBlackPen, new Point(_mousePoint.X, _mousePoint.Y - radius * 1.6), new Point(_mousePoint.X, _mousePoint.Y - 2));
          drawingContext.DrawLine(thinBlackPen, new Point(_mousePoint.X, _mousePoint.Y + radius * 1.6), new Point(_mousePoint.X, _mousePoint.Y + 2));
        }
      }

      // Draw the coordinate text

      // Works out the number of decimal places required to show the difference between
      // 2 pixels. E.g if pixels are .1 apart then use 2 places etc
      Rect rect = inverse.TransformBounds(new Rect(0, 0, 1, 1));

      int xFigures = Math.Max(1, (int)(Math.Ceiling(-Math.Log10(rect.Width)) + .1));
      int yFigures = Math.Max(1, (int)(Math.Ceiling(-Math.Log10(rect.Height)) + .1));

      // Number of significant figures for the x coordinate
      string xFormat = "#0." + new string('#', xFigures);
      /// Number of significant figures for the y coordinate
      string yFormat = "#0." + new string('#', yFigures);

      string coordinateText;

      if(_locked && _pointToText.ContainsKey(_lockPoint)) {
        coordinateText = _pointToText[_lockPoint];
      } else {
        Point coordinate = _locked ? _lockPoint : inverse.Transform(_mousePoint);
        coordinateText = XAxisLabelGenerator(Math.Round(coordinate.X, xFigures)) + " , " + YAxisLabelGenerator(Math.Round(coordinate.Y, yFigures));
      }
      
      drawingContext.PushTransform(new ScaleTransform(1, -1));

      
      FormattedText formattedText = new FormattedText(coordinateText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10, blackBrush);

      Rect textBoxRect = new Rect(new Point(_mousePoint.X + radius * .7, -_mousePoint.Y + radius * .7), new Size(formattedText.Width, formattedText.Height));
      double diff = textBoxRect.Right + 3 - ActualWidth;

      if(diff > 0)
        textBoxRect.Location = new Point(textBoxRect.Left - diff, textBoxRect.Top);

      drawingContext.DrawRectangle(textBoxPen.Brush, textBoxPen, textBoxRect);
      drawingContext.DrawText(formattedText, textBoxRect.Location);
      drawingContext.Pop();
    }

    #endregion Public Methods

    // ********************************************************************
    // Private Methods
    // ********************************************************************
    #region Private Methods

    #endregion Private Methods

    // ********************************************************************
    // Properties
    // ********************************************************************
    #region Properties

    /// <summary>
    /// Gets/Sets if the coordinates are locked to the LockPoint or not
    /// </summary>
    public bool Locked {
      get {
        return _locked;
      }
      set {
        if(_locked != value) {
          _locked = value;
          InvalidateVisual();
        }
      }
    }

    /// <summary>
    /// Gets/Sets the coordinate for the cursor to show when it is "Locked"
    /// </summary>
    public Point LockPoint {
      get {
        return _lockPoint;
      }
      set {
        if(_lockPoint != value) {
          _lockPoint = value;
          InvalidateVisual();
        }
      }
    }

    /// <summary>
    /// Gets/Sets the current mouse position
    /// </summary>
    public Point MousePoint {
      get {
        return _mousePoint;
      }
      set {
        if(_mousePoint != value) {
          _mousePoint = value;
          if(PrimitiveTransform != null) {
            _closestPointPicker.QueueMouseMove(_mousePoint, PrimitiveTransform.Inverse);
          }
          InvalidateVisual();
        }
      }
    }

    /// <summary>
    /// Flag indicating whether to draw a cursor that goes from top to bottom, and left to right
    /// over the whole plot, or just draw a small target instead
    /// </summary>
    public bool DrawWholePlotCursor {
      get {
        return _drawWholePlotCursor;
      }
      set {
        if(_drawWholePlotCursor != value) {
          _drawWholePlotCursor = value;
          InvalidateVisual();
        }
      }
    }

    public Transform PrimitiveTransform {
      get;
      set;
    }

    /// <summary>
    /// A list of lines to draw on the chart
    /// </summary>
    public List<ChartPrimitive> PrimitiveList {
      get;
      set;
    }

    public Func<double, string> XAxisLabelGenerator {
      get;
      set;
    }
    public Func<double, string> YAxisLabelGenerator {
      get;
      set;
    }

    public Dictionary<Point, string> PointToText {
      get {
        return _pointToText;
      }
    }


    #endregion Properties
  }
}