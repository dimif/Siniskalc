namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the row of a mine that every game shows: what it gives, where it is and who owns it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MineRowViewModelBaseTests
    {
        [Test]
        public void InAGameWithTwoLevelsThePlaceOfAMineSaysItsLevel()
        {
            var surface = Row(new MapPosition(12, 7, MapPosition.Above), hasLevels: true);
            var underground = Row(new MapPosition(12, 7, MapPosition.Underground), hasLevels: true);

            Assert.That((surface.PositionText, underground.PositionText), Is.EqualTo(("at 12, 7, Surface", "at 12, 7, Underground")));
            Assert.That((surface.LevelText, underground.LevelText), Is.EqualTo(("Surface", "Underground")));
            Assert.That(underground.Level, Is.EqualTo(MapPosition.Underground));
        }

        [Test]
        public void InAGameWithOneLevelThePlaceOfAMineIsOnlyItsTile()
        {
            var row = Row(new MapPosition(12, 7), hasLevels: false);

            Assert.That((row.PositionText, row.LevelText, row.HasLevel), Is.EqualTo(("at 12, 7", string.Empty, false)));
        }

        [Test]
        public void TheRowSaysWhatTheMineGivesAndWhoOwnsIt()
        {
            var row = Row(new MapPosition(12, 7), hasLevels: false);

            Assert.That((row.Index, row.Kind, row.ResourceName, row.OwnerSort, row.IsOwned), Is.EqualTo((3, "Gold mine", "Gold", "Red", true)));
        }

        private static MineRowViewModelBase Row(MapPosition position, bool hasLevels) => new TestRow(position, hasLevels);

        private sealed class TestRow : MineRowViewModelBase
        {
            public TestRow(MapPosition position, bool hasLevels)
                : base(3, () => MineKind.GoldMine, () => position, () => true, new ChoiceFieldViewModel("Owner", ["Neutral", "Red"], () => "Red", _ => { }), hasLevels)
            {
            }
        }
    }
}
