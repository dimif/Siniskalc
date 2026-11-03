namespace Siniskalc.Homm2.Tests
{
    using Moq;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// Message and file dialog doubles for tests that open a window but must not ask the user anything.
    /// </summary>
    internal static class NoUserInteraction
    {
        /// <summary>
        /// Returns messages that show nothing and answer every question with No.
        /// </summary>
        public static IMessageService Messages()
        {
            var messages = new Mock<IMessageService>();
            messages.Setup(m => m.AskYesNoCancel(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageAnswer.No);
            return messages.Object;
        }

        /// <summary>
        /// Returns file dialogs that are always cancelled.
        /// </summary>
        public static IFileDialogs FileDialogs() => new Mock<IFileDialogs>().Object;
    }
}
