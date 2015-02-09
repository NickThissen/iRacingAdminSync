using System.Collections.Generic;
using System.ComponentModel;

namespace iRacingAdmin.Models.Filtering
{ 
    public abstract class FilterManager<T>
    {
        private readonly List<Filter<T>> _filters;
        private ICollectionView _view;

        protected FilterManager()
        {
            _filters = new List<Filter<T>>();
        }

        public void Attach(ICollectionView view)
        {
            _view = view;
            _view.Filter += DoFilter;
        }

        public bool DoFilter(object obj)
        {
            var item = (T) obj;
            bool result = true;
            foreach (var filter in _filters)
            {
                result = result & filter.Execute(item);
            }
            return result;
        }

        public void AddFilter(Filter<T> filter)
        {
            filter.Manager = this;
            _filters.Add(filter);
        }

        public void Refresh()
        {
            if (_view != null) _view.Refresh();
        }
    }

    public abstract class Filter<T>
    {
        public FilterManager<T> Manager { get; set; }

        public abstract bool Execute(T item);

        private object _property;
        public object Property
        {
            get { return _property; }
            set
            {
                _property = value;
                this.Manager.Refresh();
            }
        }
    }
}
