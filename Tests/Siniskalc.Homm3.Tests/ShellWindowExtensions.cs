namespace Siniskalc.Homm3.Tests
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Siniskalc.Homm3.Views;

    /// <summary>
    /// Lets a test reach into the window of the shell, which shows the HoMM3 editor through a data template.
    /// </summary>
    internal static class ShellWindowExtensions
    {
        /// <summary>
        /// Returns the frame that shows the pages of the HoMM3 editor, which is in the view the shell makes for the editor.
        /// </summary>
        public static Frame HeroFrame(this Window window)
        {
            window.UpdateLayout();
            var view = Find<Homm3EditorView>(window) ?? throw new AssertionException("The window shows no HoMM3 editor.");
            return (Frame)view.FindName("heroFrame");
        }

        private static T? Find<T>(DependencyObject root)
            where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match)
                {
                    return match;
                }

                if (Find<T>(child) is { } nested)
                {
                    return nested;
                }
            }

            return null;
        }
    }
}
