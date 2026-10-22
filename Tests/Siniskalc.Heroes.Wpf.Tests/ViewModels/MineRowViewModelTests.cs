namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the row of a mine read from the savegame through its edit session, with the owner picked from the colours of a made-up game.
    /// </summary>
    [TestFixture]
    internal sealed class MineRowViewModelTests
    {
        [Test]
        public void TheRowReadsTheMineOfTheSession()
        {
            var row = Row(Session(new TestMine(4, MineKind.OreMine, TestColor.Blue, new MapPosition(5, 6, MapPosition.Underground))));

            Assert.That((row.Index, row.Kind, row.PositionText, row.OwnerField.Selected, row.IsOwned), Is.EqualTo((4, "Ore mine", "X: 5, Y: 6, Underground (1)", "Blue", true)));
            Assert.That(row.OwnerField.ToolTip, Is.EqualTo("Who owns it."));
        }

        [Test]
        public void TheOwnersArePickedFromNeutralAndTheColoursOfTheGame()
        {
            var row = Row(Session(new TestMine(0, MineKind.GoldMine, TestColor.Neutral, new MapPosition(1, 2))));

            Assert.That(row.OwnerField.Choices, Is.EqualTo(new[] { "Neutral", "Red", "Blue" }));
            Assert.That(row.IsOwned, Is.False);
        }

        [Test]
        public void PickingAnOwnerEditsTheMineWithTheReducerOfTheGame()
        {
            var session = Session(new TestMine(0, MineKind.GoldMine, TestColor.Neutral, new MapPosition(1, 2)));
            var row = Row(session);

            row.OwnerField.Selected = "Red";

            Assert.That(session.Object.Current.Owner, Is.EqualTo(TestColor.Red));
            Assert.That(row.IsOwned, Is.True);
        }

        private static Mock<IEditSession<TestMine>> Session(TestMine mine)
        {
            var current = mine;
            var session = new Mock<IEditSession<TestMine>>();
            session.SetupGet(s => s.Index).Returns(mine.Index);
            session.SetupGet(s => s.Current).Returns(() => current);
            session.Setup(s => s.Update(It.IsAny<Func<TestMine, TestMine>>())).Callback((Func<TestMine, TestMine> edit) => current = edit(current));
            return session;
        }

        private static MineRowViewModel<TestMine, TestColor> Row(Mock<IEditSession<TestMine>> session) =>
            new(session.Object, (mine, owner) => mine with { Owner = owner }, "Who owns it.", hasLevels: true);
    }
}
