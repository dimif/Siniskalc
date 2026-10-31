namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// Tests how the lists of heroes and castles of the players follow the owners after an edit, on the first save of Broken Alliance (Blue
    /// has Maximus, hero 7, and Xabran, castle 3).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class PlayerListsReconcilerTests
    {
        private const int Maximus = 7;
        private const int Xabran = 3;

        private readonly PlayerListsReconciler reconciler = new();
        private Game game = null!;

        [SetUp]
        public void LoadTheStart() => this.game = TestServices.LoadBrokenAlliance(1);

        [Test]
        public void AnEditThatChangesNoOwnerIsReturnedAsItIs()
        {
            var history = this.game.History;
            var edited = history.Current.WithChunk(Homm2ChunkKinds.Player, 0, history.Current.Players().Serializer.Serialize(history.Current.Players()[0], history.Current.Players().GetModel(0) with { Resources = ResourceSet.None }));

            Assert.That(this.reconciler.Reconcile(history.Original, history.Current, edited), Is.SameAs(edited));
        }

        [Test]
        public void AHeroGivenToAPlayerWhoseListIsFullIsRefusedAndTheSavegameIsLeftAsItWas()
        {
            var history = this.game.History;
            var players = history.Current.Players();
            var green = PlayerColor.Green.Code;
            var full = players.GetLists(green) with { HeroIds = new Numbers(Enumerable.Range(20, Constants.Player.HeroListSize)) };
            history.ApplyAsIs(history.Current.WithChunk(Homm2ChunkKinds.Player, green, players.ListsSerializer.Serialize(players[green], full)));
            var before = history.Current;

            Assert.That(() => history.CreateHeroSession(Maximus).Update(h => h.WithOwner(PlayerColor.Green)), Throws.TypeOf<GameRuleException>().With.Message.EqualTo("Green has the most heroes a player can have (5)."));

            Assert.That(history.Current, Is.SameAs(before));
        }

        [Test]
        public void AnEditOfTheRawBytesOfAnOwnerLeavesTheListsAlone()
        {
            var history = this.game.History;
            var lists = history.Current.Players().GetLists(PlayerColor.Blue.Code);
            var owner = history.Current.Heroes().GetModel(Maximus);
            var session = history.CreateHeroSession(Maximus);
            var edited = history.Current.Heroes().Serializer.Serialize(session.Chunk, owner.WithOwner(PlayerColor.Green)).ToArray();

            session.UpdateBytes(_ => edited);

            Assert.That(history.Current.Heroes().GetModel(Maximus).Owner, Is.EqualTo(PlayerColor.Green));
            Assert.That(history.Current.Players().GetLists(PlayerColor.Blue.Code), Is.EqualTo(lists));
        }

        [Test]
        public void TheLastCastleOfAPlayerGoingToAnotherStartsTheCountdownOfTheOneAndStopsThatOfTheOther()
        {
            var history = this.game.History;

            history.CreateCastleSession(Xabran).Update(c => c with { Owner = PlayerColor.Green });

            var players = history.Current.Players();
            Assert.That(players.GetModel(PlayerColor.Blue.Code).DaysLeft, Is.EqualTo(players.GetLists(PlayerColor.Blue.Code).CastleIds.Count == 0 ? Siniskalc.Homm2.Models.PlayerModel.DaysToOwnACastle : null));
            Assert.That(players.GetLists(PlayerColor.Green.Code).CastleIds.Last(), Is.EqualTo(Xabran));
            Assert.That(players.GetModel(PlayerColor.Green.Code).DaysLeft, Is.Null);
        }
    }
}
