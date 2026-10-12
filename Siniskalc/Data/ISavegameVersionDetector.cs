namespace Siniskalc.Data
{
    using Siniskalc.Entities;

    /// <summary>
    /// Tells which game version wrote a savefile from its header.
    /// </summary>
    public interface ISavegameVersionDetector
    {
        GameVersion Detect(byte[] bytes);
    }
}
