// ****************************************************************************
// Copyright Swordfish Computing Australia 2006                              **
// http://www.swordfish.com.au/                                              **
//                                                                           **
// Filename: Swordfish\WinFX\Charts\ClosestPointPicker.cs                    **
// Authored by: John Stewien of Swordfish Computing                          **
// Date: April 2006                                                          **
//                                                                           **
// - Change Log -                                                            **
//*****************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;
using System.Collections.Concurrent;

namespace Swordfish.NET.Charts {
  /// <summary>
  /// Event args used in the ClosestPointPicker class to indicate
  /// when the closest point has changed.
  /// </summary>
  public class ClosestPointArgs : EventArgs {
    /// <summary>
    /// Whether the closest point in within the test distance or not
    /// </summary>
    public readonly bool Locked;
    /// <summary>
    /// The closest point
    /// </summary>
    public readonly Point ClosestPoint;

    /// <summary>
    /// Constructor. Initializes all the fields.
    /// </summary>
    /// <param name="locked"></param>
    /// <param name="closestPoint"></param>
    public ClosestPointArgs(bool locked, Point closestPoint) {
      Locked = locked;
      ClosestPoint = closestPoint;
    }
  }

  struct PointAndPrimitive {
    public readonly Point Point;
    public readonly ChartPrimitive Primitive;
    public PointAndPrimitive(Point point, ChartPrimitive primitive){
      Point = point;
      Primitive = primitive;
    }

    public double X {
      get {
        return Point.X;
      }
    }

    public double Y {
      get {
        return Point.Y;
      }
    }

  }

  struct PointAndBounds {
    public readonly Point Point;
    public readonly Rect Bounds;

    public PointAndBounds(Point point, Rect bounds) {
      Point = point;
      Bounds = bounds;
    }

  }

  /// <summary>
  /// This class picks the closest point to the screen point passed in. Typically
  /// used to find the closest scene point to the mouse pointer.
  /// </summary>
  class ClosestPointPicker {
    // ********************************************************************
    #region Private Fields

    /// <summary>
    /// A list of points to check against
    /// </summary>
    private List<PointAndPrimitive> _points;
    /// <summary>
    /// The minimum distance the cursor has to be away from the closest
    /// point to be locked
    /// </summary>
    private Rect _minimumDistance;
    /// <summary>
    /// The closest point to the cursor
    /// </summary>
    private PointAndPrimitive _closestPoint = new PointAndPrimitive(new Point(0, 0),null);
    /// <summary>
    /// Flag indicating that the closest point is within the minimum range
    /// </summary>
    private bool _locked = false;

    private Dispatcher _dispatcher;

    private BlockingCollection<PointAndBounds> _mousePosQueue;

    #endregion Private Fields

    // ********************************************************************
    // Public Methods
    // ********************************************************************
    #region Public Methods

    /// <summary>
    // Constructor. Initializes class fields.
    /// </summary>
    /// <param name="minimumDistance"></param>
    public ClosestPointPicker(Size minimumDistance, Dispatcher dispatcher) {
      this._minimumDistance = new Rect(minimumDistance);
      _dispatcher = dispatcher;
      _points = new List<PointAndPrimitive>();
      _mousePosQueue = new BlockingCollection<PointAndBounds>();
      Thread thread = new Thread(new ThreadStart(() => {
        foreach(PointAndBounds mousePosIter in _mousePosQueue.GetConsumingEnumerable()) {

          PointAndBounds mousePos = mousePosIter;
          PointAndBounds nextMousePos;

          // Flush the queue to get the newest position
          while(_mousePosQueue.TryTake(out nextMousePos)) {
            mousePos = nextMousePos;
          }
          MouseMoved(mousePos.Point, mousePos.Bounds);
        }
      }));
      thread.IsBackground = true;
      thread.Start();
    }

    public void QueueMouseMove(Point mousePos, GeneralTransform screenToSceneTransform) {
      mousePos = screenToSceneTransform.Transform(mousePos);
      Rect minimumBounds = screenToSceneTransform.TransformBounds(_minimumDistance);
      _mousePosQueue.Add(new PointAndBounds(mousePos, minimumBounds));
    }

    /// <summary>
    /// Handles when the mouse moves
    /// </summary>
    /// <param name="mousePos"></param>
    /// <param name="screenToSceneTransform"></param>
    public Point MouseMoved(Point mousePos, GeneralTransform screenToSceneTransform) {
      if(screenToSceneTransform == null)
        return new Point(0, 0);
      // Convert the mouse coordinates to scene coordinates
      mousePos = screenToSceneTransform.Transform(mousePos);
      // Transform the minimum distance ignoring the translation
      Rect minimumBounds = screenToSceneTransform.TransformBounds(_minimumDistance);
      return MouseMoved(mousePos, minimumBounds);
    }

