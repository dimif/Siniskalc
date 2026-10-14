namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// Reads the header of a savegame. Nothing in it is written back: the date and the map are only shown.
    /// </summary>
    public sealed class HeaderReader : IHeaderReader
    {
        /// <inheritdoc/>
        public SaveMetadata Read(byte[] bytes, SaveLayout layout)
        {
            var date = new GameDate(
                bytes[layout.DateStart + (2 * HeaderLayout.DateStep)],
                bytes[layout.DateStart + HeaderLayout.DateStep],
                bytes[layout.DateStart]);
            var map = new MapHeader(
                Bytes.ReadText(bytes, layout.MapNameStart, HeaderLayout.MapNameLength),
                Bytes.ReadText(bytes, layout.MapDescriptionStart, HeaderLayout.MapDescriptionMaxLength),
                layout.Width,
                layout.Height);
            return new SaveMetadata(date, map);
        }
    }
}
