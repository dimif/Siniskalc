using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// Builds in the editor what the game built between two saves of the Restoration of Erathia series and compares the three building masks
    /// of the castle with the ones the game wrote. Bit 23 is left out: the game sets it with the shipyard of the Castle and the fort of the Fortress and
    /// the editor does not know why (see <c>CompanionBits</c>).
    /// </summary>
    [TestFixture]
    internal sealed class RoeBuildingEditsTests
    {
        private const ulong IgnoredBits = 1UL << 23;

        private static readonly string[] RedCastles = ["1_Castle", "2_Rampart", "3_Tower", "4_Inferno", "5_necropolis", "6_Dung", "7_Strong", "8_Fort"];

        private static Game Load(string save) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Roe", save + ".GM1"));

        // Every save up to 47 follows the one before it. The capitols were built from save 47 each (the first of them was kept in save 49 and
        // the others were not), so they are compared with 47.
        private static IEnumerable<TestCaseData> Steps()
        {
            var names = SeriesSaves.Names("Roe", "roe").Where(n => SeriesSaves.Number(n) <= 47).ToList();

            for (var i = 1; i < names.Count; i++)
            {
                yield return new TestCaseData(names[i - 1], names[i]).SetName($"{names[i]}_after_{names[i - 1]}");
            }

            foreach (var capitol in new[] { "roe_48_castle_capitol", "roe_48b_rampart_capitol", "roe_50_necropolis_capitol", "roe_51_tower_capitol", "roe_52_inferno_capitol", "roe_53_dungeon_capitol", "roe_54_stronghold_capitol", "roe_55_fortress_capitol" })
            {
                yield return new TestCaseData("roe_47_garrisons", capitol).SetName($"{capitol}_after_47_garrisons");
            }
        }

        private static ulong[] Masks(CastleChunk chunk) =>
        [
            chunk.GetUInt64At(chunk.NameEnd + CastleLayout.CurrentBuildingsFromNameEnd) & ~IgnoredBits,
            chunk.GetUInt64At(chunk.NameEnd + CastleLayout.BuiltBuildingsFromNameEnd) & ~IgnoredBits,
            chunk.GetUInt64At(chunk.NameEnd + CastleLayout.EnabledBuildingsFromNameEnd) & ~IgnoredBits,
        ];

        [TestCaseSource(nameof(Steps))]
        public void BuildingWhatTheGameBuiltGivesTheMasksTheGameWrote(string before, string after)
        {
            var from = Load(before).History.Current.Castles();
            var to = Load(after).History.Current.Castles();
            var checkedCastles = 0;

            foreach (var name in RedCastles)
            {
                var chunk = from[from.FindByName(name)];
                var model = TestServices.CastleDeserializer.Deserialize(chunk);
                var actual = to[to.FindByName(name)];
                var added = TestServices.CastleDeserializer.Deserialize(actual).Buildings.Except(model.Buildings).Where(b => !CastleBuilder.IsAutomatic(b)).ToList();
                if (added.Count != 1)
                {
                    continue;
                }

                var edited = TestServices.CastleSerializer.Serialize(chunk, CastleBuilder.Build(model, added[0]));

                Assert.That(Masks(edited), Is.EqualTo(Masks(actual)), $"{name}: {added[0].Name}");
                checkedCastles++;
            }

            Assert.That(checkedCastles, Is.GreaterThanOrEqualTo(after.Contains("capitol") ? 1 : 0));
        }

        [Test]
        public void MostStepsAreCheckedAgainstAtLeastSixCastles()
        {
            // 12 to 24 and 30 to 31 build the same thing in all eight castles; this guards the test above against skipping everything.
            var from = Load("roe_14_dwelling_1").History.Current.Castles();
            var to = Load("roe_15_dwelling_2").History.Current.Castles();

            var checkedCastles = RedCastles.Count(name =>
                TestServices.CastleDeserializer.Deserialize(to[to.FindByName(name)]).Buildings
                    .Except(TestServices.CastleDeserializer.Deserialize(from[from.FindByName(name)]).Buildings)
                    .Count(b => !CastleBuilder.IsAutomatic(b)) == 1);

            Assert.That(checkedCastles, Is.EqualTo(8));
        }

        [TestCase("roe_50_necropolis_capitol", "5_necropolis")]
        [TestCase("roe_51_tower_capitol", "3_Tower")]
        [TestCase("roe_52_inferno_capitol", "4_Inferno")]
        [TestCase("roe_53_dungeon_capitol", "6_Dung")]
        [TestCase("roe_54_stronghold_capitol", "7_Strong")]
        [TestCase("roe_55_fortress_capitol", "8_Fort")]
        public void EachOfTheLastCapitolSavesHasTheCapitolInOneCastleAndNoCityHallThere(string save, string with)
        {
            var castles = Load(save).History.Current.Castles();

            foreach (var name in RedCastles)
            {
                var model = TestServices.CastleDeserializer.Deserialize(castles[castles.FindByName(name)]);

                Assert.That(model.Buildings.Contains(Building.Capitol), Is.EqualTo(name == with), name);
                Assert.That(model.CurrentBuildings.Contains(Building.CityHall), Is.EqualTo(name != with), name);
            }
        }
    }
}
