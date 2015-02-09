using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace iRacingAdmin.Controls
{
    public abstract class PopupGridContainer : UserControl
    {
        private Popup _popup;
        private DataGrid _grid;

        protected PopupGridContainer()
        {
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _grid = this.GetGrid();
            _popup = this.GetPopup();
            _popup.CustomPopupPlacementCallback = CustomPopupPlacementCallback;

            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.LocationChanged += OnMovePopup;
                window.SizeChanged += OnMovePopup;
                window.MouseLeftButtonUp += Window_OnMouseLeftButtonUp;
                window.Deactivated += Window_OnDeactivated;
            }
        }

        public abstract Popup GetPopup();
        public abstract DataGrid GetGrid();
        public abstract object GetSelectedItem();
        public abstract void DeselectItem();

        private void OnMovePopup(object sender, EventArgs e)
        {
            _popup.HorizontalOffset += 1;
            _popup.HorizontalOffset -= 1;
        }

        private CustomPopupPlacement[] CustomPopupPlacementCallback(Size popupSize, Size targetSize, Point offset)
        {
            // Calculate position of popup relative to selected row
            var item = this.GetSelectedItem();
            if (item != null)
            {
                var row = (DataGridRow)_grid.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                {
                    var controlTopLeft = this.TransformToAncestor(this).Transform(new Point(0, 0));
                    var right = controlTopLeft.X + this.ActualWidth;
                    var x = right - popupSize.Width - 1;

                    var rowTopLeft = row.TransformToAncestor(this).Transform(new Point(0, 0));
                    var y = rowTopLeft.Y + row.ActualHeight - 1;

                    return new[] { new CustomPopupPlacement(new Point(x, y), PopupPrimaryAxis.Vertical) };
                }
            }
            return new[] { new CustomPopupPlacement(new Point(0, 0), PopupPrimaryAxis.None) };
        }

        private void Window_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_grid != null && _grid.SelectedItems != null && _grid.SelectedItems.Count == 1)
            {
                DataGridRow dgr = _grid.ItemContainerGenerator.ContainerFromItem(_grid.SelectedItem) as DataGridRow;
                if (!dgr.IsMouseOver)
                {
                    dgr.IsSelected = false;
                }
            }
        }

        private void Window_OnDeactivated(object sender, EventArgs e)
        {
            this.DeselectItem();
        }   
    }
}
