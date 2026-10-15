namespace Siniskalc.Homm2.Runtime
{
    using System;
    using System.IO;
    using System.Linq;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// Heroes of Might and Magic II as a game of the application: its savegames are not compressed (<c>.GM1</c> for the base game and
    /// <c>.GX1</c> for the Price of Loyalty), and its editor is a <see cref="Homm2EditorViewModel"/>.
    /// </summary>
    public sealed class Homm2Module : IGameModule
    {
        private readonly ISaveLayoutLocator locator;
        private readonly Func<Homm2EditorViewModel> createEditor;

        /// <param name="locator">Tells whether a file is a savegame of the game, from its first bytes.</param>
        /// <param name="createEditor">Makes an editor with nothing loaded (the composition root builds its collaborators).</param>
        public Homm2Module(ISaveLayoutLocator locator, Func<Homm2EditorViewModel> createEditor)
        {
            this.locator = locator;
            this.createEditor = createEditor;
        }

        /// <inheritdoc/>
        public string Name => "Heroes of Might and Magic II";

        /// <inheritdoc/>
        public string OpenPattern => "*.GM?;*.GX?";

        /// <inheritdoc/>
        public string SavePattern => "*.GM?;*.GX?";

        /// <inheritdoc/>
        public int HeadLength => this.locator.HeadLength;

        /// <inheritdoc/>
        public bool Recognizes(string filePath, ReadOnlySpan<byte> head) => this.locator.Recognizes(head);

        /// <inheritdoc/>
        public IGameEditor CreateEditor() => this.createEditor();

        /// <summary>
        /// Heroes of Might and Magic II lists a savegame only when the name before the extension has at most 8 characters, and no space
        /// or other character a DOS file name cannot have (the extension does not count). The file dialog does not say so, and a longer name
        /// is simply not there when the game is asked to load one.
        /// </summary>
        public string? ValidateSavegameName(string fileName)
        {
            var stem = Path.GetFileNameWithoutExtension(fileName);
            if (stem.Length is 0 or > Constants.MaxSavegameNameLength || stem.Any(c => !IsDosNameCharacter(c)))
            {
                return $"Heroes of Might and Magic II only lists savegames whose name has 1 to {Constants.MaxSavegameNameLength} letters, digits or _ before the extension, such as E01GOLD.GM1. "
                    + $"\"{stem}\" would not show in the game.";
            }

            return null;
        }

        private static bool IsDosNameCharacter(char c) => char.IsAsciiLetterOrDigit(c) || "_-!#$%&'()@^{}~".Contains(c);
    }
}
