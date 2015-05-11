using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ProcessTracker
{
    [ValueConversion(typeof(DateTime), typeof(string))]
    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is DateTime) 
            {
                DateTime date = (DateTime)value;
                string dateStr = date.ToShortDateString();
                return dateStr;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string) 
            {
                string dateStr = (string)value;
                DateTime datetime = System.Convert.ToDateTime(dateStr);
                return datetime;
            }
            return value;
        }
    }
}
