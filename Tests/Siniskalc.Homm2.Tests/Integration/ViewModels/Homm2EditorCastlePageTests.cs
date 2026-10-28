namespace Siniskalc.Homm2.Tests.Integration.ViewModels
{
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the page of a castle in the editor: garrison, buildings, Mage Guild and owner, on a Broken Alliance savegame loaded into the editor.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class Homm2EditorCastlePageTests
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
        public void TheGarrisonOfACastleIsAnArmyToo()
        {
            var castle = this.editor.CurrentGame!.History.Current.Castles().GetModels().First(c => c.IsUsed && c.Owner == PlayerColor.Blue);
            this.editor.ShowCastle(castle.Index);
            var garrison = ((CastleViewModel)this.editor.CurrentPage!).GarrisonEditor;

            garrison.SendToSlot(Creature.Paladin, 1, 10);

            Assert.That(this.editor.CurrentGame.History.Current.Castles().GetModel(castle.Index).Garrison.Slots[0], Is.EqualTo(new ArmySlot<Creature>(Creature.Paladin, 10)));
            Assert.That(garrison.CreatureSlots[0].SelectedCreature, Is.EqualTo("Paladin"));
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
            Assert.That(model().CreaturesWaiting(index + 1), Is.EqualTo(40));

            if (dwelling.Built.Choices.Count > 2)
            {
                dwelling.Built.Selected = dwelling.Built.Choices[2];
                Assert.That(model().DwellingLevel(index + 1), Is.EqualTo(2), "an upgrade replaces the dwelling");
                Assert.That(model().CreaturesWaiting(index + 1), Is.EqualTo(40), "the creatures that wait stay");
            }

            dwelling.Waiting.Value = 100_000;
            Assert.That(dwelling.Waiting.Value, Is.EqualTo(Constants.Castle.MaxWaiting));
            Assert.That(page.HasPendingChanges, Is.True);
        }

        [Test]
        public void TheMageGuildIsEditedByLevelAndByPlace()
        {
            var page = this.harness.OpenCastleOf(PlayerColor.Blue);
            var model = () => this.editor.CurrentGame!.History.Current.Castles().GetModel(page.CastleIndex);

            page.GuildLevelField.Selected = "Level 2";
            Assert.That(model().MageGuildLevel, Is.EqualTo(2));

            var slot = page.GuildLevels[1].Spells[0];
            Assert.That(slot.IsEnabled, Is.True);
            Assert.That(page.GuildLevels[1].Spells[3].IsEnabled, Is.False, "a level of this guild shows three spells, not four");
            slot.Selected = "Fireball";
            Assert.That(model().MageGuildSpell(2, 0), Is.EqualTo(Spell.Fireball));
            slot.Selected = "(none)";
            Assert.That(model().MageGuildSpell(2, 0), Is.Null);

            page.GuildLevelField.Selected = "None";
            Assert.That(model().MageGuildLevel, Is.EqualTo(0));
            Assert.That(page.GuildLevels[0].Spells[0].IsEnabled, Is.False, "the spells of a level that is not built are not edited");
        }

        [Test]
        public void ACastleChangesHandsWithTheListsOfBothPlayersInOneStepAndUndoPutsItAllBack()
        {
            var page = this.harness.OpenCastleOf(PlayerColor.Red);
            var document = () => this.editor.CurrentGame!.History.Current;
            var red = () => document().Players().GetModel(PlayerColor.Red.Code);
            var blue = () => document().Players().GetModel(PlayerColor.Blue.Code);
            var blueBefore = blue().CastleIds.Count;
            Assert.That(red().CastleIds, Does.Contain(page.CastleIndex));

            page.OwnerField.Selected = "Blue";

            Assert.That(document().Castles().GetModel(page.CastleIndex).Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That(blue().CastleIds, Does.Contain(page.CastleIndex));
            Assert.That(blue().CastleIds.Count, Is.EqualTo(blueBefore + 1));
            Assert.That(red().CastleIds, Does.Not.Contain(page.CastleIndex));
            Assert.That(red().DaysLeft, Is.EqualTo(PlayerModel.DaysToOwnACastle), "a player that lost its last castle starts counting");

            this.editor.Undo();
            Assert.That(document().Castles().GetModel(page.CastleIndex).Owner, Is.EqualTo(PlayerColor.Red));
            Assert.That(red().CastleIds, Does.Contain(page.CastleIndex));
            Assert.That(blue().CastleIds.Count, Is.EqualTo(blueBefore));
            Assert.That(red().DaysLeft, Is.Null);
        }

        [Test]
        public void ACastleCannotGoToAPlayerWhoseListOfCastlesIsFull()
        {
            var page = this.harness.OpenCastleOf(PlayerColor.Red);
            this.editor.CurrentGame!.History.CreatePlayerSession(PlayerColor.Blue.Code).Update(p => p with { CastleIds = new Numbers(Enumerable.Range(40, Constants.Player.CastleListSize)) });

            page.OwnerField.Selected = "Blue";

            Assert.That(page.HasNotice, Is.True);
            Assert.That(this.editor.CurrentGame.History.Current.Castles().GetModel(page.CastleIndex).Owner, Is.EqualTo(PlayerColor.Red));
        }
    }
}
