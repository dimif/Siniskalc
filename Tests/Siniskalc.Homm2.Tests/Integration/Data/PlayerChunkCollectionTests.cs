namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Homm2.Data;

    /// <summary>
    /// Tests which players the HoMM2 savegames have data for.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class PlayerChunkCollectionTests
    {
        [Test]
        public void EveryOwnerOfAHeroOrUsedCastleHasPlayerDataInEverySave()
        {
            foreach (var path in Saves.All())
            {
                var document = TestServices.Load(path).History.Current;
                var players = document.Players().GetColors();
                var owners = document.Heroes().GetOwners()
                    .Concat(document.Castles().GetModels().Where(c => c.IsUsed).Select(c => c.Owner))
                    .Where(c => !c.IsNeutral)
                    .Distinct();

                Assert.That(owners.Except(players), Is.Empty, path);
            }
        }
    }
}
