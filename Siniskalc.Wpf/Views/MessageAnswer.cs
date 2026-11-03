namespace Siniskalc.Wpf.Views
{
    /// <summary>
    /// The button the user pressed in a question asked through <see cref="IMessageService"/>.
    /// </summary>
    public enum MessageAnswer
    {
        /// <summary>The user agreed.</summary>
        Yes,

        /// <summary>The user declined, but wants to go on.</summary>
        No,

        /// <summary>The user wants to stop and leave everything as it was.</summary>
        Cancel,
    }
}
