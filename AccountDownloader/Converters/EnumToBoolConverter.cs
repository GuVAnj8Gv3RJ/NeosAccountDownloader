using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace AccountDownloader.Converters
{
    // https://github.com/AvaloniaUI/Avalonia/issues/3016#issuecomment-706492175
    public class EnumToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try {
                return value?.Equals(parameter);
            }
            catch
            {
                return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                return value?.Equals(true) == true ? parameter : BindingOperations.DoNothing;
            }
            catch
            {
                return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
            }
        }
    }
}
