using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace PortableClasses.Extensions
{
    public static class ColorExtensions
    {
        public static string ToHexString(this Xamarin.Forms.Color color)
        {
            var red = (int)(color.R * 255);
            var green = (int)(color.G * 255);
            var blue = (int)(color.B * 255);
            var alpha = (int)(color.A * 255);

            return String.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", alpha, red, green, blue);
        }
    }
}
