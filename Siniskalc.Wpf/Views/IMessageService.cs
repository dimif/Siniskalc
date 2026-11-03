namespace Siniskalc.Wpf.Views
{
    /// <summary>
    /// Tells the user something, or asks them a question, in a dialog of its own. What the application says to the user goes through
    /// it, so that the dialogs look alike and a test can give the application one that does not block.
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Tells the user that something went wrong, with an error icon.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">What went wrong.</param>
        void ShowError(string title, string message);

        /// <summary>
        /// Shows an error that nobody expected, with <paramref name="details"/> (the exception and its stack trace) in a section that is folded away,
        /// can be selected and copied.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">What went wrong, in words for the user.</param>
        /// <param name="details">The exception and its stack trace.</param>
        void ShowUnexpectedError(string title, string message, string details);

        /// <summary>
        /// Warns the user about something, with a warning icon.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">What the user is warned about.</param>
        void ShowWarning(string title, string message);

        /// <summary>
        /// Tells the user something that is neither a warning nor an error, with an information icon.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">What the user is told.</param>
        void ShowInformation(string title, string message);

        /// <summary>
        /// Asks a question with a Yes, a No and a Cancel answer.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The question.</param>
        /// <returns>The button the user pressed; closing the dialog without choosing counts as <see cref="MessageAnswer.Cancel"/>.</returns>
        MessageAnswer AskYesNoCancel(string title, string message);
    }
}
