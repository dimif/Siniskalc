namespace Siniskalc.Wpf.Views
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows;

    /// <summary>
    /// Tells the user that something unexpected went wrong. The message says what the user can do; the details (the exception with its stack trace)
    /// are folded away, can be selected and copied, and are what a bug report needs.
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public ErrorWindow(string title, string message, string details)
        {
            this.InitializeComponent();
            this.Title = title;
            this.messageText.Text = message;
            this.detailsText.Text = details;
            this.details.Visibility = details.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Gets what "Copy details" puts on the clipboard: the message, then the details.
        /// </summary>
        public string ClipboardText => this.messageText.Text + Environment.NewLine + Environment.NewLine + this.detailsText.Text;

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(this.ClipboardText);
                this.copyButton.Content = "Copied";
            }
            catch (COMException)
            {
                // Another program holds the clipboard; the details can still be selected and copied by hand.
                this.copyButton.Content = "Could not copy";
            }
        }
    }
}
