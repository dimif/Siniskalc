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

        private IReadOnlyList<PendingChange> Diff(HeroModel original, HeroModel current) => new HeroDiffer(this.fake.Factory, TestCatalog.Standard).Diff(7, original, current);

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

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Position", "at 10, 12, facing North", "at 10, 12, facing South")));
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
            var current = original.WithArtifact(2, TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"]);

            var change = this.Diff(original, current).Single(c => c.Change.Description == "Slot 3");

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Slot 3", ChangeText.Empty, "Ultimate Sword of Dominion")));
            var reverted = this.fake.HeroReverts["Slot 3"](current);
            Assert.That(reverted, Is.EqualTo(original));
            Assert.That(reverted.PrimarySkills.Bonus, Is.EqualTo(original.PrimarySkills.Bonus));
        }

        [TestCase("UltimateSwordOfDominion", null)]
        [TestCase("UltimateSwordOfDominion", "UltimateCloakOfProtection")]
        [TestCase("UltimateCrown", "UltimateSwordOfDominion")]
        public void AnArtifactRevertRestoresTheOriginalBonusesAndSavedPrimarySkills(string before, string? after)
        {
            var original = DifferSamples.Hero().WithArtifact(2, TestCatalog.Standard.Artifacts[before]);
            var current = original.WithArtifact(2, after is null ? null : TestCatalog.Standard.Artifacts[after]);

            this.Diff(original, current);
            var reverted = this.fake.HeroReverts["Slot 3"](current);

            Assert.That(reverted, Is.EqualTo(original));
            Assert.That(reverted.PrimarySkills.Bonus, Is.EqualTo(original.PrimarySkills.Bonus));
        }

        [Test]
        public void AnArtifactRevertPreservesOtherArtifactsAndIndependentlyEditedPrimarySkills()
        {
            var original = DifferSamples.Hero().WithArtifact(2, TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"]);
            var current = original.WithArtifact(2, TestCatalog.Standard.Artifacts["UltimateCloakOfProtection"])
                .WithArtifact(3, TestCatalog.Standard.Artifacts["UltimateCrown"]);
            current = current with
            {
                PrimarySkills = current.PrimarySkills.WithBaseAttack(20).WithBaseDefense(21).WithBaseSpellPower(22).WithBaseKnowledge(23),
            };

            this.Diff(original, current);
            var reverted = this.fake.HeroReverts["Slot 3"](current);

            Assert.That(reverted.Artifacts.Slots[2], Is.EqualTo(original.Artifacts.Slots[2]));
            Assert.That(reverted.Artifacts.Slots[3], Is.EqualTo(current.Artifacts.Slots[3]));
            Assert.That(reverted.PrimarySkills, Is.EqualTo(new PrimarySkills(36, 25, 26, 27)));
            Assert.That(reverted.PrimarySkills.Bonus, Is.EqualTo(new PrimarySkillBonus(16, 4, 4, 4)));
            Assert.That((reverted.PrimarySkills.BaseAttack, reverted.PrimarySkills.BaseDefense, reverted.PrimarySkills.BaseSpellPower, reverted.PrimarySkills.BaseKnowledge), Is.EqualTo((20, 21, 22, 23)));
        }

        [Test]
        public void ALearnedSpellIsListedByNameAndTheSecondarySkillsAsOneListThatArePutBackAsTheyWere()
        {
            var original = DifferSamples.Hero();
            var current = original with
            {
                Spells = original.Spells.With(TestCatalog.Standard.Spells["Fireball"]),
                SecondarySkills = original.SecondarySkills.WithSkill(TestCatalog.Standard.Skills["Archery"], SkillLevel.Expert),
            };

            var changes = this.Diff(original, current);

            Assert.That(changes.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EquivalentTo(new[]
            {
                ("Fireball", "not learned", "learned"),
                ("Skills", "none", "Archery (Expert)"),
            }));
            Assert.That(this.fake.HeroReverts["Fireball"](current).Spells.Contains(TestCatalog.Standard.Spells["Fireball"]), Is.False);
            Assert.That(this.fake.HeroReverts["Skills"](current).SecondarySkills, Is.EqualTo(original.SecondarySkills));
        }

        [Test]
        public void SwappingTwoSecondarySkillsIsAChangeThatPutsTheOrderBack()
        {
            var original = DifferSamples.Hero() with
            {
                SecondarySkills = new SecondarySkills([new SkillEntry(TestCatalog.Standard.Skills["Archery"], SkillLevel.Beginner), new SkillEntry(TestCatalog.Standard.Skills["Wisdom"], SkillLevel.Expert)]),
            };
            var current = original with { SecondarySkills = original.SecondarySkills.Swap(0, 1) };

            var change = this.Diff(original, current).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Skills", "Archery (Beginner), Wisdom (Expert)", "Wisdom (Expert), Archery (Beginner)")));
            Assert.That(this.fake.HeroReverts["Skills"](current), Is.EqualTo(original));
        }

        [Test]
        public void AChangedStackIsAChangeOfItsSlotAndPutBackAsItWas()
        {
            var original = DifferSamples.Hero();
            var current = original with { Army = original.Army.WithSlot(1, TestCatalog.Standard.Creatures["Archer"], 12) };

            var change = this.Diff(original, current).Single();

            Assert.That(change.Change.Description, Is.EqualTo("Slot 2"));
            Assert.That(change.Change.Before, Is.EqualTo(ChangeText.Empty));
            Assert.That(this.fake.HeroReverts["Slot 2"](current).Army.Slots[1], Is.Null);
        }
    }
}
