using Siniskalc.Core.Data;
using Siniskalc.Core.Models;
using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Tests.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// What the differ of a hero lists for two versions of the record, with the way to put each change back, on heroes made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class HeroDifferTests
    {
        private PendingChangesFake fake = null!;

        [SetUp]
        public void StartWithNoReverts() => this.fake = new();

        private IReadOnlyList<PendingChange> Diff(HeroModel original, HeroModel current) => new HeroDiffer(this.fake.Factory).Diff(4, original, current);

        private static (string Description, string Before, string After) Words(PendingChange change) => (change.Change.Description, change.Change.Before, change.Change.After);

        [Test]
        public void TwoEqualHeroesHaveNoChanges()
        {
            Assert.That(this.Diff(HeroModels.Christian(), HeroModels.Christian()), Is.Empty);
        }

        [Test]
        public void AnotherOwnerIsListedBeforeEveryOtherChangeAndPutBackInTheHeroAlone()
        {
            var original = HeroModels.Christian();
            var current = (original with { MovedToday = true }).WithOwner(PlayerColor.Red);

            var changes = this.Diff(original, current);

            Assert.That(changes.Select(c => c.Change.Description).First(), Is.EqualTo("Owner"));
            Assert.That((changes[0].Change.Before, changes[0].Change.After, changes[0].Kind, changes[0].Index, changes[0].OwnerName), Is.EqualTo(("Blue", "Red", Homm3ChunkKinds.Hero, 4, "Christian")));
            Assert.That(this.fake.HeroReverts["Owner"](current), Is.EqualTo(current with { Owner = original.Owner }), "the lists follow through the reconciler");
        }

        [Test]
        public void AnotherClassAndAnotherGenderAreListedByNameAndPutBack()
        {
            var original = HeroModels.Christian() with { Gender = null };
            var current = original with { HeroClass = HeroClass.Cleric, Gender = HeroGender.Female };

            var changes = this.Diff(original, current);

            Assert.That(changes.Select(Words), Is.EqualTo(new[] { ("Class", "Knight", "Cleric"), ("Gender", "unknown", "Female") }));
            Assert.That(this.fake.HeroReverts["Class"](current).HeroClass, Is.EqualTo(HeroClass.Knight));
            Assert.That(this.fake.HeroReverts["Gender"](current).Gender, Is.Null);
        }

        [Test]
        public void TheFlagsOfTheDayAreYesOrNoAndPutBack()
        {
            var original = HeroModels.Christian();
            var current = original with { MovedToday = true, IsAsleep = true };

            var changes = this.Diff(original, current);

            Assert.That(changes.Select(Words), Is.EqualTo(new[] { ("Moved today", "no", "yes"), ("Asleep", "no", "yes") }));
            Assert.That(this.fake.HeroReverts["Asleep"](current).IsAsleep, Is.False);
        }

        [Test]
        public void ASpellCastTodayIsOneChangeAndPuttingItBackKeepsTheOtherSpells()
        {
            var original = HeroModels.Christian() with { ActiveSpells = ActiveAdventureSpells.Fly };
            var current = original with { ActiveSpells = ActiveAdventureSpells.Fly | ActiveAdventureSpells.Visions };

            var change = this.Diff(original, current).Single();

            Assert.That(Words(change), Is.EqualTo(("Visions cast today", "no", "yes")));
            Assert.That(this.fake.HeroReverts["Visions cast today"](current).ActiveSpells, Is.EqualTo(ActiveAdventureSpells.Fly));
        }

        [Test]
        public void MoraleAndLuckForTheNextBattleAreSignedAndTheBuildingsThatGaveThemAreListedByName()
        {
            var original = HeroModels.Christian();
            original = original with { VisitEffects = VisitedBuildingEffects.Stables };
            var current = original with { MoraleBonus = 2, LuckBonus = -1, VisitEffects = VisitedBuildingEffects.Stables | VisitedBuildingEffects.WateringHole };

            var changes = this.Diff(original, current);

            Assert.That(changes.Select(Words), Is.EqualTo(new[]
            {
                ("Morale, next battle", "0", "+2"),
                ("Luck, next battle", "0", "-1"),
                ("Visited building: Watering hole", "no", "yes"),
            }));
            Assert.That(this.fake.HeroReverts["Visited building: Watering hole"](current).VisitEffects, Is.EqualTo(VisitedBuildingEffects.Stables), "the other building stays");
        }

        [Test]
        public void ThePlacesAHeroVisitedAreListedAsTheNumberOfVisitsOfEachKind()
        {
            var original = HeroModels.Christian();
            var current = original with { PlaceVisits = original.PlaceVisits.With(VisitedPlaceKind.LearningStone, 0b1011) };

            var change = this.Diff(original, current).Single();

            Assert.That(Words(change), Is.EqualTo(("Visited: learning stones", "0", "3")));
            Assert.That(this.fake.HeroReverts["Visited: learning stones"](current).PlaceVisits.Of(VisitedPlaceKind.LearningStone), Is.Zero);
        }

        [Test]
        public void TheSecondarySkillsAreOneChangeListingTheSlotsInOrderWithTheirLevels()
        {
            var original = HeroModels.Christian();
            var current = original with { SecondarySkills = original.SecondarySkills.WithSkill(SecondarySkill.Archery, SkillLevel.Beginner) };

            var change = this.Diff(original, current).Single();

            Assert.That(Words(change), Is.EqualTo(("Skills", "none", "Archery (Beginner)")));
            Assert.That(this.fake.HeroReverts["Skills"](current).SecondarySkills.GetSkillLevel(SecondarySkill.Archery), Is.EqualTo(SkillLevel.None));
        }

        [Test]
        public void ASkillBeyondTheEightSlotsIsListedAsNotShown()
        {
            var original = HeroModels.Christian();
            var full = SecondarySkill.All.Take(8).Aggregate(original.SecondarySkills, (skills, skill) => skills.WithSkill(skill, SkillLevel.Beginner));
            var extra = SecondarySkill.All[8];
            original = original with { SecondarySkills = full };
            var current = original with { SecondarySkills = full.WithSkillLevel(extra, SkillLevel.Expert) };

            var change = this.Diff(original, current).Single();

            Assert.That(change.Change.Before, Does.Not.Contain("not shown"));
            Assert.That(change.Change.After, Does.EndWith($"not shown: {extra.Name} (Expert)"));
        }

        [Test]
        public void AStackIsAChangeOfItsSlotAndPutBackAsItWas()
        {
            var original = HeroModels.Christian();
            var current = original with { Army = original.Army.WithSlot(1, Creature.Archer, 12) };

            var change = this.Diff(original, current).Single();

            Assert.That(change.Change.Description, Is.EqualTo("Slot 2"));
            Assert.That(change.Change.Before, Is.EqualTo(ChangeText.Empty));
            Assert.That(this.fake.HeroReverts["Slot 2"](current).Army.Slots[1], Is.Null);
        }

        [Test]
        public void TheWarMachinesAreOneChangeListedByNameInAnyOrder()
        {
            var original = HeroModels.Christian() with { WarMachines = [WarMachine.Ballista] };
            var current = original with { WarMachines = [WarMachine.FirstAidTent, WarMachine.Ballista] };

            var change = this.Diff(original, current).Single();

            Assert.That(change.Change.Description, Is.EqualTo("War machines"));
            Assert.That(change.Change.Before, Is.EqualTo("Ballista"));
            Assert.That(change.Change.After, Does.Contain("Ballista").And.Contain("First Aid Tent"));
            Assert.That(this.Diff(current, current with { WarMachines = [WarMachine.Ballista, WarMachine.FirstAidTent] }), Is.Empty, "the order of the machines is no change");
        }

        [Test]
        public void AnArtifactWornInASlotAndAScrollWithItsSpellAreListedBySlotAndPutBack()
        {
            var original = HeroModels.Christian();
            var current = original.WithEquippedArtifacts(original.EquippedArtifacts.WithEquipped(ArtifactSlots.Helmet, Artifact.SkullHelmet).WithEquipped(ArtifactSlots.Misc1, Artifact.SpellScroll, Spell.Fly));

            var changes = this.Diff(original, current).Where(c => c.Change.Area == ChangeAreas.Artifacts).ToList();

            Assert.That(changes.Select(Words), Is.EquivalentTo(new[]
            {
                ("Helmet", ChangeText.Empty, "Skull Helmet"),
                ("Misc1", ChangeText.Empty, SpellScroll.NameOf(Spell.Fly)),
            }));
            Assert.That(this.fake.HeroReverts["Helmet"](current).EquippedArtifacts.GetArtifact(ArtifactSlots.Helmet), Is.Null);
        }

        [Test]
        public void AnArtifactInTheBackpackIsAChangeOfItsSlotAndPutBackEmpty()
        {
            var original = HeroModels.Christian();
            var current = original with { Backpack = original.Backpack.WithSlot(2, Artifact.SkullHelmet) };

            var change = this.Diff(original, current).Single();

            Assert.That(Words(change), Is.EqualTo(("Slot 3", ChangeText.Empty, "Skull Helmet")));
            Assert.That(this.fake.HeroReverts["Slot 3"](current).Backpack.All[2], Is.Null);
        }

        [Test]
        public void TheSpellBookAndTheLearnedSpellsAreListedAndPutBack()
        {
            var original = HeroModels.Christian() with { SpellBook = new SpellBook(new HashSet<Spell>(), new HashSet<Spell>(), hasSpellBook: false) };
            var current = original with { SpellBook = original.SpellBook.WithHasSpellBook(true).WithLearned(Spell.Bless, true) };

            var changes = this.Diff(original, current);

            Assert.That(changes.Select(Words), Is.EqualTo(new[]
            {
                ("Spell book", "no", "yes"),
                ("Learned spells", "nothing forgotten", "learned: Bless"),
            }));
            Assert.That(this.fake.HeroReverts["Learned spells"](current).SpellBook.IsLearned(Spell.Bless), Is.False);
        }

        [Test]
        public void ThePrimarySkillsAndTheLevelAndThePositionAreListedByTheSharedWording()
        {
            var original = HeroModels.Christian();
            var current = original with { PrimarySkills = original.PrimarySkills with { Attack = 9 } };

            var changes = this.Diff(original, current.WithLevel(6));

            Assert.That(changes.Select(c => c.Change.Description), Is.EquivalentTo(new[] { "Attack", "Experience" }));
        }
    }
}
