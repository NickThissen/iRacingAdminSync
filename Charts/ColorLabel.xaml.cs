// ****************************************************************************
// Copyright Swordfish Computing Australia 2006                              **
// http://www.swordfish.com.au/                                              **
//                                                                           **
// Filename: Swordfish\WinFX\Controls\ColorLabel.xaml.cs                     **
// Authored by: John Stewien of Swordfish Computing                          **
// Date: April 2006                                                          **
//                                                                           **
// - Change Log -                                                            **
//*****************************************************************************

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Swordfish.NET.Charts {
  /// <summary>
  /// Interaction logic for ColorLabel.xaml
  /// </summary>
  public partial class ColorLabel : UserControl, INotifyPropertyChanged {
    // ********************************************************************
    // Constructors
    // ********************************************************************
    #region Constructors

    /// <summary>
    /// Constructor. Initializes class fields.
    /// </summary>
    public ColorLabel() {
      InitializeComponent();
      this.Background = new SolidColorBrush(Colors.Transparent);
    }

    /// <summary>
    /// Initializes the text and color properties.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="color"></param>
    public ColorLabel(string text, Color color)
      : this() {
      Text = text;
      Color = color;
    }

    #endregion Constructors

    // ********************************************************************
    // DependencyProperties
    // ********************************************************************
    #region DependencyProperties

    // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TextProperty =
      DependencyProperty.Register("Text", typeof(string), typeof(ColorLabel), new UIPropertyMetadata("Blank", new PropertyChangedCallback(UpdateText)));

    // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ColorProperty =
      DependencyProperty.Register("Color", typeof(Color), typeof(ColorLabel), new UIPropertyMetadata(Colors.Violet, new PropertyChangedCallback(UpdateColor)));

    #endregion DependencyProperties

    // ********************************************************************
    // Properties
    // ********************************************************************
    #region Properties

    /// <summary>
    /// Gets/Sets the text of the color label
    /// </summary>
    public string Text {
      get {
        return (string)GetValue(TextProperty);
      }
      set {
        SetValue(TextProperty, value);
      }
    }

    /// <summary>
    /// Gets/Sets the color to be displayed
    /// </summary>
    public Color Color {
      get {
        return (Color)GetValue(ColorProperty);
      }
      set {
        SetValue(ColorProperty, value);
      }
    }

    /// <summary>
    /// Gets/Sets the flag to highlight the Labels background
    /// </summary>
    private bool highlighted = false;
    public bool IsHighlighted {
      get { return highlighted; }
      set { if(highlighted != value) { highlighted = value; OnPropertyChanged("IsHighlighted"); } }
    }

    #endregion Properties

    // ********************************************************************
    // Event Handlers
    // ********************************************************************
    #region Event Handlers

    /// <summary>
    /// Handles when the Color property is changed
    /// </summary>
    /// <param name="dependency"></param>
    /// <param name="e"></param>
    protected static void UpdateColor(DependencyObject dependency, DependencyPropertyChangedEventArgs e) {
      ColorLabel colorLabel = (ColorLabel)dependency;
      Color newColor = (Color)e.NewValue;
      colorLabel.color.Fill = new SolidColorBrush(newColor);
    }

    /// <summary>
    /// Handles when the Text property is changed
    /// </summary>
    /// <param name="dependency"></param>
    /// <param name="e"></param>
    protected static void UpdateText(DependencyObject dependency, DependencyPropertyChangedEventArgs e) {
      ColorLabel colorLabel = (ColorLabel)dependency;
      colorLabel.textBlock.Text = (string)e.NewValue;
      colorLabel.textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
      colorLabel.color.Width = colorLabel.textBlock.DesiredSize.Height;
    }

    #endregion Event Handlers

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string name) {
      if(PropertyChanged != null) {
        PropertyChanged(this, new PropertyChangedEventArgs(name));
      }
    }

    #endregion
  }
}