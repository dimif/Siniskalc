namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Finds whose turn it is where <see cref="IPlayerControlLocator"/> finds the setup of the players: the human flags and the colour after
    /// them make one chunk (see <see cref="TurnLayout"/>). A version without a setup, or a setup whose human flags are not found, has none.
    /// </summary>
    public sealed class TurnBytesLoader : ITurnBytesLoader
    {
        private const string ChunkName = "Current turn";

        private readonly IPlayerControlLocator setupLocator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TurnBytesLoader"/> class.
        /// </summary>
        /// <param name="setupLocator">The setup locator.</param>
        public TurnBytesLoader(IPlayerControlLocator setupLocator)
        {
            this.setupLocator = setupLocator;
        }

        /// <inheritdoc/>
        public IReadOnlyList<RecordChunk> LoadTurnChunks(byte[] bytes, GameVersion gameVersion)
        {
            var location = this.setupLocator.Locate(bytes, gameVersion);
            if (location is not { HasHumanFlags: true } || location.HumanFlagsAt + TurnLayout.Length > bytes.Length)
            {
                return [];
            }

            return [new RecordChunk(ChunkName, bytes.AsSpan(location.HumanFlagsAt, TurnLayout.Length).ToArray(), location.HumanFlagsAt, 0)];
        }
    }
}
