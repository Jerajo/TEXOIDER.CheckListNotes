using PortableClasses.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace CheckListNotes.PageModels.Converters
{
    public class StringToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Convert(value);

        public Thickness Convert(object value)
        {
            if (value == null && string.IsNullOrEmpty(value?.ToString()))
                return new Thickness();
            bool susess = false;
            var cordenates = value.ToString();
            if (cordenates.Contains(","))
            {
                List<double> sizes = new List<double>();
                foreach (var item in cordenates.Split(','))
                {
                    susess = double.TryParse(item, out double size);
                    if (!susess) break;
                    else sizes.Add(size);
                }
                switch (sizes.Count)
                {
                    case 4: // "5.10.30.0" | A.B.C.D => [ l(5), t(10), r(30), b(0) ]
                        return new Thickness(sizes[0], sizes[1], sizes[2], sizes[3]);
                    case 3: // "5.10.30" | A.BD.C => [ l(5), t(10), r(30), b(10) ]
                        return new Thickness(sizes[0], sizes[1], sizes[2], sizes[1]);
                    case 2: // "5.10" | AC.BD => [ l(5), t(10), r(5), b(10) ]
                        return new Thickness(sizes[0], sizes[1], sizes[0], sizes[1]);
                    case 1: // "5" | ABCD => [ l(5), t(5), r(5), b(5) ]
                        return new Thickness(sizes[0]);
                    default:
                        return new Thickness();
                }
            }
            else
            {
                susess = double.TryParse(cordenates, out double size);
                return susess ? new Thickness(size) : new Thickness();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => ConvertBack(value);

        public string ConvertBack(object value)
        {
            var corner = (Thickness)value;

            string cordenates = "";
            cordenates += $"{corner.Left},";
            cordenates += $"{corner.Top},";
            cordenates += $"{corner.Right},";
            cordenates += $"{corner.Bottom}";

            return cordenates;
        }
    }
}
