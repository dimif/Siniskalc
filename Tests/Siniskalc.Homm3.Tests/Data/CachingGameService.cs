using Siniskalc.Homm3.Runtime;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// The game service of the editors in the tests: it serves the savefiles of <c>TestData</c> from <see cref="SavedGames"/>.
    /// </summary>
    internal sealed class CachingGameService(IGameService inner, SaveCache<Game> cache) : CachingSavegameService<Game>(inner, cache), IGameService
    {
    }
}
