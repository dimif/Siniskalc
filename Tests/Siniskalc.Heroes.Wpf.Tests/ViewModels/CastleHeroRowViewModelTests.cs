namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the line that names a hero inside a castle and its role there.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class CastleHeroRowViewModelTests
    {
        [Test]
        public void ARowKeepsTheRoleTheNameAndTheCommandAndShowsTheRoleAsItsDetail()
        {
            var open = new DelegateCommand(_ => { });

            var row = new CastleHeroRowViewModel("Garrisoned", "Orrin", open);

            Assert.That((row.Role, row.Name, row.Detail), Is.EqualTo(("Garrisoned", "Orrin", "Garrisoned")));
            Assert.That(row.OpenCommand, Is.SameAs(open));
        }

        [Test]
        public void TheRoleInTheSentenceIsInLowerCase()
        {
            Assert.That(new CastleHeroRowViewModel("Visiting", "Orrin", new DelegateCommand(_ => { })).RoleText, Is.EqualTo("visiting"));
        }
    }
}
