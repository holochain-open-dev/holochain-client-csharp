using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace NextGenSoftware.Holochain.HoloNET.Manager.Convertors
{
    [ValueConversion(typeof(DateTime), typeof(String))]
    public class DateTimeConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is DateTime && (DateTime)value < new DateTime(2, 1, 1))
            {
                return "";
            }
            else
                return value;

            //var jSONDate = parameter as string;
            //if (!string.IsNullOrEmpty(jSONDate))
            //{
            //    DateTime dt;
            //    if DateTime.TryParse(jSONDate, out dt)
            //    {
            //        return dt;
            //    }
            //}
            //// If didn't pass in string or TryParse failed return back empty datetime
            //return new DateTime();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        // Not needed just nice
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
