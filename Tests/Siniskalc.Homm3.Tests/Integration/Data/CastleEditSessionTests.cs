using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Homm3.Runtime;

    /// <summary>
    /// Giving a castle to another owner through its <see cref="CastleEditSession"/>: the castle and the countdowns of the two players change
    /// together, as the saves in TestData/Hota/Banishing show (Blue loses his only town, 2_Rampart, gets it back, and loses it again).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class CastleEditSessionTests
    {
        private const string Rampart = "2_Rampart";

        private static Game Load(string save) =>
            SavedGames.Get(Directory.GetFiles(Path.Combine("TestData", "Hota", "Banishing"), save + ".GM?").Single());

        private static int RampartIndex(Game game) => game.History.Current.Castles().FindByName(Rampart);

        private static PlayerColor RampartOwner(Game game) => game.History.Current.Castles().GetModel(RampartIndex(game)).Owner;

        private static PlayerModel? Player(Game game, PlayerColor color)
        {
            var players = game.History.Current.Players();
            var index = players.FindByColor(color);
            return index < 0 ? null : players.GetModel(index);
        }

        [TestCase("banishing_00_baseline", "banishing_01_blue_7_days_left")]
        [TestCase("banishing_01_blue_7_days_left", "banishing_02_m1w1d1")]
        [TestCase("banishing_04_m1w1d2", "banishing_05_m1w1d3")]
        public void GivingTheRampartToItsNextOwnerGivesBothPlayersTheDaysLeftOfTheNextSave(string save, string next)
        {
            var game = Load(save);
            var later = Load(next);
            var (from, to) = (RampartOwner(game), RampartOwner(later));
            Assert.That(from, Is.Not.EqualTo(to), "the Rampart changes hands between the two saves");
            Assert.That(new[] { from, to }, Does.Contain(PlayerColor.Blue));

            game.History.CreateCastleSession(RampartIndex(game)).ChangeOwner(to);

            Assert.That(RampartOwner(game), Is.EqualTo(to));
            foreach (var color in new[] { from, to }.Where(c => !c.IsNeutral))
            {
                Assert.That(Player(game, color)?.DaysLeft, Is.EqualTo(Player(later, color)?.DaysLeft), color.ToString());
            }
        }

        [Test]
        public void ChangingTheOwnerIsOneStepOfTheHistoryThatUndoPutsBackWhole()
        {
            var game = Load("banishing_00_baseline");
            var original = game.History.Current;

            game.History.CreateCastleSession(RampartIndex(game)).ChangeOwner(PlayerColor.Neutral);

            Assert.That(Player(game, PlayerColor.Blue)!.DaysLeft, Is.EqualTo(7), "Blue lost his only town");
            game.History.Undo();
            Assert.That(game.History.Current, Is.SameAs(original));
            Assert.That(game.History.CanUndo, Is.False);
        }

        [Test]
        public void GivingTheCastleToItsOwnerChangesNothing()
        {
            var game = Load("banishing_00_baseline");

            game.History.CreateCastleSession(RampartIndex(game)).ChangeOwner(PlayerColor.Blue);

            Assert.That(game.History.CanUndo, Is.False);
        }

        [Test]
        public void ABanishedPlayerKeepsItsCountdownWhenItIsGivenATown()
        {
            var game = Load("banishing_13_blue_banished");
            var owners = game.History.Current.Castles().GetOwners();
            var castle = Enumerable.Range(0, owners.Count).First(i => owners[i] != PlayerColor.Blue);

            game.History.CreateCastleSession(castle).ChangeOwner(PlayerColor.Blue);

            Assert.That(Player(game, PlayerColor.Blue)!.DaysLeft, Is.Zero);
        }

        [Test]
        public void ThePendingChangesListTheOwnerOfTheCastleAndTheDaysLeftOfThePlayer()
        {
            var game = Load("banishing_00_baseline");

            game.History.CreateCastleSession(RampartIndex(game)).ChangeOwner(PlayerColor.Neutral);

            Assert.That(game.History.GetPendingChanges().Select(c => c.Change.Description), Is.EquivalentTo(new[] { "Owner", "Days left without a castle" }));
        }
    }
}
