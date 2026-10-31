namespace Siniskalc.Homm3.Tests.Integration.Data
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Homm3.Runtime;

    /// <summary>
    /// Whose turn it is: the 32-bit colour after the human flags of the setup, read and written as the turn chunk. The Horn of the Abyss
    /// saves of <c>TestData/Hota/Mines</c> and the Shadow of Death hotseat saves of <c>TestData/Sod/Hotseat</c> have Red and Blue human.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class TurnTests
    {
        private static string Mines(string name) => Path.Combine("TestData", "Hota", "Mines", name + ".GM2");

        private static string Hotseat(string name) => Path.Combine("TestData", "Sod", "Hotseat", name + ".GM2");

        private static Game Load(string path) => TestServices.GameService.LoadGameFromFile(path);

        private static int CurrentPlayerAt(Game game) => new PlayerControlLocator().Locate(game.Bytes, game.GameVersion)!.CurrentPlayerAt;

        private static IEnumerable<TestCaseData> Turns()
        {
            yield return new TestCaseData(Mines("mines_00_game_begin"), "Red").SetName("TheTurnOfMines00IsRed");
            yield return new TestCaseData(Mines("mines_01_red_sulfur_mine_1"), "Blue").SetName("TheTurnOfMines01IsBlue");
            yield return new TestCaseData(Hotseat("hotseat_11_sir_mullich_magic_well"), "Red").SetName("TheTurnOfHotseat11IsRed");
            yield return new TestCaseData(Hotseat("hotseat_12_blue_turn"), "Blue").SetName("TheTurnOfHotseat12IsBlue");
        }

        [TestCaseSource(nameof(Turns))]
        public void TheTurnIsReadFromTheColourAfterTheHumanFlags(string path, string color)
        {
            var game = Load(path);

            Assert.That(game.History.Current.Turn(), Has.Count.EqualTo(1));
            Assert.That(game.History.Current.Turn().GetModel(0).Color.ToString(), Is.EqualTo(color));
            Assert.That(game.CurrentPlayer?.ToString(), Is.EqualTo(color));
            Assert.That(game.History.Current.Turn()[0].StartIndex + TurnLayout.CurrentPlayer, Is.EqualTo(CurrentPlayerAt(game)));
        }

        [Test]
        public void GivingTheTurnToBlueWritesOnlyTheCurrentPlayerAndTheSavegameLoadsWithBlueToPlay()
        {
            var game = Load(Hotseat("hotseat_11_sir_mullich_magic_well"));
            var at = CurrentPlayerAt(game);

            game.History.CreateTurnSession().Update(turn => turn with { Color = PlayerColor.Blue });

            Assert.That(game.CurrentPlayer, Is.EqualTo(PlayerColor.Blue));
            var written = game.GetBytesToSave();
            var changed = Enumerable.Range(0, written.Length).Where(i => written[i] != game.Bytes[i]).ToList();
            Assert.That(changed, Is.EqualTo(new[] { at }));
            Assert.That(BitConverter.ToInt32(written, at), Is.EqualTo(PlayerColor.Blue.Code));

            var reloaded = TestServices.GameService.LoadGameFromBytes(written);
            Assert.That(new PlayerControlLocator().Locate(written, reloaded.GameVersion)?.CurrentPlayerAt, Is.EqualTo(at), "the setup is still found");
            Assert.That(reloaded.CurrentPlayer, Is.EqualTo(PlayerColor.Blue));
            Assert.That(reloaded.GetControl(PlayerColor.Red), Is.EqualTo(PlayerType.Human));
        }

        [Test]
        public void GivingTheTurnIsAPendingChangeThatUndoAndRevertTakeBack()
        {
            var game = Load(Hotseat("hotseat_11_sir_mullich_magic_well"));

            game.History.CreateTurnSession().Update(turn => turn with { Color = PlayerColor.Blue });

            var change = game.History.GetPendingChanges().Single();
            Assert.That((change.OwnerName, change.Change.Area, change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Current turn", ChangeAreas.General, "Player", "Red", "Blue")));

            change.Revert(game.History);
            Assert.That((game.CurrentPlayer, game.History.GetPendingChanges().Count), Is.EqualTo((PlayerColor.Red, 0)));

            game.History.Undo();
            Assert.That(game.CurrentPlayer, Is.EqualTo(PlayerColor.Blue));
            game.History.Undo();
            Assert.That((game.CurrentPlayer, game.GetBytesToSave()), Is.EqualTo((PlayerColor.Red, game.Bytes)));
        }

        [Test]
        public void AComputerPlayerCannotBeGivenTheTurn()
        {
            var game = Load(Hotseat("hotseat_11_sir_mullich_magic_well"));
            Assert.That(game.GetControl(PlayerColor.Tan), Is.Not.EqualTo(PlayerType.Human));

            var thrown = Assert.Throws<GameRuleException>(() => game.History.CreateTurnSession().Update(turn => turn with { Color = PlayerColor.Tan }));

            Assert.That(thrown!.Message, Does.Contain("Tan"));
            Assert.That(game.History.GetPendingChanges(), Is.Empty);
        }

        [Test]
        public void TheTurnCannotBeGivenToNoColour()
        {
            var game = Load(Hotseat("hotseat_11_sir_mullich_magic_well"));

            Assert.Throws<ArgumentOutOfRangeException>(() => game.History.CreateTurnSession().Update(turn => turn with { Color = PlayerColor.Neutral }));
        }

        [Test]
        public void ACurrentPlayerThatIsNoColourReadsAsNeutralAndItsBytesAreKept()
        {
            var bytes = new byte[TurnLayout.Length];
            bytes[0] = 1;
            BitConverter.GetBytes(9).CopyTo(bytes, TurnLayout.CurrentPlayer);
            var chunk = new RecordChunk("Current turn", bytes, 100, 0);

            var model = new TurnDeserializer().Deserialize(chunk);

            Assert.That(model.Color, Is.EqualTo(PlayerColor.Neutral));
            Assert.That(new SafeTurnSerializer().Serialize(chunk, model).HasSameBytes(chunk), Is.True);
            Assert.That(new SafeTurnSerializer().Serialize(chunk, model with { Color = PlayerColor.Red }).GetInt32At(TurnLayout.CurrentPlayer), Is.EqualTo(0));
        }

        [Test]
        public void AVersionWithoutAGameSetupHasNoTurn()
        {
            var locator = new Mock<IPlayerControlLocator>();
            locator.Setup(l => l.Locate(It.IsAny<byte[]>(), It.IsAny<GameVersion>())).Returns((PlayerSetupLocation?)null);

            Assert.That(new TurnBytesLoader(locator.Object).LoadTurnChunks(new byte[64], GameVersion.HeroesChronicles), Is.Empty);
        }

        [Test]
        public void ASetupWhoseHumanFlagsAreNotFoundHasNoTurn()
        {
            var locator = new Mock<IPlayerControlLocator>();
            locator.Setup(l => l.Locate(It.IsAny<byte[]>(), It.IsAny<GameVersion>())).Returns(new PlayerSetupLocation(0, 9, 4, -1));

            Assert.That(new TurnBytesLoader(locator.Object).LoadTurnChunks(new byte[64], GameVersion.ShadowOfDeath), Is.Empty);
        }

        [Test]
        public void TheTurnChunkIsTheHumanFlagsAndTheColourAfterThem()
        {
            var bytes = Enumerable.Range(0, 64).Select(i => (byte)i).ToArray();
            var locator = new Mock<IPlayerControlLocator>();
            locator.Setup(l => l.Locate(bytes, GameVersion.ShadowOfDeath)).Returns(new PlayerSetupLocation(0, 9, 4, 10));

            var chunk = new TurnBytesLoader(locator.Object).LoadTurnChunks(bytes, GameVersion.ShadowOfDeath).Single();

            Assert.That((chunk.StartIndex, chunk.Length, chunk.Index), Is.EqualTo((10, TurnLayout.Length, 0)));
            Assert.That(chunk.ToArray(), Is.EqualTo(bytes.Skip(10).Take(TurnLayout.Length).ToArray()));
        }
    }
}
