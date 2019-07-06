using System;
using Xamarin.Forms;
using System.Globalization;

namespace CheckListNotes.PageModels.Converters
{
    class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (!string.IsNullOrEmpty((string)value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
