namespace Siniskalc.Tests.Data
{
    using Siniskalc.Data;
    using Siniskalc.Entities;

    [TestFixture]
    internal sealed class PlayersInTheGameTests
    {
        private static readonly string Save = Path.Combine("TestData", "Sod", "conflux_00_baseline.GM1");

        [Test]
        public void WithoutASetupEveryPlayerStays()
        {
            var players = Load();

            Assert.That(PlayersInTheGame.Of(players, null), Is.EqualTo(players));
        }

        [Test]
        public void AColourTheSetupSaysIsNotPlayedAndThatHasNoHeroesAndNoTownsIsDropped()
        {
            var players = Load();
            var empty = WithHeroesAndTowns(players[1], 0, 0);

            var kept = PlayersInTheGame.Of([players[0], empty], Controls(empty.Color, PlayerType.NotInGame));

            Assert.That(kept, Is.EqualTo(new[] { players[0] }));
        }

        [Test]
        public void AColourThatIsPlayedStaysEvenWithoutHeroesAndTowns()
        {
            var players = Load();
            var empty = WithHeroesAndTowns(players[1], 0, 0);

            Assert.That(PlayersInTheGame.Of([players[0], empty], Controls(empty.Color, PlayerType.Human)), Has.Count.EqualTo(2));
        }

        [Test]
        public void AColourTheSetupSaysIsNotPlayedStaysWhenItHasAHeroOrATown()
        {
            var players = Load();
            var withHero = WithHeroesAndTowns(players[1], 1, 0);
            var withTown = WithHeroesAndTowns(players[1], 0, 1);

            Assert.That(PlayersInTheGame.Of([players[0], withHero], Controls(withHero.Color, PlayerType.NotInGame)), Has.Count.EqualTo(2));
            Assert.That(PlayersInTheGame.Of([players[0], withTown], Controls(withTown.Color, PlayerType.NotInGame)), Has.Count.EqualTo(2));
        }

        [Test]
        public void IfTheSetupWouldLeaveNoPlayerAllStay()
        {
            var players = Load();
            var empty = WithHeroesAndTowns(players[1], 0, 0);
            var controls = Controls(empty.Color, PlayerType.NotInGame);

            Assert.That(PlayersInTheGame.Of([empty], controls), Is.EqualTo(new[] { empty }));
        }

        private static IReadOnlyList<PlayerType> Controls(PlayerColor color, PlayerType type)
        {
            var controls = Enumerable.Repeat(PlayerType.Human, 8).ToList();
            controls[(int)color] = type;
            return controls;
        }

        private static PlayerChunk WithHeroesAndTowns(PlayerChunk player, byte heroes, byte towns)
        {
            var bytes = player.ToArray();
            bytes[PlayerLayout.HeroCount] = heroes;
            bytes[PlayerLayout.TownCount] = towns;
            return player.WithBytes(bytes);
        }

        private static IReadOnlyList<PlayerChunk> Load()
        {
            var bytes = TestServices.SavefileCodec.Read(Save);
            return new PlayerBytesLoader().LoadPlayerChunks(bytes, GameVersion.ShadowOfDeath);
        }
    }
}
