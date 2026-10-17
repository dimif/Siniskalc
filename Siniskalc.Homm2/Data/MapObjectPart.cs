namespace Siniskalc.Homm2.Data
{
    /// <summary>
    /// Bytes of an object of the map that sit apart from its record, with where they are in the savegame.
    /// </summary>
    /// <param name="StartIndex">Where the bytes begin in the savegame.</param>
    /// <param name="Bytes">The bytes.</param>
    public sealed record MapObjectPart(int StartIndex, byte[] Bytes);
}
