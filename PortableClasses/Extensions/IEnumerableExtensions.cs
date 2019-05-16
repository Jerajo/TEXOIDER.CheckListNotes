using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PortableClasses.Interfaces;

namespace PortableClasses.Extensions
{
    public static class IEnumerableExtensions
    {
        public static void Update<T>(this List<T> list, T item) where T : IIdentity
        {
            try
            {
                if (list.Any(m => m.Id == item.Id))
                {
                    var task = list.First(m => m.Id == item.Id);
                    var index = list.IndexOf(task);
                    list[index] = item;
                }
            }
            catch (Exception) { throw; }
        }
    }
}
