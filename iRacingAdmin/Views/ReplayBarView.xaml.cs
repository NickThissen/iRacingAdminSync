using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using iRacingAdmin.Models.ViewModels;

namespace iRacingAdmin.Views
{
    /// <summary>
    /// Interaction logic for ReplayBarView.xaml
    /// </summary>
    public partial class ReplayBarView : UserControl
    {
        public ReplayBarView()
        {
            InitializeComponent();
        }

        private ReplayBarModel Model
        {
            get { return this.DataContext as ReplayBarModel; }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            var x = e.GetPosition(progress).X;
            var percentage = x/progress.ActualWidth;

            this.Model.MoveToReplayPercentage(percentage);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (this.Model == null) return;

            this.Model.BarLength = progress.ActualWidth;
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void btnLive_Click(object sender, RoutedEventArgs e)
        {
            if (this.Model == null) return;
            this.Model.MoveToLive();
        }
    }
}
