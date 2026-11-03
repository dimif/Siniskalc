namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Tells which game version wrote a savefile from its header.
    /// </summary>
    public interface ISavegameVersionDetector
    {
        GameVersion Detect(byte[] bytes);
    }
}
