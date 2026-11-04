using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Tests.Data;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class TownMerchantTests
    {
        private static string Control(string name) => Path.Combine("TestData", "Hota", "MerchantStock", $"merchant_stock_{name}.GM2");

        [Test]
        public void RedBlueAndNeutralTownPagesShareOneStockAndRefreshTogetherThroughUndoRedoAndRevert()
        {
            var game = SavedGames.Get(Control("00_baseline"));
            var pages = Enumerable.Range(0, game.History.Current.Castles().Count)
                .Select(i => TestServices.CreateCastleViewModel(game.History.CreateCastleSession(i))).ToList();
            try
            {
                var shops = pages.Where(p => p.HasArtifactMerchant).ToList();
                Assert.That(shops.Select(p => p.Name), Is.EqualTo(new[] { "Red_Tow", "Red_Dung", "Red_Fac", "Neu_Tow", "Neu_Dung", "Neu_Fac", "Blue_Dung" }));
                Assert.That(shops.Select(p => p.OwnerText), Is.SupersetOf(new[] { "Red", "Blue", "Neutral" }));
                Assert.That(shops.All(p => p.HasMerchantStock), Is.True);
                var expectedNames = new[] { "Stoic Watchman", "Ladybird of Luck", "Breastplate of Petrified Wood", "Helm of Chaos", "Ring of Life", "Red Dragon Flame Tongue", "Head of Legion" };
                foreach (var shop in shops)
                {
                    Assert.That(shop.Merchant!.Slots.Select(s => s.ArtifactName), Is.EqualTo(expectedNames));
                }

                var original = game.Bytes.ToArray();
                var chunk = game.History.Current.TownMerchant().Single();
                var expected = original.ToArray();
                BitConverter.GetBytes(uint.MaxValue).CopyTo(expected, chunk.StartIndex + 24);
                shops.First(p => p.OwnerText == "Red").Merchant!.Slots[6].ClearCommand.Execute(null);

                Assert.That(shops.All(p => !p.Merchant!.Slots[6].HasArtifact && p.Merchant.HasPendingChanges), Is.True);
                Assert.That(game.GetBytesToSave(), Is.EqualTo(expected), "only the one shared slot is written, not the towns or Black Markets");
                var pending = game.History.GetPendingChanges().Single();
                Assert.That((pending.Kind, pending.Index, pending.OwnerName, pending.Change.Description),
                    Is.EqualTo((Homm3ChunkKinds.TownMerchant, 0, "Town Artifact Merchants", "Slot 7")));
                var bought = SavedGames.Get(Control("07_red_buys_town_head_of_legion")).History.Current.TownMerchant().GetModel(0);
                Assert.That(game.History.Current.TownMerchant().GetModel(0), Is.EqualTo(bought));
                var reloaded = TestServices.GameService.LoadGameFromBytes(game.GetBytesToSave(), "town_stock_edited.GM2");
                Assert.That(reloaded.History.Current.TownMerchant().GetModel(0).Slots[6], Is.Null);
                Assert.That(reloaded.History.Current.Merchants().GetModels(), Is.EqualTo(game.History.Original.Merchants().GetModels()));

                game.History.Undo();
                Assert.That(shops.All(p => p.Merchant!.Slots[6].ArtifactName == "Head of Legion" && !p.Merchant.HasPendingChanges), Is.True);
                Assert.That(game.GetBytesToSave(), Is.EqualTo(original));
                game.History.Redo();
                Assert.That(shops.All(p => !p.Merchant!.Slots[6].HasArtifact), Is.True);
                pending.Revert(game.History);
                Assert.That(game.GetBytesToSave(), Is.EqualTo(original));
                Assert.That(shops.All(p => p.Merchant!.Slots[6].ArtifactName == "Head of Legion"), Is.True);
            }
            finally
            {
                pages.ForEach(p => p.Dispose());
            }
        }

        [Test]
        public void CapturingANeutralMerchantTownPreservesItsUnknownPrefixByteAndStillReloadsEveryTown()
        {
            var game = SavedGames.Get(Control("00_baseline"));
            var castles = game.History.Current.Castles();
            var index = castles.GetModels().ToList().FindIndex(c => c.Name == "Neu_Dung");
            var chunk = castles[index];
            var prefixByte = chunk.StartIndex + CastleLayout.NameLength - 1;
            Assert.That(game.Bytes[prefixByte], Is.EqualTo(0x73));
            using var page = TestServices.CreateCastleViewModel(game.History.CreateCastleSession(index));
            Assert.That(page.OwnerText, Is.EqualTo("Neutral"));
            page.SelectedOwner = "Red";
            page.Merchant!.Slots[0].ClearCommand.Execute(null);

            var bytes = game.GetBytesToSave();
            var reloaded = TestServices.GameService.LoadGameFromBytes(bytes, "captured_merchant.GM2");
            Assert.That(bytes[prefixByte], Is.EqualTo(0x73));
            Assert.That(reloaded.History.Current.Castles().Count, Is.EqualTo(7));
            Assert.That(reloaded.History.Current.Castles().GetModel(index).Owner, Is.EqualTo(PlayerColor.Red));
            Assert.That(reloaded.History.Current.TownMerchant().GetModel(0).Slots[0], Is.Null);
        }

        [Test]
        public void TheSharedStockResetAndTownResetDoNotResetEachOtherOrTheBlackMarkets()
        {
            var game = SavedGames.Get(Control("00_baseline"));
            var index = game.History.Current.Castles().GetModels().ToList().FindIndex(c => c.Buildings.Any(b => b.IsArtifactMerchant));
            using var page = TestServices.CreateCastleViewModel(game.History.CreateCastleSession(index));
            page.CanBuildThisTurn = !page.CanBuildThisTurn;
            page.Merchant!.Slots[0].ClearCommand.Execute(null);
            game.History.CreateMerchantSession(0).Update(s => s.WithSlot(0, null));

            page.Merchant.ResetCommand.Execute(null);
            Assert.That(page.Merchant.HasPendingChanges, Is.False);
            Assert.That(page.HasPendingChanges, Is.True);
            Assert.That(game.History.Current.Merchants().GetModel(0).Slots[0], Is.Null);

            page.Merchant.Slots[0].ClearCommand.Execute(null);
            page.ResetCommand.Execute(null);
            Assert.That(page.HasPendingChanges, Is.False);
            Assert.That(page.Merchant.HasPendingChanges, Is.True);
            Assert.That(page.Merchant.Slots[0].HasArtifact, Is.False);
            Assert.That(game.History.Current.Merchants().GetModel(0).Slots[0], Is.Null);

            game.History.MarkSaved();
            Assert.That(page.Merchant.HasPendingChanges, Is.False);
            page.Merchant.Slots[1].ClearCommand.Execute(null);
            page.Merchant.ResetCommand.Execute(null);
            Assert.That(page.Merchant.Slots[0].HasArtifact, Is.False, "reset uses the last saved shared stock");
            Assert.That(page.Merchant.Slots[1].HasArtifact, Is.True);
        }

        [Test]
        public void RemovingAndRebuildingTheMerchantBuildingHidesAndRestoresTheTabWithoutLosingStockEdits()
        {
            var game = SavedGames.Get(Control("00_baseline"));
            var index = game.History.Current.Castles().GetModels().ToList().FindIndex(c => c.Buildings.Any(b => b.IsArtifactMerchant));
            var session = game.History.CreateCastleSession(index);
            var merchantBuilding = session.Current.Buildings.Single(b => b.IsArtifactMerchant);
            using var page = TestServices.CreateCastleViewModel(session);
            page.SelectedTab = 4;
            page.Merchant!.Slots[0].ClearCommand.Execute(null);
            session.Update(c => CastleBuilder.Unbuild(c, merchantBuilding));
            Assert.That(page.HasArtifactMerchant, Is.False);
            Assert.That(page.SelectedTab, Is.EqualTo(0));
            Assert.That(page.Merchant.HasPendingChanges, Is.True);
            game.History.Undo();
            Assert.That(page.HasArtifactMerchant, Is.True);
            Assert.That(page.Merchant.Slots[0].HasArtifact, Is.False);
        }

        [Test]
        public void AnUnrecognizedDatePrefixLeavesTheTownStockUnavailableRatherThanExposingAnEditableGuess()
        {
            var game = SavedGames.Get(Control("00_baseline"));
            var bytes = game.Bytes.ToArray();
            var stock = game.History.Current.TownMerchant().Single();
            bytes[stock.StartIndex - TownMerchantStockLayout.OffsetFromDate - 4] = 0;
            var unsupported = TestServices.GameService.LoadGameFromBytes(bytes, "unknown_town_stock.GM2");
            Assert.That(unsupported.History.Current.TownMerchant(), Is.Empty);
            var index = unsupported.History.Current.Castles().GetModels().ToList().FindIndex(c => c.Buildings.Any(b => b.IsArtifactMerchant));
            using var page = TestServices.CreateCastleViewModel(unsupported.History.CreateCastleSession(index));
            Assert.That(page.HasArtifactMerchant, Is.True);
            Assert.That(page.HasMerchantStock, Is.False);
            Assert.That(page.Merchant, Is.Null);
            Assert.That(unsupported.GetBytesToSave(), Is.EqualTo(bytes));
        }

        [Test]
        public void UnknownAndSoldOutTownSlotsRemainLoadableAndArePreservedWhileEditingAnotherSlot()
        {
            var game = SavedGames.Get(Control("00_baseline"));
            var bytes = game.Bytes.ToArray();
            var offset = game.History.Current.TownMerchant().Single().StartIndex;
            Array.Fill(bytes, (byte)0xFF, offset, TownMerchantStockLayout.Length);
            BitConverter.GetBytes(0x12345678u).CopyTo(bytes, offset);
            var odd = TestServices.GameService.LoadGameFromBytes(bytes, "unknown_artifact.GM2");
            var session = new EditSession<RecordChunk, MerchantStockModel>(odd.History, Homm3ChunkKinds.TownMerchant, 0);
            Assert.That(session.Current.Slots[0], Is.EqualTo(TestCatalog.Hota.FallbackArtifact));
            Assert.That(session.Current.Slots.Skip(1), Has.All.Null);
            Assert.That(odd.GetBytesToSave(), Is.EqualTo(bytes));

            var replacement = TestCatalog.Hota.Artifacts.ById("HeadOfLegion");
            session.Update(s => s.WithSlot(1, replacement));
            var expected = bytes.ToArray();
            BitConverter.GetBytes((uint)replacement.Code).CopyTo(expected, offset + 4);
            Assert.That(odd.GetBytesToSave(), Is.EqualTo(expected));
            odd.History.GetPendingChanges().Single().Revert(odd.History);
            Assert.That(odd.GetBytesToSave(), Is.EqualTo(bytes));
        }
    }
}
