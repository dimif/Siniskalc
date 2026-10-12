namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Homm2.Data;

    /// <summary>
    /// Reading a record and writing it back changes nothing, for every record of every savegame: that is what lets the editor save a
    /// savegame whose values it does not fully understand.
    /// </summary>
    [TestFixture]
    internal sealed class Homm2RoundTripTests
    {
        [Test]
        public void EveryHeroWrittenBackIsTheSameBytes()
        {
            foreach (var path in Homm2Saves.All())
            {
                var heroes = Homm2TestServices.Load(path).History.Current.Heroes();
                foreach (var chunk in heroes)
                {
                    var written = Homm2TestServices.HeroSerializer.Serialize(chunk, Homm2TestServices.HeroDeserializer.Deserialize(chunk));

                    Assert.That(written.ToArray(), Is.EqualTo(chunk.ToArray()), $"{Path.GetFileName(path)}: {chunk.Name}");
                }
            }
        }

        [Test]
        public void EveryPlayerWrittenBackIsTheSameBytes()
        {
            foreach (var path in Homm2Saves.All())
            {
                foreach (var chunk in Homm2TestServices.Load(path).History.Current.Players())
                {
                    var written = Homm2TestServices.PlayerSerializer.Serialize(chunk, Homm2TestServices.PlayerDeserializer.Deserialize(chunk));

                    Assert.That(written.ToArray(), Is.EqualTo(chunk.ToArray()), $"{Path.GetFileName(path)}: {chunk.Name}");
                }
            }
        }

        [Test]
        public void EveryCastleWrittenBackIsTheSameBytes()
        {
            foreach (var path in Homm2Saves.All())
            {
                foreach (var chunk in Homm2TestServices.Load(path).History.Current.Castles())
                {
                    var written = Homm2TestServices.CastleSerializer.Serialize(chunk, Homm2TestServices.CastleDeserializer.Deserialize(chunk));

                    Assert.That(written.ToArray(), Is.EqualTo(chunk.ToArray()), $"{Path.GetFileName(path)}: castle {chunk.Name}");
                }
            }
        }

        [Test]
        public void TheRecordsTogetherCoverTheTablesWithoutOverlapping()
        {
            foreach (var path in Homm2Saves.All())
            {
                var game = Homm2TestServices.Load(path);
                var chunks = game.History.Current.Heroes().Cast<Siniskalc.Core.Data.BytesChunk>()
                    .Concat(game.History.Current.Players())
                    .Concat(game.History.Current.Castles())
                    .OrderBy(c => c.StartIndex)
                    .ToList();

                for (var i = 1; i < chunks.Count; i++)
                {
                    Assert.That(chunks[i].StartIndex, Is.GreaterThanOrEqualTo(chunks[i - 1].End), $"{Path.GetFileName(path)}: {chunks[i].Name}");
                }

                Assert.That(chunks[^1].End, Is.LessThanOrEqualTo(game.Bytes.Length), path);
            }
        }
    }
}
