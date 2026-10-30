using Siniskalc.Heroes.Models;
using Siniskalc.Homm2.Data;
using Siniskalc.Homm2.Entities;
using Siniskalc.Homm2.Models;

namespace Siniskalc.Homm2.Tests.Data
{
    /// <summary>
    /// The pending changes factory of the tests of the differs of HoMM2: the shared fake with the models and the kinds of chunk of the game.
    /// </summary>
    internal sealed class PendingChangesFake()
        : PendingChangesFake<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>>(Homm2ChunkKinds.Hero, Homm2ChunkKinds.Player, Homm2ChunkKinds.Castle)
    {
    }
}
