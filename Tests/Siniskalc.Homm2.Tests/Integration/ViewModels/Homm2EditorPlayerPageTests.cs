namespace Siniskalc.Homm2.Tests.Integration.ViewModels
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the page of a player in the editor: its tavern, its days left and its resources, on a Broken Alliance savegame loaded into the editor.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class Homm2EditorPlayerPageTests
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
        public void AHeroMadeNeutralLeavesTheListOfHeroesOfItsPlayerAndHasNoMoveToFinish()
        {
            this.editor.ShowHero(7);
            var hero = (HeroViewModel)this.editor.CurrentPage!;
            var document = () => this.editor.CurrentGame!.History.Current;
            Assert.That(document().Players().GetLists(PlayerColor.Blue.Code).HeroIds.Contains(7), Is.True);

            hero.SelectedOwner = "Neutral";

            Assert.That(document().Heroes().GetModel(7).Owner, Is.EqualTo(PlayerColor.Neutral));
            Assert.That(document().Heroes().GetModel(7).Destination, Is.EqualTo(MapPosition.None));
            Assert.That(document().Players().GetLists(PlayerColor.Blue.Code).HeroIds.Contains(7), Is.False);

            hero.SelectedOwner = "Green";

            Assert.That(document().Heroes().GetModel(7).Owner, Is.EqualTo(PlayerColor.Green));
            Assert.That(document().Players().GetLists(PlayerColor.Green.Code).HeroIds.Contains(7), Is.True);

            this.editor.CurrentGame!.History.Undo();
            this.editor.CurrentGame!.History.Undo();
            Assert.That(document().Heroes().GetModel(7).Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That(document().Players().GetLists(PlayerColor.Blue.Code).HeroIds.Contains(7), Is.True);
        }

        [Test]
        public void AHeroGivenToAnotherPlayerLeavesBothPlayerPagesWithoutPendingChangesAndResettingTheHeroPutsTheListsBack()
        {
            this.editor.ShowHero(7);
            var hero = (HeroViewModel)this.editor.CurrentPage!;
            var history = this.editor.CurrentGame!.History;
            var original = history.Current.ToBytes();
            hero.SelectedOwner = "Green";

            this.editor.ShowPlayer(PlayerColor.Blue);
            var blue = (PlayerViewModel)this.editor.CurrentPage!;
            this.editor.ShowPlayer(PlayerColor.Green);
            var green = (PlayerViewModel)this.editor.CurrentPage!;
            Assert.That((blue.HasPendingChanges, green.HasPendingChanges, hero.HasPendingChanges), Is.EqualTo((false, false, true)));

            hero.ResetCommand.Execute(null);

            Assert.That(history.Current.ToBytes(), Is.EqualTo(original));
        }

        [Test]
        public void TheTavernOfAPlayerIsEditedByPickingAHeroNobodyOwns()
        {
            var game = this.editor.CurrentGame!;
            var blue = new PlayerViewModel(game, PlayerColor.Blue, TestServices.BanishingText, TestServices.MineGuardianText, _ => { }, _ => { });
            var free = game.History.Current.Heroes().GetModels().First(h => h.Owner.IsNeutral && !game.History.Current.Players().GetModel(PlayerColor.Blue.Code).Tavern.Contains(h.PortraitId));

            Assert.That(blue.TavernPlaces.Select(p => p.Label), Is.EqualTo(new[] { "First hero", "Second hero" }));
            Assert.That(blue.TavernPlaces[0].Choices.Select(c => c.HeroId), Does.Contain(free.PortraitId));

            blue.TavernPlaces[0].SelectedKey = free.PortraitId;

            Assert.That(game.History.Current.Players().GetModel(PlayerColor.Blue.Code).Tavern.First, Is.EqualTo(free.PortraitId));
            Assert.That(blue.TavernPlaces[0].Selected!.Name, Is.EqualTo(free.Name));
            Assert.That(blue.TavernPlaces[1].Choices.Select(c => c.HeroId), Does.Not.Contain(free.PortraitId), "a hero is offered in one place only");
        }

        [Test]
        public void APlayerThatWasPutOutEarnsNothingWhateverTheIncomeTableHolds()
        {
            var green = new PlayerViewModel(this.editor.CurrentGame!, PlayerColor.Green, TestServices.BanishingText, TestServices.MineGuardianText, _ => { }, _ => { });
            var blue = new PlayerViewModel(this.editor.CurrentGame!, PlayerColor.Blue, TestServices.BanishingText, TestServices.MineGuardianText, _ => { }, _ => { });

            Assert.That(this.editor.CurrentGame!.History.Current.Players().GetModel(PlayerColor.Green.Code).Income.Ore, Is.EqualTo(1), "the save holds 1 ore");
            Assert.That(green.Resources.Select(r => r.Note), Is.All.Empty);
            Assert.That(blue.Resources.Select(r => r.Note), Has.Some.Not.Empty);
        }

        [Test]
        public void APlayerThatWasPutOutReadsAsBanishedOnItsPageAndInTheTree()
        {
            var green = new PlayerViewModel(this.editor.CurrentGame!, PlayerColor.Green, TestServices.BanishingText, TestServices.MineGuardianText, _ => { }, _ => { });
            var node = this.editor.NavigationNodes[0].Children.Single(n => n.Kind == NavNodeKind.Player && n.Title == "Green");

            Assert.That(green.BanishingText, Is.EqualTo("Banished"));
            Assert.That(node.Notes, Is.EqualTo(new[] { "banished" }));
        }

        [Test]
        public void APlayerWithOneDayLeftToOwnACastleReadsAsOneDay()
        {
            var game = this.editor.CurrentGame!;
            game.History.CreatePlayerSession(PlayerColor.Blue.Code).Update(p => p with { DaysLeft = 1 });

            var blue = new PlayerViewModel(game, PlayerColor.Blue, TestServices.BanishingText, TestServices.MineGuardianText, _ => { }, _ => { });

            Assert.That(blue.BanishingText, Is.EqualTo("No towns: 1 day left"));
        }

        [Test]
        public void EditingAResourceOnThePlayersPageChangesTheSavegameAndMarksTheRow()
        {
            this.editor.ShowPlayer(PlayerColor.Blue);
            var page = (PlayerViewModel)this.editor.CurrentPage!;
            var gold = page.Resources.Single(r => r.Name == "Gold");
            var before = gold.Value;

            gold.Value = before + 5_000;

            Assert.That(this.editor.CurrentGame!.History.Current.Players().GetModel(PlayerColor.Blue.Code).Resources.Gold, Is.EqualTo(before + 5_000));
            Assert.That(gold.IsChanged, Is.True);
            Assert.That(gold.DeltaText, Is.EqualTo("+5,000 from save"));
            Assert.That(page.HasPendingChanges, Is.True);

            gold.Value = long.MaxValue;
            Assert.That(gold.Value, Is.EqualTo(Constants.Player.MaxGold), "the value is kept within the limit");

            page.ResetCommand.Execute(null);
            Assert.That(gold.Value, Is.EqualTo(before));
            Assert.That(gold.IsChanged, Is.False);
        }

        [Test]
        public void TheQuickAddButtonsAddToTheResourcesAndStopAtTheLimit()
        {
            var page = new PlayerViewModel(this.editor.CurrentGame!, PlayerColor.Blue, TestServices.BanishingText, TestServices.MineGuardianText, _ => { }, _ => { });
            var gold = page.Resources.Single(r => r.Name == "Gold").Value;
            var wood = page.Resources.Single(r => r.Name == "Wood").Value;

            page.SetRichCommand.Execute(null);
            page.SetAllResourcesCommand.Execute(null);

            Assert.That(page.Resources.Single(r => r.Name == "Gold").Value, Is.EqualTo(gold + 100_000));
            Assert.That(page.Resources.Single(r => r.Name == "Wood").Value, Is.EqualTo(wood + Constants.Player.AllResourcesIncrease));
        }
    }
}
