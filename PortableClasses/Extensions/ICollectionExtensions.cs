using System.Collections.Generic;

namespace PortableClasses.Extensions
{
    public static class ICollectionExtensions
    {
        public static int IndexOf<T>(this ICollection<T> source, T value)
        {
            int index = 0;
            var comparer = EqualityComparer<T>.Default; // or pass in as a parameter
            foreach (T item in source)
            {
                if (comparer.Equals(item, value)) return index;
                index++;
            }
            return -1;
        }
    }
}
