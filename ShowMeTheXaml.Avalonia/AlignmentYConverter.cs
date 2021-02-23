using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;

// ReSharper disable once CheckNamespace
namespace ShowMeTheXaml {
    public class AlignmentYConverter : IValueConverter {
        public static AlignmentYConverter Instance { get; } = new AlignmentYConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is AlignmentY alignmentY) {
                return alignmentY switch {
                    AlignmentY.Top    => VerticalAlignment.Top,
                    AlignmentY.Center => VerticalAlignment.Center,
                    AlignmentY.Bottom => VerticalAlignment.Bottom,
                    _                 => throw new ArgumentOutOfRangeException()
                };
            }

            return BindingOperations.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}