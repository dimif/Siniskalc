namespace Siniskalc.Homm2.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Finds the savegames of Heroes of Might and Magic II in TestData (see TestData/Homm2/README.md).
    /// </summary>
    internal static class Saves
    {
        public const string BrokenAlliance = "BrokenAlliance";
        public const string ArraxTheJerk = "ArraxTheJerk";
        public const string MapDifficultySize = "MapDifficultySize";
        public const string Threedom = "Threedom";
        public const string MarkerFacing = "MarkerFacing";

        /// <summary>
        /// Returns the path of a save of a map, from its name without the extension (<c>broken_alliance_10_xabran_mage_guild_2__maximus_spell_book</c>).
        /// </summary>
        public static string Path(string map, string name) => System.IO.Path.Combine("TestData", "Homm2", map, name + (map is ArraxTheJerk or MapDifficultySize or Threedom or MarkerFacing ? ".GX1" : ".GM1"));

        /// <summary>
        /// Returns the path of the save of Broken Alliance whose number is <paramref name="number"/>.
        /// </summary>
        public static string BrokenAllianceSave(int number) =>
            Directory.GetFiles(System.IO.Path.Combine("TestData", "Homm2", BrokenAlliance), $"broken_alliance_{number:00}_*.GM1").Single();

        /// <summary>
        /// Returns the path of the save of Threedom whose number is <paramref name="number"/>.
        /// </summary>
        public static string ThreedomSave(int number) =>
            Directory.GetFiles(System.IO.Path.Combine("TestData", "Homm2", Threedom), $"threedom_{number:00}_*.GX1").Single();

        /// <summary>
        /// Returns the path of the save of MarkerFacing whose number is <paramref name="number"/> (they continue the numbers of Threedom).
        /// </summary>
        public static string MarkerFacingSave(int number) =>
            Directory.GetFiles(System.IO.Path.Combine("TestData", "Homm2", MarkerFacing), $"marker_facing_{number:00}_*.GX1").Single();

        /// <summary>
        /// Returns the path of a save of the folder <paramref name="folder"/> of TestData/Homm2, from its name without the extension (any of the game's own).
        /// </summary>
        public static string Named(string folder, string name) =>
            Directory.GetFiles(System.IO.Path.Combine("TestData", "Homm2", folder), name + ".G??").Single();

        public static IEnumerable<string> All() => Directory.GetFiles(System.IO.Path.Combine("TestData", "Homm2"), "*.G??", SearchOption.AllDirectories).Where(f => !f.Replace(System.IO.Path.DirectorySeparatorChar, '/').Contains("/Homm2/out_tested/", System.StringComparison.OrdinalIgnoreCase)).OrderBy(f => f, System.StringComparer.Ordinal);

        public static IEnumerable<string> AllOfBrokenAlliance() => All().Where(f => f.Contains(BrokenAlliance));

        public static byte[] Read(string path) => File.ReadAllBytes(path);
    }
}
