namespace Siniskalc.Homm3.Tests.Models
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Tests the reducers of the model of a hero and what it tells through the interface every game's hero has, on a hero made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class HeroModelTests
    {
        [Test]
        public void AHeroTellsItsArmyThroughTheSharedInterface()
        {
            var hero = HeroModels.Christian();

            IHeroModel<SecondarySkill, SecondarySkills, Creature, PlayerColor> shared = hero;

            Assert.That(shared.Army, Is.SameAs(hero.Army));
        }

        [Test]
        public void AHeroThatServesNobodyLeavesTheMapAndFacesEast()
        {
            var hero = HeroModels.Christian() with { Destination = new MapPosition(3, 3) };

            var freed = hero.WithOwner(PlayerColor.Neutral);

            Assert.That((freed.Owner, freed.Position, freed.Destination), Is.EqualTo((PlayerColor.Neutral, new HeroPosition(-1, -1, -1, FacingDirection.East), MapPosition.None)));
        }

        [Test]
        public void AHeroGivenToAPlayerKeepsItsPlace()
        {
            var hero = HeroModels.Christian() with { Destination = new MapPosition(3, 3) };

            var given = hero.WithOwner(PlayerColor.Pink);

            Assert.That((given.Owner, given.Position, given.Destination), Is.EqualTo((PlayerColor.Pink, hero.Position, hero.Destination)));
        }

        [Test]
        public void TheLevelAndTheExperienceOfAHeroAreSetTogetherFromTheTable()
        {
            var hero = HeroModels.Christian();

            Assert.That(hero.WithLevel(10).HeroLevel, Is.EqualTo(new HeroLevel(LevelCalculator.Table.ExperienceOf(10), 10)));
            Assert.That(hero.WithExperience(LevelCalculator.Table.ExperienceOf(20)).HeroLevel.Level, Is.EqualTo(20));
            Assert.That(() => hero.WithLevel(76), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => hero.WithExperience(-1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TheMovementLeftAndTheSpellPointsAreSetWithoutTouchingTheRest()
        {
            var hero = HeroModels.Christian();

            var changed = hero.WithMovementLeft(123).WithSpellPoints(45);

            Assert.That((changed.MovementPoints.Current, changed.MovementPoints.Max, changed.SpellPoints), Is.EqualTo((123L, hero.MovementPoints.Max, 45)));
        }

        [Test]
        public void AHeroTellsWhetherItHasASpellBookAndWhatItHasLearnedThroughTheSharedInterface()
        {
            var hero = HeroModels.Christian();
            var withBook = hero with { SpellBook = hero.SpellBook.WithHasSpellBook(true).WithLearned(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Bless"), true) };

            IHeroModel<SecondarySkill, SecondarySkills, Creature, PlayerColor> shared = withBook;

            Assert.That(shared.HasSpellBook, Is.True);
            Assert.That(shared.LearnedSpells, Does.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Bless")));
        }

        [Test]
        public void AHeroTellsWhoOwnsItWhereItStandsAndHowFarItHasComeThroughTheSharedInterface()
        {
            var hero = HeroModels.Christian();

            IHeroModel<SecondarySkill, SecondarySkills, Creature, PlayerColor> shared = hero;

            Assert.That(
                (shared.Owner, shared.Position, shared.Destination, shared.HeroLevel, shared.MovementPoints),
                Is.EqualTo((hero.Owner, hero.Position, hero.Destination, hero.HeroLevel, hero.MovementPoints)));
        }
    }
}
