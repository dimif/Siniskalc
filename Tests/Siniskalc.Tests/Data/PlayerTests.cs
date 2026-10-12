using Siniskalc;
using Siniskalc.Core.Data;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// Tests against two real HotA savegames of the same game. In empire_of_the_world_02_m5w4d6_purple_gifts_red the Purple player has sent resources to Red:
    /// 1 wood, 3 ore, 5 mercury, 7 sulfur, 11 crystal, 13 gems and 17 gold. The amounts come from the in-game resource bars.
    /// </summary>
    [TestFixture]
    internal sealed class PlayerTests
    {
        private static readonly string Save1 = Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1");
        private static readonly string Save2 = Path.Combine("TestData", "Hota", "empire_of_the_world_02_m5w4d6_purple_gifts_red.GM1");

        private static PlayerResources PurpleBefore => new(28, 153, 34, 151, 76, 91, 220976);

        private static PlayerResources PurpleAfter => new(27, 148, 31, 144, 65, 78, 220959);

        private static Game Load(string path) => TestServices.GameService.LoadGameFromFile(path);

        [Test]
        public void TheEightPlayersOfTheMapAreFoundInColourOrder()
        {
            var game = Load(Save1);

            var colors = game.History.Current.Players().Select(c => TestServices.PlayerDeserializer.DeserializeColor(c)).ToList();

            Assert.That(colors, Is.EqualTo(Enum.GetValues<PlayerColor>()));

            // A player's struct starts with its heroes, 97 bytes before its resources, so Pink, the last, ends where the castles begin.
            var chunks = game.History.Current.Players();
            Assert.That(chunks.Select(c => c.Length), Has.All.EqualTo(PlayerLayout.HornOfTheAbyss.Stride));
            Assert.That(chunks.Last().End, Is.LessThanOrEqualTo(game.History.Current.Castles().Where(c => c.StartIndex >= chunks.Last().StartIndex).Min(c => c.StartIndex)));
            Assert.That(game.History.Current.Players().Select(c => c.Name), Is.EqualTo(colors.Select(c => c.ToString())));
        }

        [Test]
        public void PurpleResourcesAreReadAsTheGameShowedThem()
        {
            var game = Load(Save1);

            Assert.That(game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(PlayerColor.Purple)).Current.Resources, Is.EqualTo(PurpleBefore));
        }

        [Test]
        public void TheSecondSaveHasThePurpleResourcesAfterTheTradesAndTheSamePlayers()
        {
            var game = Load(Save2);
            var red = game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(PlayerColor.Red)).Current.Resources;
            var original = Load(Save1);
            var redBefore = original.History.CreatePlayerSession(original.History.Current.Players().FindByColor(PlayerColor.Red)).Current.Resources;

            Assert.That(game.History.Current.Players(), Has.Count.EqualTo(original.History.Current.Players().Count));
            Assert.That(game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(PlayerColor.Purple)).Current.Resources, Is.EqualTo(PurpleAfter));
            Assert.That(red, Is.EqualTo(new PlayerResources(
                redBefore.Wood + 1, redBefore.Mercury + 5, redBefore.Ore + 3, redBefore.Sulfur + 7, redBefore.Crystal + 11, redBefore.Gems + 13, redBefore.Gold + 17)));
        }

        [Test]
        public void SavefilesOfVersionsWithoutAKnownLayoutHaveNoPlayers()
        {
            Assert.That(new PlayerBytesLoader().LoadPlayerChunks(TestServices.SavefileCodec.Read(Save1), GameVersion.ShadowOfDeath), Is.Empty);
            Assert.That(new PlayerBytesLoader().LoadPlayerChunks(TestServices.SavefileCodec.Read(Save1), GameVersion.Unknown), Is.Empty);
            Assert.That(new PlayerBytesLoader().LoadPlayerChunks(new byte[100000], GameVersion.HornOfTheAbyss), Is.Empty);
        }

        [Test]
        public void EditingResourcesChangesOnlyThoseBytesOfTheSavefile()
        {
            var game = Load(Save1);
            var session = game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(PlayerColor.Purple));

            session.Update(p => p with { Resources = p.Resources with { Gold = Constants.Resources.RichGoldPreset, Wood = Constants.Resources.AllResourcesPreset } });
            var saved = game.GetBytesToSave();

            var changed = Enumerable.Range(0, saved.Length).Where(i => saved[i] != game.Bytes[i]).ToList();
            var first = game.History.Current.Players()[game.History.Current.Players().FindByColor(PlayerColor.Purple)].StartIndex;
            Assert.That(changed, Is.Not.Empty);
            Assert.That(changed, Has.All.InRange(first, first + PlayerLayout.ResourcesOffset + (7 * PlayerLayout.ResourceSize) - 1));
            Assert.That(BitConverter.ToInt32(saved, first + PlayerLayout.OffsetOf(Resource.Gold)), Is.EqualTo(Constants.Resources.RichGoldPreset));
            Assert.That(BitConverter.ToInt32(saved, first + PlayerLayout.OffsetOf(Resource.Wood)), Is.EqualTo(99));
            Assert.That(BitConverter.ToInt32(saved, first + PlayerLayout.OffsetOf(Resource.Gems)), Is.EqualTo(PurpleBefore.Gems));
        }

        [Test]
        public void ApplyingTheTradesToTheFirstSaveGivesTheBytesOfTheSecond()
        {
            var game = Load(Save1);
            var second = Load(Save2);
            game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(PlayerColor.Purple)).Update(p => p with { Resources = PurpleAfter });
            var red = game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(PlayerColor.Red));
            red.Update(p => p with
            {
                Resources = new PlayerResources(
                    p.Resources.Wood + 1, p.Resources.Mercury + 5, p.Resources.Ore + 3, p.Resources.Sulfur + 7, p.Resources.Crystal + 11, p.Resources.Gems + 13, p.Resources.Gold + 17),
            });

            var saved = game.GetBytesToSave();
            var expected = second.GetBytesToSave();

            // The real files also differ in the save name, in the header, and in two bytes the game changed when the trade happened:
            // the hero and the town that are selected on the adventure map, the second and the 25th byte of the struct of Purple (the colour
            // after Orange).
            var purple = game.History.Current.Players()[game.History.Current.Players().FindByColor(PlayerColor.Purple)].StartIndex;
            var differences = Enumerable.Range(0, saved.Length).Where(i => saved[i] != expected[i] && (i < 859 || i > 876)).ToList();
            Assert.That(differences, Is.EqualTo(new[] { purple + PlayerLayout.SelectedHero, purple + PlayerLayout.SelectedTown }));
        }

        [Test]
        public void EditsAreUndoableAndAppearAsPendingChangesThatCanBeReverted()
        {
            var game = Load(Save1);
            var index = game.History.Current.Players().FindByColor(PlayerColor.Purple);
            var session = game.History.CreatePlayerSession(index);

            session.Update(p => p with { Resources = p.Resources with { Gold = 5, Gems = 0 } });

            var changes = game.History.GetPendingChanges();
            Assert.That(changes.Select(c => c.Path), Is.EqualTo(new[] { "Purple › Resources", "Purple › Resources" }));
            Assert.That(changes.Select(c => c.Change.Description), Is.EqualTo(new[] { "Gems", "Gold" }));
            Assert.That(changes.Single(c => c.Change.Description == "Gold").Change.Before, Is.EqualTo("220,976"));
            Assert.That(changes.Single(c => c.Change.Description == "Gold").Change.After, Is.EqualTo("5"));
            Assert.That(changes, Has.All.Property(nameof(PendingChange.Kind)).EqualTo(Homm3ChunkKinds.Player));

            changes.Single(c => c.Change.Description == "Gold").Revert(game.History);
            Assert.That(session.Current.Resources.Gold, Is.EqualTo(PurpleBefore.Gold));
            Assert.That(session.Current.Resources.Gems, Is.EqualTo(0));

            game.History.Undo();
            Assert.That(session.Current.Resources.Gold, Is.EqualTo(5));

            game.History.Apply(game.History.Original);
            Assert.That(game.History.GetPendingChanges(), Is.Empty);
        }

        [Test]
        public void AnEditThatChangesNothingIsNotRecorded()
        {
            var game = Load(Save1);

            game.History.CreatePlayerSession(0).Update(p => p);

            Assert.That(game.History.CanUndo, Is.False);
        }
    }
}
