using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TimeProviderExample.Avalonia.Converters;

/// <summary>
/// Negates a boolean, e.g. to show a collapsed-state glyph while
/// <c>IsSidebarOpen</c> is <c>false</c>.
/// </summary>
public sealed class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b ? !b : value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b ? !b : value;
    }
}
