using System.Windows;
using System.Windows.Controls;
using iRacingAdmin.Models.Admins;

namespace iRacingAdmin.Selectors
{
    public class WatchedDriverColumnTemplateSelector : DataTemplateSelector
    {
        public DataTemplate WatchingTemplate { get; set; }
        public DataTemplate NotWatchingTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var user = item as UserContainer;
            if (user != null)
            {
                if (user.WatchedDriver == null) return this.NotWatchingTemplate;
                return this.WatchingTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }

    public class LiveColumnTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LiveTemplate { get; set; }
        public DataTemplate NotLiveTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var user = item as UserContainer;
            if (user != null)
            {
                if (user.IsLive) return this.LiveTemplate;
                return this.NotLiveTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
