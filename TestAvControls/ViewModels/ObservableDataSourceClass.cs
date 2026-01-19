using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;

namespace TestAvControls.ViewModels
{
    public class ObservableDataSourceClass:AvaloniaObject,ICollection
    {
        public static readonly StyledProperty<ICollection> DataSourceProperty = AvaloniaProperty.Register<ObservableDataSourceClass, ICollection>(nameof(DataSource));

        public ICollection DataSource
        {
            get => GetValue(DataSourceProperty);
            set => SetValue(DataSourceProperty, value);
        }

        private List<object> list = new();

        public IEnumerator GetEnumerator()
        {
            return DataSource?.GetEnumerator() ?? list.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            DataSource.CopyTo(array, index);
        }

        public int Count => DataSource?.Count ?? list.Count;

        public bool IsSynchronized =>DataSource?.IsSynchronized??false;

        public object SyncRoot => DataSource.SyncRoot;
    }
}
