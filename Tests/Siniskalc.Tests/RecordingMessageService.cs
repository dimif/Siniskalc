namespace Siniskalc.Tests
{
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// A message service that shows nothing and remembers what it was asked to show, and answers every question with <see cref="Answer"/>.
    /// </summary>
    internal sealed class RecordingMessageService : IMessageService
    {
        public List<(string Kind, string Title, string Message)> Shown { get; } = new();

        public MessageAnswer Answer { get; set; } = MessageAnswer.Cancel;

        public void ShowError(string title, string message) => this.Shown.Add(("Error", title, message));

        public void ShowUnexpectedError(string title, string message, string details) => this.Shown.Add(("Unexpected error", title, message + Environment.NewLine + details));

        public void ShowWarning(string title, string message) => this.Shown.Add(("Warning", title, message));

        public void ShowInformation(string title, string message) => this.Shown.Add(("Information", title, message));

        public MessageAnswer AskYesNoCancel(string title, string message)
        {
            this.Shown.Add(("Question", title, message));
            return this.Answer;
        }
    }
}
