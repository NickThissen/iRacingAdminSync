// ****************************************************************************
// Copyright Swordfish Computing Australia 2006                              **
// http://www.swordfish.com.au/                                              **
//                                                                           **
// Filename: Swordfish\WinFX\Charts\ChartUtilities.cs                        **
// Authored by: John Stewien of Swordfish Computing                          **
// Date: April 2006                                                          **
//                                                                           **
// - Change Log -                                                            **
//*****************************************************************************

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Windows.Controls;

namespace Swordfish.NET.Charts {
  /// <summary>
  /// Class that contains various utilities for drawing charts
  /// </summary>
  public static class ChartUtilities {
    // ********************************************************************
    // Public Methods
    // ********************************************************************
    #region Public Methods

    public static void CopyChartToClipboard(FrameworkElement plotToCopy, ChartControl chartControl, double width, double height) {
      plotToCopy = plotToCopy ?? chartControl;

      System.Drawing.Bitmap bitmap = CopyFrameworkElementToBitmap(plotToCopy, width, height);
      DataObject dataObject = new DataObject();
      dataObject.SetData(DataFormats.Bitmap, bitmap);
      if(chartControl != null) {
        string text = ChartUtilities.ConvertChartToSpreadsheetText(chartControl, '\t');
        MemoryStream csv = new MemoryStream(Encoding.UTF8.GetBytes(ChartUtilities.ConvertChartToSpreadsheetText(chartControl, ',')));
        dataObject.SetData(DataFormats.Text, text);
        dataObject.SetData(DataFormats.CommaSeparatedValue, csv);
      }
      Clipboard.SetDataObject(dataObject);
    }

    public static string ConvertChartToSpreadsheetText(ChartControl xyLineChart, char token) {
      int maxPrimitiveLength = 0;
      foreach(ChartPrimitive primitive in xyLineChart.Primitives) {
        maxPrimitiveLength = Math.Max(maxPrimitiveLength, primitive.Points.Count);
      }
      string[] grid = new string[maxPrimitiveLength + 1];
      foreach(ChartPrimitive primitive in xyLineChart.Primitives) {
        int row = 0;
        grid[row] += primitive.Label + " X" + token + primitive.Label + " Y" + token;
        foreach(System.Windows.Point point in primitive.Points) {
          ++row;
          grid[row] += point.X.ToString() + token + point.Y.ToString() + token;
        }
        ++row;
        while(row < grid.Length) {
          grid[row] += token.ToString() + token.ToString();
          ++row;
        }
      }

      StringBuilder sb = new StringBuilder();
      sb.AppendLine(xyLineChart.Title);
      foreach(string line in grid) {
        if(!string.IsNullOrEmpty(line))
          sb.AppendLine(line.Substring(0, line.Length - 1));
      }

      return sb.ToString();
    }

    /// <summary>
    /// Calculates the as near to the input as possible, a power of 10 times 1,2, or 5 
    /// </summary>
    /// <param name="optimalValue"> The value to get closest to</param>
    /// <returns>The nearest value to the input value</returns>
    public static double Closest_1_2_5_Pow10(double optimalValue) {
      double[] numbersList = { 1.0, 2.0, 5.0 };
      return ClosestValueInListTimesBaseToInteger(optimalValue, numbersList, 10.0);
    }

    /// <summary>
    /// Calculates the closest possible value to the optimalValue passed
    /// in, that can be obtained by multiplying one of the numbers in the
    /// list by the baseValue to the power of any integer.
    /// </summary>
    /// <param name="optimalValue">The number to get closest to</param>
    /// <param name="numbers">List of numbers to mulitply by</param>
    /// <param name="baseValue">The base value</param>
    /// <returns></returns>
    public static double ClosestValueInListTimesBaseToInteger(double optimalValue, double[] numbers, double baseValue) {
      double multiplier = Math.Pow(baseValue, Math.Floor(Math.Log(optimalValue) / Math.Log(baseValue)));
      double minimumDifference = baseValue * baseValue * multiplier;
      double closestValue = 0.0;
      double minimumNumber = baseValue * baseValue;

      foreach(double number in numbers) {
        double difference = Math.Abs(optimalValue - number * multiplier);
        if(difference < minimumDifference) {
          minimumDifference = difference;
          closestValue = number * multiplier;
        }
        if(number < minimumNumber) {
          minimumNumber = number;
        }
      }

      if(Math.Abs(optimalValue - minimumNumber * baseValue * multiplier) < Math.Abs(optimalValue - closestValue)) {
        closestValue = minimumNumber * baseValue * multiplier;
      }

      return closestValue;
    }


