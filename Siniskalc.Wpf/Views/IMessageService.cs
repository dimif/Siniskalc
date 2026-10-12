namespace Siniskalc.Wpf.Views
{
    /// <summary>
    /// Tells the user something, or asks them a question, in a dialog of its own. What the application says to the user goes through
    /// it, so that the dialogs look alike and a test can give the application one that does not block.
    /// </summary>
    public interface IMessageService
    {
        void ShowError(string title, string message);

        /// <summary>
        /// Shows an error that nobody expected, with <paramref name="details"/> (the exception and its stack trace) in a section that is folded away,
        /// can be selected and copied.
        /// </summary>
        void ShowUnexpectedError(string title, string message, string details);

        void ShowWarning(string title, string message);

        void ShowInformation(string title, string message);

        /// <summary>
        /// Asks a question with a Yes, a No and a Cancel answer.
        /// </summary>
        MessageAnswer AskYesNoCancel(string title, string message);
    }
}
