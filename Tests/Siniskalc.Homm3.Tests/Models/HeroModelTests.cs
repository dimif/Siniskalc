namespace Siniskalc.Homm3.Tests.Models
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Homm3.Tests.Data;

    /// <summary>
    /// Tests what a hero of HoMM3 tells through the interfaces every game's hero has.
    /// </summary>
    [TestFixture]
    internal sealed class HeroModelTests
    {
        [Test]
        public void AHeroTellsItsArmyThroughTheSharedInterface()
        {
            var game = CastleSamples.Load(CastleSamples.Folder("Bulwark").First());
            var hero = game.History.CreateHeroSession(0).Current;

            IHeroModel<SecondarySkill, SecondarySkills, Creature> shared = hero;

            Assert.That(shared.Army, Is.SameAs(hero.Army));
        }
    }
}
