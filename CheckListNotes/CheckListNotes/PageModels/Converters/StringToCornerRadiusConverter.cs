using System;
using Xamarin.Forms;
using System.Globalization;
using System.Collections.Generic;

namespace CheckListNotes.PageModels.Converters
{
    public class StringToCornerRadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Convert(value);

        public CornerRadius Convert(object value)
        {
            if (value == null && string.IsNullOrEmpty(value?.ToString()))
                return new CornerRadius();
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
                    case 4: // "5.10.30.0" | A.B.D.C => [ tl(5), tr(10), bl(30), br(0) ]
                        return new CornerRadius(sizes[0], sizes[1], sizes[2], sizes[3]);
                    case 3: // "5.10.30" | A.BC.D => [ tl(5), tr(10), bl(10), br(30) ]
                        return new CornerRadius(sizes[0], sizes[1], sizes[2], sizes[1]);
                    case 2: // "5.10" | AD.BC => [ tl(5), tr(10), bl(5), br(10) ]
                        return new CornerRadius(sizes[0], sizes[1], sizes[0], sizes[1]);
                    case 1: // "5" | ABCD => [ tl(5), tr(5), bl(5), br(5) ]
                        return new CornerRadius(sizes[0]);
                    default:
                        return new CornerRadius();
                }
            }
            else
            {
                susess = double.TryParse(cordenates, out double size);
                return susess ? new CornerRadius(size) : new CornerRadius();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => ConvertBack(value);

        public string ConvertBack(object value)
        {
            var corner = (CornerRadius)value;

            string cordenates = "";
            cordenates += $"{corner.TopLeft},";
            cordenates += $"{corner.TopRight},";
            cordenates += $"{corner.BottomLeft},";
            cordenates += $"{corner.BottomRight}";

            return cordenates;
        }
    }
}
