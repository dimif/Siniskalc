namespace Siniskalc.Homm2.Runtime
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm2.Data;

    /// <summary>
    /// Builds a game from the bytes of a savegame of Heroes of Might and Magic II, the base game's or the Price of Loyalty's.
    /// </summary>
    public sealed class Homm2SavegameParser : IHomm2SavegameParser
    {
        private readonly IHomm2SaveLayoutLocator locator;
        private readonly IHomm2BytesLoader loader;
        private readonly IHomm2HeaderReader headerReader;
        private readonly IHomm2SaveDocumentFactory documentFactory;

        public Homm2SavegameParser(IHomm2SaveLayoutLocator locator, IHomm2BytesLoader loader, IHomm2HeaderReader headerReader, IHomm2SaveDocumentFactory documentFactory)
        {
            this.locator = locator;
            this.loader = loader;
            this.headerReader = headerReader;
            this.documentFactory = documentFactory;
        }

        /// <inheritdoc/>
        public Homm2Game Parse(byte[] bytes, string? file, string? name = null)
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
                this.loader.LoadCastleChunks(bytes, layout));
            return new Homm2Game(file, bytes, layout, new SaveDocumentHistory(document), this.headerReader.Read(bytes, layout), name);
        }
    }
}
