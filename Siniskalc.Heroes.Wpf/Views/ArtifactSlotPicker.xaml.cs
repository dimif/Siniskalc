namespace Siniskalc.Heroes.Wpf.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// A button showing the artifact in a slot; clicking it opens a popup with a finder of the artifacts that fit.
    /// The DataContext is an <see cref="ArtifactPickerViewModel"/>.
    /// </summary>
    public partial class ArtifactSlotPicker : UserControl
    {
        /// <summary>
        /// Identifies the <see cref="PopupWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PopupWidthProperty =
            DependencyProperty.Register(nameof(PopupWidth), typeof(double), typeof(ArtifactSlotPicker), new PropertyMetadata(1000.0));

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactSlotPicker"/> class.
        /// </summary>
        public ArtifactSlotPicker()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets how wide the popup with the finder is.
        /// </summary>
        public double PopupWidth
        {
            get => (double)this.GetValue(PopupWidthProperty);
            set => this.SetValue(PopupWidthProperty, value);
        }

        private static T? FindAncestor<T>(DependencyObject start)
            where T : DependencyObject
        {
            for (var node = VisualTreeHelper.GetParent(start); node != null; node = VisualTreeHelper.GetParent(node))
            {
                if (node is T found)
                {
                    return found;
                }
            }

            return null;
        }

        private static void FocusSearch(DependencyObject root)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is TextBox box)
                {
                    box.Focus();
                    return;
                }

                FocusSearch(child);
            }
        }

        // Pointing at the button shows what the artifact does, on the info card of the page, when the page has one.
        private void ButtonMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (FindAncestor<Page>(this)?.DataContext is IArtifactInfoSource source && this.DataContext is ArtifactPickerViewModel slot)
            {
                source.ShowArtifactInfoCommand.Execute(slot.ArtifactName);
            }
        }

        private void ButtonMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (FindAncestor<Page>(this)?.DataContext is IArtifactInfoSource source)
            {
                source.ClearArtifactInfoCommand.Execute(null);
            }
        }

        private void PopupOpened(object sender, EventArgs e)
        {
            // Start typing right away: put the cursor in the finder's search box.
            if (sender is System.Windows.Controls.Primitives.Popup { Child: Border { Child: FinderView finder } })
            {
                finder.Dispatcher.BeginInvoke(() => FocusSearch(finder), System.Windows.Threading.DispatcherPriority.Input);
            }
        }
    }
}
