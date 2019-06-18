using System;
using Xamarin.Forms;
using System.Globalization;
using PortableClasses.Extensions;

namespace CheckListNotes.PageModels.Converters
{
    class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Convert(value);

        public Color Convert(object value)
        {
            if (value == null && string.IsNullOrEmpty(value?.ToString()))
                return Color.Transparent;
            var hexadecimalColor = value.ToString();
            return Color.FromHex(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => ConvertBack(value);

        public string ConvertBack(object value) => ((Color)value).ToHexString();
    }
}
