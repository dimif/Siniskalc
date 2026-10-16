namespace Siniskalc.Homm3.Runtime
{
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Loads, reloads and saves <see cref="Game"/> instances: the file I/O. Building a game from the bytes of a savefile is the job of
    /// <see cref="ISavegameParser"/>.
    /// </summary>
    public sealed class GameService : SavegameService<Game>, IGameService
    {
        public GameService(ISavegameParser parser, ISavefileCodec codec, IFileBackupServiceProvider backups)
            : base(parser, codec, backups)
        {
        }
    }
}
