namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Makes rows of the mines of a made-up game, for the tests of the lists and cards of the mines of one owner.
    /// </summary>
    internal static class TestMineRows
    {
        public static MineRowViewModel<IMineModel<TestColor>, TestColor> Row(int index, MineKind kind, TestColor owner, int x, int y)
        {
            var mine = new Mock<IMineModel<TestColor>>();
            mine.SetupGet(m => m.Index).Returns(index);
            mine.SetupGet(m => m.Kind).Returns(kind);
            mine.SetupGet(m => m.Owner).Returns(owner);
            mine.SetupGet(m => m.Position).Returns(new MapPosition(x, y));
            var session = new Mock<IEditSession<IMineModel<TestColor>>>();
            session.SetupGet(s => s.Index).Returns(index);
            session.SetupGet(s => s.Current).Returns(mine.Object);
            return new MineRowViewModel<IMineModel<TestColor>, TestColor>(session.Object, (m, _) => m, string.Empty, hasLevels: false);
        }

        /// <summary>
        /// Gives the row of the mine with the given number among four: a red sawmill, a neutral gold mine, a red ore mine and a blue crystal mine.
        /// </summary>
        public static MineRowViewModel<IMineModel<TestColor>, TestColor> OfFour(int index) => index switch
        {
            0 => Row(0, MineKind.Sawmill, TestColor.Red, 30, 4),
            1 => Row(1, MineKind.GoldMine, TestColor.Neutral, 12, 40),
            2 => Row(2, MineKind.OreMine, TestColor.Red, 7, 8),
            _ => Row(3, MineKind.CrystalMine, TestColor.Blue, 2, 9),
        };
    }
}
