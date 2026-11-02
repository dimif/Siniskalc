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
    /// Colours and converters used by the hex editor and the hex viewer. Each kind of field has a hue; two neighbouring fields of a kind
    /// alternate between a light and a deeper shade of it, far enough apart to tell at a glance and still light enough for dark text.
    /// </summary>
    internal static class HexPalette
    {
        /// <summary>
        /// Converts the kind of a region to the brush of its colour.
        /// </summary>
        public static readonly IValueConverter KindToBrush = new KindToBrushConverter();

        /// <summary>
        /// Converts a value to its text, or to nothing for an empty one.
        /// </summary>
        public static readonly IValueConverter TextOrNull = new TextOrNullConverter();

        /// <summary>
        /// The colour of the text of a byte that was edited.
        /// </summary>
        public static readonly Brush ChangedForeground = Freeze(Color.FromRgb(0x9C, 0x2B, 0x00));

        /// <summary>
        /// The colour of the line that marks an edited byte.
        /// </summary>
        public static readonly Brush ChangedLine = Freeze(Color.FromRgb(0xE8, 0x59, 0x0C));

        /// <summary>
        /// The background of the selected bytes: a strong blue that none of the kinds uses, with white text (<see cref="SelectedForeground"/>).
        /// </summary>
        public static readonly Brush SelectedBackground = Freeze(Color.FromRgb(0x1F, 0x5F, 0xBF));

        /// <summary>
        /// The colour of the text of the selected byte.
        /// </summary>
        public static readonly Brush SelectedForeground = Freeze(Color.FromRgb(0xFF, 0xFF, 0xFF));

        private static readonly Dictionary<ByteRegionKind, (string Label, Color Light, Color Dark)> Kinds = new()
        {
            [ByteRegionKind.Identity] = Kind("Identity", 210, 0.75),
            [ByteRegionKind.Position] = Kind("Position", 125, 0.65),
            [ByteRegionKind.Movement] = Kind("Movement", 175, 0.70),
            [ByteRegionKind.Stats] = Kind("Stats", 28, 0.90),
            [ByteRegionKind.Creatures] = Kind("Creatures", 0, 0.85),
            [ByteRegionKind.Skills] = Kind("Skills", 275, 0.70),
            [ByteRegionKind.Spells] = Kind("Spells", 240, 0.70),
            [ByteRegionKind.Artifacts] = Kind("Artifacts", 50, 0.90),
            [ByteRegionKind.WarMachines] = Kind("War machines", 25, 0.35),
            [ByteRegionKind.Resources] = Kind("Resources", 80, 0.75),
            [ByteRegionKind.Header] = Kind("Header", 330, 0.55),
            [ByteRegionKind.Game] = Kind("Game", 150, 0.65),
            [ByteRegionKind.Other] = Kind("Other", 215, 0.12),
            [ByteRegionKind.Custom] = Kind("Custom", 295, 0.55),
        };

        private static readonly Dictionary<string, Brush> BrushesByKey = Kinds
            .SelectMany(k => new[] { (Key: $"{k.Key}0", Color: k.Value.Light), (Key: $"{k.Key}1", Color: k.Value.Dark) })
            .ToDictionary(k => k.Key, k => Freeze(k.Color));

        /// <summary>
        /// Gets the brush for a region kind. <paramref name="shade"/> is 0 or 1; neighbouring regions alternate.
        /// </summary>
        /// <param name="kind">The kind of region.</param>
        /// <returns>The brush.</returns>
        /// <param name="shade">0 or 1; neighbouring regions alternate.</param>
        public static Brush GetBrush(ByteRegionKind kind, int shade) => BrushesByKey[$"{kind}{shade}"];

        /// <summary>
        /// Returns the label the legend shows for a kind of region.
        /// </summary>
        /// <param name="kind">The kind of region.</param>
        /// <returns>The label.</returns>
        public static string GetLabel(ByteRegionKind kind) => Kinds[kind].Label;

        // The two shades of a hue are chosen by how bright they look, not by their lightness: a blue of the same lightness as a yellow is much
        // darker. The light shade has a luminance of 0.55 and the deeper one 0.30, which keeps black text above a contrast of 7 on both.
        private static (string Label, Color Light, Color Dark) Kind(string label, double hue, double saturation) =>
            (label, WithLuminance(hue, saturation, 0.55), WithLuminance(hue, Math.Min(1, saturation + 0.1), 0.30));

        private static Color WithLuminance(double hue, double saturation, double luminance)
        {
            double low = 0, high = 1;
            for (var i = 0; i < 24; i++)
            {
                var middle = (low + high) / 2;
                if (LuminanceOf(FromHsl(hue, saturation, middle)) < luminance)
                {
                    low = middle;
                }
                else
                {
                    high = middle;
                }
            }

            return FromHsl(hue, saturation, (low + high) / 2);
        }

        private static double LuminanceOf(Color color)
        {
            static double Channel(byte value)
            {
                var c = value / 255.0;
                return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
            }

            return (0.2126 * Channel(color.R)) + (0.7152 * Channel(color.G)) + (0.0722 * Channel(color.B));
        }

        private static Color FromHsl(double hue, double saturation, double lightness)
        {
            var chroma = (1 - Math.Abs((2 * lightness) - 1)) * saturation;
            var x = chroma * (1 - Math.Abs(((hue / 60) % 2) - 1));
            var m = lightness - (chroma / 2);
            var (r, g, b) = (hue % 360) switch
            {
                < 60 => (chroma, x, 0.0),
                < 120 => (x, chroma, 0.0),
                < 180 => (0.0, chroma, x),
                < 240 => (0.0, x, chroma),
                < 300 => (x, 0.0, chroma),
                _ => (chroma, 0.0, x),
            };
            return Color.FromRgb((byte)Math.Round((r + m) * 255), (byte)Math.Round((g + m) * 255), (byte)Math.Round((b + m) * 255));
        }

        private static Brush Freeze(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        private sealed class KindToBrushConverter : IValueConverter
        {
            /// <inheritdoc/>
            public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value is string key && BrushesByKey.TryGetValue(key, out var brush) ? brush : null;
            }

            /// <inheritdoc/>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        private sealed class TextOrNullConverter : IValueConverter
        {
            /// <inheritdoc/>
            public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value is string text && text.Length > 0 ? text : null;
            }

            /// <inheritdoc/>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }
    }
}
