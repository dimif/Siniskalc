namespace Siniskalc.Entities
{
    public static class GameVersionExtensions
    {
        /// <summary>
        /// Returns whether the editor knows where a savefile of this version keeps the date, the setup of the players and the map header.
        /// Not the case for a version it does not know, and not for Heroes Chronicles, of which no savefile has been seen.
        /// </summary>
        public static bool HasGameSetup(this GameVersion version) => version is not (GameVersion.Unknown or GameVersion.HeroesChronicles);
    }
}
