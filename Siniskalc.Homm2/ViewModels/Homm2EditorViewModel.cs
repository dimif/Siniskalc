namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The editor of Heroes of Might and Magic II: the open savegame, its history and what it shows. The application shell
    /// (<see cref="ShellViewModel"/>) works with it through <see cref="IGameEditor"/>.
    /// </summary>
    public sealed class Homm2EditorViewModel : AbstractViewModelBase, IGameEditor
    {
        private readonly IHomm2GameService gameService;
        private readonly ISaveRegionProvider saveRegions;
        private Homm2Game? currentGame;
        private Homm2OverviewViewModel? overview;

        public Homm2EditorViewModel(IHomm2GameService gameService, ISaveRegionProvider saveRegions)
        {
            this.gameService = gameService;
            this.saveRegions = saveRegions;
        }

        /// <summary>
        /// Gets the loaded game, or <c>null</c> when none is loaded.
        /// </summary>
        public Homm2Game? CurrentGame => this.currentGame;

        /// <summary>
        /// Gets what the editor shows about the loaded game, or <c>null</c> when none is loaded.
        /// </summary>
        public Homm2OverviewViewModel? Overview => this.overview;

        /// <inheritdoc/>
        public bool IsGameLoaded => this.currentGame != null;

        /// <inheritdoc/>
        public string? File => this.currentGame?.File;

        /// <inheritdoc/>
        public string? Name => this.currentGame?.Name;

        /// <inheritdoc/>
        public bool HasFile => !string.IsNullOrWhiteSpace(this.currentGame?.File);

        /// <inheritdoc/>
        public bool CanReload => this.HasFile;

        /// <inheritdoc/>
        public bool CanUndo => this.currentGame?.History.CanUndo ?? false;

        /// <inheritdoc/>
        public bool CanRedo => this.currentGame?.History.CanRedo ?? false;

        /// <inheritdoc/>
        public ObservableCollection<PendingChange> PendingChanges { get; } = new();

        /// <inheritdoc/>
        public bool HasPendingChanges => this.PendingChanges.Count > 0;

        /// <inheritdoc/>
        public void LoadGame(string filePath) => this.SetGame(this.gameService.LoadGameFromFile(filePath));

        /// <inheritdoc/>
        public void LoadGameFromBytes(byte[] bytes, string? sourceName = null) => this.SetGame(this.gameService.LoadGameFromBytes(bytes, sourceName));

        /// <inheritdoc/>
        public void ReloadGame()
        {
            if (this.currentGame == null)
            {
                throw new InvalidOperationException("No game is currently loaded");
            }

            this.SetGame(this.gameService.ReloadGame(this.currentGame));
        }

        /// <inheritdoc/>
        public void SaveGame()
        {
            var file = this.currentGame?.File;
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new InvalidOperationException("The game has no file to save to");
            }

            this.SaveGame(file);
        }

        /// <inheritdoc/>
        public void SaveGame(string filePath)
        {
            if (this.currentGame == null)
            {
                throw new InvalidOperationException("No game is currently loaded");
            }

            this.gameService.SaveGame(this.currentGame, filePath);
            this.RaiseFileChanged();
        }

        /// <inheritdoc/>
        public byte[] GetBytesToSave() => this.Loaded().GetBytesToSave();

        /// <inheritdoc/>
        public SaveHexDump CreateSaveHexDump()
        {
            var game = this.Loaded();
            var name = string.IsNullOrWhiteSpace(game.Name) ? "Savefile" : System.IO.Path.GetFileName(game.Name);
            return SaveHexDump.Create($"{name} ({game.GameVersion})", game.History, this.saveRegions);
        }

        /// <inheritdoc/>
        public void Undo() => this.currentGame?.History.Undo();

        /// <inheritdoc/>
        public void Redo() => this.currentGame?.History.Redo();

        /// <inheritdoc/>
        public void RevertChange(PendingChange? change)
        {
            if (this.currentGame != null)
            {
                change?.Revert(this.currentGame.History);
            }
        }

        /// <inheritdoc/>
        public void RevertAll()
        {
            if (this.currentGame != null && this.HasPendingChanges)
            {
                this.currentGame.History.Apply(this.currentGame.History.Original);
            }
        }

        private Homm2Game Loaded() => this.currentGame ?? throw new InvalidOperationException("No game is currently loaded");

        private void SetGame(Homm2Game game)
        {
            // What reads the new game (and so can fail on a savefile that is odd) is built before anything of the open game is let go.
            var newOverview = new Homm2OverviewViewModel(game);

            if (this.currentGame != null)
            {
                this.currentGame.History.Changed -= this.OnHistoryChanged;
            }

            this.overview?.Dispose();
            this.currentGame = game;
            this.overview = newOverview;
            this.currentGame.History.Changed += this.OnHistoryChanged;
            this.PendingChanges.Clear();
            this.RaisePropertyChanged(nameof(this.CurrentGame));
            this.RaisePropertyChanged(nameof(this.Overview));
            this.RaisePropertyChanged(nameof(this.IsGameLoaded));
            this.RaiseFileChanged();
            this.OnHistoryChanged(this, EventArgs.Empty);
        }

        private void RaiseFileChanged()
        {
            this.RaisePropertyChanged(nameof(this.File));
            this.RaisePropertyChanged(nameof(this.Name));
            this.RaisePropertyChanged(nameof(this.HasFile));
            this.RaisePropertyChanged(nameof(this.CanReload));
        }

        private void OnHistoryChanged(object? sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(this.CanUndo));
            this.RaisePropertyChanged(nameof(this.CanRedo));
            this.RefreshPendingChanges();
        }

        private void RefreshPendingChanges()
        {
            var changes = this.currentGame?.History.GetPendingChanges() ?? [];

            // Keep what has not changed, so the list tells the screen about the changes that are different and not about every one.
            var same = 0;
            while (same < changes.Count && same < this.PendingChanges.Count && changes[same].IsSameChangeAs(this.PendingChanges[same]))
            {
                same++;
            }

            while (this.PendingChanges.Count > same)
            {
                this.PendingChanges.RemoveAt(this.PendingChanges.Count - 1);
            }

            for (var i = same; i < changes.Count; i++)
            {
                this.PendingChanges.Add(changes[i]);
            }

            this.RaisePropertyChanged(nameof(this.HasPendingChanges));
        }
    }
}
