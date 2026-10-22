namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// What the pending changes of a hero say, and how they are put back.
    /// </summary>
    [TestFixture]
    internal sealed class HeroDifferTests
    {
        private const int Maximus = 7;

        private Game game = null!;

        [SetUp]
        public void LoadTheStart() => this.game = TestServices.LoadBrokenAlliance(1);

        [Test]
        public void ANewLevelIsOneChangeOfTheExperienceThatIsPutBackWithItsLevel()
        {
            var original = this.game.History.Current.Heroes().GetModel(Maximus);

            this.game.History.CreateHeroSession(Maximus).Update(h => h.WithLevel(original.HeroLevel.Level + 2));

            var change = this.game.History.GetPendingChanges().Single();
            Assert.That(change.Change.Description, Is.EqualTo("Experience"));
            Assert.That(change.Change.Before, Is.EqualTo($"{original.HeroLevel.Experience:N0} (level {original.HeroLevel.Level})"));
            Assert.That(change.Change.After, Does.EndWith($"(level {original.HeroLevel.Level + 2})"));

            change.Revert(this.game.History);

            Assert.That(this.game.History.Current.Heroes().GetModel(Maximus).HeroLevel, Is.EqualTo(original.HeroLevel));
            Assert.That(this.game.History.GetPendingChanges(), Is.Empty);
        }
    }
}
