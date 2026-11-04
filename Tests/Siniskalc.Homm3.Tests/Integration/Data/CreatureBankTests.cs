namespace Siniskalc.Homm3.Tests.Integration.Data
{
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Homm3.Tests.Data;

    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class CreatureBankTests
    {
        [Test]
        public void EveryControlledSaveExposesAllSixBankKindsAndRoundTripsWithoutAnEdit()
        {
            var controls = Directory.GetFiles(Path.Combine("TestData", "Hota", "CreatureBanks"), "creature_banks_*.GM?");
            Assert.That(controls.Length, Is.GreaterThanOrEqualTo(20));
            foreach (var path in controls)
            {
                var game = SavedGames.Get(path);
                var banks = game.History.Current.CreatureBanks();
                Assert.That(banks.Count, Is.EqualTo(28), path);
                Assert.That(banks.Select(b => b.BankIndex), Is.EqualTo(Enumerable.Range(0, 28)), path);
                Assert.That(banks.Select(b => b.Kind).Distinct(), Is.EquivalentTo(Enum.GetValues<CreatureBankKind>()), path);
                Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes), path);
            }
        }

        [TestCase("00_baseline.GM2", "02_cyclops_stockpile_1_stepped_off.GM2", 8)]
        [TestCase("00_baseline.GM2", "01_cyclops_stockpile_1_won.GM2", 8)]
        [TestCase("04_dwarven_treasury_1_before_victory.GM2", "06_dwarven_treasury_1_stepped_off.GM2", 12)]
        [TestCase("04_dwarven_treasury_1_before_victory.GM2", "05_dwarven_treasury_1_won.GM2", 12)]
        [TestCase("08_imp_cache_1_before_victory.GM1", "10_imp_cache_1_stepped_off.GM1", 16)]
        [TestCase("08_imp_cache_1_before_victory.GM1", "09_imp_cache_1_won.GM1", 16)]
        [TestCase("12_medusa_stores_1_before_victory.GM2", "14_medusa_stores_1_stepped_off.GM2", 20)]
        [TestCase("12_medusa_stores_1_before_victory.GM2", "13_medusa_stores_1_won.GM2", 20)]
        [TestCase("16_naga_bank_1_before_victory.GM1", "18_naga_bank_1_stepped_off.GM1", 24)]
        [TestCase("16_naga_bank_1_before_victory.GM1", "17_naga_bank_1_won.GM1", 24)]
        public void ResettingEachClearedControlWritesExactlyItsVerifiedGuardsAndEntranceAndCanBeUndone(string beforeName, string clearedName, int bankIndex)
        {
            var before = SavedGames.Get(Control(beforeName));
            var game = SavedGames.Get(Control(clearedName));
            var banks = game.History.Current.CreatureBanks();
            Assert.That(banks.Count, Is.EqualTo(28));
            var original = before.History.Current.CreatureBanks().Single(b => b.BankIndex == bankIndex);
            var bank = banks.Single(b => b.BankIndex == bankIndex);
            var expected = game.Bytes.ToArray();
            original.GetBytesAt(0, 56).CopyTo(expected, bank.StartIndex);
            original.Entrance.ToArray().CopyTo(expected, bank.Entrance.StartIndex);
            var originalModel = before.History.Current.CreatureBanks().GetModel(original.Index);
            Assert.That(banks.GetModel(bank.Index).IsCleared, Is.True);
            Assert.That(banks.GetModel(bank.Index).Strength, Is.EqualTo(4));
            game.History.CreateCreatureBankSession(bank.Index).Update(m => m with { IsCleared = false, UpgradedCenter = originalModel.UpgradedCenter });
            Assert.That(game.GetBytesToSave(), Is.EqualTo(expected));
            var reloaded = TestServices.GameService.LoadGameFromBytes(expected, "reset_bank.GM2");
            Assert.That(reloaded.History.Current.CreatureBanks().GetModel(bank.Index), Is.EqualTo(originalModel));
            game.History.Undo();
            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
            game.History.Redo();
            Assert.That(game.GetBytesToSave(), Is.EqualTo(expected));
            game.History.GetPendingChanges().Single(c => c.Change.Description == "Cleared").Revert(game.History);
            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
        }

        [TestCase(8, CreatureBankKind.CyclopsStockpile, 94, 10, 4)]
        [TestCase(12, CreatureBankKind.DwarvenTreasury, 16, 30, 10)]
        [TestCase(16, CreatureBankKind.ImpCache, 42, 60, 20)]
        [TestCase(20, CreatureBankKind.MedusaStores, 76, 10, 4)]
        [TestCase(24, CreatureBankKind.NagaBank, 38, 6, 2)]
        public void EachSupportedBankHasFourPresetsAndChangingItsTierWritesOnlyItsOwnGuardsAndResources(int bankIndex, CreatureBankKind kind, int creature, int strongest, int weakest)
        {
            var game = SavedGames.Get(Control("00_baseline.GM2"));
            var banks = game.History.Current.CreatureBanks();
            var group = banks.Where(b => b.Kind == kind).ToList();
            Assert.That(group.Select(b => b.BankIndex), Is.EqualTo(Enumerable.Range(bankIndex, 4)));
            Assert.That(group.Select(b => banks.GetModel(b.Index).Strength), Is.EqualTo(new[] { 4, 3, 2, 1 }));
            var bank = group[0];
            Assert.That(bank.GetInt32At(28), Is.EqualTo(strongest));
            game.History.CreateCreatureBankSession(bank.Index).Update(m => m with { Strength = 1, UpgradedCenter = true });
            var edited = game.GetBytesToSave();
            Assert.That(Enumerable.Range(0, 5).Select(i => BitConverter.ToInt32(edited, bank.StartIndex + (i * 4))), Is.EqualTo(new[] { creature, creature, creature + 1, creature, creature }));
            Assert.That(Enumerable.Range(0, 7).Select(i => BitConverter.ToInt32(edited, bank.StartIndex + 28 + (i * 4))), Is.EqualTo(new[] { weakest, weakest, weakest, weakest, weakest, 0, 0 }));
            var expectedResources = kind switch
            {
                CreatureBankKind.CyclopsStockpile => new[] { 4, 4, 4, 4, 4, 4, 0 },
                CreatureBankKind.DwarvenTreasury => [0, 0, 0, 0, 2, 0, 2500],
                CreatureBankKind.ImpCache => [0, 2, 0, 0, 0, 0, 1000],
                CreatureBankKind.MedusaStores => [0, 0, 0, 5, 0, 0, 2000],
                _ => [0, 0, 0, 0, 0, 8, 4000],
            };
            Assert.That(Enumerable.Range(0, 7).Select(i => BitConverter.ToInt32(edited, bank.StartIndex + 56 + (i * 4))), Is.EqualTo(expectedResources));
            Assert.That(edited[..bank.StartIndex], Is.EqualTo(game.Bytes[..bank.StartIndex]));
            Assert.That(edited[(bank.StartIndex + 84)..], Is.EqualTo(game.Bytes[(bank.StartIndex + 84)..]));
            Assert.That(TestServices.GameService.LoadGameFromBytes(edited, "weaker_bank.GM2").History.Current.CreatureBanks().GetModel(bank.Index).Strength, Is.EqualTo(1));
        }

        [Test]
        public void AnUnestablishedEntranceStateIsPreservedAndDoesNotHideTheOtherSupportedBanks()
        {
            var game = SavedGames.Get(Control("18_naga_bank_1_stepped_off.GM1"));
            var bank = game.History.Current.CreatureBanks().Single(b => b.BankIndex == 24);
            var bytes = game.Bytes.ToArray();
            BitConverter.GetBytes(0xFE03041Fu).CopyTo(bytes, bank.Entrance.StartIndex);
            var unknown = TestServices.GameService.LoadGameFromBytes(bytes, "unknown_bank_state.GM1");
            Assert.That(unknown.History.Current.CreatureBanks().Count, Is.EqualTo(27));
            Assert.That(unknown.History.Current.CreatureBanks().Any(b => b.BankIndex == 24), Is.False);
            Assert.That(unknown.GetBytesToSave(), Is.EqualTo(bytes));
        }

        [Test]
        public void AnArtifactEditPreservesNonPresetGuardBytesWrittenInTheHexEditor()
        {
            var game = SavedGames.Get(Control("00_baseline.GM2"));
            using var target = TestServices.ByteEditTargets.ForCreatureBank(game.History, 0);
            var raw = target.GetBytes();
            BitConverter.GetBytes(999).CopyTo(raw, 28);
            target.Update(_ => raw);
            var expected = game.GetBytesToSave();
            game.History.CreateCreatureBankSession(0).Update(m => m.WithArtifact(0, null));
            BitConverter.GetBytes(uint.MaxValue).CopyTo(expected, game.History.Current.CreatureBanks()[0].StartIndex + 91);
            Assert.That(game.GetBytesToSave(), Is.EqualTo(expected));
            game.History.Undo();
            Assert.That(game.History.Current.CreatureBanks()[0].GetInt32At(28), Is.EqualTo(999));
        }

        private static string Control(string name) => Path.Combine("TestData", "Hota", "CreatureBanks", $"creature_banks_{name}");
    }
}
