namespace Siniskalc.Homm2.Tests.Models
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Tests.Data;

    /// <summary>
    /// Tests the reducers of the model of a hero on a hero made by hand: owner, level, points, spell book and the bonuses of artifacts.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class HeroModelTests
    {
        private static HeroModel Maximus() => DifferSamples.Hero();

        [Test]
        public void AHeroTellsItsArmyThroughTheSharedInterface()
        {
            var maximus = Maximus();

            IHeroModel<SecondarySkill, SecondarySkills, Creature, PlayerColor> hero = maximus;

            Assert.That(hero.Army, Is.SameAs(maximus.Army));
        }

        [Test]
        public void AHeroTellsWhoOwnsItWhereItStandsAndHowFarItHasComeThroughTheSharedInterface()
        {
            var maximus = Maximus();

            IHeroModel<SecondarySkill, SecondarySkills, Creature, PlayerColor> hero = maximus;

            Assert.That(
                (hero.Owner, hero.Position, hero.Destination, hero.HeroLevel, hero.MovementPoints),
                Is.EqualTo((maximus.Owner, maximus.Position, maximus.Destination, maximus.HeroLevel, maximus.MovementPoints)));
            Assert.That((hero.Position.Level, hero.Owner), Is.EqualTo((MapPosition.Above, PlayerColor.Blue)));
        }

        [Test]
        public void AHeroThatServesNobodyHasNoMoveToFinishAndKeepsItsTile()
        {
            var moving = Maximus() with { Destination = new MapPosition(30, 30) };

            var freed = moving.WithOwner(PlayerColor.Neutral);

            Assert.That((freed.Owner, freed.Destination, freed.Position), Is.EqualTo((PlayerColor.Neutral, MapPosition.None, moving.Position)));
        }

        [Test]
        public void AHeroGivenToAPlayerKeepsItsDestination()
        {
            var moving = Maximus() with { Destination = new MapPosition(30, 30) };

            var given = moving.WithOwner(PlayerColor.Orange);

            Assert.That((given.Owner, given.Destination), Is.EqualTo((PlayerColor.Orange, moving.Destination)));
        }

        [Test]
        public void TheLevelAndTheExperienceOfAHeroAreSetTogetherFromTheTable()
        {
            var maximus = Maximus();

            Assert.That(maximus.WithLevel(10).HeroLevel, Is.EqualTo(new HeroLevel(HeroLevels.ExperienceOf(10), 10)));
            Assert.That(maximus.WithExperience(54_200).HeroLevel, Is.EqualTo(new HeroLevel(54_200, 18)));
            Assert.That(() => maximus.WithLevel(0), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => maximus.WithExperience(-1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TheMovementLeftAndTheSpellPointsAreSetWithoutTouchingTheRest()
        {
            var maximus = Maximus();

            var changed = maximus.WithMovementLeft(123).WithSpellPoints(45);

            Assert.That((changed.MovementPoints.Current, changed.MovementPoints.Max, changed.SpellPoints), Is.EqualTo((123L, maximus.MovementPoints.Max, 45)));
        }

        [Test]
        public void AHeroTellsWhetherItHasASpellBookAndWhatItHasLearnedThroughTheSharedInterface()
        {
            var maximus = Maximus().WithSpellBook() with { Spells = SpellSet<Spell>.None.With(TestCatalog.Standard.Spells["Bless"]) };

            IHeroModel<SecondarySkill, SecondarySkills, Creature, PlayerColor> hero = maximus;

            Assert.That(hero.HasSpellBook, Is.True);
            Assert.That(hero.LearnedSpells, Is.EqualTo(new[] { TestCatalog.Standard.Spells["Bless"] }));
        }

        [Test]
        public void AnArtifactThatGivesSkillsAddsThemWhenItIsGivenAndTakesThemOffWhenItIsTakenAway()
        {
            var maximus = Maximus();

            var armed = maximus.WithArtifact(TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"]);

            Assert.That(armed.PrimarySkills, Is.EqualTo(maximus.PrimarySkills with { Attack = maximus.PrimarySkills.Attack + 12 }));
            Assert.That(armed.Artifacts.Slots, Has.Member(TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"]));
            Assert.That(armed.WithoutArtifact(TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"]), Is.EqualTo(maximus));
        }

        [Test]
        public void GivingTheUltimateSwordToAnotherHeroMovesItsTwelveAttackPoints()
        {
            var maximus = (Maximus() with { PrimarySkills = new PrimarySkills(15, 15, 15, 15) }).WithArtifact(TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"]);
            var gallant = Maximus() with { Id = 1, Name = "Sir Gallant", PrimarySkills = new PrimarySkills(2, 2, 2, 2) };

            var maximusAfter = maximus.WithoutArtifact(TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"]);
            var gallantAfter = gallant.WithArtifact(TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"]);

            Assert.That((maximusAfter.PrimarySkills.Attack, gallantAfter.PrimarySkills.Attack), Is.EqualTo((15, 14)));
        }

        [Test]
        public void ReplacingAnArtifactSwapsItsBonusForTheNewOne()
        {
            var maximus = Maximus().WithArtifact(TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"]);
            var slot = maximus.Artifacts.Slots.ToList().IndexOf(TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"]);

            var swapped = maximus.WithArtifact(slot, TestCatalog.Standard.Artifacts["UltimateCloakOfProtection"]);

            Assert.That(swapped.PrimarySkills.Attack, Is.EqualTo(maximus.PrimarySkills.Attack - 12));
            Assert.That(swapped.PrimarySkills.Defense, Is.EqualTo(maximus.PrimarySkills.Defense + 12));
        }

        [Test]
        public void ASkillNeverFallsBelowZero()
        {
            var maximus = Maximus().WithArtifact(TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"]) with { PrimarySkills = new PrimarySkills(3, 0, 0, 0) };

            Assert.That(maximus.WithoutArtifact(TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"]).PrimarySkills.Attack, Is.EqualTo(0));
        }

        [Test]
        public void ArtifactsWithoutSkillBonusesLeaveTheSkillsAlone()
        {
            var maximus = Maximus();

            Assert.That(maximus.WithArtifact(TestCatalog.Standard.Artifacts["IceCloak"]).WithArtifact(TestCatalog.Standard.Artifacts["AmmoCart"]).PrimarySkills, Is.EqualTo(maximus.PrimarySkills));
        }
    }
}
