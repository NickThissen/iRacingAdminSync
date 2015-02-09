// ****************************************************************************
// Copyright Swordfish Computing Australia 2006                              **
// http://www.swordfish.com.au/                                              **
//                                                                           **
// Filename: Swordfish.NET\Charts\ChartControl.xaml.cs                       **
// Authored by: John Stewien of Swordfish Computing                          **
// Date: April 2006                                                          **
//                                                                           **
// - Change Log -                                                            **
//*****************************************************************************

using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Collections;
using System.Windows.Documents;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Swordfish.NET.Charts {
  public class ChartControl : DockPanel {
    // ********************************************************************
    // Private Fields
    // ********************************************************************
    #region Private Fields

    /// <summary>
    /// A list of lines to draw on the chart
    /// </summary>
    protected List<ChartPrimitive> _primitiveList;

    // internal settings

    protected MatrixTransform _shapeTransform;
    protected Point _minPoint;
    protected Point _maxPoint;

    // Helper classes

    private PanZoomCalculator _panZoomCalculator;

    private Cursor defaultCursor = Cursors.Cross;

    protected PlotRenderer _plotRenderer;

    protected Grid _host;
    protected TextBlock _titleBox;
    protected UniformWrapPanel _legendBox;
    protected Rectangle _xTitleBar;
    protected Rectangle _yTitleBar;
    protected Rectangle _xGridLineLabels;
    protected Rectangle _yGridLineLabels;
    protected GridLineCanvas _gridLineCanvas;
    protected Image _imageHost;
    protected CursorCoordinateCanvas _cursorCoordinateCanvas;
    protected ListView _subNotes;
    protected Slider _xSlider;

    #endregion Private Fields

    // ********************************************************************
    // Public Methods
    // ********************************************************************
    #region Public Methods

    /// <summary>
    /// Constructor. Initializes all the class fields.
    /// </summary>
    public ChartControl() {

      this.LastChildFill = true;
      this.IsHitTestVisible = true;
      this.Background = new SolidColorBrush(Color.FromArgb(1,255,255,255));

      AddChildControls();

      Oversize = 0.01;

      _primitiveList = new List<ChartPrimitive>();
      _shapeTransform = new MatrixTransform();

      _cursorCoordinateCanvas.PrimitiveList = _primitiveList;
      _cursorCoordinateCanvas.PrimitiveTransform = _shapeTransform;

      _plotRenderer = new PlotRenderer();
      _plotRenderer.PrimitiveList = _primitiveList;
      _plotRenderer.PrimitiveTransform = _shapeTransform;

      _gridLineCanvas.XGridLineLabelBar = _xGridLineLabels;
      _gridLineCanvas.YGridLineLabelBar = _yGridLineLabels;

      _gridLineCanvas.XAxisTitleBar = _xTitleBar;
      _gridLineCanvas.YAxisTitleBar = _yTitleBar;

      _panZoomCalculator = new PanZoomCalculator(CanvasRect);
      _panZoomCalculator.Window = CanvasRect;
      _panZoomCalculator.PanZoomChanged += new EventHandler<PanZoomArgs>(panZoomCalculator_PanZoomChanged);

      this.Cursor = defaultCursor;

      // Set up all the message handlers for the clipped plot canvas
      AttachEventsToCanvas(this);

      _gridLineCanvas.SizeChanged += new SizeChangedEventHandler(canvas_SizeChanged);

      YAxisTitle = "Y Axis Label";

      _imageHost.Source = _plotRenderer.ImageSource;

      this.PlotResized += ChartControl_PlotResized;

    }

    public ChartPrimitiveEventLine AddNewEventLine() {
      ChartPrimitiveEventLine primitive = CreateEventLine();
      _primitiveList.Add(primitive);
      return primitive;
    }

    public ChartPrimitiveHBar AddNewHBar(double centerPoint, double height) {
      ChartPrimitiveHBar primitive = CreateHBar(centerPoint, height);
      _primitiveList.Add(primitive);
      return primitive;
    }

    public ChartPrimitiveXY AddNewXY() {
      ChartPrimitiveXY primitive = CreateXY();
      _primitiveList.Add(primitive);
      return primitive;
    }

    public ChartPrimitiveLineSegments AddNewLineSegments() {
      ChartPrimitiveLineSegments primitve = CreateLineSegments();
      _primitiveList.Add(primitve);
      return primitve;
    }

    public virtual ChartPrimitiveEventLine CreateEventLine() {
      return new ChartPrimitiveEventLine();
    }

    public virtual ChartPrimitiveHBar CreateHBar(double centerPoint, double height) {
      return new ChartPrimitiveHBar(centerPoint, height);
    }

    public virtual ChartPrimitiveXY CreateXY() {
      return new ChartPrimitiveXY();
    }

    public virtual ChartPrimitiveLineSegments CreateLineSegments() {
      return new ChartPrimitiveLineSegments();
    }

    public void Reset() {
      _primitiveList.Clear();
      _cursorCoordinateCanvas.PointToText.Clear();
      GridLineOverrides.Clear();
      YAxisTitles.Clear();
      XAxisTitles.Clear();
      Title = "";
    }

    public void AddPrimitive(ChartPrimitive primitive) {
      if(!_primitiveList.Contains(primitive)) {
        _primitiveList.Add(primitive);
      }
    }

    public void AddCursorText(Point point, string text) {
      _cursorCoordinateCanvas.PointToText[point] = text;
    }

    /// <summary>
    /// Reset everything from the current collection of Chart Primitives
    /// </summary>
    public void RedrawPlotLines() {
      _cursorCoordinateCanvas.Reinitialize();
      _legendBox.Children.Clear();
      foreach(ChartPrimitive primitive in _primitiveList) {
        if(primitive.LegendColor != Colors.Transparent) {
          primitive.LegendLabel = new ColorLabel(primitive.Label, primitive.LegendColor);
          _legendBox.Children.Insert(0, primitive.LegendLabel);
        }
      }
      ResizePlot();
    }

    #endregion Public Methods

    // ********************************************************************
    // Protected Methods
    // ********************************************************************
    #region Protected Methods

    /// <summary>
    /// Attaches mouse handling events to the canvas passed in. The canvas passed in should be the top level canvas.
    /// </summary>
    /// <param name="eventCanvas"></param>
    protected void AttachEventsToCanvas(UIElement eventCanvas) {
      eventCanvas.LostMouseCapture += new MouseEventHandler(canvas_LostMouseCapture);
      eventCanvas.MouseMove += new System.Windows.Input.MouseEventHandler(canvas_MouseMove);
      eventCanvas.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(canvas_MouseLeftButtonDown);
      eventCanvas.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(canvas_MouseLeftButtonUp);
      eventCanvas.MouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(canvas_MouseRightButtonDown);
      eventCanvas.MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(canvas_MouseRightButtonUp);
      eventCanvas.MouseEnter += new MouseEventHandler(canvas_MouseEnter);
      eventCanvas.MouseLeave += new MouseEventHandler(canvas_MouseLeave);
      eventCanvas.MouseWheel += canvas_MouseWheel;
    }

    /// <summary>
    /// Resize the plot by changing the transform and drawing the grid lines
    /// </summary>
    protected void ResizePlot() {
      // Don't need to re-render the plot lines, just change the transform
      SetChartTransform();
      UpdateSlider();
      OnPlotResized();
    }

    protected void UpdateSlider() {
      double currentSliderPos = _xSlider.Value;
      _xSlider.Minimum = PrimitiveDataRange.Left;
      _xSlider.Maximum = PrimitiveDataRange.Right;
      _xSlider.Value = currentSliderPos;
      Rect transformedRect = _shapeTransform.TransformBounds(PrimitiveDataRange);

      double leftMargin = CanvasRect.Left + transformedRect.Left -5;
      double rightMargin = CanvasRect.Right - transformedRect.Right - 5;
      _xSlider.Margin = new Thickness(leftMargin, 0, rightMargin, 0);
      //_xSlider.Clip = new RectangleGeometry(new Rect(new Point(-leftMargin,0), CanvasRect.Size));
      _xSlider.SmallChange = _gridLineCanvas.LastSpacingX * 0.1;
      _xSlider.LargeChange = _gridLineCanvas.LastSpacingX;
    }

    private void ChartControl_PlotResized(object sender, EventArgs e) {
      _gridLineCanvas.MinPoint = _minPoint;
      _gridLineCanvas.MaxPoint = _maxPoint;
      _plotRenderer.ChartDataRange = ChartDataRange;
      _plotRenderer.InvalidateVisual(CanvasRect);

      // Redraw the grid lines
      _gridLineCanvas.InvalidateVisual();
    }

    /// <summary>
    /// Set the chart transform
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    protected void SetChartTransform() {
      double width = CanvasRect.Width;
      double height = CanvasRect.Height;

      // Calculate the bottom left, and top right coordinates of the plot area, and
      // calculate the geometry transform matrix

      Rect plotArea = ChartUtilities.GetPlotRectangle(_primitiveList, GridLineOverrides, Oversize);
      PrimitiveDataRange = plotArea;

      _minPoint = plotArea.Location;
      _minPoint.Offset(-plotArea.Width * _panZoomCalculator.Pan.X, plotArea.Height * _panZoomCalculator.Pan.Y);
      _minPoint.Offset(0.5 * plotArea.Width * (1 - 1 / _panZoomCalculator.Zoom.X), 0.5 * plotArea.Height * (1 - 1 / _panZoomCalculator.Zoom.Y));

      _maxPoint = _minPoint;
      _maxPoint.Offset(plotArea.Width / _panZoomCalculator.Zoom.X, plotArea.Height / _panZoomCalculator.Zoom.Y);

      Point plotScale = new Point();
      plotScale.X = (width / plotArea.Width) * _panZoomCalculator.Zoom.X;
      if(plotScale.X < 1e-7)
        plotScale.X = 1.0;
      plotScale.Y = (height / plotArea.Height) * _panZoomCalculator.Zoom.Y;
      if(plotScale.Y < 1e-7)
        plotScale.Y = 1.0;

      Matrix shapeMatrix = Matrix.Identity;
      shapeMatrix.Translate(-_minPoint.X, -_minPoint.Y);
      shapeMatrix.Scale(plotScale.X, plotScale.Y);
      _shapeTransform.Matrix = shapeMatrix;
    }


    private void AddChildControls() {

      this.ClipToBounds = true;

      //RadialGradientBrush radialBrush = new RadialGradientBrush(new GradientStopCollection(new GradientStop[]{
      //  new GradientStop(Colors.White, 0.59999999999999942),
      //  new GradientStop(Colors.Transparent, 1)}));

      Transform invertTransform = new ScaleTransform(1, -1);

      // Create the Grid
      _host = new Grid();
      _host.IsHitTestVisible = true;

      // Create the columns
      ColumnDefinition columnDef1 = new ColumnDefinition();
      ColumnDefinition columnDef2 = new ColumnDefinition();
      ColumnDefinition columnDef3 = new ColumnDefinition();
      columnDef1.Width = new GridLength(1, GridUnitType.Auto);
      columnDef2.Width = new GridLength(1, GridUnitType.Auto);
      columnDef3.Width = new GridLength(1, GridUnitType.Star);
      _host.ColumnDefinitions.Add(columnDef1);
      _host.ColumnDefinitions.Add(columnDef2);
      _host.ColumnDefinitions.Add(columnDef3);

      // Create the rows

      RowDefinition rowDef1 = new RowDefinition();
      RowDefinition rowDef2 = new RowDefinition();
      RowDefinition rowDef3 = new RowDefinition();
      RowDefinition rowDef4 = new RowDefinition();
      RowDefinition rowDef5 = new RowDefinition();
      RowDefinition rowDef6 = new RowDefinition();
      RowDefinition rowDef7 = new RowDefinition();
      rowDef1.Height = new GridLength(1, GridUnitType.Auto);
      rowDef2.Height = new GridLength(1, GridUnitType.Auto);
      rowDef3.Height = new GridLength(1, GridUnitType.Star);
      rowDef4.Height = new GridLength(1, GridUnitType.Auto);
      rowDef5.Height = new GridLength(1, GridUnitType.Auto);
      rowDef6.Height = new GridLength(1, GridUnitType.Auto);
      rowDef7.Height = new GridLength(1, GridUnitType.Auto);
      _host.RowDefinitions.Add(rowDef1);
      _host.RowDefinitions.Add(rowDef2);
      _host.RowDefinitions.Add(rowDef3);
      _host.RowDefinitions.Add(rowDef4);
      _host.RowDefinitions.Add(rowDef5);
      _host.RowDefinitions.Add(rowDef6);
      _host.RowDefinitions.Add(rowDef7);

      _xSlider = new Slider() {
        Margin = new Thickness(-5, 0, -5, 0),
        IsHitTestVisible = true,
        Visibility = System.Windows.Visibility.Collapsed
      };

      Grid.SetColumn(_xSlider, 2);
      Grid.SetRow(_xSlider, 5);
      _host.Children.Add(_xSlider);

      _titleBox = new TextBlock()
      {
          Margin = new Thickness(0),
          Text = "Title",
          HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
          VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
          TextAlignment = TextAlignment.Center,
          //Background = radialBrush,
          IsHitTestVisible = false,
          Visibility = Visibility.Collapsed
      };
      Grid.SetColumn(_titleBox, 2);
      Grid.SetRow(_titleBox, 0);
      _host.Children.Add(_titleBox);

      _legendBox = new UniformWrapPanel() {
        Margin = new Thickness(5),
        //Background = new SolidColorBrush(Color.FromArgb(0x2F, 0, 0, 0)),
        IsHitTestVisible = false
      };
      Grid.SetColumn(_legendBox, 0);
      Grid.SetRow(_legendBox, 1);
      Grid.SetColumnSpan(_legendBox, 3);
      _host.Children.Add(_legendBox);

      _xTitleBar = new Rectangle() {
        Margin = new Thickness(0),
        //Fill = radialBrush,
        IsHitTestVisible = false
      };
      Grid.SetColumn(_xTitleBar, 0);
      Grid.SetRow(_xTitleBar, 4);
      Grid.SetColumnSpan(_xTitleBar, 3);
      _host.Children.Add(_xTitleBar);

      _yTitleBar = new Rectangle() {
        Margin = new Thickness(0),
        //Fill = radialBrush,
        IsHitTestVisible = false
      };
      Grid.SetColumn(_yTitleBar, 0);
      Grid.SetRow(_yTitleBar, 2);
      Grid.SetRowSpan(_yTitleBar, 3);
      _host.Children.Add(_yTitleBar);

      _xGridLineLabels = new Rectangle() {
        Margin = new Thickness(2, 0, 0, 2),
        Fill = Brushes.Transparent, // new SolidColorBrush(Color.FromArgb(127, 255, 255, 255)),
        IsHitTestVisible = false
      };
      Grid.SetColumn(_xGridLineLabels, 1);
      Grid.SetRow(_xGridLineLabels, 3);
      Grid.SetColumnSpan(_xGridLineLabels, 2);
      _host.Children.Add(_xGridLineLabels);

      _yGridLineLabels = new Rectangle() {
        Margin = new Thickness(2, 0, 0, 2),
        Fill = Brushes.Transparent, //new SolidColorBrush(Color.FromArgb(127, 255, 255, 255)),
        IsHitTestVisible = false
      };
      Grid.SetColumn(_yGridLineLabels, 1);
      Grid.SetRow(_yGridLineLabels, 2);
      Grid.SetRowSpan(_yGridLineLabels, 2);
      _host.Children.Add(_yGridLineLabels);


        var gridLineBorder = new Border()
        {
            Margin = new Thickness(3),
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.LightGray,
            IsHitTestVisible = false
        };

      _gridLineCanvas = new GridLineCanvas() {
        Margin = new Thickness(0),
        IsHitTestVisible = false
      };
        gridLineBorder.Child = _gridLineCanvas;
      Grid.SetColumn(gridLineBorder, 2);
      Grid.SetRow(gridLineBorder, 2);
      _host.Children.Add(gridLineBorder);

      _imageHost = new Image() {
        Margin = new Thickness(0),
        Stretch = Stretch.None,
        RenderTransformOrigin = new Point(0.5, 0.5),
        RenderTransform = invertTransform,
        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
        VerticalAlignment = System.Windows.VerticalAlignment.Top,
        IsHitTestVisible = false
      };
      Grid.SetColumn(_imageHost, 2);
      Grid.SetRow(_imageHost, 2);
      _host.Children.Add(_imageHost);

      _cursorCoordinateCanvas = new CursorCoordinateCanvas() {
        Margin = new Thickness(0),
        RenderTransformOrigin = new Point(0.5, 0.5),
        RenderTransform = invertTransform,
        ClipToBounds = false,
        Background = new SolidColorBrush(Colors.Transparent),
        IsHitTestVisible = false
      };
      Grid.SetColumn(_cursorCoordinateCanvas, 2);
      Grid.SetRow(_cursorCoordinateCanvas, 2);
     // _host.Children.Add(_cursorCoordinateCanvas);

      _subNotes = new ListView() {
        FontSize = 10,
        Visibility = System.Windows.Visibility.Collapsed,
        Background = new SolidColorBrush(Colors.Transparent),
        BorderBrush = new SolidColorBrush(Colors.Transparent),
      };

      Border subNotesBorder = new Border() {
        Background = new SolidColorBrush(Color.FromArgb(0x20, 0, 0, 0)),
        BorderBrush = new SolidColorBrush(Colors.Transparent),
        BorderThickness = new Thickness(0),
        Padding = new Thickness(0),
        CornerRadius = new CornerRadius(8),
        IsHitTestVisible = false
      };
      Grid.SetColumn(subNotesBorder, 0);
      Grid.SetRow(subNotesBorder, 6);
      Grid.SetColumnSpan(subNotesBorder, 3);
      subNotesBorder.Child = _subNotes;
      _host.Children.Add(subNotesBorder);

      DockPanel.SetDock(_host, Dock.Left);
      this.Children.Add(_host);

    }

    #endregion Protected Methods

    // ********************************************************************
    // Properties
    // ********************************************************************
    #region Properties

    public Rect CanvasRect {
      get {
        return new Rect(0, 0, _gridLineCanvas.ActualWidth, _gridLineCanvas.ActualHeight);
      }
    }

    public double AxisFontSize {
      get {
        return _gridLineCanvas.FontSize;
      }
      set {
        _gridLineCanvas.FontSize = value;
        _gridLineCanvas.InvalidateVisual();
      }
    }

    public List<GridLineOverride> GridLineOverrides {
      get {
        return _gridLineCanvas.GridLineOverrides;
      }
    }

    public Func<double, string> XAxisLabelGenerator {
      get {
        return _gridLineCanvas.XAxisLabelGenerator;
      }
      set {
        _gridLineCanvas.XAxisLabelGenerator = value;
        _cursorCoordinateCanvas.XAxisLabelGenerator = value;
      }
    }

    public Func<double, string> YAxisLabelGenerator {
      get {
        return _gridLineCanvas.YAxisLabelGenerator;
      }
      set {
        _gridLineCanvas.YAxisLabelGenerator = value;
      }
    }

    public Func<double, double> GridLineSpacingX {
      get {
        return _gridLineCanvas.GridLineSpacingX;
      }
      set {
        _gridLineCanvas.GridLineSpacingX = value;
      }
    }

    public Func<double, double> GridLineSpacingY {
      get {
        return _gridLineCanvas.GridLineSpacingY;
      }
      set {
        _gridLineCanvas.GridLineSpacingY = value;
      }
    }

    /// <summary>
    /// Gets/Sets the title of the chart
    /// </summary>
    public string Title {
      get {
        return _titleBox.Text;
      }
      set {
        _titleBox.Text = value;
      }
    }

    /// <summary>
    /// Gets/Sets the X axis label
    /// </summary>
    public string XAxisTitle {
      get {
        GridLabel label = _gridLineCanvas.XAxisTitles.FirstOrDefault();
        return label != null ? label.Text : "";
      }
      set {
        _gridLineCanvas.XAxisTitles.Clear();
        _gridLineCanvas.XAxisTitles.Add(new GridLabel(value, Orientation.Horizontal));
      }
    }

    /// <summary>
    /// Gets/Sets the Y axis label
    /// </summary>
    public string YAxisTitle {
      get {
        GridLabel label = _gridLineCanvas.YAxisTitles.FirstOrDefault();
        return label != null ? label.Text : "";
      }
      set {
        _gridLineCanvas.YAxisTitles.Clear();
        _gridLineCanvas.YAxisTitles.Add(new GridLabel(value, Orientation.Vertical));
      }
    }

    public List<GridLabel> YAxisTitles {
      get {
        return _gridLineCanvas.YAxisTitles;
      }
    }

    public List<GridLabel> XAxisTitles {
      get {
        return _gridLineCanvas.XAxisTitles;
      }
    }

    /// <summary>
    /// Gets/Sets the subnotes to be printed at the bottom of the chart
    /// </summary>
    public IEnumerable SubNotes {
      get {
        return _subNotes.ItemsSource;
      }
      set {
        _subNotes.ItemsSource = value;
        _subNotes.Visibility = Visibility.Visible;
      }
    }

    /// <summary>
    /// Gets the collection of chart primitives
    /// </summary>
    public IEnumerable<ChartPrimitive> Primitives {
      get {
        return _primitiveList;
      }
    }

    /// <summary>
    /// The oversize of the plot area compared to it's content
    /// </summary>
    public double Oversize {
      get;
      set;
    }

    public bool DrawWholePlotCursor {
      get {
        return _cursorCoordinateCanvas.DrawWholePlotCursor;
      }
      set {
        _cursorCoordinateCanvas.DrawWholePlotCursor = value;
      }
    }

    public Rect ChartDataRange {
      get {
        return new Rect(_minPoint, _maxPoint);
      }
    }

    public Rect PrimitiveDataRange {
      get;
      private set;
    }

    public Slider SliderX {
      get {
        return _xSlider;
      }
    }

    #endregion Properties

    // ********************************************************************
    // Events and Triggers
    // ********************************************************************
    #region Events and Triggers

    protected void OnPlotResized() {
      if(PlotResized != null) {
        PlotResized(this, new EventArgs());
      }
    }

    public event EventHandler<EventArgs> PlotResized;

    protected void OnPointSelected(Point selectedPoint) {
      if(PointSelected != null) {
        PointSelected(this, new PointSelectedArgs(selectedPoint));
      }
    }

    public event EventHandler<PointSelectedArgs> PointSelected;

    #endregion Events and Triggers

    // ********************************************************************
    // Misc Event Handlers
    // ********************************************************************
    #region Misc Event Handlers

    /// <summary>
    /// Handles when the pan or zoom changes
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void panZoomCalculator_PanZoomChanged(object sender, PanZoomArgs e) {
      ResizePlot();
    }

    #endregion Misc Event Handlers

    // ********************************************************************
    // Canvas Event Handlers
    // ********************************************************************
    #region Canvas Event Handlers

    /// <summary>
    /// Handles when the plot size changes
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void canvas_SizeChanged(object sender, SizeChangedEventArgs e) {
      _panZoomCalculator.Window = CanvasRect;
      ResizePlot();
    }

    /// <summary>
    /// Handles when the plot loses focus
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void canvas_LostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e) {
      _panZoomCalculator.StopPanning();
      _panZoomCalculator.StopZooming();
      this.Cursor = defaultCursor;
    }

    /// <summary>
    /// Handles when the mouse moves over the plot
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
      Point mousePos = e.GetPosition(_cursorCoordinateCanvas);

      if(!_panZoomCalculator.IsPanning && !_panZoomCalculator.IsZooming) {
        _cursorCoordinateCanvas.MousePoint = mousePos;

      } else {
        _panZoomCalculator.MouseMoved(_cursorCoordinateCanvas.RenderTransform.Inverse.Transform(mousePos));
      }
    }

    /// <summary>
    /// Handles when the user clicks on the plot
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      if(e.ClickCount < 2) {
        if(((UIElement)sender).CaptureMouse()) {
          this.Cursor = Cursors.ScrollAll;
          _cursorCoordinateCanvas.Visibility = Visibility.Hidden;
          _panZoomCalculator.StartPan(_cursorCoordinateCanvas.RenderTransform.Inverse.Transform(e.GetPosition(_cursorCoordinateCanvas)));
        }
      } else {
        Point mousePos = e.GetPosition(_cursorCoordinateCanvas);
        Point selectedPoint = _cursorCoordinateCanvas.SelectedPoint(mousePos);
        _xSlider.Value = selectedPoint.X;
        OnPointSelected(selectedPoint);
      }
    }

    /// <summary>
    /// Handles when the user releases the mouse button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void canvas_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      _panZoomCalculator.StopPanning();
      if(!_panZoomCalculator.IsZooming) {
        Mouse.Capture(null);
        this.Cursor = defaultCursor;
        if(this.IsMouseOver) {
          _cursorCoordinateCanvas.Visibility = Visibility.Visible;
        }
      }
    }

    /// <summary>
    ///  Handles when the user clicks on the plot
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void canvas_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      if(e.ClickCount < 2) {
        if(((UIElement)sender).CaptureMouse()) {
          this.Cursor = Cursors.ScrollAll;
          _cursorCoordinateCanvas.Visibility = Visibility.Visible;
          _panZoomCalculator.StartZoom(_cursorCoordinateCanvas.RenderTransform.Inverse.Transform(e.GetPosition(_cursorCoordinateCanvas)));
        }
      } else {
        _panZoomCalculator.Reset();
      }
    }

    /// <summary>
    /// Handles when the user releases the mouse
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void canvas_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      _panZoomCalculator.StopZooming();
      if(!_panZoomCalculator.IsPanning) {
        Mouse.Capture(null);
        this.Cursor = defaultCursor;
      } else {
        _cursorCoordinateCanvas.Visibility = Visibility.Hidden;
        this.Cursor = Cursors.ScrollAll;
      }
    }

    /// <summary>
    /// Handles when the mouse leaves the plot. Removes the nearest point indicator.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void canvas_MouseLeave(object sender, MouseEventArgs e) {
      _cursorCoordinateCanvas.Locked = false;
      _cursorCoordinateCanvas.Visibility = Visibility.Hidden;
    }

    /// <summary>
    /// Handles when the user rolls the wheel
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void canvas_MouseWheel(object sender, MouseWheelEventArgs e) {
      if(!_panZoomCalculator.IsPanning && !_panZoomCalculator.IsZooming) {
        Point mousePos = e.GetPosition(_cursorCoordinateCanvas);
        _panZoomCalculator.StartZoom(_cursorCoordinateCanvas.RenderTransform.Inverse.Transform(mousePos));

        // Adjust for control size, otherwise small control zooms really slowly
        double delta = e.Delta * 200.0 / (this.ActualHeight > 0 ? this.ActualHeight : 1000.0);
        Point newMousePos = new Point(mousePos.X + delta, mousePos.Y + delta);

        _panZoomCalculator.MouseMoved(_cursorCoordinateCanvas.RenderTransform.Inverse.Transform(newMousePos));
        _panZoomCalculator.StopZooming();
      }
    }

    /// <summary>
    /// Handles when the mouse enters the plot. Puts back the nearest point indicator.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void canvas_MouseEnter(object sender, MouseEventArgs e) {
      _cursorCoordinateCanvas.InvalidateVisual();
      _cursorCoordinateCanvas.Visibility = Visibility.Visible;
    }

    #endregion _clippedPlotCanvas Event Handlers
  }
}