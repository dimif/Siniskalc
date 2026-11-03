using Siniskalc;
using Siniskalc.Core.Data;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    [TestFixture]
    internal sealed class CastleOwnershipTests
    {
        private static Game Load(string save) =>
            TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "TownOwnership", save + ".GM2"));

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
            var document = Load(save).History.Current;
            Assert.That(document.Castles().Select(c => c.Name), Is.EqualTo(new[] { "Red_Tower", "Red_Inferno", "Blue_Necropoli", "Neutral_Bulwar" }));
            foreach (var (color, expected) in new[] { (PlayerColor.Red, red), (PlayerColor.Blue, blue) })
            {
                var chunk = Player(document, color);
                var ids = Towns(chunk);
                Assert.That(string.Join(",", ids), Is.EqualTo(expected), color.ToString());
                Assert.That(ids.Select(id => document.Castles().GetModel(id).Owner), Has.All.EqualTo(color));
                Assert.That(document.Castles().GetOwners().Count(owner => owner == color), Is.EqualTo(ids.Length));
                Assert.That(chunk.GetBytesAt(PlayerLayout.TownIds + ids.Length, Constants.Player.MaxTownCount - ids.Length), Has.All.EqualTo(Constants.ByteOff));
            }
        }

        [TestCase("town_ownership_00_baseline", "town_ownership_01_blue_captures_red_inferno", "Red_Inferno", PlayerColor.Blue)]
        [TestCase("town_ownership_01_blue_captures_red_inferno", "town_ownership_02_blue_captures_red_tower", "Red_Tower", PlayerColor.Blue)]
        [TestCase("town_ownership_03_red_townless_day_2", "town_ownership_04_red_recaptures_red_inferno", "Red_Inferno", PlayerColor.Red)]
        [TestCase("town_ownership_06_red_idle_day_3", "town_ownership_07_red_captures_neutral_bulwar", "Neutral_Bulwar", PlayerColor.Red)]
        [TestCase("town_ownership_08_red_selects_red_inferno", "town_ownership_09_blue_captures_red_inferno__selects_red_tower", "Red_Inferno", PlayerColor.Blue)]
        [TestCase("town_ownership_10_before_red_recaptures_red_tower", "town_ownership_11_red_recaptures_red_tower", "Red_Tower", PlayerColor.Red)]
        public void EditingOwnershipProducesTheSameTownListsAsAnInGameCapture(string before, string after, string town, PlayerColor owner)
        {
            var history = Load(before).History;
            var original = history.Current;
            var expected = Load(after).History.Current;
            var index = original.Castles().FindByName(town);
            history.CreateCastleSession(index).Update(c => c with { Owner = owner });

            var reloaded = TestServices.GameService.LoadGameFromBytes(history.Current.ToBytes(), "edited.GM2").History.Current;
            foreach (var color in new[] { PlayerColor.Red, PlayerColor.Blue })
            {
                var edited = Player(history.Current, color);
                Assert.That(edited.GetByteAt(PlayerLayout.TownCount), Is.EqualTo(Player(expected, color).GetByteAt(PlayerLayout.TownCount)));
                Assert.That(edited.GetBytesAt(PlayerLayout.TownIds, Constants.Player.MaxTownCount), Is.EqualTo(Player(expected, color).GetBytesAt(PlayerLayout.TownIds, Constants.Player.MaxTownCount)));
                Assert.That(Towns(Player(reloaded, color)), Is.EqualTo(Towns(edited)));
                Assert.That(edited.GetByteAt(PlayerLayout.SelectedTown), Is.EqualTo(Player(original, color).GetByteAt(PlayerLayout.SelectedTown)));
            }
        }

        [Test]
        public void OwnershipAndBothPlayerRecordsAreOneUndoableEditAndOnePendingChange()
        {
            var history = Load("town_ownership_00_baseline").History;
            var original = history.Current.ToBytes();
            var notifications = 0;
            history.Changed += (_, _) => notifications++;
            history.CreateCastleSession(0).Update(c => c with { Owner = PlayerColor.Blue });
            var edited = history.Current.ToBytes();

            Assert.That(notifications, Is.EqualTo(1));
            Assert.That(history.GetPendingChanges().Single().Change.Description, Is.EqualTo("Owner"));
            history.Undo();
            Assert.That(history.Current.ToBytes(), Is.EqualTo(original));
            Assert.That(history.CanUndo, Is.False);
            history.Redo();
            Assert.That(history.Current.ToBytes(), Is.EqualTo(edited));
            history.GetPendingChanges().Single().Revert(history);
            Assert.That(history.Current.ToBytes(), Is.EqualTo(original));
            Assert.That(history.GetPendingChanges(), Is.Empty);
        }

        [TestCase(0)]
        [TestCase(1)]
        public void RevertingEitherOriginalTownRestoresItsOriginalPlaceWithoutRevertingOtherEdits(int town)
        {
            var history = Load("town_ownership_00_baseline").History;
            history.CreatePlayerSession(0).Update(p => p with { Resources = p.Resources.With(Resource.Gold, 123) });
            var before = history.Current.ToBytes();
            history.CreateCastleSession(town).Update(c => c with { Owner = PlayerColor.Blue });
            history.GetPendingChanges().Single(c => c.Kind == Homm3ChunkKinds.Castle).Revert(history);
            Assert.That(history.Current.ToBytes(), Is.EqualTo(before));
        }

        [Test]
        public void RecapturingATownDuringEditingAppendsItEvenForItsOriginalOwner()
        {
            var history = Load("town_ownership_00_baseline").History;
            var session = history.CreateCastleSession(0);
            session.Update(c => c with { Owner = PlayerColor.Blue });
            session.Update(c => c with { Owner = PlayerColor.Red });
            Assert.That(Towns(Player(history.Current, PlayerColor.Red)), Is.EqualTo(new byte[] { 1, 0 }));
            var pending = history.GetPendingChanges().Single();
            Assert.That(pending.Change.Description, Is.EqualTo("Town order"));
            pending.Revert(history);
            Assert.That(Towns(Player(history.Current, PlayerColor.Red)), Is.EqualTo(new byte[] { 0, 1 }));
            Assert.That(history.Current.ToBytes(), Is.EqualTo(history.Original.ToBytes()));
        }

        [Test]
        public void ATownCanBecomeNeutralAndRevertingRestoresTheEntireSave()
        {
            var history = Load("town_ownership_00_baseline").History;
            var before = history.Current.ToBytes();
            history.CreateCastleSession(0).Update(c => c with { Owner = null });
            Assert.That(Towns(Player(history.Current, PlayerColor.Red)), Is.EqualTo(new byte[] { 1 }));
            Assert.That(Towns(Player(history.Current, PlayerColor.Blue)), Is.EqualTo(new byte[] { 2 }));
            history.GetPendingChanges().Single().Revert(history);
            Assert.That(history.Current.ToBytes(), Is.EqualTo(before));
        }

        [Test]
        public void GivingATownToAHiddenColourUpdatesItsRetainedPlayerRecord()
        {
            var history = Load("town_ownership_00_baseline").History;
            var before = history.Current.ToBytes();
            Assert.That(history.Current.Players().FindByColor(PlayerColor.Green), Is.EqualTo(-1));
            history.CreateCastleSession(0).Update(c => c with { Owner = PlayerColor.Green });
            Assert.That(Towns(Player(history.Current, PlayerColor.Green)), Is.EqualTo(new byte[] { 0 }));
            var reloaded = TestServices.GameService.LoadGameFromBytes(history.Current.ToBytes(), "edited.GM2").History.Current;
            Assert.That(Towns(Player(reloaded, PlayerColor.Green)), Is.EqualTo(new byte[] { 0 }));
            history.GetPendingChanges().Single().Revert(history);
            Assert.That(history.Current.ToBytes(), Is.EqualTo(before));
        }

        [Test]
        public void RawOwnershipEditsAlsoUpdateTheTownLists()
        {
            var history = Load("town_ownership_00_baseline").History;
            history.CreateCastleSession(0).UpdateBytes(bytes =>
            {
                bytes[CastleLayout.Owner] = (byte)PlayerColor.Blue;
                return bytes;
            });
            Assert.That(Towns(Player(history.Current, PlayerColor.Red)), Is.EqualTo(new byte[] { 1 }));
            Assert.That(Towns(Player(history.Current, PlayerColor.Blue)), Is.EqualTo(new byte[] { 2, 0 }));
        }

        [Test]
        public void AnOwnershipEditChangesOnlyTheOwnerCountAndUsedTownListSlots()
        {
            var history = Load("town_ownership_08_red_selects_red_inferno").History;
            var original = history.Current;
            var before = original.ToBytes();
            history.CreateCastleSession(1).Update(c => c with { Owner = PlayerColor.Blue });
            var after = history.Current.ToBytes();
            var red = Player(original, PlayerColor.Red).StartIndex;
            var blue = Player(original, PlayerColor.Blue).StartIndex;
            var allowed = new[]
            {
                original.Castles()[1].StartIndex + CastleLayout.Owner,
                red + PlayerLayout.TownCount, red + PlayerLayout.TownIds, red + PlayerLayout.TownIds + 1,
                blue + PlayerLayout.TownCount, blue + PlayerLayout.TownIds + 2,
            };
            Assert.That(Enumerable.Range(0, before.Length).Where(i => before[i] != after[i]), Is.EquivalentTo(allowed));
        }

        [Test]
        public void AnInconsistentTownListRefusesTheWholeOwnershipEdit()
        {
            var history = Load("town_ownership_00_baseline").History;
            history.CreatePlayerSession(0).UpdateBytes(bytes =>
            {
                bytes[PlayerLayout.TownCount] = 0;
                return bytes;
            });
            var before = history.Current;
            Assert.Throws<GameRuleException>(() => history.CreateCastleSession(0).Update(c => c with { Owner = PlayerColor.Blue }));
            Assert.That(history.Current, Is.SameAs(before));
        }

        [Test]
        public void AFullReceivingListRefusesTheWholeOwnershipEdit()
        {
            var history = Load("town_ownership_00_baseline").History;
            history.CreatePlayerSession(1).UpdateBytes(bytes =>
            {
                bytes[PlayerLayout.TownCount] = Constants.Player.MaxTownCount;
                for (var i = 0; i < Constants.Player.MaxTownCount; i++)
                {
                    bytes[PlayerLayout.TownIds + i] = (byte)(i + 1);
                }

                return bytes;
            });
            var before = history.Current;
            Assert.Throws<GameRuleException>(() => history.CreateCastleSession(0).Update(c => c with { Owner = PlayerColor.Blue }));
            Assert.That(history.Current, Is.SameAs(before));
        }

        [Test]
        public void RevertingSeveralOwnerChangesInEitherOrderRestoresTheWholeSave()
        {
            foreach (var reverse in new[] { false, true })
            {
                var history = Load("town_ownership_00_baseline").History;
                var before = history.Current.ToBytes();
                history.CreateCastleSession(0).Update(c => c with { Owner = PlayerColor.Blue });
                history.CreateCastleSession(1).Update(c => c with { Owner = PlayerColor.Blue });
                var changes = history.GetPendingChanges();
                foreach (var change in reverse ? changes.Reverse() : changes)
                {
                    change.Revert(history);
                }

                Assert.That(history.Current.ToBytes(), Is.EqualTo(before));
            }
        }

        [Test]
        public void ShadowOfDeathTownOwnershipUsesTheSameCountAndListLayout()
        {
            var history = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Roe", "roe_00_baseline.GM1")).History;
            var before = history.Current.ToBytes();
            var castleIndex = history.Current.Castles().FindByName("1_Castle");
            var oldIds = Towns(Player(history.Current, PlayerColor.Red));
            history.CreateCastleSession(castleIndex).Update(c => c with { Owner = PlayerColor.Blue });
            Assert.That(Towns(Player(history.Current, PlayerColor.Red)), Is.EqualTo(oldIds.Where(id => id != castleIndex)));
            Assert.That(Towns(Player(history.Current, PlayerColor.Blue)), Does.Contain((byte)castleIndex));
            history.GetPendingChanges().Single().Revert(history);
            Assert.That(history.Current.ToBytes(), Is.EqualTo(before));
        }
    }
}
