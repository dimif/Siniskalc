namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Tests where the creatures that wait in a dwelling are written in the savegame.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class SafeCastleSerializerTests
    {
        private static CastleChunk Xabran() => TestServices.LoadBrokenAlliance(5).History.Current.Castles()[3];

        private static ushort At(CastleChunk chunk, int offset) => chunk.GetUInt16At(offset);

        [Test]
        public void UpgradingADwellingMovesItsCreaturesToTheListOfTheUpgradedOnes()
        {
            var chunk = Xabran();
            var model = TestServices.CastleDeserializer.Deserialize(chunk).WithCreaturesWaiting(2, 7);

            var written = TestServices.CastleSerializer.Serialize(chunk, model.WithDwellingLevel(2, 2));

            Assert.That(At(written, CastleLayout.CreaturesOffset(2, 2)), Is.EqualTo(7));
            Assert.That(At(written, CastleLayout.CreaturesOffset(2, 1)), Is.EqualTo(0));
            Assert.That(TestServices.CastleDeserializer.Deserialize(written).CreaturesWaiting(2), Is.EqualTo(7));
        }

        [Test]
        public void TakingTheUpgradeDownMovesTheCreaturesBackAndEmptiesTheUpgradedList()
        {
            var chunk = Xabran();
            var upgraded = TestServices.CastleSerializer.Serialize(chunk, TestServices.CastleDeserializer.Deserialize(chunk).WithCreaturesWaiting(2, 7).WithDwellingLevel(2, 2));

            var back = TestServices.CastleSerializer.Serialize(upgraded, TestServices.CastleDeserializer.Deserialize(upgraded).WithDwellingLevel(2, 1));

            Assert.That(At(back, CastleLayout.CreaturesOffset(2, 1)), Is.EqualTo(7));
            Assert.That(At(back, CastleLayout.CreaturesOffset(2, 2)), Is.EqualTo(0));
        }

        [Test]
        public void TheDragonsUpgradedTwiceWaitInTheSixthPlaceAndGetTheMarkTheGameShowsThemWith()
        {
            var chunk = TestServices.LoadBrokenAlliance(24).History.Current.Castles()[1];
            var dragons = TestServices.CastleDeserializer.Deserialize(chunk).WithDwellingLevel(6, 2).WithCreaturesWaiting(6, 3).WithDwellingLevel(6, 3);

            var written = TestServices.CastleSerializer.Serialize(chunk, dragons);

            Assert.That(At(written, CastleLayout.CreaturesOffset(6, 3)), Is.EqualTo(3));
            Assert.That(At(written, CastleLayout.UpgradedCreatures + (CastleLayout.SecondUpgradeMarker * 2)), Is.EqualTo(CastleLayout.SecondUpgradeMarkerValue));
            Assert.That(TestServices.CastleDeserializer.Deserialize(written).CreaturesWaiting(6), Is.EqualTo(3));
        }

        [Test]
        public void ACountTheGameLeftInTheListOfTheDwellingBesideTheUpgradedOnesIsKeptAndTheUpgradedOnesAreRead()
        {
            // Threedom: Hillstone's second dwelling is upgraded, with 11 waiting in the list of the upgraded and the 14 of before in its own.
            var save = TestServices.Load(Saves.All().First(f => Path.GetFileName(f).StartsWith("threedom_82", StringComparison.Ordinal)));
            var chunk = save.History.Current.Castles().First(c => c.Name == "Hillstone");

            var model = TestServices.CastleDeserializer.Deserialize(chunk);

            Assert.That((model.DwellingLevel(2), model.CreaturesWaiting(2)), Is.EqualTo((2, 11)));
            Assert.That(At(chunk, CastleLayout.CreaturesOffset(2, 1)), Is.EqualTo(14));
            Assert.That(TestServices.CastleSerializer.Serialize(chunk, model).ToArray(), Is.EqualTo(chunk.ToArray()));
            Assert.That(At(TestServices.CastleSerializer.Serialize(chunk, model.WithCreaturesWaiting(2, 9)), CastleLayout.CreaturesOffset(2, 1)), Is.EqualTo(14), "the old count is not touched while the level stays");
        }
    }
}