    /// <summary>
    /// Gets a nominally oversize rectangle that the plot will be drawn into
    /// </summary>
    /// <param name="primitiveList"></param>
    /// <param name="oversize"></param>
    /// <returns></returns>
    public static Rect GetPlotRectangle(List<ChartPrimitive> primitiveList, List<GridLineOverride> gridLines,  double oversize) {
      // Get the extent of the plot region by going through
      // all the lines, and finding the min and max points
      bool firstPassX = true;
      bool firstPassY = true;
      Vector minPoint = new Vector(0, 0);
      Vector maxPoint = new Vector(0, 0);

      foreach(ChartPrimitive primitive in primitiveList.Take(1)) {
        minPoint.X = primitive.MinPoint.X;
        maxPoint.X = primitive.MaxPoint.X;
        minPoint.Y = primitive.MinPoint.Y;
        maxPoint.Y = primitive.MaxPoint.Y;
        firstPassX = false;
        firstPassY = false;
      }
      foreach(ChartPrimitive primitive in primitiveList.Skip(1)) {
          minPoint.X = Math.Min(primitive.MinPoint.X, minPoint.X);
          minPoint.Y = Math.Min(primitive.MinPoint.Y, minPoint.Y);
          maxPoint.X = Math.Max(primitive.MaxPoint.X, maxPoint.X);
          maxPoint.Y = Math.Max(primitive.MaxPoint.Y, maxPoint.Y);
      }
      foreach(var gridLineOverride in gridLines){
        switch(gridLineOverride.Orientation) {
          case Orientation.Horizontal:
            foreach(var gridLine in gridLineOverride.GridLines) {
              minPoint.Y = firstPassY ? gridLine.Location : Math.Min(gridLine.Location, minPoint.Y);
              maxPoint.Y = firstPassY ? gridLine.Location : Math.Max(gridLine.Location, maxPoint.Y);
              firstPassY = false;
            }
            break;
          case Orientation.Vertical:
            foreach(var gridLine in gridLineOverride.GridLines) {
              minPoint.X = firstPassX ? gridLine.Location : Math.Min(gridLine.Location, minPoint.X);
              maxPoint.X = firstPassX ? gridLine.Location : Math.Max(gridLine.Location, maxPoint.X);
              firstPassX = false;
            }
            break;
        }
      }

      // Make sure that the plot size is greater than zero
      if((maxPoint.Y - minPoint.Y) <= 0) {
        if(maxPoint.Y != 0.0) {
          if(maxPoint.Y > 0) {
            maxPoint.Y = 1.05 * maxPoint.Y;
            minPoint.Y = 0.95 * maxPoint.Y;
          } else {
            minPoint.Y = 1.05 * maxPoint.Y;
            maxPoint.Y = 0.95 * maxPoint.Y;
          }
        } else {
          maxPoint.Y = 1;
          minPoint.Y = 0;
        }
      }

      if((maxPoint.X - minPoint.X) <= 0) {
        if(maxPoint.X != 0.0) {
          if(maxPoint.X > 0) {
            maxPoint.X = 1.05 * maxPoint.X;
            minPoint.X = 0.95 * maxPoint.X;
          } else {
            minPoint.X = 1.05 * maxPoint.X;
            maxPoint.X = 0.95 * maxPoint.X;
          }
        } else {
          maxPoint.X = 1;
          minPoint.X = 0;
        }
      }

      // Add a bit of a border around the plot
      maxPoint.X = maxPoint.X + (maxPoint.X - minPoint.X) * oversize * .5;
      maxPoint.Y = maxPoint.Y + (maxPoint.Y - minPoint.Y) * oversize * .5;
      minPoint.X = minPoint.X - (maxPoint.X - minPoint.X) * oversize * .5;
      minPoint.Y = minPoint.Y - (maxPoint.Y - minPoint.Y) * oversize * .5;


      return new Rect(minPoint.X, minPoint.Y, maxPoint.X - minPoint.X, maxPoint.Y - minPoint.Y);
    }//GetPlotRectangle

