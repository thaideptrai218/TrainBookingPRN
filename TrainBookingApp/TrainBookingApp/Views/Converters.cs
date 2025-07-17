using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TrainBookingApp.Views;

/// <summary>
/// Value converter to invert boolean values
/// In Java, this would be similar to a custom PropertyEditor or Converter
/// Used for enabling/disabling controls based on inverse boolean states
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    /// <summary>
    /// Converts boolean to its inverse
    /// In Java, this would be similar to a convert() method in a Converter interface
    /// </summary>
    /// <param name="value">Boolean value to invert</param>
    /// <param name="targetType">Target type (not used)</param>
    /// <param name="parameter">Parameter (not used)</param>
    /// <param name="culture">Culture info (not used)</param>
    /// <returns>Inverted boolean value</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }

    /// <summary>
    /// Converts back - inverts the boolean again
    /// In Java, this would be similar to a convertBack() method
    /// </summary>
    /// <param name="value">Boolean value to invert</param>
    /// <param name="targetType">Target type (not used)</param>
    /// <param name="parameter">Parameter (not used)</param>
    /// <param name="culture">Culture info (not used)</param>
    /// <returns>Inverted boolean value</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}

/// <summary>
/// Value converter to convert string to Visibility
/// In Java, this would be similar to a custom PropertyEditor for visibility
/// Used to show/hide UI elements based on string content
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts string to Visibility enum
    /// In Java, this would be similar to a convert() method for component visibility
    /// </summary>
    /// <param name="value">String value to check</param>
    /// <param name="targetType">Target type (not used)</param>
    /// <param name="parameter">Parameter (not used)</param>
    /// <param name="culture">Culture info (not used)</param>
    /// <returns>Visibility.Visible if string has content, Visibility.Collapsed otherwise</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    /// <summary>
    /// Converts back - not implemented as it's not needed for this use case
    /// In Java, this would be similar to a convertBack() method
    /// </summary>
    /// <param name="value">Visibility value</param>
    /// <param name="targetType">Target type (not used)</param>
    /// <param name="parameter">Parameter (not used)</param>
    /// <param name="culture">Culture info (not used)</param>
    /// <returns>Not implemented</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ConvertBack is not implemented for StringToVisibilityConverter");
    }
}