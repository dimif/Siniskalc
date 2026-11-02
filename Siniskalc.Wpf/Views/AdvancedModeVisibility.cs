namespace Siniskalc.Wpf.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// Shows an element only while the data context of its window has <c>IsAdvancedMode</c> set, as <see cref="NavButton.RequiresAdvancedMode"/>
    /// does for a tab. The binding is made once the element is in a window: a binding to an ancestor made before that reports an error.
    /// </summary>
    public static class AdvancedModeVisibility
    {
        /// <summary>
        /// Hides <paramref name="element"/> until it is loaded in a window, then shows it while advanced mode is on.
        /// </summary>
        /// <param name="element">The element to show only while the advanced mode is on.</param>
        public static void Apply(FrameworkElement element)
        {
            element.Visibility = Visibility.Collapsed;
            element.Loaded += (_, _) =>
            {
                if (Window.GetWindow(element) is { } window)
                {
                    element.SetBinding(UIElement.VisibilityProperty, new Binding("DataContext.IsAdvancedMode") { Source = window, Converter = new BooleanToVisibilityConverter() });
                }
            };
        }
    }
}
