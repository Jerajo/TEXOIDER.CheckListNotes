using System;
using Xamarin.Forms;
using System.Globalization;

namespace CheckListNotes.PageModels.Converters
{
    class DoubleToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double temporalValue = 0, convertedValue = 0;
            bool resoult = double.TryParse(value.ToString(), out temporalValue);
            if (resoult && !double.IsNaN(temporalValue)) convertedValue = temporalValue;
            GridLength gridLength = new GridLength(convertedValue, GridUnitType.Star);

            return gridLength;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GridLength val = (GridLength)value;

            return val.Value;
        }
    }
}
