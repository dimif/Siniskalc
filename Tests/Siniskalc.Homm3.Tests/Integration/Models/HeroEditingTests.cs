using Siniskalc.Heroes.Entities;
using Siniskalc.Core.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Runtime;

using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Models
{
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroEditingTests
    {
        private static readonly string SavePath = Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1");

        [Test]
        public void CreaturesSwapMovesStacksAndIntoEmptySlots()
        {
            var army = new Army<Creature>(Constants.CreatureSlotCount)
                .WithSlot(0, TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Pikeman"), 10)
                .WithSlot(1, TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Archer"), 5);

            var swapped = army.Swap(0, 1);
            Assert.That(swapped.Slots[0], Is.EqualTo(new ArmySlot<Creature>(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Archer"), 5)));
            Assert.That(swapped.Slots[1], Is.EqualTo(new ArmySlot<Creature>(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Pikeman"), 10)));

            var moved = army.Swap(1, 2);
            Assert.That(moved.Slots[1], Is.Null);
            Assert.That(moved.Slots[2], Is.EqualTo(new ArmySlot<Creature>(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Archer"), 5)));
        }

        [Test]
        public void CreaturesWithoutSlotEmptiesIt()
        {
            var army = new Army<Creature>(Constants.CreatureSlotCount).WithSlot(0, TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Pikeman"), 10).WithSlot(1, TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Archer"), 5);

            var cleared = army.WithoutSlot(0);

            Assert.That(cleared.Slots[0], Is.Null);
            Assert.That(cleared.Slots[1]!.Count, Is.EqualTo(5));
            Assert.That(() => army.WithoutSlot(7), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void SecondarySkillsSwapKeepsLevelsWithTheirSkills()
        {
            var skills = ThreeSkills();

            var swapped = skills.Swap(0, 2);

            Assert.That(swapped.GetSkillInSlot(0), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Luck")));
            Assert.That(swapped.GetSkillInSlot(2), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Archery")));
            Assert.That(swapped.GetSkillLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Archery")), Is.EqualTo(SkillLevel.Beginner));
            Assert.That(swapped.GetSkillLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Wisdom")), Is.EqualTo(SkillLevel.Expert));
        }

        [Test]
        public void SecondarySkillsWithoutSlotMovesLaterSkillsUp()
        {
            var skills = ThreeSkills();

            var cleared = skills.WithoutSlot(0);

            Assert.That(cleared.SlotCount, Is.EqualTo(2));
            Assert.That(cleared.GetSkillInSlot(0), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Wisdom")));
            Assert.That(cleared.GetSkillInSlot(1), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Luck")));
            Assert.That(cleared.GetSkillLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Archery")), Is.EqualTo(SkillLevel.None));
        }

        [Test]
        public void SecondarySkillsWithSkillAppendsAndStopsAtEightSlots()
        {
            var skills = ThreeSkills().WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Tactics"), SkillLevel.Intermediate);
            Assert.That(skills.SlotCount, Is.EqualTo(4));
            Assert.That(skills.GetSkillInSlot(3), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Tactics")));

            var levelled = skills.WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Tactics"), SkillLevel.Expert);
            Assert.That(levelled.SlotCount, Is.EqualTo(4));
            Assert.That(levelled.GetSkillLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Tactics")), Is.EqualTo(SkillLevel.Expert));

            var full = skills
                .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Scouting"), SkillLevel.Beginner)
                .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Learning"), SkillLevel.Beginner)
                .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Offense"), SkillLevel.Beginner)
                .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Armorer"), SkillLevel.Beginner);
            Assert.That(full.SlotCount, Is.EqualTo(Constants.SkillSlots.Max));

            // The ninth skill has no slot, but still has its level: the game applies it without showing it.
            var withExtra = full.WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery"), SkillLevel.Intermediate);
            Assert.That(withExtra.SlotCount, Is.EqualTo(Constants.SkillSlots.Max));
            Assert.That(withExtra.GetSlotOf(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery")), Is.EqualTo(-1));
            Assert.That(withExtra.GetSkillLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery")), Is.EqualTo(SkillLevel.Intermediate));
            Assert.That(withExtra.ExtraSkills, Is.EqualTo(new[] { TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery") }));
        }

        [Test]
        public void ExtraSkillsCanBeLevelledRemovedShownAndHidden()
        {
            var full = ThreeSkills()
                .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Tactics"), SkillLevel.Beginner)
                .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Scouting"), SkillLevel.Beginner)
                .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Learning"), SkillLevel.Beginner)
                .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Offense"), SkillLevel.Beginner)
                .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Armorer"), SkillLevel.Beginner)
                .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery"), SkillLevel.Beginner);

            // Raising an extra skill keeps it extra.
            var raised = full.WithSkillLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery"), SkillLevel.Expert);
            Assert.That(raised.GetSlotOf(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery")), Is.EqualTo(-1));
            Assert.That(raised.GetSkillLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery")), Is.EqualTo(SkillLevel.Expert));

            // It cannot be shown while the slots are full; freeing a slot allows it.
            Assert.That(raised.WithExtraSkillInSlot(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery")), Is.SameAs(raised));
            var shown = raised.WithoutSlot(0).WithExtraSkillInSlot(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery"));
            Assert.That(shown.GetSlotOf(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery")), Is.EqualTo(7));
            Assert.That(shown.ExtraSkills, Is.Empty);

            // Hiding keeps the level and moves the later skills up.
            var hidden = full.WithSlotAsExtra(1);
            Assert.That(hidden.SlotCount, Is.EqualTo(7));
            Assert.That(hidden.GetSlotOf(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Luck")), Is.EqualTo(1));
            Assert.That(hidden.GetSkillLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Wisdom")), Is.EqualTo(SkillLevel.Expert));
            Assert.That(hidden.ExtraSkills, Is.EquivalentTo(new[] { TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Wisdom"), TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery") }));

            // No level removes the skill, wherever it is.
            Assert.That(raised.WithSkillLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery"), SkillLevel.None).GetSkillLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery")), Is.EqualTo(SkillLevel.None));
            Assert.That(full.WithSkillLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Archery"), SkillLevel.None).GetSlotOf(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Wisdom")), Is.EqualTo(0));
        }

        [Test]
        public void SkillsBeyondTheEightSlotsSurviveBeingWritten()
        {
            var game = LoadPrepared();
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));

            session.Update(h => h with
            {
                SecondarySkills = h.SecondarySkills
                    .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Tactics"), SkillLevel.Beginner)
                    .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Scouting"), SkillLevel.Beginner)
                    .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Learning"), SkillLevel.Beginner)
                    .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Offense"), SkillLevel.Beginner)
                    .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Armorer"), SkillLevel.Beginner)
                    .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery"), SkillLevel.Expert),
            });

            var reloaded = TestServices.GameService.LoadGameFromBytes(game.GetBytesToSave(), SavePath);
            var skills = reloaded.History.CreateHeroSession(reloaded.History.Current.Heroes().FindByName("Christian")).Current.SecondarySkills;

            Assert.That(skills.SlotCount, Is.EqualTo(Constants.SkillSlots.Max));
            Assert.That(skills.ExtraSkills, Is.EqualTo(new[] { TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery") }));
            Assert.That(skills.GetSkillLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Sorcery")), Is.EqualTo(SkillLevel.Expert));
        }

        [Test]
        public void SpellBookLearnsAndForgetsWithoutTouchingAvailableSpells()
        {
            var book = new SpellBook(new HashSet<Spell> { TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MagicArrow") }, new HashSet<Spell> { TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Visions") });

            var edited = book.WithLearned(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Visions"), true).WithLearned(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MagicArrow"), false);

            Assert.That(edited.IsLearned(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Visions")), Is.True);
            Assert.That(edited.IsLearned(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MagicArrow")), Is.False);
            Assert.That(edited.IsAvailable(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Visions")), Is.True);
            Assert.That(book.IsLearned(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MagicArrow")), Is.True);
        }

        [Test]
        public void LearnedSpellChangesArePendingAndRevertible()
        {
            var game = LoadPrepared();
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));
            var before = session.Current.SpellBook;
            var toLearn = TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.All.First(s => !before.IsLearned(s));

            session.Update(h => h with { SpellBook = h.SpellBook.WithLearned(toLearn, true) });

            var change = game.History.GetPendingChanges().Single(c => c.Change.Area == ChangeAreas.Spells);
            Assert.That((change.Change.Description, change.Change.After), Is.EqualTo((toLearn.Name, "learned")));

            change.Revert(game.History);
            Assert.That(session.Current.SpellBook.IsLearned(toLearn), Is.False);
            Assert.That(game.History.GetPendingChanges(), Is.Empty);
        }

        [Test]
        public void SecondarySkillsEditsSurviveSerialization()
        {
            var game = LoadPrepared();
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));

            session.Update(h => h with { SecondarySkills = h.SecondarySkills.Swap(0, 2).WithoutSlot(1) });

            var skills = session.Current.SecondarySkills;
            Assert.That(skills.SlotCount, Is.EqualTo(2));
            Assert.That(skills.GetSkillInSlot(0), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Luck")));
            Assert.That(skills.GetSkillInSlot(1), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Archery")));
            Assert.That(skills.GetSkillLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Wisdom")), Is.EqualTo(SkillLevel.None));
        }

        [Test]
        public void NothingIsPendingBeforeAnyEdit()
        {
            Assert.That(LoadPrepared().History.GetPendingChanges(), Is.Empty);
        }

        [Test]
        public void PendingChangesListEachEditedValue()
        {
            var game = LoadPrepared();
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));

            session.Update(h => h with
            {
                PrimarySkills = h.PrimarySkills with { Attack = 77 },
                SpellPoints = 123,
                Army = h.Army.Swap(0, 1).WithoutSlot(2),
                SecondarySkills = h.SecondarySkills.WithoutSlot(0),
            });

            var changes = game.History.GetPendingChanges();
            var descriptions = changes.Select(c => c.Change.Description).ToList();

            Assert.That(changes, Has.All.Matches<PendingChange>(c => c.OwnerName == "Christian"));
            Assert.That(descriptions, Is.SupersetOf(new[] { "Attack", "Spell points", "Slot 1", "Slot 2", "Slot 3", "Skills" }));
            Assert.That(changes.Single(c => c.Change.Description == "Attack").Change.After, Is.EqualTo("77"));
            Assert.That(changes.Single(c => c.Change.Description == "Slot 3").Change.After, Is.EqualTo(ChangeText.Empty));
            Assert.That(changes.Single(c => c.Change.Description == "Slot 1").Path, Is.EqualTo("Christian › Creatures"));
        }

        [Test]
        public void RevertChangePutsBackOnlyThatValue()
        {
            var game = LoadPrepared();
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));
            var original = session.Current;

            session.Update(h => h with { PrimarySkills = h.PrimarySkills with { Attack = 77, Defense = 66 }, SpellPoints = 123 });
            game.History.GetPendingChanges().Single(c => c.Change.Description == "Attack").Revert(game.History);

            Assert.That(session.Current.PrimarySkills.Attack, Is.EqualTo(original.PrimarySkills.Attack));
            Assert.That(session.Current.PrimarySkills.Defense, Is.EqualTo((byte)66));
            Assert.That(session.Current.SpellPoints, Is.EqualTo((ushort)123));
            Assert.That(game.History.GetPendingChanges().Select(c => c.Change.Description), Is.EquivalentTo(new[] { "Defense", "Spell points" }));
        }

        [Test]
        public void RevertingEveryChangeLeavesNothingPending()
        {
            var game = LoadPrepared();
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));

            session.Update(h => h with
            {
                HeroClass = TestCatalog.For(GameVersion.HornOfTheAbyss).HeroClasses.ById("Knight"),
                HeroLevel = new HeroLevel { Level = 20, Experience = 81961 },
                Army = h.Army.Swap(0, 3).WithSlot(6, TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Angel"), 3),
                SecondarySkills = h.SecondarySkills.Swap(0, 1).WithoutSlot(2),
                EquippedArtifacts = h.EquippedArtifacts.WithEquipped(ArtifactSlots.Feet, TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("BootsOfLevitation")),
                Backpack = h.Backpack.WithSlot(0, TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("OrbOfInhibition")),
                WarMachines = [TestCatalog.For(GameVersion.HornOfTheAbyss).WarMachines.ById("Ballista")],
            });

            foreach (var change in game.History.GetPendingChanges())
            {
                change.Revert(game.History);
            }

            Assert.That(game.History.GetPendingChanges().Select(c => $"{c.Change.Area}/{c.Change.Description}"), Is.Empty);
        }

        [Test]
        public void PuttingBackTheOriginalDocumentRestoresEveryEdit()
        {
            var game = LoadPrepared();
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));

            session.Update(h => h with { SpellPoints = 5 });
            game.History.Apply(game.History.Original);

            Assert.That(game.History.GetPendingChanges(), Is.Empty);
            Assert.That(game.History.CanUndo, Is.True);
        }

        private static SecondarySkills ThreeSkills()
        {
            var empty = new SecondarySkills(new Dictionary<SecondarySkill, SkillLevel>(), []);
            return empty
                .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Archery"), SkillLevel.Beginner)
                .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Wisdom"), SkillLevel.Expert)
                .WithSkill(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.ById("Luck"), SkillLevel.Intermediate);
        }

        /// <summary>
        /// Loads the HotA save with Christian given a known army and skills, as if that was how the save was loaded.
        /// </summary>
        internal static Game LoadPrepared()
        {
            var game = SavedGames.Get(SavePath);
            game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian")).Update(h => h with
            {
                Army = new Army<Creature>(Constants.CreatureSlotCount)
                    .WithSlot(0, TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Pikeman"), 10)
                    .WithSlot(1, TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Archer"), 5)
                    .WithSlot(2, TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Griffin"), 3),
                SecondarySkills = ThreeSkills(),
            });

            return TestServices.GameService.LoadGameFromBytes(game.GetBytesToSave(), SavePath);
        }
    }
}
