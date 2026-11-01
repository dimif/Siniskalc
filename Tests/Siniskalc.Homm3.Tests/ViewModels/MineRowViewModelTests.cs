using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.ViewModels
{
    /// <summary>
    /// Tests the row of a mine of HoMM3 on top of the row every game shares: the owner written with the reducer of the game and the
    /// guard of seven slots.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MineRowViewModelTests
    {
        private static MineModel Mine(IArmy<Creature>? guard = null) =>
            new(2, MineKind.GoldMine, PlayerColor.Blue, new MapPosition(14, 15, MapPosition.Underground), guard ?? new Army<Creature>(Constants.CreatureSlotCount), TestCatalog.Hota);

        private static Mock<IEditSession<MineModel>> Session(MineModel mine)
        {
            var current = mine;
            var session = new Mock<IEditSession<MineModel>>();
            session.SetupGet(s => s.Index).Returns(mine.Index);
            session.SetupGet(s => s.Current).Returns(() => current);
            session.Setup(s => s.Update(It.IsAny<Func<MineModel, MineModel>>())).Callback((Func<MineModel, MineModel> edit) => current = edit(current));
            return session;
        }

        [Test]
        public void TheRowReadsTheMineAndPicksItsOwnerFromTheColoursOfTheGame()
        {
            var row = new MineRowViewModel(Session(Mine()).Object);

            Assert.That((row.Index, row.OwnerField.Selected, row.IsOwned), Is.EqualTo((2, "Blue", true)));
            Assert.That(row.OwnerField.Choices, Does.Contain("Neutral").And.Contain("Red"));
            Assert.That(row.OwnerField.ToolTip, Does.Contain("Who owns the mine"));
            Assert.That(row.PositionText, Does.Contain("Underground"), "the mines of HoMM3 have levels");
        }

        [Test]
        public void PickingAnOwnerWritesItToTheMine()
        {
            var session = Session(Mine());
            var row = new MineRowViewModel(session.Object);

            row.OwnerField.Selected = "Red";

            Assert.That(session.Object.Current.Owner, Is.EqualTo(PlayerColor.Red));
        }

        [Test]
        public void AnUnguardedMineSaysSoAndAGuardedOneCountsItsSlots()
        {
            var session = Session(Mine());
            var row = new MineRowViewModel(session.Object);
            Assert.That(row.Summary, Is.EqualTo("Unguarded"));

            row.UpdateGuard(guard => guard.WithSlot(0, TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Centaur"), 5).WithSlot(3, TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Angel"), 1));

            Assert.That(row.Summary, Is.EqualTo($"2 of {Constants.CreatureSlotCount} slots"));
            Assert.That(row.Guard.Filled.Count(), Is.EqualTo(2));
            Assert.That(session.Object.Current.Guard.Slots[3]?.Creature, Is.SameAs(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Angel")));
        }

        [Test]
        public void TheGuardIsThatOfTheMineOfTheSession()
        {
            var guard = new Army<Creature>(Constants.CreatureSlotCount).WithSlot(1, TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("GoldDragon"), 2);

            var row = new MineRowViewModel(Session(Mine(guard)).Object);

            Assert.That(row.Guard, Is.SameAs(guard));
        }
    }
}
