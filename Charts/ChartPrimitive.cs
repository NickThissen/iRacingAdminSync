// ****************************************************************************
// Copyright Swordfish Computing Australia 2006                              **
// http://www.swordfish.com.au/                                              **
//                                                                           **
// Filename: Swordfish\WinFX\Charts\ChartPrimitive.cs                        **
// Authored by: John Stewien of Swordfish Computing                          **
// Date: April 2006                                                          **
//                                                                           **
// - Change Log -                                                            **
//*****************************************************************************

using System;
using System.Collections;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Linq;
using System.Collections.ObjectModel;

namespace Swordfish.NET.Charts {
  public abstract class ChartPrimitive {
    // ********************************************************************
    // Private Fields
    // ********************************************************************
    #region Private Fields

    /// <summary>
    /// Flag indicating that the geometry needs to be updated
    /// </summary>
    protected bool _recalcGeometry = true;
    protected bool _recalcMinMax = true;

    protected Point? _minPoint = null;
    protected Point? _maxPoint = null;

    #endregion Private Fields

    // ********************************************************************
    // Methods
    // ********************************************************************
    #region Methods

    /// <summary>
    /// Constructor. Initializes class fields.
    /// </summary>
    /// <param name="primitiveType"></param>
    public ChartPrimitive() {

      // Flag indicating whether to hit test the primitive or not
      IsHitTest = true;

      // Flag indicating if the line is dashed or not
      IsDashed = false;

      // The line thickness
      LineThickness = 1;

      /// The label for the primitive
      Label = "Unlabled";

      LegendColor = Colors.Transparent;

      PointColor = Colors.Black;

      // Points that make up the plot
      Points = new ObservableCollection<Point>();
      Points.CollectionChanged += Points_CollectionChanged;
    }

    protected virtual void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
      _recalcGeometry = true;
      _recalcMinMax = true;
    }

    /// <summary>
    /// Copy constructor. Deep copies the ChartPrimitive passed in.
    /// </summary>
    /// <param name="chartPrimitive"></param>
    protected ChartPrimitive(ChartPrimitive chartPrimitive)
      : this() {
      IsHitTest = chartPrimitive.IsHitTest;
      Label = chartPrimitive.Label;
      IsHitTest = chartPrimitive.IsHitTest;
      LineThickness = chartPrimitive.LineThickness;
      IsDashed = chartPrimitive.IsDashed;
      LegendColor = chartPrimitive.LegendColor;

      Points = new ObservableCollection<Point>(chartPrimitive.Points);
      Points.CollectionChanged += Points_CollectionChanged;
    }

    protected void CalculateMinMax() {
      if(_recalcMinMax) {
        _minPoint = null;
        _maxPoint = null;
        foreach(Point point in Points) {
          _minPoint = _minPoint.HasValue ? new Point(Math.Min(_minPoint.Value.X, point.X), Math.Min(_minPoint.Value.Y, point.Y)) : point;
          _maxPoint = _maxPoint.HasValue ? new Point(Math.Max(_maxPoint.Value.X, point.X), Math.Max(_maxPoint.Value.Y, point.Y)) : point;
        }
        if(!_minPoint.HasValue) {
          _minPoint = new Point(0, 0);
        }
        if(!_maxPoint.HasValue) {
          _maxPoint = new Point(0, 0);
        }
        _recalcMinMax = false;
      }
    }

    #endregion Methods

    // ********************************************************************
    // Properties
    // ********************************************************************
    #region Properties

    /// <summary>
    /// Gets a list of all the points in this primitive
    /// </summary>
    public ObservableCollection<Point> Points { get; private set; }

    /// <summary>
    /// Gets the minium x,y values in the point collection
    /// </summary>
    public Point MinPoint {
      get {
				if (MinPointOverride.HasValue) {
					return MinPointOverride.Value;
				}
        CalculateMinMax();
				if (MinXOverride.HasValue) {
					return new Point(MinXOverride.Value, _minPoint.Value.Y);
				}
				if (MinYOverride.HasValue) {
					return new Point(_minPoint.Value.X, MinYOverride.Value);
				}
        return _minPoint.Value;
      }
    }

		/// <summary>
		/// Gets the maximum x,y values in the point collection
		/// </summary>
		public Point MaxPoint {
			get {
				if (MaxPointOverride.HasValue) {
					return MaxPointOverride.Value;
				}
				CalculateMinMax();
				if (MaxXOverride.HasValue) {
					return new Point(MaxXOverride.Value, _maxPoint.Value.Y);
				}
				if (MaxYOverride.HasValue) {
					return new Point(_maxPoint.Value.X, MaxYOverride.Value);
				}
				return _maxPoint.Value;
			}
		}

		public Point? MinPointOverride {
			get {
				if (MinXOverride.HasValue && MinYOverride.HasValue) {
					return new Point(MinXOverride.Value, MinYOverride.Value);
				} else {
					return null;
				}
			}
			set {
				if (value.HasValue) {
					MinXOverride = value.Value.X;
					MinYOverride = value.Value.Y;
				} else {
					MinXOverride = null;
					MinYOverride = null;
				}
			}
		}
		public Point? MaxPointOverride {
			get {
				if (MaxXOverride.HasValue && MaxYOverride.HasValue) {
					return new Point(MaxXOverride.Value, MaxYOverride.Value);
				} else {
					return null;
				}
			}
			set {
				if (value.HasValue) {
					MaxXOverride = value.Value.X;
					MaxYOverride = value.Value.Y;
				} else {
					MaxXOverride = null;
					MaxYOverride = null;
				}
			}
		}

		public double? MinXOverride { get; set; }
		public double? MinYOverride { get; set; }
		public double? MaxXOverride { get; set; }
		public double? MaxYOverride { get; set; }

    /// <summary>
    /// Gets/Sets the line label
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Gets/Sets if the the line is dashed or not
    /// </summary>
    public bool IsDashed { get; set; }

    /// <summary>
    /// Gets/Sets if the line is to be hit tested or not
    /// </summary>
    public bool IsHitTest { get; set; }

    /// <summary>
    /// Gets/Sets the line thickness
    /// </summary>
    public double LineThickness { get; set; }

    /// <summary>
    /// Gets/Sets the ColorLabel shown in the plot legend for this ChartPrimitive
    /// </summary>
    public ColorLabel LegendLabel { get; set; }

    /// <summary>
    /// Gets/Sets if the primitve should be shown in the plot legend
    /// by setting this to a none transparent color.
    /// </summary>
    public Color LegendColor { get; set; }

    public bool ShowPoints {
      get;
      set;
    }

    public Color PointColor { get; set; }

    #endregion Properties

  }
}
