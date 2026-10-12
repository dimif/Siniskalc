namespace Siniskalc.Homm2.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Finds the savegames of Heroes of Might and Magic II in TestData (see TestData/Homm2/README.md).
    /// </summary>
    internal static class Homm2Saves
    {
        public const string BrokenAlliance = "BrokenAlliance";
        public const string ArraxTheJerk = "ArraxTheJerk";

        /// <summary>
        /// Returns the path of a save of a map, from its name without the extension (<c>broken_alliance_10_xabran_mage_guild_2__maximus_spell_book</c>).
        /// </summary>
        public static string Path(string map, string name) => System.IO.Path.Combine("TestData", "Homm2", map, name + (map == ArraxTheJerk ? ".GX1" : ".GM1"));

        /// <summary>
        /// Returns the path of the save of Broken Alliance whose number is <paramref name="number"/>.
        /// </summary>
        public static string BrokenAllianceSave(int number) =>
            Directory.GetFiles(System.IO.Path.Combine("TestData", "Homm2", BrokenAlliance), $"broken_alliance_{number:00}_*.GM1").Single();

        public static IEnumerable<string> All() => Directory.GetFiles(System.IO.Path.Combine("TestData", "Homm2"), "*.G??", SearchOption.AllDirectories).OrderBy(f => f, System.StringComparer.Ordinal);

        public static IEnumerable<string> AllOfBrokenAlliance() => All().Where(f => f.Contains(BrokenAlliance));

        public static byte[] Read(string path) => File.ReadAllBytes(path);
    }
}
