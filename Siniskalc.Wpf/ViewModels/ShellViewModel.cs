namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// The application around the editors of the games: the savegame that is open, its history, the preferences of the user and what
    /// the panel of pending changes shows. It knows no game: each game is an <see cref="IGameModule"/> that makes the
    /// <see cref="IGameEditor"/> that does the work, and the shell picks the module that recognises the file the user opens.
    /// </summary>
    public sealed class ShellViewModel : AbstractViewModelBase
    {
        // What the shell mirrors from the editor it shows, so its bindings read the same names.
        private static readonly string[] Mirrored =
        [
            nameof(IGameEditor.IsGameLoaded),
            nameof(IGameEditor.File),
            nameof(IGameEditor.Name),
            nameof(IGameEditor.HasFile),
            nameof(IGameEditor.CanReload),
            nameof(IGameEditor.CanUndo),
            nameof(IGameEditor.CanRedo),
            nameof(IGameEditor.HasPendingChanges),
        ];

        private readonly IReadOnlyList<IGameModule> modules;
        private readonly SettingsStore settings;
        private readonly string applicationName;
        private readonly Dictionary<IGameModule, IGameEditor> editors = new();
        private IGameModule currentModule;
        private IGameEditor editor;
        private bool isPendingChangesOpen;

        /// <param name="modules">The games the application can edit. The first is shown until a file of another one is opened.</param>
        /// <param name="settings">Where the preferences of the user are kept.</param>
        /// <param name="applicationName">The name of the application, for the title of the window.</param>
        public ShellViewModel(IReadOnlyList<IGameModule> modules, SettingsStore settings, string applicationName)
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
        public IReadOnlyList<IGameModule> Modules => this.modules;

        /// <summary>
        /// Gets the game whose editor is shown.
        /// </summary>
        public IGameModule CurrentModule => this.currentModule;

        /// <summary>
        /// Gets the editor that is shown: the one of the game whose savegame was opened last, or of the first game when none was.
        /// </summary>
        public IGameEditor Editor => this.editor;

        /// <summary>
        /// Gets the title of the window.
        /// </summary>
        public string Title => $"{this.applicationName} - {this.currentModule.Name}";

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

        public ICommand RevertChangeCommand { get; }

        public ICommand RevertAllCommand { get; }

        public bool IsGameLoaded => this.editor.IsGameLoaded;

        /// <summary>
        /// Gets what to call the loaded game: its file, or the name it was given when loaded from bytes.
        /// </summary>
        public string? Name => this.editor.Name;

        /// <summary>
        /// Gets the file the loaded game is tied to, or <c>null</c> when it has none.
        /// </summary>
        public string? File => this.editor.File;

        public bool HasFile => this.editor.HasFile;

        public bool CanReload => this.editor.CanReload;

        public bool CanUndo => this.editor.CanUndo;

        public bool CanRedo => this.editor.CanRedo;

        /// <summary>
        /// Gets what has been edited since the savefile was loaded. It is the list of the editor that is shown.
        /// </summary>
        public ObservableCollection<PendingChange> PendingChanges => this.editor.PendingChanges;

        public bool HasPendingChanges => this.editor.HasPendingChanges;

        /// <summary>
        /// Gets the file dialog filter for opening a savegame: every game's types together, then each game's alone.
        /// </summary>
        public string OpenFilter => this.modules.Count == 1
            ? $"{this.modules[0].Name} savegames |{this.modules[0].OpenPattern}"
            : "Savegames |" + string.Join(";", this.modules.Select(m => m.OpenPattern)) + "|" + string.Join("|", this.modules.Select(m => $"{m.Name} savegames |{m.OpenPattern}"));

        /// <summary>
        /// Gets the file dialog filter for saving the game that is shown.
        /// </summary>
        public string SaveFilter => $"{this.currentModule.Name} savegames |{this.currentModule.SavePattern}";

        /// <summary>
        /// Returns the game whose savegame the file is, or <c>null</c> when none recognises it or it cannot be read.
        /// </summary>
        public IGameModule? Recognize(string filePath)
        {
            try
            {
                var head = new byte[this.modules.Max(m => m.HeadLength)];
                using var stream = System.IO.File.OpenRead(filePath);
                var read = stream.Read(head, 0, head.Length);
                return this.modules.FirstOrDefault(m => m.Recognizes(filePath, head.AsSpan(0, Math.Min(read, m.HeadLength))));
            }
            catch (IOException)
            {
                // TODO:LOG warn: the file {filePath} could not be read to find its game; the editor that is shown tells the user why.
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                // TODO:LOG warn: the file {filePath} could not be read to find its game; the editor that is shown tells the user why.
                return null;
            }
        }

        /// <summary>
        /// Loads a savegame from a file with the editor of the game it belongs to. A file that no game recognises is given to the editor
        /// that is shown, which tells why it cannot be opened. What is open stays as it was when the load fails.
        /// </summary>
        public void LoadGame(string filePath)
        {
            var target = this.EditorOf(this.Recognize(filePath) ?? this.currentModule);
            target.LoadGame(filePath);
            this.Show(target);
        }

        /// <summary>
        /// Loads a savegame from the bytes of a file as the game wrote them, before any compression, with the editor that is shown.
        /// </summary>
        public void LoadGameFromBytes(byte[] bytes, string? sourceName = null) => this.editor.LoadGameFromBytes(bytes, sourceName);

        public void ReloadGame() => this.editor.ReloadGame();

        public void SaveGame() => this.editor.SaveGame();

        public void SaveGame(string filePath) => this.editor.SaveGame(filePath);

        public byte[] GetBytesToSave() => this.editor.GetBytesToSave();

        public SaveHexDump CreateSaveHexDump() => this.editor.CreateSaveHexDump();

        public void Undo() => this.editor.Undo();

        public void Redo() => this.editor.Redo();

        public void RevertChange(PendingChange? change) => this.editor.RevertChange(change);

        public void RevertAll() => this.editor.RevertAll();

        private IGameEditor EditorOf(IGameModule module)
        {
            if (!this.editors.TryGetValue(module, out var made))
            {
                made = module.CreateEditor();
                this.editors[module] = made;
            }

            return made;
        }

        private void Show(IGameEditor target)
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

            if (e.PropertyName == nameof(IGameEditor.HasPendingChanges))
            {
                (this.RevertAllCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            }
        }
    }
}
