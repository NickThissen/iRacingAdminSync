using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Swordfish.NET.Charts {

  /// <summary>
  /// This renders the lines and labels on the underlying gridline canvas
  /// </summary>
  public class GridLineCanvas : Canvas {

    // ************************************************************************
    // Nested Classes
    // ************************************************************************
    #region Nested Classes

    /// <summary>
    /// Size that allows for negative values
    /// </summary>
    class UnrestrictedSize {
      public UnrestrictedSize(double width, double height) {
        Width = width;
        Height = height;
      }
      public double Width { get; set; }
      public double Height { get; set; }
    }

    /// <summary>
    /// Holds the text to be rendered, the location, the bar to be rendered in, and the orientation
    /// </summary>
    class LabelAndPos {
      public LabelAndPos(FormattedText text, Point location, int layer, Orientation orientation) {
        Text = text;
        Location = location;
        Layer = layer;
        Orientation = orientation;
      }

      public FormattedText Text { get; private set; }
      public Point Location { get; private set; }
      public int Layer { get; private set; }
      public Orientation Orientation { get; private set; }
    }

    #endregion Nested Classes

    // ************************************************************************
    // Private Fields
    // ************************************************************************
    #region Private Fields

    /// <summary>
    /// The optimal spacing between gridlines. Grid lines will be added or
    /// removed to get as close as possible to this optimum
    /// </summary>
    private Point _optimalGridLineSpacing = new Point(75, 75);
    /// <summary>
    /// The pen to use to render the grid lines
    /// </summary>
    private Pen _gridLinePen = GridLine._defaultPen;
    /// <summary>
    /// The brush to use to render the labels
    /// </summary>
    private Brush _gridLabelBrush = GridLabel._defaultBrush;

    #endregion Private Fields

    public GridLineCanvas() {
      GridLineOverrides = new List<GridLineOverride>();
      YAxisTitles = new List<GridLabel>();
      XAxisTitles = new List<GridLabel>();
      FontSize = 12;

        
    }

    protected FormattedText GetText(object text, Brush brush) {
      FormattedText formattedText = new FormattedText(
        text.ToString(),
        CultureInfo.CurrentCulture,
        FlowDirection.LeftToRight,
        new Typeface("Arial"),
        Math.Max(1,FontSize),
        brush,
        null,
        TextFormattingMode.Display);

      return formattedText;
    }

    /// <summary>
    /// Renders the grid lines and labels in immediate mode rendering style
    /// </summary>
    /// <param name="dc"></param>
    protected override void OnRender(DrawingContext dc) {

      Func<double, string> DefaultLabelGenerator = x => x.ToString();

      // Make sure these functions are assigned
      XAxisLabelGenerator = XAxisLabelGenerator ?? DefaultLabelGenerator;
      YAxisLabelGenerator = YAxisLabelGenerator ?? DefaultLabelGenerator;
      GridLineSpacingX = GridLineSpacingX ?? GridLineSpacings.Base10;
      GridLineSpacingY = GridLineSpacingY ?? GridLineSpacings.Base10;

      // Work out all the limits and scaling factors for rendering grid lines
      
      Size size = new Size(this.ActualWidth, this.ActualHeight);

      double scaleX = 0.0;
      double scaleY = 0.0;

      if(MaxPoint.X != MinPoint.X)
        scaleX = size.Width / (MaxPoint.X - MinPoint.X);
      if(MaxPoint.Y != MinPoint.Y)
        scaleY = size.Height / (MaxPoint.Y - MinPoint.Y);

      double spacingX = GridLineSpacingX(_optimalGridLineSpacing.X / scaleX);
      double spacingY = GridLineSpacingY(_optimalGridLineSpacing.Y / scaleY);

      int startXmult = (int)Math.Ceiling(MinPoint.X / spacingX);
      int endXmult = (int)Math.Floor(MaxPoint.X / spacingX);
      int startYmult = (int)Math.Ceiling(MinPoint.Y / spacingY);
      int endYmult = (int)Math.Floor(MaxPoint.Y / spacingY);

      double maxYLabelWidth = 0;
      double maxXLabelHeight = 0;

      // Do a first pass of the x axis labels to make sure we have enough grid spacing to fit them in
      double maxXLabelWidth = 0;
      for(int lineNo = startXmult; lineNo <= endXmult; ++lineNo) {
        // Get the x position in graphing coordinates
        double xValue = lineNo * spacingX;
        // Check if there are any grid line overrides for this area of the chart
        if(GridLineOverrides.Any(x => x.Orientation == Orientation.Vertical && x.Range.Contains(xValue))) {
          continue;
        }
        FormattedText formattedText = GetText(XAxisLabelGenerator(xValue), _gridLabelBrush);
        maxXLabelWidth = Math.Max(maxXLabelWidth, formattedText.Width);
      }

      // Adjust the X spacing accordingly
      double minGridLineSpacingX = (maxXLabelWidth+8)*2;
      if(minGridLineSpacingX > _optimalGridLineSpacing.X) {
        spacingX = GridLineSpacingX(minGridLineSpacingX / scaleX);
        startXmult = (int)Math.Ceiling(MinPoint.X / spacingX);
        endXmult = (int)Math.Floor(MaxPoint.X / spacingX);
      }

      // Do a first pass of the y axis labels to make sure we have enough grid spacing to fit them in
      double maxYLabelHeight = 0;
      for(int lineNo = startYmult; lineNo <= endYmult; ++lineNo) {
        // Get the y position in graphing coordinates
        double yValue = lineNo * spacingY;
        // Check if there are any grid line overrides for this area of the chart
        if(GridLineOverrides.Any(y => y.Orientation == Orientation.Horizontal && y.Range.Contains(yValue))) {
          continue;
        }
        FormattedText formattedText = GetText(YAxisLabelGenerator(yValue), _gridLabelBrush);
        maxYLabelHeight = Math.Max(maxYLabelHeight, formattedText.Width);
      }

      // Adjust the Y spacing accordingly
      double minGridLineSpacingY = (maxYLabelHeight + 8) * 2;
      if(minGridLineSpacingY > _optimalGridLineSpacing.Y) {
        spacingY = GridLineSpacingY(minGridLineSpacingY / scaleY);
        startYmult = (int)Math.Ceiling(MinPoint.Y / spacingY);
        endYmult = (int)Math.Floor(MaxPoint.Y / spacingY);
      }

      LastSpacingX = spacingX;
      LastSpacingY = spacingY;

      // Draw all the vertical gridlines

      for(int lineNo = startXmult; lineNo <= endXmult; ++lineNo) {
        
        // Get the x position in graphing coordinates
        double xValue = lineNo * spacingX;

        // Check if there are any grid line overrides for this area of the chart
        if(GridLineOverrides.Any(x => x.Orientation == Orientation.Vertical && x.Range.Contains(xValue))) {
          continue;
        }

        double xPos = (xValue - MinPoint.X) * scaleX;
        Point startPoint = new Point(xPos, size.Height);
        Point endPoint = new Point(xPos, 0);

        FormattedText formattedText = GetText(XAxisLabelGenerator(xValue), _gridLabelBrush);

        maxXLabelHeight = Math.Max(maxXLabelHeight, formattedText.Height);

        Point textPoint = new Point(xPos - formattedText.Width * .5, size.Height + 1);
        dc.DrawText(formattedText, textPoint);
        //dc.DrawLine(_gridLinePen, startPoint, endPoint);
      }

      // Draw all the horizontal gridlines

      for(int lineNo = startYmult; lineNo <= endYmult; ++lineNo) {

        // Get the y position in graphing coordinates
        double yValue = lineNo * spacingY;

        // Check if there are any grid line overrides for this area of the chart
        if(GridLineOverrides.Any(y => y.Orientation == Orientation.Horizontal && y.Range.Contains(yValue))) {
          continue;
        }

        double yPos = (-yValue + MinPoint.Y) * scaleY + size.Height;
        Point startPoint = new Point(0, yPos);
        Point endPoint = new Point(size.Width, yPos);

        FormattedText formattedText = GetText(YAxisLabelGenerator(yValue), _gridLabelBrush);
        RotateTransform rotateTransform = new RotateTransform(-90);
        Point textPoint = new Point(-formattedText.Height - 1, yPos + formattedText.Width * .5);
        textPoint = rotateTransform.Inverse.Transform(textPoint);
        dc.PushTransform(rotateTransform);
        dc.DrawText(formattedText, textPoint);
        dc.Pop();

        //dc.DrawLine(_gridLinePen, startPoint, endPoint);

        maxYLabelWidth = Math.Max(maxYLabelWidth, formattedText.Height);

      }

      //foreach(var gridLineOverride in GridLineOverrides) {
      //  switch(gridLineOverride.Orientation) {
      //    case Orientation.Vertical:

      //      // Draw the Vertical Lines

      //      foreach(var gridLine in gridLineOverride.GridLines) {
      //        double xValue = gridLine.Location;
      //        if(!RangeX.Contains(xValue)) {
      //          continue;
      //        }
      //        double xPos = (xValue - MinPoint.X) * scaleX;
      //        Point startPoint = new Point(xPos, size.Height);
      //        Point endPoint = new Point(xPos, 0);
      //        dc.DrawLine(gridLine.Pen, startPoint, endPoint);
      //      }

      //      break;
      //    case Orientation.Horizontal:

      //      // Draw the Horizontal lines

      //      foreach(var gridLine in gridLineOverride.GridLines) {
      //        double yValue = gridLine.Location;
      //        if(!RangeY.Contains(yValue)) {
      //          continue;
      //        }
      //        double yPos = (-yValue + MinPoint.Y) * scaleY + size.Height;
      //        Point startPoint = new Point(gridLine.Extended ? -YGridLineLabelBar.Width : 0, yPos);
      //        Point endPoint = new Point(size.Width, yPos);
      //        dc.DrawLine(gridLine.Pen, startPoint, endPoint);
      //      }
      //      break;
      //  }
      //}

      //// Draw the grid line override Labels

      //foreach(var gridLineOverride in GridLineOverrides) {
      //  switch(gridLineOverride.Orientation) {
      //    case Orientation.Vertical:
      //      // TODO: Draw the X-Axis labels
      //      break;
      //    case Orientation.Horizontal:
      //      // Draw the Y-Axis labels
      //      maxYLabelWidth = Math.Max(maxYLabelWidth, DrawYAxisLabels(dc, scaleY, gridLineOverride.GridLabels, 0));
      //      break;
      //  }
      //}

      XGridLineLabelBar.Height = maxXLabelHeight + 2;
      YGridLineLabelBar.Width = maxYLabelWidth + 2;

      // Now render the Y axis label

      YAxisTitleBar.Width = DrawYAxisLabels(dc, scaleY, YAxisTitles, -YGridLineLabelBar.Width - 2) + 2;
      XAxisTitleBar.Height = DrawXAxisLabels(dc, scaleX, XAxisTitles, XGridLineLabelBar.Height + 2+this.ActualHeight) + 2;

    }

    /// <summary>
    /// Draws the Y Axis labels, returns the width required to fit all the labels.
    /// </summary>
    /// <param name="dc"></param>
    /// <param name="scaleY"></param>
    /// <param name="labels"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    private double DrawYAxisLabels(DrawingContext dc, double scaleY, IEnumerable<GridLabel> labels, double offset) {

      // A list of areas on the label area on which we've already rendered text.
      // If we go to render text on a area that's already used, then go to the next
      // area and move out one width
      List<List<Range<double>>> usedDrawingAreas = new List<List<Range<double>>>();
      usedDrawingAreas.Add(new List<Range<double>>());

      List<double> maxWidths = new List<double>();
      maxWidths.Add(0);

      // First of all work out all of the render positions on the y axis

      List<LabelAndPos> labelAndPos = new List<LabelAndPos>();

      foreach(var gridLabel in labels) {
        double yValue = gridLabel.IsFloating ? (MinPoint.Y + MaxPoint.Y) * 0.5 : gridLabel.Location;
        if(!RangeY.Contains(yValue)) {
          continue;
        }
        double yPos = (-yValue + MinPoint.Y) * scaleY + this.ActualHeight;
        int labelIndex = 0;
        FormattedText formattedText = GetText(gridLabel.Text, gridLabel.Brush);

        UnrestrictedSize labelArea;
        if(gridLabel.Orientation == Orientation.Vertical) {
          labelArea = new UnrestrictedSize(formattedText.Height, -formattedText.Width);
        } else {
          labelArea = new UnrestrictedSize(formattedText.Width, formattedText.Height);
        }

        Point textPoint = new Point(-labelArea.Width - 1 + offset, yPos - labelArea.Height * 0.5 - 1);
        textPoint.Y = Math.Max(textPoint.Y, Math.Max(0,-labelArea.Height));
        // Uncomment this line to stop any labels going below the bottom chart line
        //textPoint.Y = Math.Min(textPoint.Y, this.ActualHeight - Math.Max(0,labelArea.Height));
        Range<double> range = new Range<double>(textPoint.Y, textPoint.Y + labelArea.Height);

        bool intersects;
        do {
          intersects = false;
          foreach(var usedRange in usedDrawingAreas[labelIndex]) {
            if(range.Intersects(usedRange)) {
              intersects = true;
              labelIndex++;
              if(usedDrawingAreas.Count <= labelIndex) {
                usedDrawingAreas.Add(new List<Range<double>>());
                maxWidths.Add(0);
              }
              break;
            }
          }
        } while(intersects);

        usedDrawingAreas[labelIndex].Add(range);
        maxWidths[labelIndex] = Math.Max(maxWidths[labelIndex], labelArea.Width);
        labelAndPos.Add(new LabelAndPos(formattedText, textPoint, labelIndex, gridLabel.Orientation));
      }

      List<double> labelOffsets = new List<double>();
      labelOffsets.Add(0);
      double labelOffset = -2;
      foreach(double width in maxWidths) {
        labelOffset += 2 + width;
        labelOffsets.Add(labelOffset);
      }

      foreach(var gridLabel in labelAndPos) {
        FormattedText formattedText = gridLabel.Text;
        Point textPoint = gridLabel.Location;
        textPoint.X -= labelOffsets[gridLabel.Layer];
        if(gridLabel.Orientation == Orientation.Vertical) {
          RotateTransform rotateTransform = new RotateTransform(-90);
          dc.PushTransform(rotateTransform);
          textPoint = rotateTransform.Inverse.Transform(textPoint);
          dc.DrawText(formattedText, textPoint);
          dc.Pop();
        } else {
          dc.DrawText(formattedText, textPoint);
        }
      }

      double totalMaxWidth = -2;
      foreach(var maxWidth in maxWidths) {
        totalMaxWidth += maxWidth + 2;
      }
      return totalMaxWidth;
    }

    /// <summary>
    /// Draws the X Axis labels, returns the height required to fit all the labels.
    /// </summary>
    /// <param name="dc"></param>
    /// <param name="scaleY"></param>
    /// <param name="labels"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    private double DrawXAxisLabels(DrawingContext dc, double scaleX, IEnumerable<GridLabel> labels, double offset) {

      // A list of areas on the label area on which we've already rendered text.
      // If we go to render text on a area that's already used, then go to the next
      // area and move out one width
      List<List<Range<double>>> usedDrawingAreas = new List<List<Range<double>>>();
      usedDrawingAreas.Add(new List<Range<double>>());

      List<double> maxHeights = new List<double>();
      maxHeights.Add(0);

      // First of all work out all of the render positions on the y axis

      List<LabelAndPos> labelAndPos = new List<LabelAndPos>();

      foreach(var gridLabel in labels) {
        double xValue = gridLabel.IsFloating ? (MinPoint.X + MaxPoint.X) * 0.5 : gridLabel.Location;
        if(!RangeX.Contains(xValue)) {
          continue;
        }
        double xPos = (-xValue + MinPoint.X) * scaleX + this.ActualWidth;
        int labelIndex = 0;
        FormattedText formattedText = GetText(gridLabel.Text, gridLabel.Brush);

        UnrestrictedSize labelArea;
        if(gridLabel.Orientation == Orientation.Vertical) {
          labelArea = new UnrestrictedSize(formattedText.Height, formattedText.Width);
        } else {
          labelArea = new UnrestrictedSize(formattedText.Width, formattedText.Height);
        }

        Point textPoint = new Point(xPos - labelArea.Width * 0.5 - 1 , offset);
        textPoint.X = Math.Min(textPoint.X, this.ActualWidth - Math.Max(0, labelArea.Width * 0.5));
        // Uncomment this line to stop any labels going below the bottom chart line
        //textPoint.X = Math.Max(textPoint.X, 0);
        Range<double> range = new Range<double>(textPoint.X, textPoint.X + labelArea.Width);

        bool intersects;
        do {
          intersects = false;
          foreach(var usedRange in usedDrawingAreas[labelIndex]) {
            if(range.Intersects(usedRange)) {
              intersects = true;
              labelIndex++;
              if(usedDrawingAreas.Count <= labelIndex) {
                usedDrawingAreas.Add(new List<Range<double>>());
                maxHeights.Add(0);
              }
              break;
            }
          }
        } while(intersects);

        usedDrawingAreas[labelIndex].Add(range);
        maxHeights[labelIndex] = Math.Max(maxHeights[labelIndex], labelArea.Height);
        labelAndPos.Add(new LabelAndPos(formattedText, textPoint, labelIndex, gridLabel.Orientation));
      }

      List<double> labelOffsets = new List<double>();
      labelOffsets.Add(0);
      double labelOffset = -2;
      foreach(double height in maxHeights) {
        labelOffset += 2 + height;
        labelOffsets.Add(labelOffset);
      }

      foreach(var gridLabel in labelAndPos) {
        FormattedText formattedText = gridLabel.Text;
        Point textPoint = gridLabel.Location;
        textPoint.X -= labelOffsets[gridLabel.Layer];
        if(gridLabel.Orientation == Orientation.Vertical) {
          RotateTransform rotateTransform = new RotateTransform(-90);
          dc.PushTransform(rotateTransform);
          textPoint = rotateTransform.Inverse.Transform(textPoint);
          dc.DrawText(formattedText, textPoint);
          dc.Pop();
        } else {
          dc.DrawText(formattedText, textPoint);
        }
      }

      double totalMaxHeight = -2;
      foreach(var maxHeight in maxHeights) {
        totalMaxHeight += maxHeight + 2;
      }
      return totalMaxHeight;
    }

    // ************************************************************************
    // Properties
    // ************************************************************************
    #region Properties

    /// <summary>
    /// Gets the range of the x axis
    /// </summary>
    public Range<double> RangeX {
      get {
        return new Range<double>(MinPoint.X, MaxPoint.X);
      }
    }

    /// <summary>
    /// Gets the range of the y axis
    /// </summary>
    public Range<double> RangeY {
      get {
        return new Range<double>(MinPoint.Y, MaxPoint.Y);
      }
    }

    public List<GridLineOverride> GridLineOverrides { get; private set; }
    public List<GridLabel> YAxisTitles { get; private set; }
    public List<GridLabel> XAxisTitles { get; private set; }

    /// <summary>
    /// The coordinates at the bottom left of the plot area
    /// </summary>
    public Point MinPoint { get; set; }
    /// <summary>
    /// The coordinates at the top right of the plot area
    /// </summary>
    public Point MaxPoint { get; set; }

    public double FontSize { get; set; }

    public Func<double, string> XAxisLabelGenerator { get; set; }
    public Func<double, string> YAxisLabelGenerator { get; set; }

    public Func<double, double> GridLineSpacingX { get; set; }
    public Func<double, double> GridLineSpacingY { get; set; }

    public Rectangle XGridLineLabelBar { get; set; }
    public Rectangle YGridLineLabelBar { get; set; }

    public Rectangle XAxisTitleBar { get; set; }
    public Rectangle YAxisTitleBar { get; set; }

    public double LastSpacingX {
      get;
      private set;
    }

    public double LastSpacingY {
      get;
      private set;
    }

    #endregion Properties
  }
}
