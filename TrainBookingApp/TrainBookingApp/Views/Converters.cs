using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

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

public class BooleanToCompartmentTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCompartmented)
        {
            return isCompartmented ? "COMPARTMENT" : "OPEN";
        }
        return "OPEN";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BooleanToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCompartmented)
        {
            return isCompartmented ? new SolidColorBrush(Color.FromRgb(40, 167, 69)) : new SolidColorBrush(Color.FromRgb(108, 117, 125));
        }
        return new SolidColorBrush(Color.FromRgb(108, 117, 125));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BooleanToHeaderBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isAdding)
        {
            return isAdding ? new SolidColorBrush(Color.FromRgb(40, 167, 69)) : new SolidColorBrush(Color.FromRgb(23, 162, 184));
        }
        return new SolidColorBrush(Color.FromRgb(23, 162, 184));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BooleanToFormTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isAdding)
        {
            return isAdding ? "Add New Coach Type" : "Edit Coach Type";
        }
        return "Edit Coach Type";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BooleanToSeatFormTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isAdding)
        {
            return isAdding ? "Add New Seat Type" : "Edit Seat Type";
        }
        return "Edit Seat Type";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BooleanToSaveBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isAdding)
        {
            return isAdding ? new SolidColorBrush(Color.FromRgb(40, 167, 69)) : new SolidColorBrush(Color.FromRgb(255, 193, 7));
        }
        return new SolidColorBrush(Color.FromRgb(255, 193, 7));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BooleanToSaveTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isAdding)
        {
            return isAdding ? "➕ Create" : "💾 Update";
        }
        return "💾 Update";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BerthLevelToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int berthLevel)
        {
            return berthLevel switch
            {
                1 => new SolidColorBrush(Color.FromRgb(76, 175, 80)),   // Green for Lower
                2 => new SolidColorBrush(Color.FromRgb(255, 152, 0)),   // Orange for Middle
                3 => new SolidColorBrush(Color.FromRgb(244, 67, 54)),   // Red for Upper
                _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))   // Gray for Regular
            };
        }
        return new SolidColorBrush(Color.FromRgb(158, 158, 158));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BerthLevelToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int berthLevel)
        {
            return berthLevel switch
            {
                1 => "Lower",
                2 => "Middle", 
                3 => "Upper",
                _ => "Regular"
            };
        }
        return "Regular";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}