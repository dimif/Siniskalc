namespace Siniskalc.Wpf.ViewModels
{
    /// <summary>
    /// What a card that holds rows of yes-or-no boxes needs of one: whether it is checked, and to show it again after the model changed.
    /// </summary>
    public interface IToggleRow
    {
        /// <summary>
        /// Gets what the box is about, such as "Tavern".
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the box is checked; setting it writes it to the model.
        /// </summary>
        bool IsChecked { get; set; }

        /// <summary>
        /// Gets a value indicating whether the box can be changed right now.
        /// </summary>
        bool CanToggle { get; }

        /// <summary>
        /// Shows the row again. Call after the model changed.
        /// </summary>
        void Refresh();
    }
}
