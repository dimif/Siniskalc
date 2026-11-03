namespace Siniskalc.Heroes.Wpf.Views
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// The header of a page about a hero, a player or a castle: a marker in the colour of the owner, the name, what the page adds after
    /// it (its content), and the "edited" mark and Reset of a page whose savegame holds unsaved edits. The data context is the page's
    /// view model, which has <c>HasPendingChanges</c> and <c>ResetCommand</c>.
    /// </summary>
    public class PageHeader : ContentControl
    {
        /// <summary>
        /// Identifies the <see cref="ColorHex"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColorHexProperty = DependencyProperty.Register(nameof(ColorHex), typeof(string), typeof(PageHeader), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the <see cref="Title"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(PageHeader), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the <see cref="EditedToolTip"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EditedToolTipProperty = DependencyProperty.Register(nameof(EditedToolTip), typeof(string), typeof(PageHeader), new PropertyMetadata("This page has changes that are not saved yet"));

        /// <summary>
        /// Identifies the <see cref="ResetToolTip"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ResetToolTipProperty = DependencyProperty.Register(nameof(ResetToolTip), typeof(string), typeof(PageHeader), new PropertyMetadata("Put everything on this page back to what the savegame had"));

        /// <summary>
        /// Gets or sets the colour of the marker, such as "#C0392B".
        /// </summary>
        public string ColorHex
        {
            get => (string)this.GetValue(ColorHexProperty);
            set => this.SetValue(ColorHexProperty, value);
        }

        /// <summary>
        /// Gets or sets the title of the page.
        /// </summary>
        public string Title
        {
            get => (string)this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Gets or sets the tooltip of the mark that shows the page has edits.
        /// </summary>
        public string EditedToolTip
        {
            get => (string)this.GetValue(EditedToolTipProperty);
            set => this.SetValue(EditedToolTipProperty, value);
        }

        /// <summary>
        /// Gets or sets the tooltip of the button that puts the page back to the loaded values.
        /// </summary>
        public string ResetToolTip
        {
            get => (string)this.GetValue(ResetToolTipProperty);
            set => this.SetValue(ResetToolTipProperty, value);
        }
    }
}
