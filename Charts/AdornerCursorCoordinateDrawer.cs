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

namespace Swordfish.NET.Charts {
  public class AdornerCursorCoordinateDrawer : Adorner {
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
    /// <summary>
    /// The transform of the element being adorned
    /// </summary>
    private MatrixTransform _elementTransform;

    private bool _drawWholePlotCursor = false;

    #endregion Private Fields

    // ********************************************************************
    // Public Methods
    // ********************************************************************
    #region Public Methods

    /// <summary>
    /// Constructor. Initializes class fields.
    /// </summary>
    public AdornerCursorCoordinateDrawer(UIElement adornedElement, MatrixTransform shapeTransform)
      : base(adornedElement) {
      this._elementTransform = shapeTransform;
      this.IsHitTestVisible = false;
    }

    /// <summary>
    /// Draws a mouse cursor on the adorened element
    /// </summary>
    /// <param name="drawingContext"></param>
    protected override void OnRender(DrawingContext drawingContext) {
      GeneralTransform inverse = _elementTransform.Inverse;
      if(inverse == null)
        return;

      Brush blackBrush = new SolidColorBrush(Colors.Black);
      Pen thinBlackPen = new Pen(blackBrush, 0.5);

      float radius = 15;
      if(_locked) {
        // Draw the little circle around the lock point
        Pen blackPen = new Pen(blackBrush, 3);

        Point point = _elementTransform.Transform(_lockPoint);
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

      Point coordinate = _locked ? _lockPoint : inverse.Transform(_mousePoint);

      string coordinateText = coordinate.X.ToString(xFormat) + "," + coordinate.Y.ToString(yFormat);
      drawingContext.PushTransform(new ScaleTransform(1, -1));
      FormattedText formattedText = new FormattedText(coordinateText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10, blackBrush);
      Pen textBoxPen = new Pen(new SolidColorBrush(Color.FromArgb(127, 255, 255, 255)), 1);


      Rect textBoxRect = new Rect(new Point(_mousePoint.X + radius * .7, -_mousePoint.Y + radius * .7), new Size(formattedText.Width, formattedText.Height));
      double diff = textBoxRect.Right + 3 - ((FrameworkElement)AdornedElement).ActualWidth;

      if(diff > 0)
        textBoxRect.Location = new Point(textBoxRect.Left - diff, textBoxRect.Top);

      drawingContext.DrawRectangle(textBoxPen.Brush, textBoxPen, textBoxRect);
      drawingContext.DrawText(formattedText, textBoxRect.Location);
      drawingContext.Pop();
    }

    protected void OnDataChanged() {
      AdornerLayer parent = AdornerLayer.GetAdornerLayer(AdornedElement);
      if(parent != null) {
        parent.Update();
      }
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
          OnDataChanged();
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
          OnDataChanged();
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
          OnDataChanged();
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
          OnDataChanged();
        }
      }
    }

    #endregion Properties

  }
}