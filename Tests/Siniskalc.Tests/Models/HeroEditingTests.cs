using Siniskalc;
using Siniskalc.Core.Data;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Models
{
    [TestFixture]
    internal sealed class HeroEditingTests
    {
        private static readonly string SavePath = Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1");

        [Test]
        public void CreaturesSwapMovesStacksAndIntoEmptySlots()
        {
            var army = new Army([])
                .WithSlot(1, Creature.Pikeman, 10)
                .WithSlot(2, Creature.Archer, 5);

            var swapped = army.Swap(1, 2);
            Assert.That(swapped.GetSlot(1), Is.EqualTo(new ArmySlot(1, Creature.Archer.Code, 5)));
            Assert.That(swapped.GetSlot(2), Is.EqualTo(new ArmySlot(2, Creature.Pikeman.Code, 10)));

            var moved = army.Swap(2, 3);
            Assert.That(moved.FindSlot(2), Is.Null);
            Assert.That(moved.GetSlot(3), Is.EqualTo(new ArmySlot(3, Creature.Archer.Code, 5)));
        }

        [Test]
        public void CreaturesWithoutSlotEmptiesIt()
        {
            var army = new Army([]).WithSlot(1, Creature.Pikeman, 10).WithSlot(2, Creature.Archer, 5);

            var cleared = army.WithoutSlot(1);

            Assert.That(cleared.FindSlot(1), Is.Null);
            Assert.That(cleared.GetSlot(2).CreatureCount, Is.EqualTo(5u));
            Assert.That(() => army.WithoutSlot(8), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void SecondarySkillsSwapKeepsLevelsWithTheirSkills()
        {
            var skills = ThreeSkills();

            var swapped = skills.Swap(0, 2);

            Assert.That(swapped.GetSkillInSlot(0), Is.EqualTo(SecondarySkill.Luck));
            Assert.That(swapped.GetSkillInSlot(2), Is.EqualTo(SecondarySkill.Archery));
            Assert.That(swapped.GetSkillLevel(SecondarySkill.Archery), Is.EqualTo(SkillLevel.Beginner));
            Assert.That(swapped.GetSkillLevel(SecondarySkill.Wisdom), Is.EqualTo(SkillLevel.Expert));
        }

        [Test]
        public void SecondarySkillsWithoutSlotMovesLaterSkillsUp()
        {
            var skills = ThreeSkills();

            var cleared = skills.WithoutSlot(0);

            Assert.That(cleared.SlotCount, Is.EqualTo(2));
            Assert.That(cleared.GetSkillInSlot(0), Is.EqualTo(SecondarySkill.Wisdom));
            Assert.That(cleared.GetSkillInSlot(1), Is.EqualTo(SecondarySkill.Luck));
            Assert.That(cleared.GetSkillLevel(SecondarySkill.Archery), Is.Null);
        }

        [Test]
        public void SecondarySkillsWithSkillAppendsAndStopsAtEightSlots()
        {
            var skills = ThreeSkills().WithSkill(SecondarySkill.Tactics, SkillLevel.Intermediate);
            Assert.That(skills.SlotCount, Is.EqualTo(4));
            Assert.That(skills.GetSkillInSlot(3), Is.EqualTo(SecondarySkill.Tactics));

            var levelled = skills.WithSkill(SecondarySkill.Tactics, SkillLevel.Expert);
            Assert.That(levelled.SlotCount, Is.EqualTo(4));
            Assert.That(levelled.GetSkillLevel(SecondarySkill.Tactics), Is.EqualTo(SkillLevel.Expert));

            var full = skills
                .WithSkill(SecondarySkill.Scouting, SkillLevel.Beginner)
                .WithSkill(SecondarySkill.Learning, SkillLevel.Beginner)
                .WithSkill(SecondarySkill.Offense, SkillLevel.Beginner)
                .WithSkill(SecondarySkill.Armorer, SkillLevel.Beginner);
            Assert.That(full.SlotCount, Is.EqualTo(Constants.SkillSlots.Max));

            // The ninth skill has no slot, but still has its level: the game applies it without showing it.
            var withExtra = full.WithSkill(SecondarySkill.Sorcery, SkillLevel.Intermediate);
            Assert.That(withExtra.SlotCount, Is.EqualTo(Constants.SkillSlots.Max));
            Assert.That(withExtra.GetSlotOf(SecondarySkill.Sorcery), Is.EqualTo(-1));
            Assert.That(withExtra.GetSkillLevel(SecondarySkill.Sorcery), Is.EqualTo(SkillLevel.Intermediate));
            Assert.That(withExtra.ExtraSkills, Is.EqualTo(new[] { SecondarySkill.Sorcery }));
        }

        [Test]
        public void ExtraSkillsCanBeLevelledRemovedShownAndHidden()
        {
            var full = ThreeSkills()
                .WithSkill(SecondarySkill.Tactics, SkillLevel.Beginner)
                .WithSkill(SecondarySkill.Scouting, SkillLevel.Beginner)
                .WithSkill(SecondarySkill.Learning, SkillLevel.Beginner)
                .WithSkill(SecondarySkill.Offense, SkillLevel.Beginner)
                .WithSkill(SecondarySkill.Armorer, SkillLevel.Beginner)
                .WithSkill(SecondarySkill.Sorcery, SkillLevel.Beginner);

            // Raising an extra skill keeps it extra.
            var raised = full.WithSkillLevel(SecondarySkill.Sorcery, SkillLevel.Expert);
            Assert.That(raised.GetSlotOf(SecondarySkill.Sorcery), Is.EqualTo(-1));
            Assert.That(raised.GetSkillLevel(SecondarySkill.Sorcery), Is.EqualTo(SkillLevel.Expert));

            // It cannot be shown while the slots are full; freeing a slot allows it.
            Assert.That(raised.WithExtraSkillInSlot(SecondarySkill.Sorcery), Is.SameAs(raised));
            var shown = raised.WithoutSlot(0).WithExtraSkillInSlot(SecondarySkill.Sorcery);
            Assert.That(shown.GetSlotOf(SecondarySkill.Sorcery), Is.EqualTo(7));
            Assert.That(shown.ExtraSkills, Is.Empty);

            // Hiding keeps the level and moves the later skills up.
            var hidden = full.WithSlotAsExtra(1);
            Assert.That(hidden.SlotCount, Is.EqualTo(7));
            Assert.That(hidden.GetSlotOf(SecondarySkill.Luck), Is.EqualTo(1));
            Assert.That(hidden.GetSkillLevel(SecondarySkill.Wisdom), Is.EqualTo(SkillLevel.Expert));
            Assert.That(hidden.ExtraSkills, Is.EquivalentTo(new[] { SecondarySkill.Wisdom, SecondarySkill.Sorcery }));

            // No level removes the skill, wherever it is.
            Assert.That(raised.WithSkillLevel(SecondarySkill.Sorcery, null).GetSkillLevel(SecondarySkill.Sorcery), Is.Null);
            Assert.That(full.WithSkillLevel(SecondarySkill.Archery, null).GetSlotOf(SecondarySkill.Wisdom), Is.EqualTo(0));
        }

        [Test]
        public void ExtraSkillsSurviveSerialization()
        {
            var game = LoadPrepared();
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));

            session.Update(h => h with
            {
                SecondarySkills = h.SecondarySkills
                    .WithSkill(SecondarySkill.Tactics, SkillLevel.Beginner)
                    .WithSkill(SecondarySkill.Scouting, SkillLevel.Beginner)
                    .WithSkill(SecondarySkill.Learning, SkillLevel.Beginner)
                    .WithSkill(SecondarySkill.Offense, SkillLevel.Beginner)
                    .WithSkill(SecondarySkill.Armorer, SkillLevel.Beginner)
                    .WithSkill(SecondarySkill.Sorcery, SkillLevel.Expert),
            });

            var reloaded = TestServices.GameService.LoadGameFromBytes(game.GetBytesToSave(), SavePath);
            var skills = reloaded.History.CreateHeroSession(reloaded.History.Current.Heroes().FindByName("Christian")).Current.SecondarySkills;

            Assert.That(skills.SlotCount, Is.EqualTo(Constants.SkillSlots.Max));
            Assert.That(skills.ExtraSkills, Is.EqualTo(new[] { SecondarySkill.Sorcery }));
            Assert.That(skills.GetSkillLevel(SecondarySkill.Sorcery), Is.EqualTo(SkillLevel.Expert));
        }

        [Test]
        public void SpellBookLearnsAndForgetsWithoutTouchingAvailableSpells()
        {
            var book = new SpellBook(new HashSet<Spell> { Spell.MagicArrow }, new HashSet<Spell> { Spell.Visions });

            var edited = book.WithLearned(Spell.Visions, true).WithLearned(Spell.MagicArrow, false);

            Assert.That(edited.IsLearned(Spell.Visions), Is.True);
            Assert.That(edited.IsLearned(Spell.MagicArrow), Is.False);
            Assert.That(edited.IsAvailable(Spell.Visions), Is.True);
            Assert.That(book.IsLearned(Spell.MagicArrow), Is.True);
        }

        [Test]
        public void LearnedSpellChangesArePendingAndRevertible()
        {
            var game = LoadPrepared();
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));
            var before = session.Current.SpellBook;
            var toLearn = Spell.All.First(s => !before.IsLearned(s));

            session.Update(h => h with { SpellBook = h.SpellBook.WithLearned(toLearn, true) });

            var change = game.History.GetPendingChanges().Single(c => c.Change.Area == ChangeAreas.Spells);
            Assert.That(change.Change.After, Does.Contain(toLearn.Name));

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
            Assert.That(skills.GetSkillInSlot(0), Is.EqualTo(SecondarySkill.Luck));
            Assert.That(skills.GetSkillInSlot(1), Is.EqualTo(SecondarySkill.Archery));
            Assert.That(skills.GetSkillLevel(SecondarySkill.Wisdom), Is.Null);
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
                PrimarySkills = h.PrimarySkills with { Atk = 77 },
                SpellPoints = 123,
                Army = h.Army.Swap(1, 2).WithoutSlot(3),
                SecondarySkills = h.SecondarySkills.WithoutSlot(0),
            });

            var changes = game.History.GetPendingChanges();
            var descriptions = changes.Select(c => c.Change.Description).ToList();

            Assert.That(changes, Has.All.Matches<PendingChange>(c => c.OwnerName == "Christian"));
            Assert.That(descriptions, Is.SupersetOf(new[] { "Attack", "Spell points", "Slot 1", "Slot 2", "Slot 3", "Skills" }));
            Assert.That(changes.Single(c => c.Change.Description == "Attack").Change.After, Is.EqualTo("77"));
            Assert.That(changes.Single(c => c.Change.Description == "Slot 3").Change.After, Is.EqualTo(HeroDiffer.Empty));
            Assert.That(changes.Single(c => c.Change.Description == "Slot 1").Path, Is.EqualTo("Christian › Creatures"));
        }

        [Test]
        public void RevertChangePutsBackOnlyThatValue()
        {
            var game = LoadPrepared();
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));
            var original = session.Current;

            session.Update(h => h with { PrimarySkills = h.PrimarySkills with { Atk = 77, Def = 66 }, SpellPoints = 123 });
            game.History.GetPendingChanges().Single(c => c.Change.Description == "Attack").Revert(game.History);

            Assert.That(session.Current.PrimarySkills.Atk, Is.EqualTo(original.PrimarySkills.Atk));
            Assert.That(session.Current.PrimarySkills.Def, Is.EqualTo((byte)66));
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
                HeroClass = HeroClass.Knight,
                HeroLevel = new HeroLevel { Level = 20, Experience = 81961 },
                Army = h.Army.Swap(1, 4).WithSlot(7, Creature.Angel, 3),
                SecondarySkills = h.SecondarySkills.Swap(0, 1).WithoutSlot(2),
                EquippedArtifacts = h.EquippedArtifacts.WithEquipped(ArtifactSlots.Feet, Artifact.BootsOfLevitation),
                Backpack = h.Backpack.WithSlot(0, Artifact.OrbOfInhibition),
                WarMachines = [WarMachine.Ballista],
            });

            foreach (var change in game.History.GetPendingChanges())
            {
                change.Revert(game.History);
            }

            Assert.That(game.History.GetPendingChanges().Select(c => $"{c.Change.Area}/{c.Change.Description}"), Is.Empty);
        }

        [Test]
        public void HistoryOriginalRestoresEverything()
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
            var empty = new SecondarySkills(new Dictionary<SecondarySkill, SkillLevel?>(), []);
            return empty
                .WithSkill(SecondarySkill.Archery, SkillLevel.Beginner)
                .WithSkill(SecondarySkill.Wisdom, SkillLevel.Expert)
                .WithSkill(SecondarySkill.Luck, SkillLevel.Intermediate);
        }

        /// <summary>
        /// Loads the HotA save with Christian given a known army and skills, as if that was how the save was loaded.
        /// </summary>
        internal static Game LoadPrepared()
        {
            var game = TestServices.GameService.LoadGameFromFile(SavePath);
            game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian")).Update(h => h with
            {
                Army = new Army([])
                    .WithSlot(1, Creature.Pikeman, 10)
                    .WithSlot(2, Creature.Archer, 5)
                    .WithSlot(3, Creature.Griffin, 3),
                SecondarySkills = ThreeSkills(),
            });

            return TestServices.GameService.LoadGameFromBytes(game.GetBytesToSave(), SavePath);
        }
    }
}
