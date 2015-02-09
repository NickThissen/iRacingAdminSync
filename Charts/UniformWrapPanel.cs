// ****************************************************************************
// Copyright Swordfish Computing Australia 2006                              **
// http://www.swordfish.com.au/                                              **
//                                                                           **
// Filename: Swordfish\WinFX\Charts\UniformWrapPanel.cs                      **
// Authored by: John Stewien of Swordfish Computing                          **
// Date: April 2006                                                          **
//                                                                           **
// - Change Log -                                                            **
//*****************************************************************************

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Swordfish.NET.Charts {
  public class UniformWrapPanel : Panel {
    // ********************************************************************
    // Private Fields
    // ********************************************************************
    #region Private Fields

    /// <summary>
    /// The size of the cells to draw in the panel
    /// </summary>
    protected Size _uniformSize;
    /// <summary>
    /// number of columns
    /// </summary>
    protected int _columnCount;
    /// <summary>
    /// number of rows
    /// </summary>
    protected int _rowCount;
    /// <summary>
    /// item ordering direction
    /// </summary>
    protected Orientation _itemOrdering = Orientation.Vertical;
    /// <summary>
    /// item position flow direction
    /// </summary>
    protected Orientation _wrapMode = Orientation.Horizontal;

    #endregion

    // ********************************************************************
    // Base Class Overrides
    // ********************************************************************
    #region Base Class Overrides


    /// <summary>
    /// Override the default Measure method of Panel 
    /// </summary>
    /// <param name="availableSize"></param>
    /// <returns></returns>
    protected override Size MeasureOverride(Size availableSize) {
      Size childSize = availableSize;
      _uniformSize = new Size(1, 1);

      // Get the maximum size
      foreach(UIElement child in InternalChildren) {
        child.Measure(childSize);
        _uniformSize.Width = Math.Max(_uniformSize.Width, child.DesiredSize.Width);
        _uniformSize.Height = Math.Max(_uniformSize.Height, child.DesiredSize.Height);
      }

      // Work out the size required depending if we are going to flow them down the left side, or across the top
      switch(_wrapMode) {
        case Orientation.Horizontal:
          _columnCount = (int)Math.Max(1, Math.Min(InternalChildren.Count, availableSize.Width / _uniformSize.Width));
          _rowCount = (int)Math.Ceiling((double)InternalChildren.Count / (double)_columnCount);
          if(_itemOrdering == Orientation.Vertical && _rowCount != 0)
            _columnCount = (int)Math.Ceiling((double)InternalChildren.Count / (double)_rowCount);
          break;
        case Orientation.Vertical:
          _rowCount = (int)Math.Max(1, Math.Min(InternalChildren.Count, availableSize.Height / _uniformSize.Height));
          _columnCount = (int)Math.Ceiling((double)InternalChildren.Count / (double)_rowCount);
          if(_itemOrdering == Orientation.Horizontal && _columnCount != 0)
            _rowCount = (int)Math.Ceiling((double)InternalChildren.Count / (double)_columnCount);
          break;
      }

      Size requestedSize = new Size(_columnCount * _uniformSize.Width, _rowCount * _uniformSize.Height);

      return requestedSize;
    }

    /// <summary>
    /// Override the default Arrange method
    /// </summary>
    /// <param name="finalSize"></param>
    /// <returns></returns>
    protected override Size ArrangeOverride(Size finalSize) {
      int columnNo = 0;
      int rowNo = 0;

      Size returnSize = new Size(
          Math.Max(_uniformSize.Width, _columnCount != 0 ? finalSize.Width / _columnCount : 0),
          Math.Max(_uniformSize.Height, _rowCount != 0 ? finalSize.Height / _rowCount : 0));

      Size renderedSize = new Size(Math.Round(_uniformSize.Width), Math.Round(_uniformSize.Height));

      foreach(UIElement child in InternalChildren) {
        child.Arrange(new Rect(new Point(Math.Round(columnNo * _uniformSize.Width), Math.Round(rowNo * _uniformSize.Height)), renderedSize));

        switch(_itemOrdering) {
          case Orientation.Vertical:
            rowNo++;
            if(rowNo >= _rowCount) {
              rowNo = 0;
              columnNo++;
            }
            break;
          case Orientation.Horizontal:
            columnNo++;
            if(columnNo >= _columnCount) {
              columnNo = 0;
              rowNo++;
            }
            break;
        }
      }

      return new Size(returnSize.Width * _columnCount, returnSize.Height * _rowCount); // Returns the final Arranged size
    }

    #endregion Base Class Overrides

    // ********************************************************************
    // Properties
    // ********************************************************************
    #region Properties

    /// <summary>
    /// Gets sets the orientation of the order that items appear
    /// </summary>
    public Orientation ItemOrdering {
      get {
        return _itemOrdering;
      }
      set {
        _itemOrdering = value;
        InvalidateMeasure();
      }
    }

    /// <summary>
    /// Gets/Sets the method of flowing the layout of the items
    /// </summary>
    public Orientation WrapMode {
      get {
        return _wrapMode;
      }
      set {
        _wrapMode = value;
        InvalidateMeasure();
      }
    }

    #endregion Properties
  }
}



