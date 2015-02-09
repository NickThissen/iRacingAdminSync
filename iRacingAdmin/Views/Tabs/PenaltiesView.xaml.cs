using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using iRacingAdmin.Classes.DragDrop;
using iRacingAdmin.Controls;
using iRacingAdmin.Models.ViewModels;

namespace iRacingAdmin.Views.Tabs
{
    /// <summary>
    /// Interaction logic for PenaltiesView.xaml
    /// </summary>
    public partial class PenaltiesView
    {
        public PenaltiesView()
        {
            InitializeComponent();

            DragDropHelper.Instance.DroppedDriver += Grid_OnDroppedDriver;
        }

        private void Grid_OnDroppedDriver(object sender, DragDropHelper.DroppedDriverEventArgs e)
        {
            var model = this.DataContext as PenaltyListModel;
            if (model != null)
            {
                model.AddDriver(e.Driver, e.Penalty);
            }
        }

        private void Grid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var model = this.DataContext as PenaltyListModel;
            topGrid.CancelEdit();
            if (model.SelectedPenalty != null) model.DecidePenaltyResult(model.SelectedPenalty);
        }

        private void BottomGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var model = this.DataContext as PenaltyListModel;
            topGrid.CancelEdit();
            if (model.SelectedDecidedPenalty != null)
            {
                var dialog = new SendCommandWindow(model.SelectedDecidedPenalty.Penalty.Result.Command);
                App.Instance.MainModel.ShowDialog(dialog);
            }
        }
    }
}
