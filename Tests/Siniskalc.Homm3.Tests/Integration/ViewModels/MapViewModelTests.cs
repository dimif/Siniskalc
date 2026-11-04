using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// The page of the map: what the savegame keeps about its merchants.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class MapViewModelTests
    {
        [Test]
        public void ADefeatedImpCacheExplainsItsUnknownUpgradeAndRestoresTheChosenCentreWithoutChangingItsRewards()
        {
            var game = SavedGames.Get(Path.Combine("TestData", "Hota", "CreatureBanks", "creature_banks_10_imp_cache_1_stepped_off.GM1"));
            using var page = new MapViewModel(game, TestServices.EffectLines, TestServices.BankText, TestServices.ByteEditTargets);
            page.ShowSection(MapSections.ImpCaches, 16);
            var bank = page.SelectedBank!;
            Assert.That(page.VisibleBanks, Has.Count.EqualTo(4));
            Assert.That(page.BankGroupName, Is.EqualTo("Imp Caches"));
            Assert.That(bank.Location, Is.EqualTo("2, 27, Surface"));
            Assert.That(bank.Heading, Is.EqualTo("Imp Cache 1 at 2, 27, Surface"));
            Assert.That(bank.HasUnknownOriginalGuards, Is.True);
            Assert.That(bank.UpgradedCenter, Is.False);
            Assert.That(bank.RewardSummary, Is.EqualTo("6 mercury, 3k gold"));
            bank.UpgradedCenter = true;
            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes), "choosing a future reset's centre alone does not write a defeated record");
            bank.RestoreCommand.Execute(null);
            Assert.That(bank.HasPendingChanges, Is.True);
            Assert.That(game.History.Current.CreatureBanks()[16].GetInt32At(8), Is.EqualTo(43));
            Assert.That(bank.RewardSummary, Is.EqualTo("6 mercury, 3k gold"));
            game.History.Undo();
            Assert.That(bank.IsCleared, Is.True);
            Assert.That(bank.UpgradedCenter, Is.True);
            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
            game.History.Redo();
            Assert.That(bank.IsCleared, Is.False);
            page.ShowSection(MapSections.NagaBanks);
            Assert.That(page.SelectedBank, Is.Null);
            Assert.That(page.VisibleBanks.Select(b => b.Kind), Is.All.EqualTo(CreatureBankKind.NagaBank));
            Assert.That(page.Banks.Single(b => b.Index == 16).HasPendingChanges, Is.True);
        }

        [Test]
        public void TheMerchantsTabHasASlotPickerForEachOfTheSevenArtifactsOfEachMerchantAndClearingOneMarksItSold()
        {
            var game = SavedGames.Get(Path.Combine("TestData", "Hota", "VisitedPlaces", "visited_places_27_tazar_artifact_merchant.GM2"));
            var page = new MapViewModel(game, TestServices.EffectLines, TestServices.BankText, TestServices.ByteEditTargets);

            Assert.That(page.Merchants.Select(m => m.Name), Is.EqualTo(new[] { "Black market 1", "Black market 2" }));
            Assert.That(page.Merchants[0].Slots.Select(s => s.ArtifactName), Is.EqualTo(new[] { "Bow of Elven Cherrywood", "Pendant of Total Recall", "Clover of Fortune", "Inexhaustible Cart of Ore", "Scales of the Greater Basilisk", "Vampire's Cowl", "Everflowing Crystal Cloak" }));

            page.Merchants[1].Slots[0].ClearCommand.Execute(null);

            Assert.That(page.Merchants[1].Slots[0].HasArtifact, Is.False);
            Assert.That(game.History.Current.Merchants().GetModel(1).Slots[0], Is.Null);
            Assert.That(page.HasPendingChanges, Is.True);

            page.ResetCommand.Execute(null);

            Assert.That(page.Merchants[1].Slots[0].ArtifactName, Is.EqualTo(TestCatalog.Hota.Artifacts.Find(0x49)!.Name));
            Assert.That(page.HasPendingChanges, Is.False);
        }

        [Test]
        public void TheEightPurchaseVerifiedMarketsShowTheirMapAnchorsInStockOrder()
        {
            foreach (var path in Directory.GetFiles(Path.Combine("TestData", "Hota", "MerchantStock"), "*.GM2"))
            {
                var game = SavedGames.Get(path);
                var page = new MapViewModel(game, TestServices.EffectLines, TestServices.BankText, TestServices.ByteEditTargets);

                Assert.That(game.Places.Merchants.Select(m => (m.Number, m.Position.X, m.Position.Y, m.Position.Level)), Is.EqualTo(
                    Enumerable.Range(0, 8).Select(i => (i, 2 + (3 * i), 8, 0))), path);
                Assert.That(page.Merchants.Select(m => m.Name), Is.EqualTo(Enumerable.Range(1, 8).Select(i => $"Black market {i}")), path);
                Assert.That(page.Merchants.Select(m => m.Location), Is.EqualTo(Enumerable.Range(0, 8).Select(i => $"At {2 + (3 * i)}, 8")), path);
                Assert.That(page.Merchants.Select(m => m.HasLocation), Has.All.True, path);
            }
        }

        [Test]
        public void AMapWithFourBlackMarketsShowsTheStockAndTheTileOfEachAndEditsOnlyTheOnePicked()
        {
            var game = SavedGames.Get(Path.Combine("TestData", "Hota", "multi_merchants.GM1"));
            var page = new MapViewModel(game, TestServices.EffectLines, TestServices.BankText, TestServices.ByteEditTargets);

            Assert.That(page.Merchants.Select(m => m.Name), Is.EqualTo(new[] { "Black market 1", "Black market 2", "Black market 3", "Black market 4" }));
            Assert.That(page.Merchants.Select(m => m.Location), Is.EqualTo(new[] { "At 28, 34", "At 28, 37", "At 31, 34", "At 31, 37" }));
            Assert.That(page.Merchants.Select(m => string.Join(",", m.Slots.Select(s => s.ArtifactName))).Distinct().Count(), Is.EqualTo(4));
            var before = game.History.Current.Merchants().GetModels().Select(m => m.Slots.ToList()).ToList();

            page.Merchants[2].Slots[0].ClearCommand.Execute(null);

            var after = game.History.Current.Merchants().GetModels();
            Assert.That(after[2].Slots[0], Is.Null);
            Assert.That(after.Select((m, i) => i == 2 ? m.Slots.Skip(1).SequenceEqual(before[i].Skip(1)) : m.Slots.SequenceEqual(before[i])), Has.All.True);
        }

        [Test]
        public void EditingAClearedUtopiaDoesNotRestoreItUntilRequestedAndItsUndoLeavesMarketEditsAlone()
        {
            var game = SavedGames.Get(Path.Combine("TestData", "Hota", "DragonUtopias", "dragon_utopias_01_first_cleared.GM2"));
            using var page = new MapViewModel(game, TestServices.EffectLines, TestServices.BankText, TestServices.ByteEditTargets);
            var utopia = page.VisibleBanks[0];
            Assert.That(page.HasVisibleBanks, Is.True);
            Assert.That(utopia.IsCleared, Is.True);
            Assert.That(utopia.Status, Is.EqualTo("Defeated"));
            Assert.That(utopia.Tier, Is.EqualTo(4));
            Assert.That(utopia.Location, Is.EqualTo("4, 15, Surface"));
            Assert.That(utopia.RewardSummary, Is.EqualTo("50k gold, Admiral's Hat, Angel Wings, Angelic Alliance, Armageddon's Blade"));
            Assert.That(utopia.Garrison, Is.EqualTo(new[] { "8 Green Dragons", "7 Red Dragons", "6 Gold Dragons", "5 Black Dragons" }));
            utopia.SelectedStrength = 0;
            Assert.That(utopia.Garrison, Is.EqualTo(new[] { "8 Green Dragons", "5 Red Dragons", "2 Gold Dragons", "1 Black Dragon" }));
            Assert.That(utopia.ResourceReward, Is.EqualTo(TestServices.BankText.Resources(CreatureBankKind.DragonUtopia, 1)));
            utopia.Artifacts[3].ClearCommand.Execute(null);
            Assert.That(utopia.IsCleared, Is.True);
            Assert.That(game.History.Current.CreatureBanks()[0].GetUInt32At(28), Is.Zero);
            utopia.RestoreCommand.Execute(null);
            Assert.That(utopia.IsCleared, Is.False);
            Assert.That(utopia.Status, Is.EqualTo("Uncleared"));
            Assert.That(game.History.Current.CreatureBanks()[0].GetUInt32At(80), Is.EqualTo(20000));
            Assert.That(game.History.Current.CreatureBanks()[0].GetUInt32At(40), Is.EqualTo(1));
            Assert.That(utopia.Artifacts[3].HasArtifact, Is.False);
            game.History.Undo();
            Assert.That(utopia.IsCleared, Is.True);
            game.History.Redo();
            Assert.That(utopia.IsCleared, Is.False);
            page.Merchants[0].Slots[0].ClearCommand.Execute(null);
            utopia.ResetCommand.Execute(null);
            Assert.That(utopia.IsCleared, Is.True);
            Assert.That(utopia.HasPendingChanges, Is.False);
            Assert.That(page.Merchants[0].Slots[0].HasArtifact, Is.False);
            Assert.That(page.HasPendingChanges, Is.True);
            page.ResetCommand.Execute(null);
            Assert.That(page.HasPendingChanges, Is.False);
            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
        }

        [Test]
        public void TheOverviewStateSelectorRestoresOrClearsTheGuardsAndUndoRestoresTheSavedBytes()
        {
            var game = SavedGames.Get(Path.Combine("TestData", "Hota", "DragonUtopias", "dragon_utopias_01_first_cleared.GM2"));
            using var page = new MapViewModel(game, TestServices.EffectLines, TestServices.BankText, TestServices.ByteEditTargets);
            var bank = page.VisibleBanks[0];
            Assert.That(bank.DefeatChoices, Is.EqualTo(new[] { "Undefeated", "Defeated" }));
            Assert.That(bank.SelectedDefeat, Is.EqualTo(1));
            var reward = bank.RewardSummary;
            bank.SelectedDefeat = 0;
            Assert.That(bank.IsCleared, Is.False);
            Assert.That(game.History.Current.CreatureBanks()[0].GetUInt32At(28), Is.EqualTo(8));
            Assert.That(page.BankSummary, Is.EqualTo("8 Dragon Utopias: 8 uncleared, 0 defeated"));
            bank.SelectedDefeat = 1;
            Assert.That(bank.IsCleared, Is.True);
            Assert.That(game.History.Current.CreatureBanks()[0].GetUInt32At(28), Is.Zero);
            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
            game.History.Undo();
            Assert.That(bank.SelectedDefeat, Is.Zero);
            Assert.That(bank.RewardSummary, Is.EqualTo(reward));
            game.History.Undo();
            Assert.That(bank.SelectedDefeat, Is.EqualTo(1));
            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
            bank.SelectedStrength = 0;
            Assert.That(bank.Tier, Is.EqualTo(1));
            Assert.That(bank.RewardSummary, Does.StartWith("20k gold, "));
            bank.Artifacts[0].ClearCommand.Execute(null);
            Assert.That(bank.RewardSummary, Does.StartWith("20k gold, (empty), Angel Wings, "));
        }
    }
}
