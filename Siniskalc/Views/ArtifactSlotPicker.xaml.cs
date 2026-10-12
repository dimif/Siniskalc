namespace Siniskalc.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// A button showing the artifact in a slot; clicking it opens a popup with a finder of the artifacts that fit.
    /// The DataContext is an <see cref="ViewModels.ArtifactSlotViewModel"/>.
    /// </summary>
    public partial class ArtifactSlotPicker : UserControl
    {
        public static readonly DependencyProperty PopupWidthProperty =
            DependencyProperty.Register(nameof(PopupWidth), typeof(double), typeof(ArtifactSlotPicker), new PropertyMetadata(1000.0));

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
