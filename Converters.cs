using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Drawing;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Catsfract
{
    public class NullBooleanToBoolean : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => (bool)value;
        public object ConvertBack(object value, Type targetType, object parameter, string language) => (bool?)value;
    }

    public class BooleanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => (bool)value ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, string language) => (Visibility)value == Visibility.Visible;
    }

    public class BooleanToHorizontalOrientation : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => (bool)value ? Orientation.Horizontal : Orientation.Vertical;
        public object ConvertBack(object value, Type targetType, object parameter, string language) => (Orientation)value == Orientation.Horizontal;
    }
}
