namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    [TestFixture]
    internal sealed class Homm2EditorViewModelTests
    {
        private string folder = string.Empty;
        private Homm2EditorViewModel editor = null!;

        [SetUp]
        public void LoadASavegame()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-homm2-editor-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
            this.editor = new AppServices(settingsPath: Path.Combine(this.folder, "settings.json")).CreateHomm2Editor();
            this.editor.LoadGame(Homm2Saves.BrokenAllianceSave(24));
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void ALoadedSavegameShowsTheOverviewAndSelectsTheGameNode()
        {
            Assert.That(this.editor.CurrentPage, Is.SameAs(this.editor.Overview));
            Assert.That(this.editor.NavigationNodes, Has.Count.EqualTo(1));
            Assert.That(this.editor.NavigationNodes[0].Title, Is.EqualTo("Game"));
            Assert.That(this.editor.NavigationNodes[0].IsSelected, Is.True);
        }

        [Test]
        public void TheGameHoldsAPlayerForEachColourInTheGameWithItsHeroesAndTowns()
        {
            var document = this.editor.CurrentGame!.History.Current;
            var game = this.editor.NavigationNodes[0];

            var players = game.Children.Where(n => n.Kind == Homm2NavNodeKind.Player).ToList();

            Assert.That(players.Select(p => p.Title), Is.EqualTo(this.editor.Overview!.Players.Select(p => p.Color.ToString())));
            foreach (var player in players)
            {
                var color = (Homm2Color)player.Index;
                var heroes = document.Heroes().GetModels().Where(h => h.Owner == color).Select(h => h.Name).Order();
                var towns = document.Castles().GetModels().Where(c => c.IsUsed && c.Owner == color).Select(c => c.Name).Order();
                Assert.That(Titles(player, "Heroes"), Is.EqualTo(heroes), color.ToString());
                Assert.That(Titles(player, "Towns"), Is.EqualTo(towns), color.ToString());
            }
        }

        [Test]
        public void PickingAHeroNodeShowsThePageOfThatHero()
        {
            var hero = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == Homm2NavNodeKind.Hero);

            hero.IsSelected = true;

            var page = (Homm2HeroViewModel)this.editor.CurrentPage!;
            Assert.That(page.HeroId, Is.EqualTo(hero.Index));
            Assert.That(page.Name, Is.EqualTo(hero.Title));
        }

        [Test]
        public void PickingATownNodeShowsThePageOfThatTown()
        {
            var town = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == Homm2NavNodeKind.Castle);

            town.IsSelected = true;

            var page = (Homm2CastleViewModel)this.editor.CurrentPage!;
            Assert.That(page.CastleIndex, Is.EqualTo(town.Index));
            Assert.That(page.Name, Is.EqualTo(town.Title));
        }

        [Test]
        public void PickingTheHeroesOfAPlayerShowsThePlayerOnTheTabOfHeroes()
        {
            var tab = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == Homm2NavNodeKind.PlayerTab && n.Tab == 1);

            tab.IsSelected = true;

            var page = (Homm2PlayerViewModel)this.editor.CurrentPage!;
            Assert.That(page.Color, Is.EqualTo((Homm2Color)tab.Index));
            Assert.That(page.SelectedTab, Is.EqualTo(1));
        }

        [Test]
        public void OpeningAHeroFromAPlayersPageSelectsItsNodeInTheTree()
        {
            this.editor.ShowPlayer(Homm2Color.Blue, 1);
            var player = (Homm2PlayerViewModel)this.editor.CurrentPage!;

            player.Heroes[0].OpenCommand.Execute(null);

            var hero = (Homm2HeroViewModel)this.editor.CurrentPage!;
            Assert.That(Descendants(this.editor.NavigationNodes[0]).Single(n => n.Kind == Homm2NavNodeKind.Hero && n.Index == hero.HeroId).IsSelected, Is.True);
        }

        [Test]
        public void TheOverviewOpensThePageOfAPlayer()
        {
            this.editor.Overview!.OpenPlayerCommand.Execute(Homm2Color.Green);

            Assert.That(((Homm2PlayerViewModel)this.editor.CurrentPage!).Color, Is.EqualTo(Homm2Color.Green));
        }

        [Test]
        public void TheFilterNarrowsTheTreeToWhatMatches()
        {
            var hero = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == Homm2NavNodeKind.Hero);

            this.editor.HeroFilter = hero.Title;

            var visible = Descendants(this.editor.NavigationNodes[0]).Where(n => n.IsVisible && n.Kind == Homm2NavNodeKind.Hero).ToList();
            Assert.That(visible.Select(n => n.Title), Does.Contain(hero.Title));
            Assert.That(visible.Count, Is.LessThan(Descendants(this.editor.NavigationNodes[0]).Count(n => n.Kind == Homm2NavNodeKind.Hero)));

            this.editor.HeroFilter = string.Empty;
            Assert.That(Descendants(this.editor.NavigationNodes[0]).All(n => n.IsVisible), Is.True);
        }

        [Test]
        public void AnEditedHeroIsMarkedInTheTreeAndOnItsPageAndResetPutsItBack()
        {
            var node = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == Homm2NavNodeKind.Hero && n.Subtitle.Length > 0);
            this.editor.ShowHero(node.Index);
            var page = (Homm2HeroViewModel)this.editor.CurrentPage!;
            var experience = this.editor.CurrentGame!.History.Current.Heroes().GetModel(node.Index).Experience;

            this.editor.CurrentGame!.History.CreateHeroSession(node.Index).Update(h => h with { Experience = experience + 1000 });

            Assert.That(node.HasChanges, Is.True);
            Assert.That(this.editor.NavigationNodes[0].HasChanges, Is.True, "the nodes above the hero are marked");
            Assert.That(page.HasPendingChanges, Is.True);
            Assert.That(page.Facts.Single(f => f.Name == "Experience").Value, Is.EqualTo(experience + 1000));

            page.ResetCommand.Execute(null);

            Assert.That(node.HasChanges, Is.False);
            Assert.That(page.HasPendingChanges, Is.False);
            Assert.That(page.Facts.Single(f => f.Name == "Experience").Value, Is.EqualTo(experience));
        }

        [Test]
        public void AHeroThatChangesHandsMovesToTheGroupOfItsNewOwner()
        {
            var document = this.editor.CurrentGame!.History.Current;
            var hero = document.Heroes().GetModels().First(h => h.Owner == Homm2Color.Blue);

            this.editor.CurrentGame.History.CreateHeroSession(hero.HeroId).Update(h => h with { Owner = Homm2Color.Green });

            var owner = Descendants(this.editor.NavigationNodes[0]).Single(n => n.Kind == Homm2NavNodeKind.Player && n.Index == (int)Homm2Color.Green);
            Assert.That(Titles(owner, "Heroes"), Does.Contain(hero.Name));
        }

        [Test]
        public void ALoadedGameWithoutAHeroOrATownOfAPlayerSaysSo()
        {
            var document = this.editor.CurrentGame!.History.Current;
            var color = Enum.GetValues<Homm2Color>().First(c => document.Heroes().GetModels().All(h => h.Owner != c) && document.Castles().GetModels().All(t => t.Owner != c));
            var player = new Homm2PlayerViewModel(this.editor.CurrentGame!, color, _ => { }, _ => { });

            Assert.That(player.HasHeroes, Is.False);
            Assert.That(player.HasCastles, Is.False);
            Assert.That(player.Resources.Select(r => r.Name), Is.EqualTo(new[] { "Gold", "Wood", "Ore", "Mercury", "Sulfur", "Crystal", "Gems" }));
        }

        [Test]
        public void APlayerThatWasPutOutEarnsNothingWhateverTheIncomeTableHolds()
        {
            var green = new Homm2PlayerViewModel(this.editor.CurrentGame!, Homm2Color.Green, _ => { }, _ => { });
            var blue = new Homm2PlayerViewModel(this.editor.CurrentGame!, Homm2Color.Blue, _ => { }, _ => { });

            Assert.That(this.editor.CurrentGame!.History.Current.Players().GetModel((int)Homm2Color.Green).Income.Ore, Is.EqualTo(1), "the save holds 1 ore");
            Assert.That(green.Resources.Select(r => r.Note), Is.All.Empty);
            Assert.That(blue.Resources.Select(r => r.Note), Has.Some.Not.Empty);
        }

        [Test]
        public void EditingAResourceOnThePlayersPageChangesTheSavegameAndMarksTheRow()
        {
            this.editor.ShowPlayer(Homm2Color.Blue);
            var page = (Homm2PlayerViewModel)this.editor.CurrentPage!;
            var gold = page.Resources.Single(r => r.Name == "Gold");
            var before = gold.Value;

            gold.Value = before + 5_000;

            Assert.That(this.editor.CurrentGame!.History.Current.Players().GetModel((int)Homm2Color.Blue).Resources.Gold, Is.EqualTo(before + 5_000));
            Assert.That(gold.IsChanged, Is.True);
            Assert.That(gold.DeltaText, Is.EqualTo("+5,000 from save"));
            Assert.That(page.HasPendingChanges, Is.True);

            gold.Value = 99_999_999;
            Assert.That(gold.Value, Is.EqualTo(Constants.Player.MaxGold), "the value is kept within the limit");

            page.ResetCommand.Execute(null);
            Assert.That(gold.Value, Is.EqualTo(before));
            Assert.That(gold.IsChanged, Is.False);
        }

        [Test]
        public void TheQuickAddButtonsAddToTheResourcesAndStopAtTheLimit()
        {
            var page = new Homm2PlayerViewModel(this.editor.CurrentGame!, Homm2Color.Blue, _ => { }, _ => { });
            var gold = page.Resources.Single(r => r.Name == "Gold").Value;
            var wood = page.Resources.Single(r => r.Name == "Wood").Value;

            page.SetRichCommand.Execute(null);
            page.SetAllResourcesCommand.Execute(null);

            Assert.That(page.Resources.Single(r => r.Name == "Gold").Value, Is.EqualTo(gold + 100_000));
            Assert.That(page.Resources.Single(r => r.Name == "Wood").Value, Is.EqualTo(wood + 99));
        }

        [Test]
        public void EditingAHeroSkillOnItsPageChangesTheHero()
        {
            this.editor.ShowHero(7);
            var page = (Homm2HeroViewModel)this.editor.CurrentPage!;

            page.PrimarySkills.Single(r => r.Name == "Attack").Value = 20;

            Assert.That(this.editor.CurrentGame!.History.Current.Heroes().GetModel(7).PrimarySkills.Attack, Is.EqualTo(20));
            Assert.That(page.HasPendingChanges, Is.True);
        }

        [Test]
        public void TheArmyOfAHeroIsEditedThroughItsSlotsAndTheFinder()
        {
            var hero = this.HeroWithRoom();
            this.editor.ShowHero(hero);
            var army = ((Homm2HeroViewModel)this.editor.CurrentPage!).ArmyEditor;
            var model = () => this.editor.CurrentGame!.History.Current.Heroes().GetModel(hero).Army;
            var free = army.CreatureSlots.First(s => !s.HasCreature);

            free.SelectedCreature = "Titan";
            free.CreatureAmount = 7;

            Assert.That(model().Slots[free.SlotNumber], Is.EqualTo(new Homm2ArmySlot(Homm2Creature.Titan, 7)));
            Assert.That(free.Facts.Select(f => f.Label), Is.EqualTo(new[] { "Town", "Level", "Growth" }));

            army.Finder.AddAmount = 12;
            army.Finder.Choose(army.Finder.Items.Cast<object>().OfType<Homm2CreatureChoiceViewModel>().Single(c => c.Name == "Peasant"));
            Assert.That(model().Slots.OfType<Homm2ArmySlot>(), Has.Member(new Homm2ArmySlot(Homm2Creature.Peasant, 12)));

            army.MoveSlot(free.SlotNumber, 1);
            Assert.That(model().Slots[free.SlotNumber + 1]?.Creature, Is.EqualTo(Homm2Creature.Titan));

            army.ClearSlot(free.SlotNumber + 1);
            Assert.That(model().Slots[free.SlotNumber + 1], Is.Null);
        }

        [Test]
        public void TheGarrisonOfACastleIsAnArmyToo()
        {
            var castle = this.editor.CurrentGame!.History.Current.Castles().GetModels().First(c => c.IsUsed && c.Owner == Homm2Color.Blue);
            this.editor.ShowCastle(castle.Index);
            var garrison = ((Homm2CastleViewModel)this.editor.CurrentPage!).GarrisonEditor;

            garrison.SendToSlot(Homm2Creature.Paladin, 1, 10);

            Assert.That(this.editor.CurrentGame.History.Current.Castles().GetModel(castle.Index).Garrison.Slots[0], Is.EqualTo(new Homm2ArmySlot(Homm2Creature.Paladin, 10)));
            Assert.That(garrison.CreatureSlots[0].SelectedCreature, Is.EqualTo("Paladin"));
        }

        [Test]
        public void ANewStackIsNeverEmptyAndNeverMoreThanTheGameHolds()
        {
            var hero = this.HeroWithRoom();
            this.editor.ShowHero(hero);
            var slot = ((Homm2HeroViewModel)this.editor.CurrentPage!).ArmyEditor.CreatureSlots.First(s => !s.HasCreature);

            slot.SelectedCreature = "Peasant";
            Assert.That(this.editor.CurrentGame!.History.Current.Heroes().GetModel(hero).Army.Slots[slot.SlotNumber]!.Count, Is.EqualTo(1).Or.GreaterThan(0));

            slot.CreatureAmount = 1_000_000;
            Assert.That(this.editor.CurrentGame.History.Current.Heroes().GetModel(hero).Army.Slots[slot.SlotNumber]!.Count, Is.EqualTo((int)Constants.MaxCreatureCount));
        }

        [Test]
        public void TheSecondarySkillsOfAHeroAreEditedInSlotsAndInTheList()
        {
            this.editor.ShowHero(7);
            var skills = ((Homm2HeroViewModel)this.editor.CurrentPage!).SkillsEditor;
            var model = () => this.editor.CurrentGame!.History.Current.Heroes().GetModel(7).SecondarySkills;
            var before = model().Count;
            Assert.That(skills.SlotSummary, Is.EqualTo($"{before} of 8 slots used"));

            var wisdom = skills.AllSkills.First(r => r.LevelName == SkillLevelText.None);
            var skill = Homm2SecondarySkill.All.Single(s => s.Name == wisdom.Name);
            wisdom.LevelName = SkillLevelText.Advanced;
            Assert.That(model().LevelOf(skill), Is.EqualTo(Homm2SkillLevel.Advanced));
            Assert.That(model().Count, Is.EqualTo(before + 1), "a new skill is learned last");

            var slot = skills.Slots[before];
            Assert.That(slot.SkillName, Is.EqualTo(skill.Name));
            slot.LevelName = SkillLevelText.Expert;
            Assert.That(model().LevelOf(skill), Is.EqualTo(Homm2SkillLevel.Expert));

            skills.MoveSkill(before, -1);
            Assert.That(model().Entries[before - 1].Skill, Is.EqualTo(skill));

            wisdom.LevelName = SkillLevelText.None;
            Assert.That(model().LevelOf(skill), Is.EqualTo(Homm2SkillLevel.None));
            Assert.That(model().Count, Is.EqualTo(before));
        }

        [Test]
        public void ASkillCannotBeLearnedWhenAllEightSlotsAreTaken()
        {
            this.editor.ShowHero(7);
            var skills = ((Homm2HeroViewModel)this.editor.CurrentPage!).SkillsEditor;
            foreach (var row in skills.AllSkills.Where(r => r.LevelName == SkillLevelText.None).Take(8))
            {
                row.LevelName = SkillLevelText.Basic;
            }

            Assert.That(this.editor.CurrentGame!.History.Current.Heroes().GetModel(7).SecondarySkills.Count, Is.EqualTo(8));
            Assert.That(skills.AllSkills.Count(r => r.LevelName != SkillLevelText.None), Is.EqualTo(8), "the list shows what the hero has");
        }

        [Test]
        public void TheSpellBookAndTheSpellsOfAHeroAreEditedOnItsSpellsPage()
        {
            this.editor.ShowHero(7);
            var spells = ((Homm2HeroViewModel)this.editor.CurrentPage!).SpellsEditor;
            var model = () => this.editor.CurrentGame!.History.Current.Heroes().GetModel(7);
            Assert.That(spells.HasSpellBook, Is.True, "Maximus bought the book in save 10");

            spells.LearnAllCommand.Execute(null);
            Assert.That(model().Spells.Count, Is.EqualTo(Homm2Spell.All.Count));

            spells.Finder.Filter = "fire";
            spells.ForgetAllCommand.Execute(null);
            Assert.That(model().Spells.Contains(Homm2Spell.Fireball), Is.False);
            Assert.That(model().Spells.Contains(Homm2Spell.Cure), Is.True, "only the spells the list shows are forgotten");

            spells.HasSpellBook = false;
            Assert.That(model().HasSpellBook, Is.False);
            Assert.That(model().Spells.Contains(Homm2Spell.Cure), Is.True, "the spells stay marked without the book, as in the game");
            spells.HasSpellBook = true;
            Assert.That(model().HasSpellBook, Is.True);
        }

        private int HeroWithRoom() => this.editor.CurrentGame!.History.Current.Heroes().GetModels().First(h => h.Owner != null && h.Army.Slots.Any(s => s == null)).HeroId;

        private static IEnumerable<string> Titles(Homm2NavNodeViewModel player, string group) =>
            (player.Children.FirstOrDefault(c => c.Title == group)?.Children.Select(c => c.Title) ?? []).Order();

        private static IEnumerable<Homm2NavNodeViewModel> Descendants(Homm2NavNodeViewModel node) =>
            node.Children.SelectMany(c => Descendants(c).Prepend(c));
    }
}
