namespace Siniskalc.Homm2.Runtime
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm2.Data;

    /// <summary>
    /// Builds a game from the bytes of a savegame of Heroes of Might and Magic II, the base game's or the Price of Loyalty's.
    /// </summary>
    public sealed class SavegameParser : ISavegameParser
    {
        private readonly ISaveLayoutLocator locator;
        private readonly IBytesLoader loader;
        private readonly IHeaderReader headerReader;
        private readonly ISaveDocumentFactory documentFactory;
        private readonly IMapPlacesReader placesReader;

        public SavegameParser(ISaveLayoutLocator locator, IBytesLoader loader, IHeaderReader headerReader, ISaveDocumentFactory documentFactory, IMapPlacesReader placesReader)
        {
            this.locator = locator;
            this.loader = loader;
            this.headerReader = headerReader;
            this.documentFactory = documentFactory;
            this.placesReader = placesReader;
        }

        /// <inheritdoc/>
        public Game Parse(byte[] bytes, string? file, string? name = null)
        {
            var layout = this.locator.Locate(bytes);
            if (layout is null)
            {
                // TODO:LOG warning: {file or name} ({bytes.Length} bytes) was refused as unsupported; log its first bytes (the header) to help recognise the version.
                throw new UnsupportedSavefileException("This is not a Heroes of Might and Magic II savegame, or it comes from a version of the game this editor does not know.");
            }

            var document = this.documentFactory.Create(
                bytes,
                this.loader.LoadHeroChunks(bytes, layout),
                this.loader.LoadPlayerChunks(bytes, layout),
                this.loader.LoadCastleChunks(bytes, layout),
                this.loader.LoadObeliskChunks(bytes, layout));
            return new Game(file, bytes, layout, new SaveDocumentHistory(document), this.headerReader.Read(bytes, layout), name) { Places = this.placesReader.Read(bytes, layout) };
        }
    }
}
