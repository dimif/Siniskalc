namespace Siniskalc.Homm2.Runtime
{
    using System;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// Heroes of Might and Magic II as a game of the application: its savegames are not compressed (<c>.GM1</c> for the base game and
    /// <c>.GX1</c> for the Price of Loyalty), and its editor is a <see cref="Homm2EditorViewModel"/>.
    /// </summary>
    public sealed class Homm2Module : IGameModule
    {
        private readonly IHomm2SaveLayoutLocator locator;
        private readonly Func<Homm2EditorViewModel> createEditor;

        /// <param name="locator">Tells whether a file is a savegame of the game, from its first bytes.</param>
        /// <param name="createEditor">Makes an editor with nothing loaded (the composition root builds its collaborators).</param>
        public Homm2Module(IHomm2SaveLayoutLocator locator, Func<Homm2EditorViewModel> createEditor)
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
    }
}
