using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace PortableClasses.Extensions
{
    public static class DynamicExtensions
    {
        public static bool HasProperty(this object item, string name)
        {
            if (item is ExpandoObject)
                return ((IDictionary<string, object>)item).ContainsKey(name);

            return (item.GetType().GetProperty(name) != null);
        }

        public static bool HasAtribute(dynamic item, string name)
        {
            if (item is ExpandoObject)
                return ((IDictionary<string, object>)item).ContainsKey(name);

            return (item.GetType().GetProperty(name) != null);
        }
    }
}
