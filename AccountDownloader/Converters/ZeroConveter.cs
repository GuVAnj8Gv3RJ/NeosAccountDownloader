using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AccountDownloader.Converters
{
    // Due to MVVM, its kinda not possible to make "Ifs" in XAML,
    // So the only way to basically say if x >= 0 x : "Pending" is this conveter OR making a bunch of properties on the classes and hooking them up.
    // This seemed nicer at the time.
    // https://docs.avaloniaui.net/docs/data-binding/converting-binding-values

    // The parameter for this converter is the replacement text, which allows us to do smarter replacements if we need it.
    // <Binding Path="Status.DownloadedMessageCount" Converter="{StaticResource PendingConverter}" ConverterParameter="{x:Static p:Resources.Pending}" />
    // Will result in "Pending" when downloaded message count is 0
    // <Binding Path="Status.DownloadedMessageCount" Converter="{StaticResource PendingConverter}" ConverterParameter="..." />
    // Will result in "..." when downloaded message count is 0
    public class ZeroConverter: IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is not string)
                return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

            if (value is float v)
            {
                return v > 0 ? v.ToString("0.##") : parameter;
            }

            if (value is int vi)
            {
                return vi > 0 ? vi.ToString() : parameter;
            }

            if (value is double vd)
            {
                return vd > 0 ? vd.ToString("0.##") : parameter;
            }

            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
