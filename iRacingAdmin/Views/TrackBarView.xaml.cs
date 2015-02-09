using System.Windows;
using System.Windows.Controls;
using iRacingAdmin.Models.ViewModels;

namespace iRacingAdmin.Views
{
    /// <summary>
    /// Interaction logic for TrackBarView.xaml
    /// </summary>
    public partial class TrackBarView : UserControl
    {
        public TrackBarView()
        {
            InitializeComponent();
        }

        private TrackBarModel Model
        {
            get { return this.DataContext as TrackBarModel; }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (this.Model == null) return;

            this.Model.LineWidth = (float)this.ActualWidth - 10f;
            this.Model.TrackWidth = (float)this.ActualWidth;
            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}
