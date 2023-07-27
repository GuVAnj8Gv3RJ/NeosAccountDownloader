using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using BaseX;

namespace AccountDownloader.Converters;

public class BytesConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return "0 B";

        var bytes = System.Convert.ToDouble(value);

        return UnitFormatting.FormatBytes(bytes) ?? Res.Error;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
