using System;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.Controls;

namespace iRacingAdmin.Views
{
    public abstract class WindowBase : MetroWindow
    {
        private bool _shown;

        protected WindowBase()
        {
            if (this != App.Instance.MainWindow)
            {
                try
                {
                    this.Owner = App.Instance.MainWindow;
                    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                catch (Exception)
                {
                }

            }

            //var lightBlue = (SolidColorBrush)FindResource("AccentColorBrush1");
            var darkBlue = (SolidColorBrush)FindResource("DarkAccentColorBrush");

            this.Style = (Style) FindResource("CleanWindowStyleKey");
            this.GlowBrush = darkBlue;
            this.NonActiveGlowBrush = null;
            this.BorderBrush = darkBlue;
            this.BorderThickness = new Thickness(2);
            this.NonActiveBorderBrush = darkBlue;
            this.NonActiveGlowBrush = darkBlue;
            this.WindowTransitionsEnabled = false;
            this.FontSize = (double) new FontSizeConverter().ConvertFrom("10pt");
        }
        
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (_shown)
                return;

            _shown = true;
            this.OnShown();
        }

        public virtual void OnShown() { }
    }
}
