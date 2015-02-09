// ****************************************************************************
// Copyright Swordfish Computing Australia 2006                              **
// http://www.swordfish.com.au/                                              **
//                                                                           **
// Filename: Swordfish\WinFX\Charts\PanZoomCalculator.cs                     **
// Authored by: John Stewien of Swordfish Computing                          **
// Date: April 2006                                                          **
//                                                                           **
// - Change Log -                                                            **
//*****************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Diagnostics;

namespace Swordfish.NET.Charts {
  /// <summary>
  /// Event Arguments containing the updated pan and zoom
  /// </summary>
  class PanZoomArgs : EventArgs {
    /// <summary>
    /// The current pan
    /// </summary>
    public readonly Point CurrentPan;
    /// <summary>
    /// The current zoom
    /// </summary>
    public readonly Point CurrentZoom;

    /// <summary>
    /// Constructor. Initializes class fields.
    /// </summary>
    /// <param name="currentPan"></param>
    /// <param name="currentZoom"></param>
    public PanZoomArgs(Point currentPan, Point currentZoom) {
      this.CurrentPan = currentPan;
      this.CurrentZoom = currentZoom;
    }
  }

  /// <summary>
  /// This class handles calculating the pan and zoom for a control. Implements
  /// it like a state machine.
  /// </summary>
  class PanZoomCalculator {
    // ********************************************************************
    // Private Fields
    // ********************************************************************
    #region Private Fields

    /// <summary>
    /// The cursor position that the user is zooming in on
    /// </summary>
    private Point _zoomCentre;
    /// <summary>
    /// The starting point that the mouse was at when the user started
    /// zooming or panning
    /// </summary>
    private Point _lastPosition;
    /// <summary>
    /// The current pan position
    /// </summary>
    private Point _currentPan;
    /// <summary>
    /// The current zoom position
    /// </summary>
    private Point _currentZoom;
    /// <summary>
    /// Flag indicating if the left mouse button is down
    /// </summary>
    private bool _isPanning = false;
    /// <summary>
    /// Flag indicating if the right mouse button is down
    /// </summary>
    private bool _isZooming = false;
    /// <summary>
    /// The windows dimensions that we are zooming/panning in
    /// </summary>
    private Rect _window;

    #endregion Private Fields

    // ********************************************************************
    // Public Methods
    // ********************************************************************
    #region Public Methods

    /// <summary>
    /// Constructor. Initializes class fields.
    /// </summary>
    public PanZoomCalculator(Rect window) {
      // Initialize class fields
      _currentPan = new Point(0, 0);
      _currentZoom = new Point(1, 1);

      this._window = window;
    }

    /// <summary>
    /// Call this to start panning
    /// </summary>
    /// <param name="startPoint"></param>
    public void StartPan(Point position) {
      this._lastPosition = position;
      this._isPanning = true;
    }

    /// <summary>
    /// Call this to start zooming
    /// </summary>
    /// <param name="startPoint"></param>
    public void StartZoom(Point position) {
      this._zoomCentre = position;
      this._lastPosition = position;
      this._isZooming = true;

      MouseMoved(position);
    }

    /// <summary>
    /// Call this to Stop Panning
    /// </summary>
    public void StopPanning() {
      _isPanning = false;
      OnPanZoomChanged();
    }

    /// <summary>
    /// Call this to Stop Zooming
    /// </summary>
    public void StopZooming() {
      _isZooming = false;
      OnPanZoomChanged();
    }

    /// <summary>
    /// Event handler for when the mouse moves over the control.
    /// Changes the tool tip to show the graph coordinates at the
    /// current mouse point, and does zooming and panning.
    /// </summary>
    public void MouseMoved(Point newPosition) {
      if(_isPanning) {
        _currentPan.X += (newPosition.X - _lastPosition.X) / _currentZoom.X / _window.Width;
        _currentPan.Y += (newPosition.Y - _lastPosition.Y) / _currentZoom.Y / _window.Height;
      }

      if(_isZooming) {
        Point oldZoom = _currentZoom;

        _currentZoom.X *= Math.Pow(1.002, newPosition.X - _lastPosition.X);
        _currentZoom.Y *= Math.Pow(1.002, -newPosition.Y + _lastPosition.Y);
        _currentZoom.X = Math.Max(1, _currentZoom.X);
        _currentZoom.Y = Math.Max(1, _currentZoom.Y);

        _currentPan.X += (_window.Width * .5 - _zoomCentre.X) * (1 / oldZoom.X - 1 / _currentZoom.X) / _window.Width;
        _currentPan.Y += (-_window.Height * .5 - _zoomCentre.Y) * (1 / oldZoom.Y - 1 / _currentZoom.Y) / _window.Height;
      }

      _lastPosition = newPosition;

      if(_isPanning || _isZooming) {
        // Limit Pan
        Point maxPan = new Point();
        maxPan.X = .5 * (_currentZoom.X - 1) / (_currentZoom.X);
        maxPan.Y = .5 * (_currentZoom.Y - 1) / (_currentZoom.Y);
        _currentPan.X = Math.Min(maxPan.X, _currentPan.X);
        _currentPan.X = Math.Max(-maxPan.X, _currentPan.X);
        _currentPan.Y = Math.Min(maxPan.Y, _currentPan.Y);
        _currentPan.Y = Math.Max(-maxPan.Y, _currentPan.Y);

        if(Double.IsNaN(_currentPan.X) || Double.IsNaN(_currentPan.Y))
          _currentPan = new Point(0f, 0f);
        if(Double.IsNaN(_currentZoom.X) || Double.IsNaN(_currentZoom.Y))
          _currentZoom = new Point(1f, 1f);

        this.OnPanZoomChanged();
      }
    }

    /// <summary>
    /// Call this to reset the Pan/Zoom state machine
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void Reset() {
      // Reset zoom and pan
      _isPanning = false;
      _isZooming = false;
      _currentZoom = new Point(1, 1);
      _currentPan = new Point(0, 0);
      this.OnPanZoomChanged();
    }

    #endregion Public Methods

    // ********************************************************************
    // Properties
    // ********************************************************************
    #region Properties

    /// <summary>
    /// Gets whether panning is activated or not
    /// </summary>
    public bool IsPanning {
      get {
        return _isPanning;
      }
    }

    /// <summary>
    /// Gets whether zooming is activated or not
    /// </summary>
    public bool IsZooming {
      get {
        return _isZooming;
      }
    }

    /// <summary>
    /// Gets/Sets the window rectangle that is being zoomed in
    /// </summary>
    public Rect Window {
      get {
        return _window;
      }
      set {
        _window = value;
      }
    }

    /// <summary>
    /// Gets the current pan
    /// </summary>
    public Point Pan {
      get {
        return _currentPan;
      }
    }

    /// <summary>
    /// Gets the current zoom
    /// </summary>
    public Point Zoom {
      get {
        return _currentZoom;
      }
    }

    #endregion Properties

    // ********************************************************************
    // Events and Triggers
    // ********************************************************************
    #region Events and Triggers

    /// <summary>
    /// Gets fired when the pan or zoom changes
    /// </summary>
    public event EventHandler<PanZoomArgs> PanZoomChanged;

    /// <summary>
    /// Fires the PanZoomChanged event
    /// </summary>
    protected void OnPanZoomChanged() {
      if(PanZoomChanged != null) {
        PanZoomChanged(this, new PanZoomArgs(_currentPan, _currentZoom));
      }
    }

    #endregion Events and Triggers
  }
}
