namespace Siniskalc.Wpf.ViewModels
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
        public string? Open(ShellViewModel shell)
        {
            var path = this.dialogs.PickFileToOpen(shell.OpenFilter);
            return path == null ? null : this.LoadFile(shell, path);
        }

        /// <summary>
        /// Loads a savegame from the list of recent files. A file that is gone is taken off the list.
        /// </summary>
        public string? OpenRecent(ShellViewModel shell, string path)
        {
            if (!File.Exists(path))
            {
                this.messages.ShowWarning("Load Game Error", $"File no longer exists:\n{path}");
                this.recentFiles.Remove(path);
                return null;
            }

            return this.LoadFile(shell, path);
        }

        /// <summary>
        /// Reads the game again from its file, discarding the edits.
        /// </summary>
        public bool Reload(ShellViewModel shell) => this.Run(shell.ReloadGame, "Reload Game Error", "reload game");

        /// <summary>
        /// Saves over the file the game has (it is backed up first), or asks for one when the game has none.
        /// </summary>
        public string? Save(ShellViewModel shell)
        {
            if (!shell.HasFile)
            {
                return this.SaveAs(shell);
            }

            return this.Run(shell.SaveGame, "Save Game Error", "save game") ? shell.File : null;
        }

        /// <summary>
        /// Asks for a file and saves the game there.
        /// </summary>
        public string? SaveAs(ShellViewModel shell)
        {
            var path = this.dialogs.PickFileToSave(shell.SaveFilter);
            if (path == null || !this.Run(() => shell.SaveGame(path), "Save Game Error", "save game"))
            {
                return null;
            }

            this.recentFiles.Add(path);
            return path;
        }

        /// <summary>
        /// Asks for a file and writes the bytes the savegame would have to it, without compression.
        /// </summary>
        public async Task<string?> ExportBytesAsync(ShellViewModel shell)
        {
            if (!shell.IsGameLoaded)
            {
                return null;
            }

            var name = string.IsNullOrWhiteSpace(shell.Name) ? "game" : Path.GetFileNameWithoutExtension(shell.Name);
            var path = this.dialogs.PickFileToSave(BytesFilter, $"{name}.bin", ".bin");
            if (path == null)
            {
                return null;
            }

            try
            {
                await File.WriteAllBytesAsync(path, shell.GetBytesToSave());
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
        public string? LoadBytes(ShellViewModel shell)
        {
            var path = this.dialogs.PickFileToOpen(BytesFilter);

            // Reading the file is part of the load: bytes that cannot be read are told the same way as bytes that cannot be understood.
            return path != null && this.Run(() => shell.LoadGameFromBytes(File.ReadAllBytes(path), path), "Load save from bytes", "load save from bytes") ? path : null;
        }

        /// <summary>
        /// Returns whether the editor may close: nothing is unsaved, or the user saved it or chose to throw it away.
        /// </summary>
        public bool ConfirmClose(ShellViewModel shell)
        {
            if (!shell.HasPendingChanges)
            {
                return true;
            }

            return this.messages.AskYesNoCancel("Unsaved changes", "You have unsaved changes. Save before closing?") switch
            {
                MessageAnswer.Yes => this.Save(shell) != null,
                MessageAnswer.No => true,
                _ => false,
            };
        }

        private string? LoadFile(ShellViewModel shell, string path)
        {
            if (!this.Run(() => shell.LoadGame(path), "Load Game Error", "load game"))
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
