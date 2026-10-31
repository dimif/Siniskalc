namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// What the differ of a hero lists for two versions of the record, with the way to put each change back, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class HeroDifferTests
    {
        private PendingChangesFake fake = null!;

        [SetUp]
        public void StartWithNoReverts() => this.fake = new();

        private IReadOnlyList<PendingChange> Diff(HeroModel original, HeroModel current) => new HeroDiffer(this.fake.Factory).Diff(7, original, current);

        [Test]
        public void TwoEqualHeroesHaveNoChanges()
        {
            Assert.That(this.Diff(DifferSamples.Hero(), DifferSamples.Hero()), Is.Empty);
        }

        [Test]
        public void ANewLevelIsOneChangeOfTheExperienceThatPutsTheLevelBackToo()
        {
            var original = DifferSamples.Hero();
            var current = original.WithLevel(5);

            var change = this.Diff(original, current).Single();

            Assert.That(change.Change.Description, Is.EqualTo("Experience"));
            Assert.That(change.Change.Before, Does.EndWith("(level 3)"));
            Assert.That(change.Change.After, Does.EndWith("(level 5)"));
            Assert.That(this.fake.HeroReverts["Experience"](current).HeroLevel, Is.EqualTo(original.HeroLevel));
        }

        [Test]
        public void TurningAHeroIsAChangeOfItsPositionThatShowsTheTileAndTheFacing()
        {
            var original = DifferSamples.Hero();
            var current = original with { Position = original.Position with { FacingDirection = FacingDirection.South } };

            var change = this.Diff(original, current).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Position", "Pos: 10, 12, facing North", "Pos: 10, 12, facing South")));
            Assert.That(this.fake.HeroReverts["Position"](current), Is.EqualTo(original));
        }

        [Test]
        public void ChangedMovementShowsWhatIsLeftAndWhatAFullDayGives()
        {
            var original = DifferSamples.Hero();
            var current = original.WithMovementLeft(0);

            var change = this.Diff(original, current).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Movement points", "100 / 1,500", "0 / 1,500")));
        }

        [Test]
        public void AnotherOwnerIsListedBeforeEveryOtherChangeAndIsPutBackLikeAnyOtherChange()
        {
            var original = DifferSamples.Hero();
            var current = (original with { Owner = PlayerColor.Green }).WithLevel(4);

            var changes = this.Diff(original, current);

            Assert.That(changes.Select(c => c.Change.Description), Is.EqualTo(new[] { "Owner", "Experience" }));
            Assert.That((changes[0].Change.Before, changes[0].Change.After, changes[0].Kind, changes[0].Index), Is.EqualTo(("Blue", "Green", Homm2ChunkKinds.Hero, 7)));
            Assert.That(this.fake.HeroReverts["Owner"](current).Owner, Is.EqualTo(PlayerColor.Blue));
        }

        [Test]
        public void ChangedMoraleAndLuckForTheNextBattleAreSignedAndPutBack()
        {
            var original = DifferSamples.Hero();
            var current = original with { NextBattle = new NextBattleBonus(2, -1) };

            var changes = this.Diff(original, current);

            Assert.That(changes.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EqualTo(new[]
            {
                ("Morale, next battle", "0", "+2"),
                ("Luck, next battle", "0", "-1"),
            }));
            Assert.That(this.fake.HeroReverts["Morale, next battle"](current).NextBattle, Is.EqualTo(new NextBattleBonus(0, -1)));
        }

        [Test]
        public void AnArtifactInASlotIsAChangeOfThatSlotAndPutBackEmpty()
        {
            var original = DifferSamples.Hero();
            var current = original with { Artifacts = original.Artifacts.WithSlot(2, Artifact.UltimateSwordOfDominion) };

            var change = this.Diff(original, current).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Slot 3", ChangeText.Empty, "Ultimate Sword of Dominion")));
            Assert.That(this.fake.HeroReverts["Slot 3"](current).Artifacts.Slots[2], Is.Null);
        }

        [Test]
        public void ALearnedSpellAndASecondarySkillAreListedByName()
        {
            var original = DifferSamples.Hero();
            var current = original with
            {
                Spells = original.Spells.With(Spell.Fireball),
                SecondarySkills = original.SecondarySkills.WithSkill(SecondarySkill.Archery, SkillLevel.Expert),
            };

            var changes = this.Diff(original, current);

            Assert.That(changes.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EquivalentTo(new[]
            {
                ("Fireball", "not known", "known"),
                ("Archery", "none", "Expert"),
            }));
            Assert.That(this.fake.HeroReverts["Fireball"](current).Spells.Contains(Spell.Fireball), Is.False);
            Assert.That(this.fake.HeroReverts["Archery"](current).SecondarySkills.GetSkillLevel(SecondarySkill.Archery), Is.EqualTo(SkillLevel.None));
        }

        [Test]
        public void AChangedStackIsAChangeOfItsSlotAndPutBackAsItWas()
        {
            var original = DifferSamples.Hero();
            var current = original with { Army = original.Army.WithSlot(1, Creature.Archer, 12) };

            var change = this.Diff(original, current).Single();

            Assert.That(change.Change.Description, Is.EqualTo("Slot 2"));
            Assert.That(change.Change.Before, Is.EqualTo(ChangeText.Empty));
            Assert.That(this.fake.HeroReverts["Slot 2"](current).Army.Slots[1], Is.Null);
        }
    }
}
