using System.IO;
using Moq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Siniskalc.Core.Data;
using Siniskalc.Core.Models;
using Siniskalc.Core.Runtime;
using Siniskalc.Wpf.ViewModels;

namespace Siniskalc.Wpf.Tests.ViewModels
{
    /// <summary>
    /// The editor of a game that does not exist: it "loads" a file by remembering its path, and says so through the notifications a real
    /// editor sends.
    /// </summary>
    internal sealed class FakeGameEditor : IObservableGameEditor
    {
        private string? file;
        private string? name;
        private bool loaded;
        private ISavegame? savegame;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool FailToLoad { get; set; }

        public int Undone { get; private set; }

        public int Reverted { get; private set; }

        public List<string> Loaded { get; } = new();

        public ISavegame? Savegame => this.loaded ? this.savegame : null;

        public bool CanUndo => this.PendingChanges.Count > 0;

        public bool CanRedo => false;

        public ObservableCollection<PendingChange> PendingChanges { get; } = new();

        public bool HasPendingChanges => this.PendingChanges.Count > 0;

        public void LoadGame(string filePath)
        {
            if (this.FailToLoad)
            {
                throw new InvalidDataException("not a savegame");
            }

            this.Loaded.Add(filePath);
            this.loaded = true;
            this.file = filePath;
            this.name = filePath;
            this.Update();
        }

        public void LoadGameFromBytes(byte[] bytes, string? sourceName = null)
        {
            this.loaded = true;
            this.file = null;
            this.name = sourceName;
            this.Update();
        }

        public void ReloadGame() => this.LoadGame(this.file ?? throw new InvalidOperationException("No file."));

        public void SaveGame() => this.SaveGame(this.file ?? throw new InvalidOperationException("No file."));

        public void SaveGame(string filePath)
        {
            this.file = filePath;
            this.Update();
        }

        public byte[] GetBytesToSave() => [1, 2, 3];

        public SaveHexDump CreateSaveHexDump() => throw new NotSupportedException();

        public AbstractFinderViewModel CreateArtifactCatalog() => throw new NotSupportedException();

        public AbstractFinderViewModel CreateCreatureCatalog() => throw new NotSupportedException();

        public void Undo() => this.Undone++;

        public void Redo()
        {
        }

        public void RevertChange(PendingChange? change) => this.Reverted++;

        public void RevertAll() => this.Reverted++;

        /// <summary>
        /// Makes the editor say it has a pending change, as a real one does after an edit.
        /// </summary>
        public void Edit()
        {
            this.PendingChanges.Add(new PendingChange("Somebody", new Change(new ChangeArea("Resources"), "Gold", "1", "2"), new ChunkKind("Player", 1), 0, _ => { }));
            this.Raise(nameof(this.HasPendingChanges), nameof(this.CanUndo));
        }

        private void Update()
        {
            var savegame = new Mock<ISavegame>();
            savegame.SetupGet(g => g.FileName).Returns(this.file);
            savegame.SetupGet(g => g.Name).Returns(this.name ?? "Savefile");
            savegame.SetupGet(g => g.VersionName).Returns("v1");
            this.savegame = savegame.Object;
            this.Raise(nameof(this.Savegame));
        }

        private void Raise(params string[] names)
        {
            foreach (var name in names)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
