namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Input;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// The application around the editors of the games: the savegame that is open, its history, the preferences of the user and what
    /// the panel of pending changes shows. It knows no game: each game is an <see cref="IGameModule"/> that makes the
    /// <see cref="IGameEditor"/> that does the work, and the shell picks the module that recognises the file the user opens.
    /// </summary>
    public sealed class ShellViewModel : AbstractViewModelBase
    {
        private const string NoGameSubtitle = "Save game editor";

        // What the shell mirrors from the editor it shows, so its bindings read the same names.
        private static readonly string[] Mirrored =
        [
            nameof(IGameEditor.CanUndo),
            nameof(IGameEditor.CanRedo),
            nameof(IGameEditor.HasPendingChanges),
        ];

        private static readonly ILogger<ShellViewModel> Logger = LogFactory.For<ShellViewModel>();

        private readonly IReadOnlyList<IGameModule<IObservableGameEditor>> modules;
        private readonly ISettingsStore settings;
        private readonly string applicationName;
        private readonly Dictionary<IGameModule<IObservableGameEditor>, IObservableGameEditor> editors = new();
        private IGameModule<IObservableGameEditor> currentModule;
        private IObservableGameEditor editor;
        private bool isPendingChangesOpen;

        /// <param name="modules">The games the application can edit. The first is shown until a file of another one is opened.</param>
        /// <param name="settings">Where the preferences of the user are kept.</param>
        /// <param name="applicationName">The name of the application, for the title of the window.</param>
        public ShellViewModel(IReadOnlyList<IGameModule<IObservableGameEditor>> modules, ISettingsStore settings, string applicationName)
        {
            if (modules.Count == 0)
            {
                throw new ArgumentException("The application needs at least one game.", nameof(modules));
            }

            this.modules = modules;
            this.settings = settings;
            this.applicationName = applicationName;
            this.currentModule = modules[0];
            this.editor = this.EditorOf(this.currentModule);
            this.editor.PropertyChanged += this.OnEditorPropertyChanged;
            this.RevertChangeCommand = new DelegateCommand(p => this.RevertChange((PendingChange?)p), p => p is PendingChange);
            this.RevertAllCommand = new DelegateCommand(_ => this.RevertAll(), _ => this.HasPendingChanges);
        }

        /// <summary>
        /// Gets the games the application can edit.
        /// </summary>
        public IReadOnlyList<IGameModule<IObservableGameEditor>> Modules => this.modules;

        /// <summary>
        /// Gets the game whose editor is shown.
        /// </summary>
        public IGameModule<IObservableGameEditor> CurrentModule => this.currentModule;

        /// <summary>
        /// Gets the editor that is shown: the one of the game whose savegame was opened last, or of the first game when none was.
        /// </summary>
        public IObservableGameEditor Editor => this.editor;

        /// <summary>
        /// Gets the title of the window: the name of the application and "Save game editor" until a savegame is open, then the name of the application
        /// and the game and version of the savegame, such as "Siniskalc - Heroes of Might and Magic III (Shadow of Death)".
        /// </summary>
        public string Title => this.editor.Savegame is { } savegame
            ? $"{this.applicationName} - {this.currentModule.Name}{(savegame.VersionName.Length > 0 ? $" ({savegame.VersionName})" : string.Empty)}{(this.HasFile ? $" - {System.IO.Path.GetFullPath(this.File!)}" : string.Empty)}"
            : $"{this.applicationName} - {NoGameSubtitle}";

        /// <summary>
        /// Gets or sets whether a copy of a savefile is kept before it is saved over. Kept in the settings of the user.
        /// </summary>
        public bool BackupOnSave
        {
            get => this.settings.BackupOnSave;
            set
            {
                if (this.settings.BackupOnSave != value)
                {
                    this.settings.BackupOnSave = value;
                    this.RaisePropertyChanged(nameof(this.BackupOnSave));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the Advanced menu and the hex tabs are shown. Kept in the settings of the user.
        /// </summary>
        public bool IsAdvancedMode
        {
            get => this.settings.AdvancedMode;
            set
            {
                if (this.settings.AdvancedMode != value)
                {
                    this.settings.AdvancedMode = value;
                    this.RaisePropertyChanged(nameof(this.IsAdvancedMode));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the pending changes panel is shown.
        /// </summary>
        public bool IsPendingChangesOpen
        {
            get => this.isPendingChangesOpen;
            set
            {
                if (this.isPendingChangesOpen != value)
                {
                    this.isPendingChangesOpen = value;
                    this.RaisePropertyChanged(nameof(this.IsPendingChangesOpen));
                }
            }
        }

        /// <summary>
        /// Gets the command that puts one pending change back; its parameter is the change.
        /// </summary>
        public ICommand RevertChangeCommand { get; }

        /// <summary>
        /// Gets the command that puts every edit back to the loaded values.
        /// </summary>
        public ICommand RevertAllCommand { get; }

        /// <summary>
        /// Gets a value indicating whether a savegame is open in the current editor.
        /// </summary>
        public bool IsGameLoaded => this.editor.Savegame != null;

        /// <summary>
        /// Gets what to call the loaded game: its file, or the name it was given when loaded from bytes.
        /// </summary>
        public string? Name => this.editor.Savegame?.Name;

        /// <summary>
        /// Gets the file the loaded game is tied to, or <c>null</c> when it has none.
        /// </summary>
        public string? File => this.editor.Savegame?.FileName;

        /// <summary>
        /// Gets a value indicating whether the open savegame is tied to a file.
        /// </summary>
        public bool HasFile => !string.IsNullOrWhiteSpace(this.File);

        /// <summary>
        /// Gets a value indicating whether the savegame can be read again, which it can when it has a file.
        /// </summary>
        public bool CanReload => this.HasFile;

        /// <summary>
        /// Gets a value indicating whether there is an edit to undo.
        /// </summary>
        public bool CanUndo => this.editor.CanUndo;

        /// <summary>
        /// Gets a value indicating whether there is an undone edit to redo.
        /// </summary>
        public bool CanRedo => this.editor.CanRedo;

        /// <summary>
        /// Gets what has been edited since the savefile was loaded. It is the list of the editor that is shown.
        /// </summary>
        public ObservableCollection<PendingChange> PendingChanges => this.editor.PendingChanges;

        /// <summary>
        /// Gets a value indicating whether anything has been edited since the savefile was loaded or saved.
        /// </summary>
        public bool HasPendingChanges => this.editor.HasPendingChanges;

        /// <summary>
        /// Gets the file dialog filter for opening a savegame: every game's types together, then each game's alone.
        /// </summary>
        public string OpenFilter => this.modules.Count == 1
            ? $"{this.modules[0].Name} savegames |{this.modules[0].OpenPattern}"
            : "Savegames |" + string.Join(";", this.modules.Select(m => m.OpenPattern)) + "|" + string.Join("|", this.modules.Select(m => $"{m.Name} savegames |{m.OpenPattern}"));

        /// <summary>
        /// Gets the file filter of the Save As dialog for the game being edited.
        /// </summary>
        public string SaveFilter => $"{this.currentModule.Name} savegames |{this.currentModule.SavePattern}";

        /// <summary>
        /// Tells why the game of the current module cannot list a savegame called like <paramref name="path"/>, or <c>null</c> when it can.
        /// </summary>
        /// <param name="path">The path the game would be saved to; only its file name is looked at.</param>
        /// <returns><see langword="null"/> if the game can list a savegame with that name; otherwise a sentence for the user saying what is wrong.</returns>
        public string? ValidateSavegameName(string path) => this.currentModule.ValidateSavegameName(System.IO.Path.GetFileName(path));

        /// <summary>
        /// Returns the game whose savegame the file is, or <c>null</c> when none recognises it or it cannot be read.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        /// <returns>The module of the game, or <see langword="null"/>.</returns>
        public IGameModule<IObservableGameEditor>? Recognize(string filePath)
        {
            try
            {
                var head = new byte[this.modules.Max(m => m.HeadLength)];
                using var stream = System.IO.File.OpenRead(filePath);
                var read = stream.Read(head, 0, head.Length);
                return this.modules.FirstOrDefault(m => m.IsOwnSavefile(filePath, head.AsSpan(0, Math.Min(read, m.HeadLength))));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // The editor that is shown tells the user why the file cannot be opened.
                Logger.LogWarning(ex, "Could not read {File} to find its game", System.IO.Path.GetFileName(filePath));
                return null;
            }
        }

        /// <summary>
        /// Loads a savegame from a file with the editor of the game it belongs to. A file that no game recognises is given to the editor
        /// that is shown, which tells why it cannot be opened. What is open stays as it was when the load fails.
        /// </summary>
        /// <param name="filePath">The path of the savefile.</param>
        public void LoadGame(string filePath)
        {
            var target = this.EditorOf(this.Recognize(filePath) ?? this.currentModule);
            target.LoadGame(filePath);
            this.Show(target);
        }

        /// <summary>
        /// Loads a savegame from the bytes of a file as the game wrote them, before any compression, with the editor that is shown.
        /// </summary>
        /// <param name="bytes">The bytes of the savefile.</param>
        /// <param name="sourceName">What to call the game, as it has no file.</param>
        public void LoadGameFromBytes(byte[] bytes, string? sourceName = null) => this.editor.LoadGameFromBytes(bytes, sourceName);

        /// <summary>
        /// Reads the game again from its file, discarding unsaved edits.
        /// </summary>
        public void ReloadGame() => this.editor.ReloadGame();

        /// <summary>
        /// Saves the game over its file, or to the path given.
        /// </summary>
        /// <param name="filePath">The path of the file to write, which is then the file of the game.</param>
        public void SaveGame() => this.editor.SaveGame();

        /// <summary>
        /// Saves the game over its file, or to the path given.
        /// </summary>
        /// <param name="filePath">The path of the file to write, which is then the file of the game.</param>
        public void SaveGame(string filePath) => this.editor.SaveGame(filePath);

        /// <summary>
        /// Returns the bytes the savefile would have, with every edit applied.
        /// </summary>
        /// <returns>The bytes of the savefile as it would be saved.</returns>
        public byte[] GetBytesToSave() => this.editor.GetBytesToSave();

        /// <summary>
        /// Takes the bytes of the whole savefile as they would be saved, with its known regions, for the hex view.
        /// </summary>
        /// <returns>The dump of the savefile.</returns>
        public SaveHexDump CreateSaveHexDump() => this.editor.CreateSaveHexDump();

        /// <summary>
        /// Goes back to the state before the last edit.
        /// </summary>
        public void Undo() => this.editor.Undo();

        /// <summary>
        /// Goes forward again to the edit that was undone last.
        /// </summary>
        public void Redo() => this.editor.Redo();

        /// <summary>
        /// Puts one pending change back to its loaded value.
        /// </summary>
        /// <param name="change">The change to put back; nothing happens when it is <see langword="null"/>.</param>
        public void RevertChange(PendingChange? change) => this.editor.RevertChange(change);

        /// <summary>
        /// Puts every edit back to the loaded values.
        /// </summary>
        public void RevertAll() => this.editor.RevertAll();

        private IObservableGameEditor EditorOf(IGameModule<IObservableGameEditor> module)
        {
            if (!this.editors.TryGetValue(module, out var made))
            {
                made = module.CreateEditor();
                this.editors[module] = made;
            }

            return made;
        }

        private void Show(IObservableGameEditor target)
        {
            if (ReferenceEquals(target, this.editor))
            {
                return;
            }

            this.editor.PropertyChanged -= this.OnEditorPropertyChanged;
            this.editor = target;
            this.currentModule = this.editors.Single(pair => ReferenceEquals(pair.Value, target)).Key;
            this.editor.PropertyChanged += this.OnEditorPropertyChanged;

            // Everything that is read from the editor is another editor's now.
            this.RaiseAllPropertiesChanged();
        }

        private void OnEditorPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is { } name && Array.IndexOf(Mirrored, name) >= 0)
            {
                this.RaisePropertyChanged(name);
            }

            if (e.PropertyName == nameof(IGameEditor.Savegame))
            {
                this.RaisePropertyChanged(nameof(this.IsGameLoaded));
                this.RaisePropertyChanged(nameof(this.Title));
                this.RaisePropertyChanged(nameof(this.Name));
                this.RaisePropertyChanged(nameof(this.File));
                this.RaisePropertyChanged(nameof(this.HasFile));
                this.RaisePropertyChanged(nameof(this.CanReload));
            }

            if (e.PropertyName == nameof(IGameEditor.HasPendingChanges))
            {
                (this.RevertAllCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            }
        }
    }
}
