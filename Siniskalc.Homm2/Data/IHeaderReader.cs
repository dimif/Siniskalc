namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Reads the date and the description of the map from the header of a savegame.
    /// </summary>
    public interface IHeaderReader
    {
        /// <summary>
        /// Builds the metadata from four parts of the header, each found through <paramref name="layout"/>:
        /// <list type="bullet">
        /// <item>the date: the day, the week and the month are single bytes <see cref="HeaderLayout.DateStep"/> bytes apart, starting with the day at
        /// <see cref="SaveLayout.DateStart"/>, and they are handed to <see cref="GameDate"/> as month, week, day;</item>
        /// <item>the map: its name and description are text fields of a fixed size, its difficulty is a 16-bit code and its width comes from the layout;</item>
        /// <item>the difficulty the game is played at, a single byte;</item>
        /// <item>who controls each colour (see <see cref="ReadControls"/>).</item>
        /// </list>
        /// A difficulty code that is no level reads as the first one instead of failing, so a savegame the editor does not understand still loads.
        /// Nothing is written back.
        /// </remarks>
        /// <param name="bytes">
        /// The bytes of the savegame, which must be at least <see cref="SaveLayout.HeaderLength"/> long.
        /// </param>
        /// <param name="layout">
        /// The layout of the save file.
        /// </param>
        SaveMetadata<PlayerColor> Read(byte[] bytes, SaveLayout layout);
    }
}
