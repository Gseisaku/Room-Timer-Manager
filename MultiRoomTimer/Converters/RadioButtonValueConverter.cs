using System;
using System.Globalization;
using System.Windows.Data;

namespace MultiRoomTimer.Converters
{
    public class RadioButtonValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            // Check if the current value matches the button's parameter
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter != null)
            {
                var paramString = parameter.ToString();
                var underlyingType = Nullable.GetUnderlyingType(targetType);

                try
                {
                    if (underlyingType != null && underlyingType.IsEnum)
                    {
                        // Handle Nullable<Enum>
                        return Enum.Parse(underlyingType, paramString);
                    }
                    else if (targetType.IsEnum)
                    {
                        // Handle regular Enum
                        return Enum.Parse(targetType, paramString);
                    }
                    else
                    {
                        // Handle other types like int
                        return System.Convert.ChangeType(parameter, targetType);
                    }
                }
                catch
                {
                    return Binding.DoNothing;
                }
            }
            return Binding.DoNothing;
        }
    }
}