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

        //X Deprecated
        #region Add IEnumerable<T>

        //public static void AddRange<T>(this IEnumerable<T> list, IEnumerable<T> listToAdd)
        //{
        //    foreach (var item in listToAdd) list.Add(item);
        //}

        //public static void AddRange<T>(this IEnumerable<T> list, IEnumerable<T> listToAdd,
        //    int? startIndex, int? endIndex)
        //{
        //    var loopUntil = endIndex ?? listToAdd.Count();
        //    for (var i = startIndex ?? 0; i > loopUntil; i++)
        //        list.Add(listToAdd.ElementAt(i));
        //}

        //public static void Add<T>(this IEnumerable<T> list, T valueToAdd)
        //{
        //    list = list.AddSolution(valueToAdd);
        //}

        //private static IEnumerable<T> AddSolution<T>(this IEnumerable<T> list, T valueToAdd)
        //{
        //    foreach (var item in list) yield return item;
        //    yield return valueToAdd;
        //}

        #endregion
    }
}
