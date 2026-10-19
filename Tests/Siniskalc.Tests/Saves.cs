namespace Siniskalc.Tests
{
    /// <summary>
    /// The savegames the tests of the application open, one of each game (see TestData/README.md).
    /// </summary>
    internal static class Saves
    {
        public static string Homm2() => Directory.GetFiles(Path.Combine("TestData", "Homm2", "BrokenAlliance"), "broken_alliance_10_*.GM1").Single();

        public static string Homm3() => Path.Combine("TestData", "Sod", "conflux_00_baseline.GM1");
    }
}
