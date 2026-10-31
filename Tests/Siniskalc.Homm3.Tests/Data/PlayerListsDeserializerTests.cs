using Siniskalc.Core.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// What the bytes of a record of a player read as the list of heroes and the selected hero, on records made by hand, for both layouts
    /// of the record.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PlayerListsDeserializerTests
    {
        private readonly PlayerListsDeserializer deserializer = new();

        private static IEnumerable<PlayerLayout> Layouts => PlayerRecords.Layouts;

        [TestCaseSource(nameof(Layouts))]
        public void TheHeroesAreTheIdsThatAreNotTheEmptyMarkAndTheSelectedHeroIsNoneForTheEmptyMark(PlayerLayout layout)
        {
            var empty = this.deserializer.Deserialize(PlayerRecords.Blank(layout, PlayerColor.Red, 1));
            var set = this.deserializer.Deserialize(PlayerRecords.Blank(layout, PlayerColor.Red, 1, b =>
            {
                new byte[] { 21, 0xFF, 22, 0xFF, 0xFF, 0xFF, 0xFF, 40 }.CopyTo(b, PlayerLayout.HeroIds);
                b[PlayerLayout.SelectedHero] = 22;
            }));

            Assert.That((empty.HeroIds, empty.SelectedHero), Is.EqualTo((new Numbers([]), (int?)null)));
            Assert.That(set.HeroIds, Is.EqualTo(new[] { 21, 22, 40 }));
            Assert.That(set.SelectedHero, Is.EqualTo(22));
        }

        [TestCaseSource(nameof(Layouts))]
        public void TheHeroWithIdZeroIsAHeroInTheListAndTheSelectedHeroAndNotNone(PlayerLayout layout)
        {
            var lists = this.deserializer.Deserialize(PlayerRecords.Blank(layout, PlayerColor.Red, 1, b =>
            {
                b[PlayerLayout.HeroIds] = 0;
                b[PlayerLayout.SelectedHero] = 0;
            }));

            Assert.That(lists.HeroIds, Is.EqualTo(new[] { 0 }));
            Assert.That(lists.SelectedHero, Is.EqualTo(0));
        }
    }
}