    /// <summary>
    /// Converts a ChartLine object to a ChartPolygon object that has
    /// one edge along the bottom Horizontal base line in the plot.
    /// </summary>
    /// <param name="chartLine"></param>
    /// <returns></returns>
    public static ChartPrimitiveXY ChartLineToBaseLinedPolygon(ChartPrimitiveXY chartLine) {
      ChartPrimitiveXY chartPolygon = chartLine.Clone();

      Point firstPoint = chartPolygon.Points[0];
      firstPoint.Y = 0;
      Point lastPoint = chartPolygon.Points[chartPolygon.Points.Count - 1];
      lastPoint.Y = 0;

      chartPolygon.InsertPoint(firstPoint, 0);
      chartPolygon.AddPoint(lastPoint);
      chartPolygon.FillColor = chartLine.LineColor;

      return chartPolygon;
    }

    /// <summary>
    /// Takes two lines and creates a polyon between them
    /// </summary>
    /// <param name="baseLine"></param>
    /// <param name="topLine"></param>
    /// <returns></returns>
    public static ChartPrimitiveXY LineDiffToPolygon(ChartControl factory, ChartPrimitiveXY baseLine, ChartPrimitiveXY topLine) {
      ChartPrimitiveXY polygon = factory.CreateXY();
      IList<Point> topLinePoints = topLine.Points;

      if(baseLine != null) {
        for(int pointNo = baseLine.Points.Count - 1; pointNo >= 0; --pointNo) {
          polygon.AddPoint(baseLine.Points[pointNo]);
        }
      } else {
        polygon.AddPoint(new Point(topLinePoints[topLinePoints.Count - 1].X, 0));
        polygon.AddPoint(new Point(0, 0));
      }

      for(int pointNo = 0; pointNo < topLinePoints.Count; ++pointNo) {
        polygon.AddPoint(topLinePoints[pointNo]);
      }

      polygon.FillColor = topLine.LineColor;

      return polygon;
    }

    public static void AddTestLines(ChartControl xyLineChart) {
      ChartControl factory = xyLineChart;
      // Add test Lines to demonstrate the control

      xyLineChart.Reset();

      double limit = 5;
      double increment = .05;

      // Create 3 normal lines
      ChartPrimitiveXY[] lines = new ChartPrimitiveXY[3];

      for(int lineNo = 0; lineNo < 3; ++lineNo) {
        ChartPrimitiveXY line = factory.CreateXY();

        // Label the lines
        line.Label = "Test Line " + (lineNo + 1).ToString();
        line.IsDashed = false;
        line.IsHitTest = true;
        line.LineThickness = 1.5;
        line.ShowPoints = true;

        line.AddPoint(0, 0);

        // Draw 3 sine curves
        for(double x = 0; x < limit + increment * .5; x += increment) {
          line.AddPoint(x, Math.Cos(x * Math.PI - lineNo * Math.PI / 1.5));
        }
        line.AddPoint(limit, 0);

        // Add the lines to the chart
        xyLineChart.AddPrimitive(line);
        lines[lineNo] = line;
      }

      // Set the line colors to Red, Green, and Blue
      lines[0].FillColor = Color.FromArgb(90, 255, 0, 0);
      lines[1].FillColor = Color.FromArgb(90, 0, 180, 0);
      lines[2].FillColor = Color.FromArgb(90, 0, 0, 255);

      // Set the line colors to Red, Green, and Blue
      lines[0].LineColor = Colors.Red;
      lines[1].LineColor = Colors.Green;
      lines[2].LineColor = Colors.Blue;

      lines[0].LegendColor = Colors.Red;
      lines[1].LegendColor = Colors.Green;
      lines[2].LegendColor = Colors.Blue;


      ChartPrimitiveHBar[] bars = new ChartPrimitiveHBar[3];

      // Set the line colors to Red, Green, and Blue
      Color[] barLineColors = new Color[]{
        Colors.Red,
        Colors.Green,
        Colors.Blue
      };

      Color[] barFillColors = new Color[]{
        Color.FromArgb(90, 255, 0, 0),
        Color.FromArgb(90, 0, 180, 0),
        Color.FromArgb(90, 0, 0, 255)
      };

      int colorIndex = 0;

      GridLineOverride gridLineOverride = new GridLineOverride(Orientation.Horizontal);
      gridLineOverride.Range = new Range<double>(-0.2 * (0 + 1) - 1 + 0.05, -0.2 * (2 + 1) - 1 - 0.05);

      for(int barNo = 0; barNo < 3; ++barNo) {
        ChartPrimitiveHBar bar = factory.CreateHBar(-0.2 * (barNo + 1) - 1, 0.05);

        gridLineOverride.AddLabel("Bar " + (barNo + 1).ToString(),-0.2 * (barNo + 1) - 1, Orientation.Horizontal);

        // Label the lines
        bar.IsDashed = false;
        bar.LineThickness = 1;
        bar.IsHitTest = true;
        bar.Label = "Test Bar " + (barNo + 1).ToString();

        colorIndex = barNo;
        for(double x = 0; x < 4 + barNo; x += 0.1 * (4 + barNo)) {
          bar.AddSegment(x, x+ 0.1 * (4 + barNo), barLineColors[colorIndex], barFillColors[colorIndex]);
          colorIndex = (colorIndex + 1) % 3;
        }
        // Add the lines to the chart
        xyLineChart.AddPrimitive(bar);
        bars[barNo] = bar;
      }

      xyLineChart.GridLineOverrides.Add(gridLineOverride);

      xyLineChart.Title = "Test Chart Title";
      xyLineChart.XAxisTitle = "Test Chart X Axis";
      xyLineChart.YAxisTitle = "Test Chart Y Axis";

      xyLineChart.RedrawPlotLines();
    }

