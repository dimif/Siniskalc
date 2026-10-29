namespace Siniskalc.Homm2.Tests.Data
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// A pending changes factory for the tests of the differs: it makes each change with no way to put it back through a history, and keeps
    /// the function that puts the record back, by the description of the change.
    /// </summary>
    internal sealed class PendingChangesFake
    {
        public PendingChangesFake()
        {
            var mock = new Mock<IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>>>(MockBehavior.Strict);
            mock.Setup(f => f.ForHero(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Change>(), It.IsAny<Func<HeroModel, HeroModel>>()))
                .Returns((string name, int index, Change change, Func<HeroModel, HeroModel> revert) =>
                {
                    this.HeroReverts[change.Description] = revert;
                    return new PendingChange(name, change, Homm2ChunkKinds.Hero, index, _ => { });
                });
            mock.Setup(f => f.ForPlayer(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Change>(), It.IsAny<Func<PlayerModel, PlayerModel>>()))
                .Returns((string name, int index, Change change, Func<PlayerModel, PlayerModel> revert) =>
                {
                    this.PlayerReverts[change.Description] = revert;
                    return new PendingChange(name, change, Homm2ChunkKinds.Player, index, _ => { });
                });
            mock.Setup(f => f.ForCastle(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Change>(), It.IsAny<Func<CastleModel, CastleModel>>()))
                .Returns((string name, int index, Change change, Func<CastleModel, CastleModel> revert) =>
                {
                    this.CastleReverts[change.Description] = revert;
                    return new PendingChange(name, change, Homm2ChunkKinds.Castle, index, _ => { });
                });
            this.Factory = mock.Object;
        }

        public IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> Factory { get; }

        public Dictionary<string, Func<HeroModel, HeroModel>> HeroReverts { get; } = [];

        public Dictionary<string, Func<PlayerModel, PlayerModel>> PlayerReverts { get; } = [];

        public Dictionary<string, Func<CastleModel, CastleModel>> CastleReverts { get; } = [];
    }
}
