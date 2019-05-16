using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PortableClasses.Extensions
{
    public static class ObservableCollectionExtenxions
    {
        public static void Refresh<T>(this ObservableCollection<T> value)
        {
            var tempArray = value.ToArray();
            value.Clear();
            value = default;
            value = new ObservableCollection<T>(tempArray);
        }
    }
}
