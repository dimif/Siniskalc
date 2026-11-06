namespace Siniskalc.Homm3.Tests.Integration.Data
{
    using System.Collections.Concurrent;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Tests.Data;

    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class CreatureBankSavegameTests
    {
        [Test]
        public void EveryLocatedUtopiaInTheSaveCorpusRoundTripsWithoutChangingAnyBytes()
        {
            var banksChecked = 0;
            var mixedTables = 0;
            var failures = new ConcurrentBag<string>();

            // Each save is loaded once, checked and dropped; the saves are independent, so they are walked in parallel.
            Parallel.ForEach(Directory.EnumerateFiles("TestData", "*.GM?", SearchOption.AllDirectories), path =>
            {
                var game = TestServices.GameService.LoadGameFromFile(path);
                var banks = game.History.Current.CreatureBanks();
                if (game.History.Current.GameVersion() != GameVersion.HornOfTheAbyss && banks.Count > 0)
                {
                    failures.Add($"{path}: a save that is not Horn of the Abyss has {banks.Count} Utopia records");
                }

                foreach (var bank in banks)
                {
                    if (!banks.Serializer.Serialize(bank, banks.Deserializer.Deserialize(bank)).HasSameBytes(bank))
                    {
                        failures.Add($"{path}: the Utopia at {bank.StartIndex} changed when it was deserialized and serialized again");
                    }

                    Interlocked.Increment(ref banksChecked);
                }

                if (banks.Count > 0)
                {
                    if (!game.GetBytesToSave().AsSpan().SequenceEqual(game.Bytes))
                    {
                        failures.Add($"{path}: the unedited save is not written back byte for byte");
                    }

                    if (!path.Contains("MerchantStock") && !path.Contains("DragonUtopias"))
                    {
                        Interlocked.Increment(ref mixedTables);
                    }
                }
            });

            Assert.That(failures, Is.Empty);
            Assert.That(banksChecked, Is.GreaterThanOrEqualTo(8 * 14));
            Assert.That(mixedTables, Is.GreaterThan(0));
        }
    }
}
