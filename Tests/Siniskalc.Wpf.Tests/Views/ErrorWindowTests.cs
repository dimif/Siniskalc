namespace Siniskalc.Wpf.Tests.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using Siniskalc.Wpf.Views;

    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class ErrorWindowTests
    {
        private const string Trace = "System.InvalidOperationException: broken\r\n   at Siniskalc.Something.Go() in Something.cs:line 12";

        [Test]
        public void TheDetailsAreFoldedAwayReadOnlyAndSelectable()
        {
            var window = new ErrorWindow("Unexpected error", "Something went wrong: broken", Trace);

            var details = (Expander)window.FindName("details");
            var text = (TextBox)window.FindName("detailsText");
            Assert.That(details.IsExpanded, Is.False);
            Assert.That(details.Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(text.IsReadOnly, Is.True);
            Assert.That(text.Text, Is.EqualTo(Trace));
        }

        [Test]
        public void CopyingTakesTheMessageAndTheStackTrace()
        {
            var window = new ErrorWindow("Unexpected error", "Something went wrong: broken", Trace);

            Assert.That(window.ClipboardText, Does.StartWith("Something went wrong: broken").And.EndWith(Trace));
        }

        [Test]
        public void AnErrorWithoutDetailsHasNoDetailsSection()
        {
            var window = new ErrorWindow("Unexpected error", "Something went wrong", string.Empty);

            Assert.That(((Expander)window.FindName("details")).Visibility, Is.EqualTo(Visibility.Collapsed));
        }
    }
}
