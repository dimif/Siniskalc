using Moq;
using Siniskalc.Heroes.ViewModels;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Tests.Models;
using Siniskalc.Homm3.Tests.ViewModels;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels
{
    /// <summary>
    /// The page of a castle over a castle session with no savefile: what it shows, how its edits reach the castle, the notice of an
    /// edit the rules refuse, and the heroes inside. The castle is a Rampart town with only its village hall.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class CastlePageTests
    {
        private FakeCastleSession session = null!;
        private Mock<IByteEditTargetFactory> byteEditTargets = null!;
        private readonly List<int> openedHeroes = [];

        [SetUp]
        public void StartWithARampartVillage()
        {
            this.openedHeroes.Clear();
            this.session = new FakeCastleSession(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart")));
        }

        private CastleViewModel Page(FakeCastleSession? other = null)
        {
            this.byteEditTargets = new Mock<IByteEditTargetFactory>();
            return new CastleViewModel(other ?? this.session, this.byteEditTargets.Object, new CastleBuildMessageProvider(), this.openedHeroes.Add);
        }

        private static BuildingRowViewModel Row(CastleViewModel page, string name) =>
            page.BuildingGroups.SelectMany(g => g.Rows).First(r => r.Name == name);

        [Test]
        public void TheHeaderNamesTheCastleItsFactionItsMageGuildAndWhereItStands()
        {
            using var page = this.Page();

            Assert.That(page.Name, Is.EqualTo("Test Town"));
            Assert.That(page.Title, Is.EqualTo("Test Town"));
            Assert.That(page.CastleIndex, Is.EqualTo(2));
            Assert.That(page.FactionText, Is.EqualTo("Rampart"));
            Assert.That(page.StatusText, Is.EqualTo("No Mage Guild · nothing built this turn"));
            Assert.That(page.OwnerText, Is.EqualTo("Red"));
            Assert.That(page.PositionText, Is.EqualTo("at 1, 1, Surface"));
            Assert.That(page.ShipyardText, Is.EqualTo("Not on the coast, so no boats"));
            Assert.That(page.Garrisoned, Is.EqualTo("No garrison"));
        }

        [Test]
        public void ACastleOfNoKnownFactionHasNoBuildingsAndSaysSo()
        {
            this.session = new FakeCastleSession(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart")) with { Faction = TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Neutral") });
            using var page = this.Page();

            Assert.That(page.FactionText, Is.EqualTo("Unknown faction"));
            Assert.That(page.BuildingGroups, Is.Empty);
        }

        [Test]
        public void TickingABuildingBuildsItAndPuttingTheCastleBackUnbuildsIt()
        {
            using var page = this.Page();
            var row = Row(page, "Fort");
            Assert.That(row.IsChecked, Is.False);
            Assert.That(row.CanToggle, Is.True);

            row.IsChecked = true;

            Assert.That(Row(page, "Fort").IsBuilt, Is.True);
            Assert.That(this.session.Current.HasBuilt(TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort")), Is.True);
            Assert.That(page.HasPendingChanges, Is.True);

            this.session.Update(_ => this.session.Original);

            Assert.That(Row(page, "Fort").IsBuilt, Is.False);
            Assert.That(page.HasPendingChanges, Is.False);
        }

        [Test]
        public void ABuildingTheRulesNeedAnotherForSaysWhyAndStaysBuilt()
        {
            this.session = new FakeCastleSession(CastleModels.With(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Citadel")));
            using var page = this.Page();

            var fort = Row(page, "Fort");
            Assert.That(fort.CanToggle, Is.False);
            Assert.That(fort.ToggleToolTip, Does.StartWith("Needed by Citadel"));

            fort.IsChecked = false;

            Assert.That(Row(page, "Fort").IsBuilt, Is.True);
        }

        [Test]
        public void ARefusedEditShowsANoticeAndChangesNothingAndTheNextGoodEditClearsIt()
        {
            using var page = this.Page();

            page.SelectedMageGuildLevel = 9;

            Assert.That(page.HasNotice, Is.True);
            Assert.That(page.Notice, Does.Contain("level 5"));
            Assert.That(this.session.IsDirty, Is.False);

            page.SelectedMageGuildLevel = 2;

            Assert.That(page.HasNotice, Is.False);
            Assert.That(this.session.Current.MageGuildLevel, Is.EqualTo(2));
        }

        [Test]
        public void TheMageGuildLevelCanBeRaisedAndLowered()
        {
            using var page = this.Page();
            Assert.That(page.MageGuildLevelChoices, Is.EqualTo(new[] { 0, 1, 2, 3, 4, 5 }));
            Assert.That(page.MageGuildSummary, Is.EqualTo("No Mage Guild, up to level 5"));

            page.SelectedMageGuildLevel = 5;
            Assert.That(page.MageGuildSummary, Is.EqualTo("Level 5 of 5"));
            Assert.That(page.MageGuildLevels.Select(l => l.IsBuilt), Is.All.True);

            page.SelectedMageGuildLevel = 2;
            Assert.That(page.MageGuildLevels.Select(l => l.IsBuilt), Is.EqualTo(new[] { true, true, false, false, false }));
        }

        [Test]
        public void TheDwellingCountIsEditedFromTheRowAndKeptWithinTheLimit()
        {
            var dwelling = Building.FromDwellingLevel(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart"), 2, upgraded: false)!;
            this.session = new FakeCastleSession(CastleModels.With(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart")), dwelling));
            using var page = this.Page();
            var row = page.Dwellings.Single(d => d.Level == 3);
            Assert.That(row.CanEdit, Is.True);

            row.Count = 55;

            Assert.That(page.Dwellings.Single(d => d.Level == 3).Count, Is.EqualTo(55));
            Assert.That(this.session.Current.CreaturesAvailable[2], Is.EqualTo(55));

            page.Dwellings.Single(d => d.Level == 3).Count = Constants.Castle.MaxDwellingCreatures + 1;

            Assert.That(page.Dwellings.Single(d => d.Level == 3).Count, Is.EqualTo(Constants.Castle.MaxDwellingCreatures));
        }

        [Test]
        public void ADwellingThatIsNotBuiltCannotBeEdited()
        {
            using var page = this.Page();

            Assert.That(page.Dwellings.Single(d => d.Level == 3).CanEdit, Is.False);
        }

        [Test]
        public void TheOwnersToPickFromAreNeutralAndTheEightColoursAndPickingOneGivesTheCastleToThePlayer()
        {
            using var page = this.Page();
            Assert.That(page.OwnerChoices, Is.EqualTo(new[] { "Neutral", "Red", "Blue", "Tan", "Green", "Orange", "Purple", "Teal", "Pink" }));
            Assert.That(page.SelectedOwner, Is.EqualTo("Red"));

            page.SelectedOwner = "Blue";

            Assert.That(this.session.Current.Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That(page.OwnerText, Is.EqualTo("Blue"));
            Assert.That(page.ColorHex, Is.EqualTo(PlayerPalette.Of(PlayerColor.Blue)));
        }

        [Test]
        public void TheGarrisonIsEditedThroughTheSharedArmyEditor()
        {
            using var page = this.Page();

            page.GarrisonEditor.CreatureSlots[2].SelectedCreature = "Pikeman";
            page.GarrisonEditor.CreatureSlots[2].CreatureAmount = 40;

            Assert.That(page.Garrison[2].CreatureName, Is.EqualTo("Pikeman"));
            Assert.That(page.Garrison[2].CountText, Is.EqualTo("40"));
            Assert.That(page.Garrisoned, Is.EqualTo("1 of 7 slots in use"));

            page.GarrisonEditor.CreatureSlots[2].ClearCommand.Execute(null);

            Assert.That(this.session.Current.Garrison.Filled, Is.Empty);
        }

        [Test]
        public void BuiltThisTurnIsAFreeFlagThatBuildingNeverChanges()
        {
            using var page = this.Page();
            Assert.That((page.BuiltThisTurn, page.CanBuildThisTurn), Is.EqualTo((false, true)));

            Row(page, "Fort").IsChecked = true;
            Assert.That(page.BuiltThisTurn, Is.False);

            page.BuiltThisTurn = true;
            Assert.That((page.BuiltThisTurn, page.CanBuildThisTurn), Is.EqualTo((true, false)));

            Row(page, "Fort").IsChecked = false;
            Assert.That(page.BuiltThisTurn, Is.True);
        }

        [Test]
        public void ResetPutsTheWholeCastleBack()
        {
            using var page = this.Page();
            page.SelectedMageGuildLevel = 1;
            page.BuiltThisTurn = true;
            Assert.That(page.ResetCommand.CanExecute(null), Is.True);

            page.ResetCommand.Execute(null);

            Assert.That(page.HasPendingChanges, Is.False);
            Assert.That(this.session.Current, Is.SameAs(this.session.Original));
            Assert.That(page.ResetCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void AChangeMadeBehindThePageShowsOnIt()
        {
            using var page = this.Page();

            this.session.Update(c => c with { Owner = PlayerColor.Teal });

            Assert.That(page.OwnerText, Is.EqualTo("Teal"));
        }

        [Test]
        public void ADisposedPageNoLongerListensToTheHistory()
        {
            var page = this.Page();
            page.Dispose();

            this.session.Update(c => c with { Owner = PlayerColor.Teal });

            Assert.That(page.OwnerText, Is.EqualTo("Red"));
        }

        [Test]
        public void AGarrisonedHeroNamesItAndItsArmyTakesTheGarrisonsPlaceAndCannotBeEdited()
        {
            var christian = HeroModels.Christian();
            this.session = new FakeCastleSession(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart")) with { GarrisonedHeroId = christian.Id });
            this.session.HeroModels[christian.Id] = christian;
            using var page = this.Page();

            Assert.That(page.HasGarrisonedHero, Is.True);
            Assert.That(page.Garrisoned, Is.EqualTo("Garrisoned by Christian"));
            Assert.That(page.GarrisonedHeroName, Is.EqualTo("Christian"));
            Assert.That(page.GarrisonEditor.CreatureSlots[0].SelectedCreature, Is.EqualTo("Pikeman"));
            Assert.That(page.GarrisonEditor.CreatureSlots[0].CreatureAmount, Is.EqualTo(10));
            Assert.That(page.GarrisonEditor.IsEditable, Is.False);
        }

        [Test]
        public void TheStoredSlotsOfACastleAreLeftAloneWhileAHeroIsGarrisonedAndTheNoticeSaysWhy()
        {
            var christian = HeroModels.Christian();
            this.session = new FakeCastleSession(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart")) with { GarrisonedHeroId = christian.Id });
            this.session.HeroModels[christian.Id] = christian;
            using var page = this.Page();

            page.GarrisonEditor.CreatureSlots[1].SelectedCreature = "Pikeman";
            page.GarrisonEditor.CreatureSlots[1].CreatureAmount = 40;

            Assert.That(this.session.IsDirty, Is.False);
            Assert.That(this.session.Current.Garrison.Filled, Is.Empty);
        }

        [Test]
        public void ACastleWithNoGarrisonedHeroHasAnEditableGarrison()
        {
            using var page = this.Page();

            Assert.That(page.HasGarrisonedHero, Is.False);
            Assert.That(page.GarrisonEditor.IsEditable, Is.True);
        }

        [Test]
        public void TheHeroesInTheCastleAreListedAndOpenedByTheirPlaceAmongTheHeroes()
        {
            var christian = HeroModels.Christian();
            this.session = new FakeCastleSession(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart")) with { GarrisonedHeroId = christian.Id });
            this.session.HeroModels[7] = christian with { Id = 7, Name = "Other" };
            this.session.HeroModels[christian.Id] = christian;
            using var page = this.Page();

            var row = page.Heroes.Single();
            row.OpenCommand.Execute(null);

            Assert.That(page.HasHeroes, Is.True);
            Assert.That((row.Role, row.Name), Is.EqualTo(("Garrisoned", "Christian")));
            Assert.That(this.openedHeroes, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void AHeroThatNoHeroHasTheIdOfIsNamedByItsIdAndCannotBeOpened()
        {
            this.session = new FakeCastleSession(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart")) with { GarrisonedHeroId = 5 });
            using var page = this.Page();

            Assert.That(page.GarrisonedHeroName, Is.EqualTo("Unknown hero 5"));
            Assert.That(page.OpenGarrisonedHeroCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void TheHexEditorTargetComesFromTheSessionWithTheFactoryOfThePage()
        {
            using var page = this.Page();

            var target = page.CreateByteEditTarget();

            Assert.That(target, Is.SameAs(this.session.Target));
            Assert.That(this.session.FactoryAsked, Is.SameAs(this.byteEditTargets.Object));
        }
    }
}
