using Siniskalc.Core.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// Town captures replayed against the HotA hotseat saves supplied for count, acquisition order and selection.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class TownOwnershipTests
    {
        private static SaveDocumentHistory Load(string save) =>
            SavedGames.Get(Path.Combine("TestData", "Hota", "TownOwnership", save + ".GM2")).History;

        private static PlayerChunk Player(SaveDocument document, PlayerColor color) =>
            document.Players().Concat(document.InactivePlayers()).Single(p => p.Color == color);

        private static byte[] Towns(PlayerChunk chunk) =>
            chunk.GetBytesAt(PlayerLayout.TownIds, chunk.GetByteAt(PlayerLayout.TownCount));

        [TestCase("town_ownership_00_baseline", "0,1", "2")]
        [TestCase("town_ownership_01_blue_captures_red_inferno", "0", "2,1")]
        [TestCase("town_ownership_02_blue_captures_red_tower", "", "2,1,0")]
        [TestCase("town_ownership_03_red_townless_day_2", "", "2,1,0")]
        [TestCase("town_ownership_04_red_recaptures_red_inferno", "1", "2,0")]
        [TestCase("town_ownership_05_blue_idle_day_2", "1", "2,0")]
        [TestCase("town_ownership_06_red_idle_day_3", "1", "2,0")]
        [TestCase("town_ownership_07_red_captures_neutral_bulwar", "1,3", "2,0")]
        [TestCase("town_ownership_08_red_selects_red_inferno", "1,3", "2,0")]
        [TestCase("town_ownership_09_blue_captures_red_inferno__selects_red_tower", "3", "2,0,1")]
        [TestCase("town_ownership_10_before_red_recaptures_red_tower", "3", "2,0,1")]
        [TestCase("town_ownership_11_red_recaptures_red_tower", "3,0", "2,1")]
        public void TheTownCountAndAcquisitionOrderMatchTheInGameCaptureSequence(string save, string red, string blue)
        {
            var document = Load(save).Current;
            Assert.That(document.Castles().Select(c => c.Name), Is.EqualTo(new[] { "Red_Tower", "Red_Inferno", "Blue_Necropoli", "Neutral_Bulwar" }));
            foreach (var (color, expected) in new[] { (PlayerColor.Red, red), (PlayerColor.Blue, blue) })
            {
                var chunk = Player(document, color);
                var ids = Towns(chunk);
                Assert.That(string.Join(",", ids), Is.EqualTo(expected), color.Name);
                Assert.That(ids.Select(id => document.Castles().GetModel(id).Owner), Has.All.EqualTo(color));
                Assert.That(document.Castles().GetOwners().Count(owner => owner == color), Is.EqualTo(ids.Length));
                Assert.That(chunk.GetBytesAt(PlayerLayout.TownIds + ids.Length, Constants.Player.TownListSize - ids.Length), Has.All.EqualTo(Constants.ByteOff));
                var players = document.Players();
                Assert.That(players.ListsSerializer.Serialize(chunk, players.GetLists(players.FindByColor(color))).ToArray(), Is.EqualTo(chunk.ToArray()));
            }
        }

        [TestCase("town_ownership_00_baseline", "town_ownership_01_blue_captures_red_inferno", "Red_Inferno", "blue")]
        [TestCase("town_ownership_01_blue_captures_red_inferno", "town_ownership_02_blue_captures_red_tower", "Red_Tower", "blue")]
        [TestCase("town_ownership_03_red_townless_day_2", "town_ownership_04_red_recaptures_red_inferno", "Red_Inferno", "red")]
        [TestCase("town_ownership_06_red_idle_day_3", "town_ownership_07_red_captures_neutral_bulwar", "Neutral_Bulwar", "red")]
        [TestCase("town_ownership_08_red_selects_red_inferno", "town_ownership_09_blue_captures_red_inferno__selects_red_tower", "Red_Inferno", "blue")]
        [TestCase("town_ownership_10_before_red_recaptures_red_tower", "town_ownership_11_red_recaptures_red_tower", "Red_Tower", "red")]
        public void EditingOwnershipWritesTheSameTownListsAsAnInGameCapture(string before, string after, string town, string ownerName)
        {
            var history = Load(before);
            var original = history.Current;
            var expected = Load(after).Current;
            var owner = ownerName == "blue" ? PlayerColor.Blue : PlayerColor.Red;
            history.CreateCastleSession(original.Castles().FindByName(town)).Update(c => c with { Owner = owner });

            var reloaded = TestServices.GameService.LoadGameFromBytes(history.Current.ToBytes(), "edited.GM2").History.Current;
            foreach (var color in new[] { PlayerColor.Red, PlayerColor.Blue })
            {
                var edited = Player(history.Current, color);
                Assert.That(edited.GetByteAt(PlayerLayout.TownCount), Is.EqualTo(Player(expected, color).GetByteAt(PlayerLayout.TownCount)));
                Assert.That(edited.GetBytesAt(PlayerLayout.TownIds, Constants.Player.TownListSize), Is.EqualTo(Player(expected, color).GetBytesAt(PlayerLayout.TownIds, Constants.Player.TownListSize)));
                Assert.That(Towns(Player(reloaded, color)), Is.EqualTo(Towns(edited)));
                Assert.That(edited.GetByteAt(PlayerLayout.SelectedTown), Is.EqualTo(Player(original, color).GetByteAt(PlayerLayout.SelectedTown)));
            }
        }

        [Test]
        public void OwnershipAndBothPlayerRecordsAreOneUndoableEditAndOnePendingOwnerChange()
        {
            var history = Load("town_ownership_00_baseline");
            var original = history.Current;
            var notifications = 0;
            history.Changed += (_, _) => notifications++;
            history.CreateCastleSession(0).Update(c => c with { Owner = PlayerColor.Blue });
            var edited = history.Current;
            Assert.That(notifications, Is.EqualTo(1));
            Assert.That(history.GetPendingChanges().Where(c => c.Change.Description == "Owner").Count(), Is.EqualTo(1));
            history.Undo();
            Assert.That(history.Current, Is.SameAs(original));
            Assert.That(history.CanUndo, Is.False);
            history.Redo();
            Assert.That(history.Current, Is.SameAs(edited));
            history.GetPendingChanges().Single(c => c.Change.Description == "Owner").Revert(history);
            Assert.That(history.Current.ToBytes(), Is.EqualTo(original.ToBytes()));
            Assert.That(history.GetPendingChanges(), Is.Empty);
        }

        [Test]
        public void RevertingSeveralOwnerChangesInEitherOrderRestoresTheWholeSave()
        {
            foreach (var reverse in new[] { false, true })
            {
                var history = Load("town_ownership_00_baseline");
                var before = history.Current.ToBytes();
                history.CreateCastleSession(0).Update(c => c with { Owner = PlayerColor.Blue });
                history.CreateCastleSession(1).Update(c => c with { Owner = PlayerColor.Blue });
                var changes = history.GetPendingChanges().Where(c => c.Change.Description == "Owner").ToList();
                foreach (var change in reverse ? changes.AsEnumerable().Reverse() : changes)
                {
                    change.Revert(history);
                }

                Assert.That(history.Current.ToBytes(), Is.EqualTo(before));
            }
        }

        [Test]
        public void ANeutralTransferRemovesTheTownAndRevertingRestoresTheWholeSave()
        {
            var history = Load("town_ownership_00_baseline");
            var before = history.Current.ToBytes();
            history.CreateCastleSession(0).Update(c => c with { Owner = PlayerColor.Neutral });
            Assert.That(Towns(Player(history.Current, PlayerColor.Red)), Is.EqualTo(new byte[] { 1 }));
            history.GetPendingChanges().Single(c => c.Change.Description == "Owner").Revert(history);
            Assert.That(history.Current.ToBytes(), Is.EqualTo(before));
        }

        [Test]
        public void ResettingACastleRestoresItsOwnerAndTheOriginalTownOrder()
        {
            var history = Load("town_ownership_00_baseline");
            var before = history.Current.ToBytes();
            var castle = history.CreateCastleSession(0);
            castle.Update(c => c with { Owner = PlayerColor.Blue });
            castle.Update(_ => castle.Original);
            Assert.That(history.Current.ToBytes(), Is.EqualTo(before));
        }

        [Test]
        public void AFullTownListRefusesAnAcquisitionWithoutChangingEitherPlayerOrTheCastle()
        {
            var history = Load("town_ownership_00_baseline");
            history.CreatePlayerSession(history.Current.Players().FindByColor(PlayerColor.Blue)).UpdateBytes(bytes =>
            {
                bytes[PlayerLayout.TownCount] = Constants.Player.TownListSize;
                for (var i = 0; i < Constants.Player.TownListSize; i++)
                {
                    bytes[PlayerLayout.TownIds + i] = (byte)(i + 1);
                }

                return bytes;
            });
            var before = history.Current;
            var notifications = 0;
            history.Changed += (_, _) => notifications++;
            Assert.That(() => history.CreateCastleSession(0).Update(c => c with { Owner = PlayerColor.Blue }), Throws.TypeOf<GameRuleException>());
            Assert.That(history.Current, Is.SameAs(before));
            Assert.That(notifications, Is.Zero);
        }

        [Test]
        public void GivingATownToAHiddenColourUpdatesItsRetainedPlayerRecord()
        {
            var history = Load("town_ownership_00_baseline");
            var before = history.Current.ToBytes();
            Assert.That(history.Current.Players().FindByColor(PlayerColor.Green), Is.EqualTo(-1));
            history.CreateCastleSession(0).Update(c => c with { Owner = PlayerColor.Green });
            Assert.That(Towns(Player(history.Current, PlayerColor.Green)), Is.EqualTo(new byte[] { 0 }));
            var reloaded = TestServices.GameService.LoadGameFromBytes(history.Current.ToBytes(), "edited.GM2").History.Current;
            Assert.That(Towns(Player(reloaded, PlayerColor.Green)), Is.EqualTo(new byte[] { 0 }));
            history.GetPendingChanges().Single(c => c.Change.Description == "Owner").Revert(history);
            Assert.That(history.Current.ToBytes(), Is.EqualTo(before));
        }

        [Test]
        public void RawOwnershipEditsLeaveTheTownListsAloneAsTheCurrentHexEditorContractRequires()
        {
            var history = Load("town_ownership_00_baseline");
            var before = history.Current;
            history.CreateCastleSession(0).UpdateBytes(bytes =>
            {
                bytes[CastleLayout.Owner] = (byte)PlayerColor.Blue.Code;
                return bytes;
            });
            Assert.That(Towns(Player(history.Current, PlayerColor.Red)), Is.EqualTo(Towns(Player(before, PlayerColor.Red))));
            Assert.That(Towns(Player(history.Current, PlayerColor.Blue)), Is.EqualTo(Towns(Player(before, PlayerColor.Blue))));
        }

        [Test]
        public void AnOwnershipEditChangesOnlyTheOwnerCountAndUsedTownListSlotsWhenBothPlayersStillHaveTowns()
        {
            var history = Load("town_ownership_08_red_selects_red_inferno");
            var original = history.Current;
            var before = original.ToBytes();
            history.CreateCastleSession(1).Update(c => c with { Owner = PlayerColor.Blue });
            var red = Player(original, PlayerColor.Red).StartIndex;
            var blue = Player(original, PlayerColor.Blue).StartIndex;
            var allowed = new[]
            {
                original.Castles()[1].StartIndex + CastleLayout.Owner,
                red + PlayerLayout.TownCount, red + PlayerLayout.TownIds, red + PlayerLayout.TownIds + 1,
                blue + PlayerLayout.TownCount, blue + PlayerLayout.TownIds + 2,
            };
            Assert.That(ByteDiff.Of(before, history.Current.ToBytes()).Select(d => d.Item1), Is.EquivalentTo(allowed));
        }

        [Test]
        public void ShadowOfDeathTownOwnershipUsesTheSameCountAndListLayout()
        {
            var history = SavedGames.Get(Path.Combine("TestData", "Roe", "roe_00_baseline.GM1")).History;
            var before = history.Current.ToBytes();
            var index = history.Current.Castles().FindByName("1_Castle");
            var oldIds = Towns(Player(history.Current, PlayerColor.Red));
            history.CreateCastleSession(index).Update(c => c with { Owner = PlayerColor.Blue });
            Assert.That(Towns(Player(history.Current, PlayerColor.Red)), Is.EqualTo(oldIds.Where(id => id != index)));
            Assert.That(Towns(Player(history.Current, PlayerColor.Blue)), Does.Contain((byte)index));
            history.GetPendingChanges().Single(c => c.Change.Description == "Owner").Revert(history);
            Assert.That(history.Current.ToBytes(), Is.EqualTo(before));
        }
    }
}