    /// <summary>
    /// Handles when the mouse moves in a way that that it can execute on a thread that isn't
    /// the GUI thread.
    /// </summary>
    /// <param name="mousePos"></param>
    /// <param name="minimumBounds"></param>
    /// <returns></returns>
    public Point MouseMoved(Point mousePos, Rect minimumBounds) {

      bool newLocked = false;
      PointAndPrimitive newPoint = new PointAndPrimitive(mousePos, null);

      if(_points.Count > 0) {

        double nearestDistanceSquared;
        newPoint = GetEllipseScaledNearestPoint(_points, mousePos, (Vector)(minimumBounds.Size), out nearestDistanceSquared);
        newLocked = nearestDistanceSquared <= 1;
      }

      bool lockedChanged = newLocked != _locked;
      bool pointChanged = newPoint.Point != _closestPoint.Point;

      if(_closestPoint.Primitive != null && _closestPoint.Primitive.LegendLabel != null) {
        _closestPoint.Primitive.LegendLabel.IsHighlighted = false;
      }

      _locked = newLocked;
      _closestPoint = newPoint;

      Action updateGui = () => {
        if(_closestPoint.Primitive != null) {
          ChartPrimitive primitive = _closestPoint.Primitive;

          if(primitive.LegendColor != Colors.Transparent && primitive.LegendLabel != null && _locked) {
            primitive.LegendLabel.IsHighlighted = true;
          }
        }
        if((pointChanged && _locked) || lockedChanged) {
          OnClosestPointChanged();
        }
      };

      if(_dispatcher.Thread != Thread.CurrentThread) {
        _dispatcher.Invoke(updateGui);
      } else {
        updateGui();
      }

      return _locked ? _closestPoint.Point : mousePos;
    }

    /// <summary>
    /// Gets the nearest point but allows for normalising the x and y
    /// by different distances, which is required if you scale the x
    /// and y seperately, but want to get the visually closest point.
    /// </summary>
    /// <param name="points"></param>
    /// <param name="point"></param>
    /// <param name="inverseNormalisation"></param>
    /// <param name="nearestDistanceSquared"></param>
    /// <returns></returns>
    public static PointAndPrimitive GetEllipseScaledNearestPoint(IEnumerable<PointAndPrimitive> points, Point point, Vector ratio, out double nearestDistanceSquared) {
      Point inverseNormalisation = new Point(1 / ratio.X, 1 / ratio.Y);

      nearestDistanceSquared = 0;

      PointAndPrimitive nearestPoint = new PointAndPrimitive(point, null);

      // Just pick off the first point to initialize stuff
      foreach(PointAndPrimitive testPoint in points) {
        nearestDistanceSquared = new Vector((testPoint.X - point.X) * inverseNormalisation.X, (testPoint.Y - point.Y) * inverseNormalisation.Y).LengthSquared;
        nearestPoint = testPoint;
        break;
      }

      // Loop through all points to find the closest
      foreach(PointAndPrimitive testPoint in points) {
        double distanceSquared = new Vector((testPoint.X - point.X) * inverseNormalisation.X, (testPoint.Y - point.Y) * inverseNormalisation.Y).LengthSquared;
        if(distanceSquared < nearestDistanceSquared) {
          nearestDistanceSquared = distanceSquared;
          nearestPoint = testPoint;
        }
      }

      return nearestPoint;
    }

    public void ClearPoints() {
      _points.Clear();
    }

    public void AddPoints(IEnumerable<Point> pointsToAdd, ChartPrimitive primitive) {
      foreach(Point point in pointsToAdd) {
        _points.Add(new PointAndPrimitive(point, primitive));
      }
    }

    #endregion Public Methods

    // ********************************************************************
    // Properties
    // ********************************************************************
    #region Properties

    /// <summary>
    /// Gets if the current point is close enough to any of the other points
    /// </summary>
    public bool Locked {
      get {
        return _locked;
      }
    }

    /// <summary>
    /// Gets the closest point
    /// </summary>
    public Point ClosestPoint {
      get {
        return _closestPoint.Point;
      }
    }

    /// <summary>
    /// Gets/Sets the minimum distance checked for
    /// </summary>
    public Size MinimumDistance {
      get {
        return _minimumDistance.Size;
      }
      set {
        _minimumDistance = new Rect(value);
      }
    }

    #endregion Properties

    // ********************************************************************
    // Events and Event Triggers
    // ********************************************************************
    #region Events and Event Triggers

    /// <summary>
    /// Event that gets fired when the closest point changes and is within
    /// minimum range
    /// </summary>
    public event EventHandler<ClosestPointArgs> ClosestPointChanged;

    /// <summary>
    /// Fires the ClosestPointChanged event
    /// </summary>
    protected void OnClosestPointChanged() {
      if(ClosestPointChanged != null) {
        ClosestPointChanged(this, new ClosestPointArgs(_locked, _closestPoint.Point));
      }
    }

    #endregion Events and Event Triggers
  }
}
