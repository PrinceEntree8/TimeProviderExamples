using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace TimeProviderExample.Avalonia.Converters;

/// <summary>
/// Bridges the shared view-model's <see cref="DateTime"/> date-only value
/// with Avalonia's <c>DatePicker.SelectedDate</c> (<see cref="DateTimeOffset"/>?).
/// The time-of-day component lives in the separate <c>SelectedTime</c> property,
/// which the view-model combines with the date when POSTing.
/// A cleared picker yields <see cref="BindingOperations.DoNothing"/> so the
/// non-nullable <c>DateTime</c> target is never assigned <c>null</c>.
/// </summary>
public sealed class DateTimeToDateTimeOffsetConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            DateTime dt => new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Local)),
            _ => null,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            DateTimeOffset dto => dto.LocalDateTime,
            _ => BindingOperations.DoNothing,
        };
    }
}
