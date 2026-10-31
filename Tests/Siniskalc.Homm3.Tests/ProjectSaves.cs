using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests
{
    /// <summary>
    /// Runs once for the whole test project: frees the savegames the tests loaded when the last test is done. Nothing is loaded in advance,
    /// so a run of one test parses only the files it asks for.
    /// </summary>
    [SetUpFixture]
    internal sealed class ProjectSaves
    {
        [OneTimeTearDown]
        public void FreeTheSavegames() => SavedGames.Clear();
    }
}
