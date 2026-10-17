namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
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
            this.editor.LoadGame(Saves.BrokenAllianceSave(24));
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

            var players = game.Children.Where(n => n.Kind == NavNodeKind.Player).ToList();

            Assert.That(players.Select(p => p.Title), Is.EqualTo(this.editor.Overview!.Players.Select(p => p.Name)));
            foreach (var player in players)
            {
                var color = (Color)player.Index;
                var heroes = document.Heroes().GetModels().Where(h => h.Owner == color).Select(h => h.Name).Order();
                var towns = document.Castles().GetModels().Where(c => c.IsUsed && c.Owner == color).Select(c => c.Name).Order();
                Assert.That(Titles(player, "Heroes"), Is.EqualTo(heroes), color.ToString());
                Assert.That(Titles(player, "Towns"), Is.EqualTo(towns), color.ToString());
            }
        }

        [Test]
        public void PickingAHeroNodeShowsThePageOfThatHero()
        {
            var hero = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == NavNodeKind.Hero);

            hero.IsSelected = true;

            var page = (HeroViewModel)this.editor.CurrentPage!;
            Assert.That(page.HeroId, Is.EqualTo(hero.Index));
            Assert.That(page.Name, Is.EqualTo(hero.Title));
        }

        [Test]
        public void PickingATownNodeShowsThePageOfThatTown()
        {
            var town = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == NavNodeKind.Castle);

            town.IsSelected = true;

            var page = (CastleViewModel)this.editor.CurrentPage!;
            Assert.That(page.CastleIndex, Is.EqualTo(town.Index));
            Assert.That(page.Name, Is.EqualTo(town.Title));
        }

        [Test]
        public void PickingTheHeroesOfAPlayerShowsThePlayerOnTheTabOfHeroes()
        {
            var tab = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == NavNodeKind.PlayerTab && n.Tab == 1);

            tab.IsSelected = true;

            var page = (PlayerViewModel)this.editor.CurrentPage!;
            Assert.That(page.Color, Is.EqualTo((Color)tab.Index));
            Assert.That(page.SelectedTab, Is.EqualTo(1));
        }

        [Test]
        public void OpeningAHeroFromAPlayersPageSelectsItsNodeInTheTree()
        {
            this.editor.ShowPlayer(Color.Blue, 1);
            var player = (PlayerViewModel)this.editor.CurrentPage!;

            player.Heroes[0].OpenCommand.Execute(null);

            var hero = (HeroViewModel)this.editor.CurrentPage!;
            Assert.That(Descendants(this.editor.NavigationNodes[0]).Single(n => n.Kind == NavNodeKind.Hero && n.Index == hero.HeroId).IsSelected, Is.True);
        }

        [Test]
        public void EachPlayerCardOfTheOverviewHoldsTheResourcesHeroesAndCastlesOfThePlayer()
        {
            var document = this.editor.CurrentGame!.History.Current;
            var blue = this.editor.Overview!.Players.Single(p => p.Name == "Blue");
            var model = document.Players().GetModel((int)Color.Blue);

            Assert.That(blue.HasResources, Is.True);
            Assert.That(blue.Gold, Is.EqualTo(model.Resources.Gold.ToString("N0")));
            Assert.That(blue.Heroes.Select(h => h.Name), Is.EquivalentTo(document.Heroes().GetModels().Where(h => h.Owner == Color.Blue).Select(h => h.Name)));
            Assert.That(blue.Castles.Select(c => c.Name), Is.EquivalentTo(document.Castles().GetModels().Where(c => c.IsUsed && c.Owner == Color.Blue).Select(c => c.Name)));
            Assert.That(blue.Castles.Select(c => c.Status), Has.All.Not.Empty);
        }

        [Test]
        public void TheOverviewOpensThePageOfAPlayer()
        {
            this.editor.Overview!.Players.Single(p => p.Name == "Green").OpenCommand.Execute(null);

            Assert.That(((PlayerViewModel)this.editor.CurrentPage!).Color, Is.EqualTo(Color.Green));
        }

        [Test]
        public void TheFilterNarrowsTheTreeToWhatMatches()
        {
            var hero = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == NavNodeKind.Hero);

            this.editor.NavigationFilter = hero.Title;

            var visible = Descendants(this.editor.NavigationNodes[0]).Where(n => n.IsVisible && n.Kind == NavNodeKind.Hero).ToList();
            Assert.That(visible.Select(n => n.Title), Does.Contain(hero.Title));
            Assert.That(visible.Count, Is.LessThan(Descendants(this.editor.NavigationNodes[0]).Count(n => n.Kind == NavNodeKind.Hero)));

            this.editor.NavigationFilter = string.Empty;
            Assert.That(Descendants(this.editor.NavigationNodes[0]).All(n => n.IsVisible), Is.True);
        }

        [Test]
        public void AnEditedHeroIsMarkedInTheTreeAndOnItsPageAndResetPutsItBack()
        {
            var node = Descendants(this.editor.NavigationNodes[0]).First(n => n.Kind == NavNodeKind.Hero && n.Subtitle.Length > 0);
            this.editor.ShowHero(node.Index);
            var page = (HeroViewModel)this.editor.CurrentPage!;
            var experience = this.editor.CurrentGame!.History.Current.Heroes().GetModel(node.Index).Experience;

            this.editor.CurrentGame!.History.CreateHeroSession(node.Index).Update(h => h with { Experience = experience + 1000 });

            Assert.That(node.HasChanges, Is.True);
            Assert.That(this.editor.NavigationNodes[0].HasChanges, Is.True, "the nodes above the hero are marked");
            Assert.That(page.HasPendingChanges, Is.True);
            Assert.That(page.LevelAndClass.Experience, Is.EqualTo(experience + 1000));

            page.ResetCommand.Execute(null);

            Assert.That(node.HasChanges, Is.False);
            Assert.That(page.HasPendingChanges, Is.False);
            Assert.That(page.LevelAndClass.Experience, Is.EqualTo(experience));
        }

        [Test]
        public void AHeroThatChangesHandsMovesToTheGroupOfItsNewOwner()
        {
            var document = this.editor.CurrentGame!.History.Current;
            var hero = document.Heroes().GetModels().First(h => h.Owner == Color.Blue);

            this.editor.CurrentGame.History.CreateHeroSession(hero.HeroId).Update(h => h with { Owner = Color.Green });

            var owner = Descendants(this.editor.NavigationNodes[0]).Single(n => n.Kind == NavNodeKind.Player && n.Index == (int)Color.Green);
            Assert.That(Titles(owner, "Heroes"), Does.Contain(hero.Name));
        }

        [Test]
        public void ALoadedGameWithoutAHeroOrATownOfAPlayerSaysSo()
        {
            var document = this.editor.CurrentGame!.History.Current;
            var color = Enum.GetValues<Color>().First(c => document.Heroes().GetModels().All(h => h.Owner != c) && document.Castles().GetModels().All(t => t.Owner != c));
            var player = new PlayerViewModel(this.editor.CurrentGame!, color, _ => { }, _ => { });

            Assert.That(player.HasHeroes, Is.False);
            Assert.That(player.HasCastles, Is.False);
            Assert.That(player.Resources.Select(r => r.Name), Is.EqualTo(new[] { "Gold", "Wood", "Ore", "Mercury", "Sulfur", "Crystal", "Gems" }));
        }

        [Test]
        public void AHeroMadeNeutralLeavesTheListOfHeroesOfItsPlayerAndHasNoMoveToFinish()
        {
            this.editor.ShowHero(7);
            var hero = (HeroViewModel)this.editor.CurrentPage!;
            var document = () => this.editor.CurrentGame!.History.Current;
            Assert.That(document().Players().GetModel((int)Color.Blue).HeroIds.Contains(7), Is.True);

            hero.SelectedOwner = "Neutral";

            Assert.That(document().Heroes().GetModel(7).Owner, Is.Null);
            Assert.That(document().Heroes().GetModel(7).Destination, Is.EqualTo(MapPosition.None));
            Assert.That(document().Players().GetModel((int)Color.Blue).HeroIds.Contains(7), Is.False);

            hero.SelectedOwner = "Green";

            Assert.That(document().Heroes().GetModel(7).Owner, Is.EqualTo(Color.Green));
            Assert.That(document().Players().GetModel((int)Color.Green).HeroIds.Contains(7), Is.True);

            this.editor.CurrentGame!.History.Undo();
            this.editor.CurrentGame!.History.Undo();
            Assert.That(document().Heroes().GetModel(7).Owner, Is.EqualTo(Color.Blue));
            Assert.That(document().Players().GetModel((int)Color.Blue).HeroIds.Contains(7), Is.True);
        }

        [Test]
        public void TheTavernOfAPlayerIsEditedByPickingAHeroNobodyOwns()
        {
            var game = this.editor.CurrentGame!;
            var blue = new PlayerViewModel(game, Color.Blue, _ => { }, _ => { });
            var free = game.History.Current.Heroes().GetModels().First(h => h.Owner == null && !game.History.Current.Players().GetModel((int)Color.Blue).Tavern.Contains(h.HeroId));

            Assert.That(blue.TavernPlaces.Select(p => p.Label), Is.EqualTo(new[] { "First hero", "Second hero" }));
            Assert.That(blue.TavernPlaces[0].Choices.Select(c => c.HeroId), Does.Contain(free.HeroId));

            blue.TavernPlaces[0].SelectedKey = free.HeroId;

            Assert.That(game.History.Current.Players().GetModel((int)Color.Blue).Tavern.First, Is.EqualTo(free.HeroId));
            Assert.That(blue.TavernPlaces[0].Selected!.Name, Is.EqualTo(free.Name));
            Assert.That(blue.TavernPlaces[1].Choices.Select(c => c.HeroId), Does.Not.Contain(free.HeroId), "a hero is offered in one place only");
        }

        [Test]
        public void APlayerThatWasPutOutEarnsNothingWhateverTheIncomeTableHolds()
        {
            var green = new PlayerViewModel(this.editor.CurrentGame!, Color.Green, _ => { }, _ => { });
            var blue = new PlayerViewModel(this.editor.CurrentGame!, Color.Blue, _ => { }, _ => { });

            Assert.That(this.editor.CurrentGame!.History.Current.Players().GetModel((int)Color.Green).Income.Ore, Is.EqualTo(1), "the save holds 1 ore");
            Assert.That(green.Resources.Select(r => r.Note), Is.All.Empty);
            Assert.That(blue.Resources.Select(r => r.Note), Has.Some.Not.Empty);
        }

        [Test]
        public void EditingAResourceOnThePlayersPageChangesTheSavegameAndMarksTheRow()
        {
            this.editor.ShowPlayer(Color.Blue);
            var page = (PlayerViewModel)this.editor.CurrentPage!;
            var gold = page.Resources.Single(r => r.Name == "Gold");
            var before = gold.Value;

            gold.Value = before + 5_000;

            Assert.That(this.editor.CurrentGame!.History.Current.Players().GetModel((int)Color.Blue).Resources.Gold, Is.EqualTo(before + 5_000));
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
            var page = new PlayerViewModel(this.editor.CurrentGame!, Color.Blue, _ => { }, _ => { });
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
            var page = (HeroViewModel)this.editor.CurrentPage!;

            page.PrimarySkills.Single(r => r.Name == "Attack").Value = 20;

            Assert.That(this.editor.CurrentGame!.History.Current.Heroes().GetModel(7).PrimarySkills.Attack, Is.EqualTo(20));
            Assert.That(page.HasPendingChanges, Is.True);
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

            Assert.That(model().Slots[free.SlotNumber], Is.EqualTo(new ArmySlot<Creature>(Creature.Titan, 7)));
            Assert.That(free.Facts.Select(f => f.Label), Is.EqualTo(new[] { "Town", "Level", "Growth" }));

            army.Finder.AddAmount = 12;
            army.Finder.Choose(army.Finder.Items.Cast<object>().OfType<CreatureChoiceViewModel>().Single(c => c.Name == "Peasant"));
            Assert.That(model().Slots.OfType<ArmySlot<Creature>>(), Has.Member(new ArmySlot<Creature>(Creature.Peasant, 12)));

            army.MoveSlot(free.SlotNumber, 1);
            Assert.That(model().Slots[free.SlotNumber + 1]?.Creature, Is.EqualTo(Creature.Titan));

            army.ClearSlot(free.SlotNumber + 1);
            Assert.That(model().Slots[free.SlotNumber + 1], Is.Null);
        }

        [Test]
        public void TheGarrisonOfACastleIsAnArmyToo()
        {
            var castle = this.editor.CurrentGame!.History.Current.Castles().GetModels().First(c => c.IsUsed && c.Owner == Color.Blue);
            this.editor.ShowCastle(castle.Index);
            var garrison = ((CastleViewModel)this.editor.CurrentPage!).GarrisonEditor;

            garrison.SendToSlot(Creature.Paladin, 1, 10);

            Assert.That(this.editor.CurrentGame.History.Current.Castles().GetModel(castle.Index).Garrison.Slots[0], Is.EqualTo(new ArmySlot<Creature>(Creature.Paladin, 10)));
            Assert.That(garrison.CreatureSlots[0].SelectedCreature, Is.EqualTo("Paladin"));
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
            var before = model().Count;
            Assert.That(skills.SlotSummary, Is.EqualTo($"{before} of 8 slots used"));

            var wisdom = skills.AllSkills.First(r => r.LevelName == SkillLevelText.None);
            var skill = SecondarySkill.All.Single(s => s.Name == wisdom.Name);
            wisdom.LevelName = SkillLevelText.Advanced;
            Assert.That(model().LevelOf(skill), Is.EqualTo(SkillLevel.Advanced));
            Assert.That(model().Count, Is.EqualTo(before + 1), "a new skill is learned last");

            var slot = skills.Slots[before];
            Assert.That(slot.SkillName, Is.EqualTo(skill.Name));
            slot.LevelName = SkillLevelText.Expert;
            Assert.That(model().LevelOf(skill), Is.EqualTo(SkillLevel.Expert));

            skills.MoveSkill(before, -1);
            Assert.That(model().Entries[before - 1].Skill, Is.EqualTo(skill));

            wisdom.LevelName = SkillLevelText.None;
            Assert.That(model().LevelOf(skill), Is.EqualTo(SkillLevel.None));
            Assert.That(model().Count, Is.EqualTo(before));
        }

        [Test]
        public void ASkillCannotBeLearnedWhenAllEightSlotsAreTaken()
        {
            this.editor.ShowHero(7);
            var skills = ((HeroViewModel)this.editor.CurrentPage!).SkillsEditor;
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
            var spells = ((HeroViewModel)this.editor.CurrentPage!).SpellsEditor;
            var model = () => this.editor.CurrentGame!.History.Current.Heroes().GetModel(7);
            Assert.That(spells.HasSpellBook, Is.True, "Maximus bought the book in save 10");

            spells.LearnAllCommand.Execute(null);
            Assert.That(model().Spells.Count, Is.EqualTo(Spell.All.Count));

            spells.Finder.Filter = "fire";
            spells.ForgetAllCommand.Execute(null);
            Assert.That(model().Spells.Contains(Spell.Fireball), Is.False);
            Assert.That(model().Spells.Contains(Spell.Cure), Is.True, "only the spells the list shows are forgotten");

            spells.HasSpellBook = false;
            Assert.That(model().HasSpellBook, Is.False);
            Assert.That(model().Spells.Contains(Spell.Cure), Is.True, "the spells stay marked without the book, as in the game");
            spells.HasSpellBook = true;
            Assert.That(model().HasSpellBook, Is.True);
        }

        [Test]
        public void AnArtifactEquippedFromTheFinderGoesInTheFirstFreeSlotAndMovesItsSkillBonus()
        {
            this.editor.ShowHero(7);
            var artifacts = ((HeroViewModel)this.editor.CurrentPage!).ArtifactsEditor;
            var before = this.editor.CurrentGame!.History.Current.Heroes().GetModel(7);

            artifacts.Finder.Choose(artifacts.Finder.Items.Cast<ArtifactChoiceViewModel>().Single(c => c.Artifact == Artifact.UltimateSwordOfDominion));

            var after = this.editor.CurrentGame.History.Current.Heroes().GetModel(7);
            Assert.That(after.Artifacts.Slots, Has.Member(Artifact.UltimateSwordOfDominion));
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

            ((ArtifactFinderViewModel)free.Finder).Choose(((ArtifactFinderViewModel)free.Finder).Items.Cast<ArtifactChoiceViewModel>().Single(c => c.Artifact == Artifact.UltimateCloakOfProtection));
            Assert.That(model().Artifacts.Slots[index], Is.EqualTo(Artifact.UltimateCloakOfProtection));
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
                artifacts.Finder.Choose(artifacts.Finder.Items.Cast<ArtifactChoiceViewModel>().First(c => c.Artifact == Artifact.AmmoCart));
            }

            Assert.That(artifacts.Slots.All(s => s.HasArtifact), Is.True);
            Assert.That(() => artifacts.Finder.Choose(artifacts.Finder.Items.Cast<ArtifactChoiceViewModel>().First()), Throws.Nothing);
        }

        [Test]
        public void TheHexTabEditsTheRawBytesOfAHeroAPlayerAndACastle()
        {
            this.editor.ShowHero(7);
            var hero = (HeroViewModel)this.editor.CurrentPage!;
            using (var target = hero.CreateByteEditTarget()!)
            {
                Assert.That(target.Title, Is.EqualTo("Maximus"));
                Assert.That(target.Regions.Select(r => r.Name), Does.Contain("Experience").And.Contain("Artifacts"));
                var level = target.Regions.Single(r => r.Name == "Level").Start;
                Assert.That(target.Update(bytes => { bytes[level] = 33; return bytes; }), Is.True);
            }

            Assert.That(this.editor.CurrentGame!.History.Current.Heroes().GetModel(7).Level, Is.EqualTo(33));
            Assert.That(hero.HasPendingChanges, Is.True);

            this.editor.ShowPlayer(Color.Blue);
            using (var target = ((PlayerViewModel)this.editor.CurrentPage!).CreateByteEditTarget()!)
            {
                Assert.That(target.Title, Is.EqualTo("Blue player"));
                Assert.That(target.Regions.Select(r => r.Name), Does.Contain("Resources"));
            }

            var castle = this.OpenCastleOf(Color.Blue);
            using (var target = castle.CreateByteEditTarget()!)
            {
                Assert.That(target.Title, Is.EqualTo(castle.Name));
                Assert.That(target.Regions.Select(r => r.Name), Does.Contain("Dwellings").And.Contain("Town buildings"));
            }
        }

        private CastleViewModel OpenCastleOf(Color owner)
        {
            var castle = this.editor.CurrentGame!.History.Current.Castles().GetModels().First(c => c.IsUsed && c.Owner == owner);
            this.editor.ShowCastle(castle.Index);
            return (CastleViewModel)this.editor.CurrentPage!;
        }

        [Test]
        public void TheBuildingsAndTheDwellingsOfACastleAreEditedWithBoxesAndSteppers()
        {
            this.editor.ShowCastle(this.editor.CurrentGame!.History.Current.Castles().FindByName("Xabran"));
            var page = (CastleViewModel)this.editor.CurrentPage!;
            var model = () => this.editor.CurrentGame!.History.Current.Castles().GetModel(page.CastleIndex);

            var tavern = page.Buildings.Single(b => b.Name == "Tavern");
            var had = tavern.IsChecked;
            tavern.IsChecked = !had;
            Assert.That(model().Town.HasFlag(TownBuildings.Tavern), Is.EqualTo(!had));

            var dwelling = page.Dwellings.First(d => !d.IsBuilt);
            var index = page.Dwellings.ToList().IndexOf(dwelling);
            dwelling.Built.Selected = dwelling.Built.Choices[1];
            dwelling.Waiting.Value = 40;
            Assert.That(model().DwellingLevel(index + 1), Is.EqualTo(1));
            Assert.That(model().Waiting(index + 1), Is.EqualTo(40));

            if (dwelling.Built.Choices.Count > 2)
            {
                dwelling.Built.Selected = dwelling.Built.Choices[2];
                Assert.That(model().DwellingLevel(index + 1), Is.EqualTo(2), "an upgrade replaces the dwelling");
                Assert.That(model().Waiting(index + 1), Is.EqualTo(40), "the creatures that wait stay");
            }

            dwelling.Waiting.Value = 100_000;
            Assert.That(dwelling.Waiting.Value, Is.EqualTo(Constants.Castle.MaxWaiting));
            Assert.That(page.HasPendingChanges, Is.True);
        }

        [Test]
        public void TheMageGuildIsEditedByLevelAndByPlace()
        {
            var page = this.OpenCastleOf(Color.Blue);
            var model = () => this.editor.CurrentGame!.History.Current.Castles().GetModel(page.CastleIndex);

            page.GuildLevelField.Selected = "Level 2";
            Assert.That(model().MageGuildLevel, Is.EqualTo(2));

            var slot = page.GuildLevels[1].Spells[0];
            Assert.That(slot.IsEnabled, Is.True);
            Assert.That(page.GuildLevels[1].Spells[3].IsEnabled, Is.False, "a level of this guild shows three spells, not four");
            slot.Selected = "Fireball";
            Assert.That(model().GuildSpell(2, 0), Is.EqualTo(Spell.Fireball));
            slot.Selected = "(none)";
            Assert.That(model().GuildSpell(2, 0), Is.Null);

            page.GuildLevelField.Selected = "None";
            Assert.That(model().MageGuildLevel, Is.EqualTo(0));
            Assert.That(page.GuildLevels[0].Spells[0].IsEnabled, Is.False, "the spells of a level that is not built are not edited");
        }

        [Test]
        public void ACastleChangesHandsWithTheListsOfBothPlayersInOneStepAndUndoPutsItAllBack()
        {
            var page = this.OpenCastleOf(Color.Red);
            var document = () => this.editor.CurrentGame!.History.Current;
            var red = () => document().Players().GetModel((int)Color.Red);
            var blue = () => document().Players().GetModel((int)Color.Blue);
            var blueBefore = blue().CastleIds.Count;
            Assert.That(red().CastleIds, Does.Contain(page.CastleIndex));

            page.OwnerField.Selected = "Blue";

            Assert.That(document().Castles().GetModel(page.CastleIndex).Owner, Is.EqualTo(Color.Blue));
            Assert.That(blue().CastleIds, Does.Contain(page.CastleIndex));
            Assert.That(blue().CastleIds.Count, Is.EqualTo(blueBefore + 1));
            Assert.That(red().CastleIds, Does.Not.Contain(page.CastleIndex));
            Assert.That(red().DaysLeft, Is.EqualTo(PlayerModel.DaysToOwnACastle), "a player that lost its last castle starts counting");

            this.editor.Undo();
            Assert.That(document().Castles().GetModel(page.CastleIndex).Owner, Is.EqualTo(Color.Red));
            Assert.That(red().CastleIds, Does.Contain(page.CastleIndex));
            Assert.That(blue().CastleIds.Count, Is.EqualTo(blueBefore));
            Assert.That(red().DaysLeft, Is.Null);
        }

        [Test]
        public void ACastleCannotGoToAPlayerWhoseListOfCastlesIsFull()
        {
            var page = this.OpenCastleOf(Color.Red);
            this.editor.CurrentGame!.History.CreatePlayerSession((int)Color.Blue).Update(p => p with { CastleIds = new Numbers(Enumerable.Range(40, Constants.Player.CastleListSize)) });

            page.OwnerField.Selected = "Blue";

            Assert.That(page.HasNotice, Is.True);
            Assert.That(this.editor.CurrentGame.History.Current.Castles().GetModel(page.CastleIndex).Owner, Is.EqualTo(Color.Red));
        }

        private int HeroWithRoom() => this.editor.CurrentGame!.History.Current.Heroes().GetModels().First(h => h.Owner != null && h.Army.Slots.Any(s => s == null)).HeroId;

        private static IEnumerable<string> Titles(NavNodeViewModel player, string group) =>
            (player.Children.FirstOrDefault(c => c.Title == group)?.Children.Select(c => c.Title) ?? []).Order();

        private static IEnumerable<NavNodeViewModel> Descendants(NavNodeViewModel node) =>
            node.Children.SelectMany(c => Descendants(c).Prepend(c));
    }
}
