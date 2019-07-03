using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace CheckListNotes.PageModels.Converters
{
    class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int toConvert = (int?)value ?? -1;
            return toConvert.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string toConvert = (string)value ?? "0";
            if (toConvert == "-" || string.IsNullOrEmpty(toConvert)) toConvert = "0";
            bool sucess = int.TryParse(toConvert, out int resoult);
            return sucess ? resoult : -1;
        }
    }
}
