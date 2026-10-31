namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System.Windows.Input;
    using Moq;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the button of one player in the order of play.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class OrderOfPlayItemViewModelTests
    {
        [Test]
        public void TheItemKeepsTheNameTheColourTheTurnAndTheToolTip()
        {
            var command = Mock.Of<ICommand>();

            var item = new OrderOfPlayItemViewModel("Blue", "#3333FF", true, command, "Give the turn to Blue");

            Assert.That((item.Name, item.ColorHex, item.IsCurrent, item.ToolTip), Is.EqualTo(("Blue", "#3333FF", true, "Give the turn to Blue")));
            Assert.That(item.Command, Is.SameAs(command));
        }

        [Test]
        public void AnItemOfAGameThatCannotGiveTheTurnHasNoCommand()
        {
            var item = new OrderOfPlayItemViewModel("Red", "#FF0000", false, null, "The game does not let the editor do this");

            Assert.That(item.Command, Is.Null);
            Assert.That(item.IsCurrent, Is.False);
        }
    }
}
