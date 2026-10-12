namespace Siniskalc.Wpf.Views
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Media;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Colours and converters used by the hex editor and the hex viewer. The colours are soft enough to read dark text on.
    /// </summary>
    internal static class HexPalette
    {
        public static readonly IValueConverter KindToBrush = new KindToBrushConverter();

        public static readonly IValueConverter TextOrNull = new TextOrNullConverter();

        public static readonly Brush ChangedForeground = Freeze(Color.FromRgb(0x9C, 0x2B, 0x00));

        public static readonly Brush ChangedLine = Freeze(Color.FromRgb(0xE8, 0x59, 0x0C));

        public static readonly Brush SelectedBackground = Freeze(Color.FromRgb(0xBB, 0xD7, 0xF5));

        private static readonly Dictionary<ByteRegionKind, (string Label, Color Light, Color Dark)> Kinds = new()
        {
            [ByteRegionKind.Identity] = ("Identity", Color.FromRgb(0xCF, 0xE3, 0xFA), Color.FromRgb(0xB5, 0xD2, 0xF2)),
            [ByteRegionKind.Position] = ("Position", Color.FromRgb(0xD5, 0xED, 0xD3), Color.FromRgb(0xBB, 0xE0, 0xB8)),
            [ByteRegionKind.Movement] = ("Movement", Color.FromRgb(0xCB, 0xEC, 0xE8), Color.FromRgb(0xAF, 0xE0, 0xDA)),
            [ByteRegionKind.Stats] = ("Stats", Color.FromRgb(0xFB, 0xDF, 0xC1), Color.FromRgb(0xF5, 0xCB, 0x9C)),
            [ByteRegionKind.Creatures] = ("Creatures", Color.FromRgb(0xF8, 0xD3, 0xD3), Color.FromRgb(0xEF, 0xB8, 0xB8)),
            [ByteRegionKind.Skills] = ("Skills", Color.FromRgb(0xE3, 0xD7, 0xF4), Color.FromRgb(0xCE, 0xBC, 0xEA)),
            [ByteRegionKind.Spells] = ("Spells", Color.FromRgb(0xD6, 0xDA, 0xF6), Color.FromRgb(0xBA, 0xC1, 0xEE)),
            [ByteRegionKind.Artifacts] = ("Artifacts", Color.FromRgb(0xF7, 0xED, 0xB8), Color.FromRgb(0xEE, 0xDE, 0x92)),
            [ByteRegionKind.WarMachines] = ("War machines", Color.FromRgb(0xE3, 0xD9, 0xCF), Color.FromRgb(0xD0, 0xC1, 0xB1)),
            [ByteRegionKind.Resources] = ("Resources", Color.FromRgb(0xE0, 0xEF, 0xB4), Color.FromRgb(0xCD, 0xE2, 0x93)),
            [ByteRegionKind.Header] = ("Header", Color.FromRgb(0xEA, 0xD5, 0xDD), Color.FromRgb(0xDD, 0xBF, 0xCB)),
            [ByteRegionKind.Game] = ("Game", Color.FromRgb(0xC8, 0xEE, 0xDC), Color.FromRgb(0xAB, 0xE2, 0xC6)),
            [ByteRegionKind.Other] = ("Other", Color.FromRgb(0xE4, 0xE6, 0xEA), Color.FromRgb(0xD0, 0xD3, 0xDA)),
            [ByteRegionKind.Custom] = ("Custom", Color.FromRgb(0xE9, 0xE2, 0xF2), Color.FromRgb(0xD8, 0xCC, 0xE8)),
        };

        private static readonly Dictionary<string, Brush> BrushesByKey = Kinds
            .SelectMany(k => new[] { (Key: $"{k.Key}0", Color: k.Value.Light), (Key: $"{k.Key}1", Color: k.Value.Dark) })
            .ToDictionary(k => k.Key, k => Freeze(k.Color));

        /// <summary>
        /// Gets the brush for a region kind. <paramref name="shade"/> is 0 or 1; neighbouring regions alternate.
        /// </summary>
        public static Brush GetBrush(ByteRegionKind kind, int shade) => BrushesByKey[$"{kind}{shade}"];

        public static string GetLabel(ByteRegionKind kind) => Kinds[kind].Label;

        private static Brush Freeze(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        private sealed class KindToBrushConverter : IValueConverter
        {
            public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value is string key && BrushesByKey.TryGetValue(key, out var brush) ? brush : null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        private sealed class TextOrNullConverter : IValueConverter
        {
            public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value is string text && text.Length > 0 ? text : null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }
    }
}
