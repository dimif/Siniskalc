namespace Siniskalc.Heroes.Wpf.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// A button showing the creature of a stack; clicking it opens a popup with a finder of creatures.
    /// The DataContext is a <see cref="ViewModels.StackSlotViewModel"/>.
    /// </summary>
    public partial class CreatureSlotPicker : UserControl
    {
        /// <summary>
        /// Identifies the <see cref="PopupWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PopupWidthProperty =
            DependencyProperty.Register(nameof(PopupWidth), typeof(double), typeof(CreatureSlotPicker), new PropertyMetadata(1000.0));

        private bool? opensAbove;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatureSlotPicker"/> class.
        /// </summary>
        public CreatureSlotPicker()
        {
            this.InitializeComponent();
            this.popup.CustomPopupPlacementCallback = this.PlacePopup;
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

        // The side is chosen once, when the popup opens with the whole list, and kept until it closes. Left to the default placement the
        // popup flipped from above the button to below it as soon as a search made it short enough to fit there.
        private CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset)
        {
            this.opensAbove ??= this.DoesNotFitBelow(popupSize, targetSize);
            var y = this.opensAbove.Value ? -popupSize.Height : targetSize.Height;
            return [new CustomPopupPlacement(new Point(0, y), PopupPrimaryAxis.Horizontal)];
        }

        private bool DoesNotFitBelow(Size popupSize, Size targetSize)
        {
            if (PresentationSource.FromVisual(this.button) is not { CompositionTarget: { } target })
            {
                return false;
            }

            var bottomOfButton = target.TransformFromDevice.Transform(this.button.PointToScreen(new Point(0, 0))).Y + targetSize.Height;
            return bottomOfButton + popupSize.Height > SystemParameters.WorkArea.Bottom;
        }

        private void PopupClosed(object sender, EventArgs e) => this.opensAbove = null;

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
