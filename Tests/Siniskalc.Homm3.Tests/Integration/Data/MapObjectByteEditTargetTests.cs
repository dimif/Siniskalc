using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// The hex view of one mine and of one dwelling of the map (<c>TestData/Hota/Mines</c> and <c>TestData/Hota/Dwellings</c>).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class MapObjectByteEditTargetTests
    {
        private static Siniskalc.Homm3.Runtime.Game LoadMines() => SavedGames.Get(Path.Combine("TestData", "Hota", "Mines", "mines_08_red_abandoned_mine_sulfur.GM2"));

        private static Siniskalc.Homm3.Runtime.Game LoadDwellings() => SavedGames.Get(Path.Combine("TestData", "Hota", "Dwellings", "dwellings_01_tazar_flags_guardhouse.GM2"));

        [Test]
        public void TheHexTabOfTheMapListsTheMerchantsBeforeTheBanksAndShowsTheBytesOfAMerchant()
        {
            var game = SavedGames.Get(Path.Combine("TestData", "Hota", "MerchantStock", "merchant_stock_00_baseline.GM2"));
            using var page = new MapViewModel(game, TestServices.EffectLines, TestServices.BankText, TestServices.ByteEditTargets);
            var merchants = game.History.Current.Merchants().Count;

            Assert.That(merchants, Is.GreaterThan(0));
            Assert.That(page.Records, Has.Count.EqualTo(merchants + page.Banks.Count));
            Assert.That(page.Records.Take(merchants).Select(r => r.Name), Is.EqualTo(page.Merchants.Select(m => m.Name)));
            page.SelectedRecord = page.Records[0];
            using var target = page.CreateByteEditTarget()!;
            Assert.That(target.GetBytes(), Is.EqualTo(game.History.Current.Merchants()[0].ToArray()));
        }

        [Test]
        public void TheBytesOfAMineAreNamedByTheirFieldsAndAnEditChangesTheMine()
        {
            var game = LoadMines();
            var mine = game.History.Current.Mines().GetModel(0);

            using var target = TestServices.ByteEditTargets.ForMine(game.History, 0);

            Assert.That(target.Title, Is.EqualTo(mine.Name));
            Assert.That(target.Regions.Select(r => r.Name), Is.EqualTo(new[] { "Owner", "Kind", "Abandoned", "Guard creatures", "Guard count", "Pos X", "Pos Y", "Pos Lvl" }));
            var owner = target.Regions.Single(r => r.Name == "Owner").Start;
            Assert.That(target.Update(bytes => { bytes[owner] = (byte)PlayerColor.Blue.Code; return bytes; }), Is.True);
            Assert.That(game.History.Current.Mines().GetModel(0).Owner, Is.EqualTo(PlayerColor.Blue));
        }

        [Test]
        public void TheBytesOfADwellingAreNamedByTheirFieldsAndAnEditChangesTheDwelling()
        {
            var game = LoadDwellings();
            var dwelling = game.History.Current.Dwellings().GetModel(0);

            using var target = TestServices.ByteEditTargets.ForDwelling(game.History, 0);

            Assert.That(target.Title, Is.EqualTo(dwelling.Name));
            Assert.That(target.Regions.Select(r => r.Name), Does.Contain("Owner").And.Contain("Creature").And.Contain("Creatures to hire"));
            var stock = target.Regions.Single(r => r.Name == "Creatures to hire").Start;
            Assert.That(target.Update(bytes => { bytes[stock] = 77; return bytes; }), Is.True);
            Assert.That(game.History.Current.Dwellings().GetModel(0).Stock % 256, Is.EqualTo(77));
        }

        [Test]
        public void ThePickedUtopiasHexEditsItsRewardAndTracksModelEditsUndoAndDisposal()
        {
            var path = Path.Combine("TestData", "Hota", "DragonUtopias", "dragon_utopias_01_first_cleared.GM2");
            var game = SavedGames.Get(path);
            using var page = new MapViewModel(game, TestServices.EffectLines, TestServices.BankText, TestServices.ByteEditTargets);
            page.SelectedBank = page.VisibleBanks[1];
            using var target = page.CreateByteEditTarget()!;
            var original = target.GetBytes();
            Assert.That(original, Is.EqualTo(game.History.Current.CreatureBanks()[1].ToArray()));
            var notifications = 0;
            target.Changed += (_, _) => notifications++;
            var artifact = target.Regions.Single(r => r.Name == "Artifacts").Start;
            target.Update(bytes =>
            {
                Array.Fill(bytes, byte.MaxValue, artifact, 4);
                return bytes;
            });
            Assert.That(page.VisibleBanks[1].Artifacts[0].HasArtifact, Is.False);
            Assert.That(target.GetBytes(), Is.EqualTo(original.Take(artifact).Concat(Enumerable.Repeat(byte.MaxValue, 4)).Concat(original.Skip(artifact + 4))));
            game.History.Undo();
            Assert.That(target.GetBytes(), Is.EqualTo(original));
            Assert.That(notifications, Is.EqualTo(2));
            page.VisibleBanks[1].SelectedStrength = 0;
            Assert.That(BitConverter.ToInt32(target.GetBytes(), 80), Is.EqualTo(20000));
            Assert.That(notifications, Is.EqualTo(3));
            target.Dispose();
            game.History.Undo();
            Assert.That(notifications, Is.EqualTo(3));
        }

        [Test]
        public void AnOwnerPicksADwellingAndTheBytesAreThoseOfThePickedOne()
        {
            var game = LoadDwellings();
            var objects = new OwnedMapObjectsViewModel(game.History, PlayerColor.Neutral, TestServices.ByteEditTargets);
            Assert.That(objects.Dwellings, Has.Count.GreaterThan(1));
            Assert.That(objects.SelectedDwelling, Is.SameAs(objects.Dwellings[0]));

            objects.SelectedDwelling = objects.Dwellings[1];
            using var target = objects.CreateDwellingByteEditTarget()!;

            Assert.That(target.Title, Is.EqualTo(game.History.Current.Dwellings().GetModel(objects.Dwellings[1].Index).Name));
        }

        [Test]
        public void ADwellingGivenToAPlayerLeavesTheListAndThePickedOneMovesOn()
        {
            var game = LoadDwellings();
            var objects = new OwnedMapObjectsViewModel(game.History, PlayerColor.Neutral, TestServices.ByteEditTargets);
            var first = objects.SelectedDwelling!;

            first.OwnerField.Selected = "Blue";
            objects.Refresh();

            Assert.That(objects.Dwellings, Does.Not.Contain(first));
            Assert.That(objects.SelectedDwelling, Is.Not.SameAs(first));
            Assert.That(objects.SelectedDwelling, Is.SameAs(objects.Dwellings.FirstOrDefault()));
        }
    }
}
