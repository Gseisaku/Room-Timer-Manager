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
            if (value is bool isChecked && isChecked)
            {
                // If the button is checked, return its parameter as the new value
                if (parameter != null)
                {
                    // Try to convert parameter to the target type (e.g., int or enum)
                    try
                    {
                        if (targetType.IsEnum)
                        {
                            return Enum.Parse(targetType, parameter.ToString());
                        }
                        return System.Convert.ChangeType(parameter, targetType);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            // If not checked, do nothing
            return Binding.DoNothing;
        }
    }
}