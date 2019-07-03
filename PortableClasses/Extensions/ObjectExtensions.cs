using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PortableClasses.Extensions
{
    public static class ObjectExtensions
    {
        public static T Clone<T>(this T value)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, value);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
    }
}
