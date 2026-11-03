using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// The pending changes factory of the tests of the differs of HoMM3: the shared fake with the models and the kinds of chunk of the game.
    /// </summary>
    internal sealed class PendingChangesFake()
        : PendingChangesFake<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>>(Homm3ChunkKinds.Hero, Homm3ChunkKinds.Player, Homm3ChunkKinds.Castle)
    {
    }
}
