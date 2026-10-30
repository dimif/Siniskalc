namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System.Windows.Input;
    using Moq;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the link to a hero in the list of the heroes of a player.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class HeroLinkViewModelTests
    {
        [Test]
        public void TheLinkNamesTheHeroAndSaysWhereItIsAndWhatOpensIt()
        {
            var open = Mock.Of<ICommand>();

            var link = new HeroLinkViewModel(7, "Orrin", "Knight, level 5", open);

            Assert.That((link.Index, link.Name, link.Detail), Is.EqualTo((7, "Orrin", "Knight, level 5")));
            Assert.That(link.OpenCommand, Is.SameAs(open));
        }

        [Test]
        public void OpeningTheLinkRunsTheCommandItWasGiven()
        {
            var open = new Mock<ICommand>();
            var link = new HeroLinkViewModel(7, "Orrin", string.Empty, open.Object);

            link.OpenCommand.Execute(link.Index);

            open.Verify(c => c.Execute(7), Times.Once);
        }
    }
}
