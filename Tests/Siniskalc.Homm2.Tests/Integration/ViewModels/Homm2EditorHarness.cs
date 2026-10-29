namespace Siniskalc.Homm2.Tests.Integration.ViewModels
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// An editor of Heroes of Might and Magic II with a Broken Alliance savegame loaded, which keeps its settings in a folder of its own
    /// that is deleted when the harness is disposed.
    /// </summary>
    internal sealed class Homm2EditorHarness : IDisposable
    {
        private readonly string folder;

        public Homm2EditorHarness()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-homm2-editor-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
            this.Editor = new TestApplicationServices(settingsPath: Path.Combine(this.folder, "settings.json")).CreateEditor();
            this.Editor.LoadGame(Saves.BrokenAllianceSave(24));
        }

        public Homm2EditorViewModel Editor { get; }

        public void Dispose() => Directory.Delete(this.folder, recursive: true);

        /// <summary>
        /// Shows the page of the first castle of the map that <paramref name="owner"/> owns, and returns it.
        /// </summary>
        public CastleViewModel OpenCastleOf(PlayerColor owner)
        {
            var castle = this.Editor.CurrentGame!.History.Current.Castles().GetModels().First(c => c.IsUsed && c.Owner == owner);
            this.Editor.ShowCastle(castle.Index);
            return (CastleViewModel)this.Editor.CurrentPage!;
        }
    }
}
