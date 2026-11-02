namespace Siniskalc.Heroes.Wpf.Views
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// The choice of who owns a hero or a castle: a label and a list of "Neutral" and the player colours. The data context is an
    /// <see cref="Siniskalc.Heroes.ViewModels.IOwnerChoice"/>.
    /// </summary>
    public class OwnerPicker : Control
    {
        /// <summary>
        /// Identifies the <see cref="PickerToolTip"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PickerToolTipProperty = DependencyProperty.Register(nameof(PickerToolTip), typeof(string), typeof(OwnerPicker), new PropertyMetadata("Which player owns it. Neutral is nobody."));

        /// <summary>
        /// Gets or sets what the list says it does when the pointer rests on it.
        /// </summary>
        public string PickerToolTip
        {
            get => (string)this.GetValue(PickerToolTipProperty);
            set => this.SetValue(PickerToolTipProperty, value);
        }
    }
}
