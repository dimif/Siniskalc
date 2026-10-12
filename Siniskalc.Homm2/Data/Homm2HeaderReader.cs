namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// Reads the header of a savegame. Nothing in it is written back: the date and the map are only shown.
    /// </summary>
    public sealed class Homm2HeaderReader : IHomm2HeaderReader
    {
        /// <inheritdoc/>
        public Homm2SaveMetadata Read(byte[] bytes, Homm2SaveLayout layout)
        {
            var date = new Homm2GameDate(
                bytes[layout.DateStart],
                bytes[layout.DateStart + Homm2HeaderLayout.DateStep],
                bytes[layout.DateStart + (2 * Homm2HeaderLayout.DateStep)]);
            var map = new Homm2MapHeader(
                Homm2Bytes.ReadText(bytes, layout.MapNameStart, Homm2HeaderLayout.MapNameLength),
                Homm2Bytes.ReadText(bytes, layout.MapDescriptionStart, Homm2HeaderLayout.MapDescriptionMaxLength),
                layout.Width,
                layout.Height);
            return new Homm2SaveMetadata(date, map);
        }
    }
}