    public static DrawingBrush CreateHatch50(Color color, Size blockSize) {
      GeometryGroup group = new GeometryGroup();
      RectangleGeometry rectangle1 = new RectangleGeometry(new Rect(new Point(0, 0), blockSize));
      RectangleGeometry rectangle2 = new RectangleGeometry(new Rect((Point)blockSize, blockSize));
      group.Children.Add(rectangle1);
      group.Children.Add(rectangle2);

      GeometryDrawing drawing = new GeometryDrawing(new SolidColorBrush(color), null, group);

      DrawingBrush brush = new DrawingBrush(drawing);
      brush.TileMode = TileMode.Tile;
      brush.ViewportUnits = BrushMappingMode.Absolute;
      brush.Viewport = new Rect(0, 0, blockSize.Width * 2, blockSize.Height * 2);
      return brush;
    }

    /// <summary>
    /// Copies a Framework Element to the clipboard as a bitmap
    /// </summary>
    /// <param name="copyTarget">The Framework Element to be copied</param>
    /// <param name="width">The width of the bitmap</param>
    /// <param name="height">The height of the bitmap</param>
    public static void CopyFrameworkElementToClipboard(FrameworkElement copyTarget, double width, double height) {
      if(copyTarget == null)
        return;

      Clipboard.SetDataObject(CopyFrameworkElementToBitmap(copyTarget, width, height));
    }

    public static System.Drawing.Bitmap CopyFrameworkElementToBitmap(FrameworkElement copyTarget, double width, double height) {
      if(copyTarget == null)
        return new System.Drawing.Bitmap((int)width, (int)height);

      System.Drawing.Bitmap bitmap;
      // Convert from a WinFX Bitmap Source to a Win32 Bitamp
      using(Stream outStream = CopyFrameworkElementToStream(copyTarget, width, height, new BmpBitmapEncoder(), new MemoryStream(), 96.0)) {
        bitmap = new System.Drawing.Bitmap(outStream);
      }

      return bitmap;
    }

    public static void CopyFrameworkElementToPNGFileNew(FrameworkElement copyTarget, double width, double height, string filename, double dpi) {
      BitmapEncoder pngEncoder = new PngBitmapEncoder();
      using(FileStream fileSteam = new FileStream(filename, FileMode.Create)) {
        CopyFrameworkElementToStreamNew(copyTarget, width, height, pngEncoder, fileSteam, dpi);
      }
    }


    public static void CopyFrameworkElementToPNGFile(FrameworkElement copyTarget, double width, double height, string filename, double dpi) {
      BitmapEncoder pngEncoder = new PngBitmapEncoder();
      using(FileStream fileSteam = new FileStream(filename, FileMode.Create)) {
        CopyFrameworkElementToStream(copyTarget, width, height, pngEncoder, fileSteam, dpi);
      }
    }

