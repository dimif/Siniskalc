namespace Siniskalc.Tests.Homm.Common
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;

    /// <summary>
    /// A pending changes factory for the tests of the differs: it makes a change of a hero, a player or a castle with no way to put it back through a history,
    /// and keeps the function that puts the record back, by the description of the change. A game derives from it with its own models and kinds of chunk.
    /// </summary>
    /// <typeparam name="THero">The model of a hero.</typeparam>
    /// <typeparam name="TPlayer">The model of a player.</typeparam>
    /// <typeparam name="TCastle">The model of a castle.</typeparam>
    /// <typeparam name="TObelisks">The model of the obelisks.</typeparam>
    public class PendingChangesFake<THero, TPlayer, TCastle, TObelisks>
        where THero : class
        where TPlayer : class
        where TCastle : class
        where TObelisks : class
    {
        public PendingChangesFake(ChunkKind heroKind, ChunkKind playerKind, ChunkKind castleKind)
        {
            var mock = new Mock<IPendingChangesFactory<THero, TPlayer, TCastle, TObelisks>>(MockBehavior.Strict);
            mock.Setup(f => f.ForHero(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Change>(), It.IsAny<Func<THero, THero>>()))
                .Returns((string name, int index, Change change, Func<THero, THero> revert) =>
                {
                    this.HeroReverts[change.Description] = revert;
                    return new PendingChange(name, change, heroKind, index, _ => { });
                });
            mock.Setup(f => f.ForPlayer(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Change>(), It.IsAny<Func<TPlayer, TPlayer>>()))
                .Returns((string name, int index, Change change, Func<TPlayer, TPlayer> revert) =>
                {
                    this.PlayerReverts[change.Description] = revert;
                    return new PendingChange(name, change, playerKind, index, _ => { });
                });
            mock.Setup(f => f.ForCastle(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Change>(), It.IsAny<Func<TCastle, TCastle>>()))
                .Returns((string name, int index, Change change, Func<TCastle, TCastle> revert) =>
                {
                    this.CastleReverts[change.Description] = revert;
                    return new PendingChange(name, change, castleKind, index, _ => { });
                });
            this.Factory = mock.Object;
        }

        public IPendingChangesFactory<THero, TPlayer, TCastle, TObelisks> Factory { get; }

        public Dictionary<string, Func<THero, THero>> HeroReverts { get; } = [];

        public Dictionary<string, Func<TPlayer, TPlayer>> PlayerReverts { get; } = [];

        public Dictionary<string, Func<TCastle, TCastle>> CastleReverts { get; } = [];
    }
}
