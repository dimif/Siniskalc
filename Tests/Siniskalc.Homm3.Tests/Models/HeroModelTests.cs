namespace Siniskalc.Homm3.Tests.Models
{
    using Siniskalc.Heroes.Entities;
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

            IHeroModel<SecondarySkill, SecondarySkills, Creature, PlayerColor> shared = hero;

            Assert.That(shared.Army, Is.SameAs(hero.Army));
        }

        [Test]
        public void AHeroThatServesNobodyLeavesTheMapAndFacesEast()
        {
            var game = CastleSamples.Load(CastleSamples.Folder("Bulwark").First());
            var hero = game.History.CreateHeroSession(0).Current with { Destination = new MapPosition(3, 3) };

            var freed = hero.WithOwner(PlayerColor.Neutral);

            Assert.That((freed.Owner, freed.Position, freed.Destination), Is.EqualTo((PlayerColor.Neutral, new HeroPosition(-1, -1, -1, FacingDirection.East), MapPosition.None)));
        }

        [Test]
        public void AHeroGivenToAPlayerKeepsItsPlace()
        {
            var game = CastleSamples.Load(CastleSamples.Folder("Bulwark").First());
            var hero = game.History.CreateHeroSession(0).Current;

            var given = hero.WithOwner(PlayerColor.Pink);

            Assert.That((given.Owner, given.Position, given.Destination), Is.EqualTo((PlayerColor.Pink, hero.Position, hero.Destination)));
        }

        [Test]
        public void AHeroTellsWhoOwnsItWhereItStandsAndHowFarItHasComeThroughTheSharedInterface()
        {
            var game = CastleSamples.Load(CastleSamples.Folder("Bulwark").First());
            var hero = game.History.CreateHeroSession(0).Current;

            IHeroModel<SecondarySkill, SecondarySkills, Creature, PlayerColor> shared = hero;

            Assert.That(
                (shared.Owner, shared.Position, shared.Destination, shared.HeroLevel, shared.MovementPoints),
                Is.EqualTo((hero.Owner, hero.Position, hero.Destination, hero.HeroLevel, hero.MovementPoints)));
        }
    }
}