    /// <summary>
    /// Note RenderTargetBitmap has a huge memory leak that was fixed in .NET 4.0
    /// but only if you reference Winforms.
    /// </summary>
    /// <param name="copyTarget"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="enc"></param>
    /// <param name="outStream"></param>
    /// <param name="dpi"></param>
    /// <returns></returns>
    public static Stream CopyFrameworkElementToStreamNew(FrameworkElement copyTarget,
      double width, double height, BitmapEncoder enc, Stream outStream, double dpi) {

      // Store the Frameworks current layout transform, as this will be restored later
      Transform storedLayoutTransform = copyTarget.LayoutTransform;
      Transform storedRenderTransform = copyTarget.RenderTransform;

      double scale = dpi / 96.0;
      width *= scale;
      height *= scale;
      copyTarget.RenderTransform = new ScaleTransform(scale, scale);

      // Set the layout transform to unity to get the nominal width and height
      copyTarget.LayoutTransform = new ScaleTransform(1, 1);
      copyTarget.UpdateLayout();

      RenderTargetBitmap rtb;

      copyTarget.LayoutTransform =
        new ScaleTransform(1.0 / scale, 1.0 / scale);
      copyTarget.UpdateLayout();
      // Render to a Bitmap Source, note that the DPI is changed for the 
      // render target as a way of scaling the FrameworkElement
      rtb = new RenderTargetBitmap(
        (int)width,
        (int)height,
        dpi,
        dpi,
        PixelFormats.Default);

      rtb.Render(copyTarget);

      // Convert from a WinFX Bitmap Source to a Win32 Bitamp

      enc.Frames.Add(BitmapFrame.Create(rtb));

      enc.Save(outStream);
      // Restore the Framework Element to it's previous state
      copyTarget.LayoutTransform = storedLayoutTransform;
      copyTarget.RenderTransform = storedRenderTransform;

      copyTarget.UpdateLayout();

      return outStream;
    }

    /// <summary>
    /// Note RenderTargetBitmap has a huge memory leak that was fixed in .NET 4.0
    /// but only if you reference Winforms.
    /// </summary>
    /// <param name="copyTarget"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="enc"></param>
    /// <param name="outStream"></param>
    /// <param name="dpi"></param>
    /// <returns></returns>
    public static Stream CopyFrameworkElementToStream(FrameworkElement copyTarget,
      double width, double height, BitmapEncoder enc, Stream outStream, double? dpi) {
      // Store the Frameworks current layout transform, as this will be restored later
      Transform storedTransform = copyTarget.LayoutTransform;

      // Set the layout transform to unity to get the nominal width and height
      copyTarget.LayoutTransform = new ScaleTransform(1, 1);
      copyTarget.UpdateLayout();

      RenderTargetBitmap rtb;

      double baseHeight = copyTarget.ActualHeight;
      double baseWidth = copyTarget.ActualWidth;

      // Now scale the layout to fit the bitmap
      copyTarget.LayoutTransform =
        new ScaleTransform(baseWidth / width, baseHeight / height);
      copyTarget.UpdateLayout();

      // Render to a Bitmap Source, note that the DPI is changed for the 
      // render target as a way of scaling the FrameworkElement
      rtb = new RenderTargetBitmap(
        (int)width,
        (int)height,
        96d * width / baseWidth,
        96d * height / baseHeight,
        PixelFormats.Default);

      rtb.Render(copyTarget);

      // Convert from a WinFX Bitmap Source to a Win32 Bitamp

      if(dpi == null) {
        enc.Frames.Add(BitmapFrame.Create(rtb));
      } else {
        // Change the DPI

        // TODO: To avoid using multiple RenderTargetBitmap use VisualBrush instead of ImageBrush

        ImageBrush brush = new ImageBrush(BitmapFrame.Create(rtb));

        RenderTargetBitmap rtbDpi = new RenderTargetBitmap(
          (int)width, // PixelWidth
          (int)height, // PixelHeight
          (double)dpi, // DpiX
          (double)dpi, // DpiY
          PixelFormats.Default);

        DrawingVisual drawVisual = new DrawingVisual();
        using(DrawingContext dc = drawVisual.RenderOpen()) {
          dc.DrawRectangle(brush, null,
            new Rect(0, 0, rtbDpi.Width, rtbDpi.Height));
        }

        rtbDpi.Render(drawVisual);

        enc.Frames.Add(BitmapFrame.Create(rtbDpi));
      }
      enc.Save(outStream);
      // Restore the Framework Element to it's previous state
      copyTarget.LayoutTransform = storedTransform;
      copyTarget.UpdateLayout();

      return outStream;
    }

    #endregion Public Methods

  }//ChartUtilities
}//Swordfish.Charts
