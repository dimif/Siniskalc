namespace Siniskalc.Wpf.ViewModels
{
    /// <summary>
    /// What a card that holds rows of numbers needs of one: its value, and to show it again after the model changed.
    /// </summary>
    public interface IAmountRow
    {
        /// <summary>
        /// Gets what the row is about, such as "Morale".
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the value of the row; setting it writes it to the model.
        /// </summary>
        long Value { get; set; }

        /// <summary>
        /// Shows the row again. Call after the model changed.
        /// </summary>
        void Refresh();
    }
}
