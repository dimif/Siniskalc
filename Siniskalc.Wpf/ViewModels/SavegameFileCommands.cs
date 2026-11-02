namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// What the file commands of the editor do: open, reload, save, save as, export and load the bytes of a savegame, and decide whether
    /// the editor may close. It asks the user for files and tells them when something fails, so the window only has to call it.
    /// Every method that can end with something to show returns the path it worked on, or <c>null</c> when nothing was done
    /// (the user cancelled, or it failed and the user was told).
    /// </summary>
    public sealed class SavegameFileCommands
    {
        private const string BytesFilter = "Binary dump|*.bin|All files|*.*";

        private static readonly ILogger<SavegameFileCommands> Logger = LogFactory.For<SavegameFileCommands>();

        private readonly IRecentFilesStore recentFiles;
        private readonly IFileDialogs dialogs;
        private readonly IMessageService messages;

        /// <summary>
        /// Initializes a new instance of the <see cref="SavegameFileCommands"/> class.
        /// </summary>
        /// <param name="recentFiles">The recent files.</param>
        /// <param name="dialogs">The dialogs.</param>
        /// <param name="messages">The messages.</param>
        public SavegameFileCommands(IRecentFilesStore recentFiles, IFileDialogs dialogs, IMessageService messages)
        {
            this.recentFiles = recentFiles;
            this.dialogs = dialogs;
            this.messages = messages;
        }

        /// <summary>
        /// Runs <paramref name="action"/> and tells the user if it fails. False when it failed.
        /// </summary>
        /// <param name="title">The title of the message shown on failure.</param>
        /// <param name="what">What the action was doing, for the message.</param>
        /// <returns><see langword="true"/> if the action succeeded; <see langword="false"/> if it failed and the user was told.</returns>
        /// <param name="action">The action to run.</param>
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
        /// <param name="shell">The shell that loads the game.</param>
        /// <returns>The path of the file loaded, or <see langword="null"/> if the user cancelled or the load failed.</returns>
        public string? Open(ShellViewModel shell)
        {
            var path = this.dialogs.PickFileToOpen(shell.OpenFilter);
            return path == null ? null : this.LoadFile(shell, path);
        }

        /// <summary>
        /// Loads a savegame from the list of recent files. A file that is gone is taken off the list.
        /// </summary>
        /// <param name="shell">The shell that loads the game.</param>
        /// <param name="path">The path from the list.</param>
        /// <returns>The path of the file loaded, or <see langword="null"/> if it could not be loaded.</returns>
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
        /// Reads the game again from its file, discarding the edits. When there are unsaved edits the user is asked first: save them, throw
        /// them away, or cancel. False when the game was not reloaded.
        /// </summary>
        /// <param name="shell">The shell that reloads the game.</param>
        /// <returns><see langword="true"/> if the game was read again; <see langword="false"/> if the user cancelled or it failed.</returns>
        public bool Reload(ShellViewModel shell)
        {
            if (shell.HasPendingChanges)
            {
                switch (this.messages.AskYesNoCancel("Reload save", "Reloading throws away your unsaved changes. Save them first?"))
                {
                    case MessageAnswer.Yes:
                        if (this.Save(shell) == null)
                        {
                            return false;
                        }

                        break;
                    case MessageAnswer.No:
                        break;
                    default:
                        return false;
                }
            }

            return this.Run(shell.ReloadGame, "Reload Game Error", "reload game");
        }

        /// <summary>
        /// Saves over the file the game has (it is backed up first), or asks for one when the game has none.
        /// </summary>
        /// <param name="shell">The shell that saves the game.</param>
        /// <returns>The path saved to, or <see langword="null"/> if the user cancelled or the save failed.</returns>
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
        /// <param name="shell">The shell that saves the game.</param>
        /// <returns>The path saved to, or <see langword="null"/> if the user cancelled or the save failed.</returns>
        public string? SaveAs(ShellViewModel shell)
        {
            var path = this.dialogs.PickFileToSave(shell.SaveFilter);
            if (path == null)
            {
                return null;
            }

            if (shell.ValidateSavegameName(path) is { } problem)
            {
                this.messages.ShowWarning("Save Game", problem);
                return null;
            }

            if (!this.Run(() => shell.SaveGame(path), "Save Game Error", "save game"))
            {
                return null;
            }

            this.recentFiles.Add(path);
            return path;
        }

        /// <summary>
        /// Asks for a file and writes the bytes the savegame would have to it, without compression.
        /// </summary>
        /// <param name="shell">The shell that gives the bytes.</param>
        /// <returns>The path written, or <see langword="null"/> if the user cancelled or the write failed.</returns>
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
        /// <param name="shell">The shell that loads the game.</param>
        /// <returns>The path of the file loaded, or <see langword="null"/> if the user cancelled or the load failed.</returns>
        public string? LoadBytes(ShellViewModel shell)
        {
            var path = this.dialogs.PickFileToOpen(BytesFilter);

            // Reading the file is part of the load: bytes that cannot be read are told the same way as bytes that cannot be understood.
            return path != null && this.Run(() => shell.LoadGameFromBytes(File.ReadAllBytes(path), path), "Load save from bytes", "load save from bytes") ? path : null;
        }

        /// <summary>
        /// Returns whether the editor may close: nothing is unsaved, or the user saved it or chose to throw it away.
        /// </summary>
        /// <param name="shell">The shell whose game is closed.</param>
        /// <returns><see langword="true"/> if the editor may close.</returns>
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
            // The message of a savefile exception is already written for the user, and a refused savegame is not a failure of the editor.
            if (ex is SavefileException)
            {
                Logger.LogWarning(ex, "{Title}: refused to {What}", title, what);
            }
            else
            {
                Logger.LogError(ex, "{Title}: failed to {What}", title, what);
            }

            var message = ex is SavefileException ? ex.Message : $"Failed to {what}: {ex.Message}";
            this.messages.ShowError(title, message);
        }
    }
}
