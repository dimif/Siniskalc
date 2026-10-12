namespace Siniskalc.ViewModels
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// What the file commands of the editor do: open, reload, save, save as, export and load the bytes of a savegame, and decide whether
    /// the editor may close. It asks the user for files and tells them when something fails, so the window only has to call it.
    /// Every method that can end with something to show returns the text for the status bar, or <c>null</c> when nothing was done
    /// (the user cancelled, or it failed and the user was told).
    /// </summary>
    public sealed class SavegameFileCommands
    {
        private const string SavegameOpenFilter = "HoMM3 Savegames |*.CGM;*.GM*";
        private const string SavegameSaveFilter = "HoMM3 Savegames |*.*GM;*.GM*";
        private const string BytesFilter = "Binary dump|*.bin|All files|*.*";

        private readonly RecentFilesStore recentFiles;
        private readonly IFileDialogs dialogs;
        private readonly IMessageService messages;

        public SavegameFileCommands(RecentFilesStore recentFiles, IFileDialogs dialogs, IMessageService messages)
        {
            this.recentFiles = recentFiles;
            this.dialogs = dialogs;
            this.messages = messages;
        }

        /// <summary>
        /// Runs <paramref name="action"/> and tells the user if it fails. False when it failed.
        /// </summary>
        public bool Run(Action action, string title, string what)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                this.ReportError(title, what, ex);
                return false;
            }
        }

        /// <summary>
        /// Asks for a savegame and loads it.
        /// </summary>
        public string? Open(MainViewModel main)
        {
            var path = this.dialogs.PickFileToOpen(SavegameOpenFilter);
            return path == null ? null : this.LoadFile(main, path);
        }

        /// <summary>
        /// Loads a savegame from the list of recent files. A file that is gone is taken off the list.
        /// </summary>
        public string? OpenRecent(MainViewModel main, string path)
        {
            if (!File.Exists(path))
            {
                this.messages.ShowWarning("Load Game Error", $"File no longer exists:\n{path}");
                this.recentFiles.Remove(path);
                return null;
            }

            return this.LoadFile(main, path);
        }

        /// <summary>
        /// Reads the game again from its file, discarding the edits.
        /// </summary>
        public bool Reload(MainViewModel main) => this.Run(main.ReloadGame, "Reload Game Error", "reload game");

        /// <summary>
        /// Saves over the file the game has (it is backed up first), or asks for one when the game has none.
        /// </summary>
        public string? Save(MainViewModel main)
        {
            if (!main.HasFile)
            {
                return this.SaveAs(main);
            }

            return this.Run(main.SaveGame, "Save Game Error", "save game") ? main.CurrentGame?.File : null;
        }

        /// <summary>
        /// Asks for a file and saves the game there.
        /// </summary>
        public string? SaveAs(MainViewModel main)
        {
            var path = this.dialogs.PickFileToSave(SavegameSaveFilter);
            if (path == null || !this.Run(() => main.SaveGame(path), "Save Game Error", "save game"))
            {
                return null;
            }

            this.recentFiles.Add(path);
            return path;
        }

        /// <summary>
        /// Asks for a file and writes the bytes the savegame would have to it, without compression.
        /// </summary>
        public async Task<string?> ExportBytesAsync(MainViewModel main)
        {
            var game = main.CurrentGame;
            if (game == null)
            {
                return null;
            }

            var name = string.IsNullOrWhiteSpace(game.Name) ? "game" : Path.GetFileNameWithoutExtension(game.Name);
            var path = this.dialogs.PickFileToSave(BytesFilter, $"{name}.bin", ".bin");
            if (path == null)
            {
                return null;
            }

            try
            {
                await File.WriteAllBytesAsync(path, game.GetBytesToSave());
                return path;
            }
            catch (Exception ex)
            {
                this.ReportError("Export Save Bytes Error", "export save bytes", ex);
                return null;
            }
        }

        /// <summary>
        /// Asks for a file of bytes written by <see cref="ExportBytesAsync"/> and loads it as a game.
        /// </summary>
        public string? LoadBytes(MainViewModel main)
        {
            var path = this.dialogs.PickFileToOpen(BytesFilter);

            // Reading the file is part of the load: bytes that cannot be read are told the same way as bytes that cannot be understood.
            return path != null && this.Run(() => main.LoadGameFromBytes(File.ReadAllBytes(path), path), "Load save from bytes", "load save from bytes") ? path : null;
        }

        /// <summary>
        /// Returns whether the editor may close: nothing is unsaved, or the user saved it or chose to throw it away.
        /// </summary>
        public bool ConfirmClose(MainViewModel main)
        {
            if (!main.HasPendingChanges)
            {
                return true;
            }

            return this.messages.AskYesNoCancel("Unsaved changes", "You have unsaved changes. Save before closing?") switch
            {
                MessageAnswer.Yes => this.Save(main) != null,
                MessageAnswer.No => true,
                _ => false,
            };
        }

        private string? LoadFile(MainViewModel main, string path)
        {
            if (!this.Run(() => main.LoadGame(path), "Load Game Error", "load game"))
            {
                return null;
            }

            this.recentFiles.Add(path);
            return path;
        }

        private void ReportError(string title, string what, Exception ex)
        {
            // The message of a savefile exception is already written for the user.
            // TODO:LOG error: {title}: {what} failed; log the whole exception (type, message, inner exception, stack trace) and the path involved.
            var message = ex is SavefileException ? ex.Message : $"Failed to {what}: {ex.Message}";
            this.messages.ShowError(title, message);
        }
    }
}
