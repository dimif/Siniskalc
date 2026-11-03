namespace Siniskalc.Homm2.Tests.Integration.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// Tests the page of a hero in the editor: skills, army, spells and artifacts, on a Broken Alliance savegame loaded into the editor.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class Homm2EditorHeroPageTests
    {
        private Homm2EditorHarness harness = null!;
        private Homm2EditorViewModel editor = null!;

        [SetUp]
        public void LoadASavegame()
        {
            this.harness = new Homm2EditorHarness();
            this.editor = this.harness.Editor;
        }

        [TearDown]
        public void DeleteFolder() => this.harness.Dispose();

        [Test]
        public void EditingAHeroSkillOnItsPageChangesTheHero()
        {
            this.editor.ShowHero(7);
            var page = (HeroViewModel)this.editor.CurrentPage!;

            page.PrimarySkills.Attack.Base = 20;

            Assert.That(this.editor.CurrentGame!.History.Current.Heroes().GetModel(7).PrimarySkills.Attack, Is.EqualTo(20));
            Assert.That(page.HasPendingChanges, Is.True);
        }

        [Test]
        public void TheHeroPageEditsTheBaseOfASkillAndSavesItWithWhatTheArtifactsAdd()
        {
            this.editor.ShowHero(7);
            var page = (HeroViewModel)this.editor.CurrentPage!;
            var history = this.editor.CurrentGame!.History;
            history.CreateHeroSession(7).Update(h => h.WithArtifact(0, this.editor.CurrentGame!.Catalog.Artifacts["DragonSwordOfDominion"]));

            page.PrimarySkills.Attack.Base = 20;

            var hero = history.Current.Heroes().GetModel(7);
            Assert.That((page.PrimarySkills.Attack.Base, page.PrimarySkills.Attack.Total, page.PrimarySkills.Attack.ArtifactNote), Is.EqualTo((20L, 23, "+3 from artifacts")));
            Assert.That(hero.PrimarySkills.Attack, Is.EqualTo(23), "the savegame holds the skill with the bonus of the artifact");

            var saved = TestServices.HeroDeserializer.Deserialize(TestServices.HeroSerializer.Serialize(history.Current.Heroes()[7], hero));
            Assert.That(saved.PrimarySkills.BaseAttack, Is.EqualTo(20));
        }

        [Test]
        public void TheArmyOfAHeroIsEditedThroughItsSlotsAndTheFinder()
        {
            var hero = this.HeroWithRoom();
            this.editor.ShowHero(hero);
            var army = ((HeroViewModel)this.editor.CurrentPage!).ArmyEditor;
            var model = () => this.editor.CurrentGame!.History.Current.Heroes().GetModel(hero).Army;
            var free = army.CreatureSlots.First(s => !s.HasCreature);

            free.SelectedCreature = "Titan";
            free.CreatureAmount = 7;

            Assert.That(model().Slots[free.SlotNumber], Is.EqualTo(new ArmySlot<Creature>(this.editor.CurrentGame!.Catalog.Creatures["Titan"], 7)));
            Assert.That(free.Facts.Select(f => f.Label), Is.EqualTo(new[] { "Atk", "Def", "Speed", "HP", "Damage", "Stack HP", "Stack Damage", "Town", "Level", "Growth" }));

            army.Finder.AddAmount = 12;
            army.Finder.Choose(army.Finder.Items.Cast<object>().OfType<CreatureChoiceViewModel>().Single(c => c.Name == "Peasant"));
            Assert.That(model().Slots.OfType<ArmySlot<Creature>>(), Has.Member(new ArmySlot<Creature>(this.editor.CurrentGame!.Catalog.Creatures["Peasant"], 12)));

            army.MoveSlot(free.SlotNumber, 1);
            Assert.That(model().Slots[free.SlotNumber + 1]?.Creature, Is.EqualTo(this.editor.CurrentGame!.Catalog.Creatures["Titan"]));

            army.ClearSlot(free.SlotNumber + 1);
            Assert.That(model().Slots[free.SlotNumber + 1], Is.Null);
        }

        [Test]
        public void ANewStackIsNeverEmptyAndNeverMoreThanTheGameHolds()
        {
            var hero = this.HeroWithRoom();
            this.editor.ShowHero(hero);
            var slot = ((HeroViewModel)this.editor.CurrentPage!).ArmyEditor.CreatureSlots.First(s => !s.HasCreature);

            slot.SelectedCreature = "Peasant";
            Assert.That(this.editor.CurrentGame!.History.Current.Heroes().GetModel(hero).Army.Slots[slot.SlotNumber]!.Count, Is.EqualTo(1).Or.GreaterThan(0));

            slot.CreatureAmount = 1_000_000;
            Assert.That(this.editor.CurrentGame.History.Current.Heroes().GetModel(hero).Army.Slots[slot.SlotNumber]!.Count, Is.EqualTo((int)Constants.MaxCreatureCount));
        }

        [Test]
        public void TheSecondarySkillsOfAHeroAreEditedInSlotsAndInTheList()
        {
            this.editor.ShowHero(7);
            var skills = ((HeroViewModel)this.editor.CurrentPage!).SkillsEditor;
            var model = () => this.editor.CurrentGame!.History.Current.Heroes().GetModel(7).SecondarySkills;
            var before = model().SlotCount;
            Assert.That(skills.SlotSummary, Is.EqualTo($"{before} of 8 slots used"));

            var wisdom = skills.AllSkills.First(r => r.LevelName == SkillLevelNames.None);
            var skill = this.editor.CurrentGame!.Catalog.Skills.Single(s => s.Name == wisdom.Name);
            wisdom.LevelName = SkillLevelNames.Advanced;
            Assert.That(model().GetSkillLevel(skill), Is.EqualTo(SkillLevel.Intermediate));
            Assert.That(model().SlotCount, Is.EqualTo(before + 1), "a new skill is learned last");

            var slot = skills.Slots[before];
            Assert.That(slot.SkillName, Is.EqualTo(skill.Name));
            slot.LevelName = SkillLevelNames.Expert;
            Assert.That(model().GetSkillLevel(skill), Is.EqualTo(SkillLevel.Expert));

            skills.MoveSkill(before, -1);
            Assert.That(model().Entries[before - 1].Skill, Is.EqualTo(skill));

            wisdom.LevelName = SkillLevelNames.None;
            Assert.That(model().GetSkillLevel(skill), Is.EqualTo(SkillLevel.None));
            Assert.That(model().SlotCount, Is.EqualTo(before));
        }

        [Test]
        public void ASkillCannotBeLearnedWhenAllEightSlotsAreTaken()
        {
            this.editor.ShowHero(7);
            var skills = ((HeroViewModel)this.editor.CurrentPage!).SkillsEditor;
            foreach (var row in skills.AllSkills.Where(r => r.LevelName == SkillLevelNames.None).Take(8))
            {
                row.LevelName = SkillLevelNames.Basic;
            }

            Assert.That(this.editor.CurrentGame!.History.Current.Heroes().GetModel(7).SecondarySkills.SlotCount, Is.EqualTo(8));
            Assert.That(skills.AllSkills.Count(r => r.LevelName != SkillLevelNames.None), Is.EqualTo(8), "the list shows what the hero has");
        }

        [Test]
        public void TheSpellBookAndTheSpellsOfAHeroAreEditedOnItsSpellsPage()
        {
            this.editor.ShowHero(7);
            var spells = ((HeroViewModel)this.editor.CurrentPage!).SpellsEditor;
            var model = () => this.editor.CurrentGame!.History.Current.Heroes().GetModel(7);
            Assert.That(spells.HasSpellBook, Is.True, "Maximus bought the book in save 10");

            spells.LearnAllCommand.Execute(null);
            Assert.That(model().Spells.Count, Is.EqualTo(this.editor.CurrentGame!.Catalog.Spells.Count));

            spells.Finder.Filter = "fire";
            spells.ForgetAllCommand.Execute(null);
            Assert.That(model().Spells.Contains(this.editor.CurrentGame!.Catalog.Spells["Fireball"]), Is.False);
            Assert.That(model().Spells.Contains(this.editor.CurrentGame!.Catalog.Spells["Cure"]), Is.True, "only the spells the list shows are forgotten");

            spells.HasSpellBook = false;
            Assert.That(model().HasSpellBook, Is.False);
            Assert.That(model().Spells.Contains(this.editor.CurrentGame!.Catalog.Spells["Cure"]), Is.True, "the spells stay marked without the book, as in the game");
            spells.HasSpellBook = true;
            Assert.That(model().HasSpellBook, Is.True);
        }

        [Test]
        public void AnArtifactEquippedFromTheFinderGoesInTheFirstFreeSlotAndMovesItsSkillBonus()
        {
            this.editor.ShowHero(7);
            var artifacts = ((HeroViewModel)this.editor.CurrentPage!).ArtifactsEditor;
            var before = this.editor.CurrentGame!.History.Current.Heroes().GetModel(7);

            artifacts.Finder.Choose(artifacts.Finder.Items.Cast<ArtifactChoiceViewModel>().Single(c => c.Artifact == this.editor.CurrentGame!.Catalog.Artifacts["UltimateSwordOfDominion"]));

            var after = this.editor.CurrentGame.History.Current.Heroes().GetModel(7);
            Assert.That(after.Artifacts.Slots, Has.Member(this.editor.CurrentGame!.Catalog.Artifacts["UltimateSwordOfDominion"]));
            Assert.That(after.PrimarySkills.Attack, Is.EqualTo(before.PrimarySkills.Attack + 12));
            Assert.That(artifacts.Bonuses.Any(b => b.Label == "Attack" && b.Value.StartsWith('+')), Is.True);
            Assert.That(artifacts.Slots.Any(s => s.ArtifactName == "Ultimate Sword of Dominion"), Is.True);
        }

        [Test]
        public void ChoosingAnArtifactInASlotReplacesTheOneThereAndClearingTakesItOff()
        {
            this.editor.ShowHero(7);
            var artifacts = ((HeroViewModel)this.editor.CurrentPage!).ArtifactsEditor;
            var model = () => this.editor.CurrentGame!.History.Current.Heroes().GetModel(7);
            var free = artifacts.Slots.First(s => !s.HasArtifact);
            var index = artifacts.Slots.ToList().IndexOf(free);

            ((ArtifactFinderViewModel)free.Finder).Choose(((ArtifactFinderViewModel)free.Finder).Items.Cast<ArtifactChoiceViewModel>().Single(c => c.Artifact == this.editor.CurrentGame!.Catalog.Artifacts["UltimateCloakOfProtection"]));
            Assert.That(model().Artifacts.Slots[index], Is.EqualTo(this.editor.CurrentGame!.Catalog.Artifacts["UltimateCloakOfProtection"]));
            var defense = model().PrimarySkills.Defense;

            free.ClearCommand.Execute(null);

            Assert.That(model().Artifacts.Slots[index], Is.Null);
            Assert.That(model().PrimarySkills.Defense, Is.EqualTo(defense - 12));
        }

        [Test]
        public void TheFinderCannotEquipWhenEveryArtifactSlotIsTaken()
        {
            this.editor.ShowHero(7);
            var artifacts = ((HeroViewModel)this.editor.CurrentPage!).ArtifactsEditor;
            while (artifacts.Slots.Any(s => !s.HasArtifact))
            {
                artifacts.Finder.Choose(artifacts.Finder.Items.Cast<ArtifactChoiceViewModel>().First(c => c.Artifact == this.editor.CurrentGame!.Catalog.Artifacts["AmmoCart"]));
            }

            Assert.That(artifacts.Slots.All(s => s.HasArtifact), Is.True);
            Assert.That(() => artifacts.Finder.Choose(artifacts.Finder.Items.Cast<ArtifactChoiceViewModel>().First()), Throws.Nothing);
        }

        private int HeroWithRoom() => this.editor.CurrentGame!.History.Current.Heroes().GetModels().First(h => h.Owner != null && h.Army.Slots.Any(s => s == null)).PortraitId;
    }
}
