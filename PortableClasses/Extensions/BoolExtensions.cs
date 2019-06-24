using System;
using System.Threading.Tasks;

namespace PortableClasses.Extensions
{
    public static class BoolExtensions
    {
        public static Task<bool> ToggleOn(this bool value, double time) => 
            ToggleOn(value, TimeSpan.FromMilliseconds(time));

        public static async Task<bool> ToggleOn(this bool value, TimeSpan time)
        {
            await Task.Delay(time);
            return !value;
        }
    }
}
