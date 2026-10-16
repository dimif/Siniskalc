namespace Siniskalc.Homm3.Runtime
{
    using System;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// Heroes of Might and Magic III as a game of the application: its savegames are gzip files, and its editor is a
    /// <see cref="Homm3EditorViewModel"/>.
    /// </summary>
    public sealed class Homm3Module : IGameModule<Homm3EditorViewModel>
    {
        private readonly Func<Homm3EditorViewModel> createEditor;

        /// <param name="createEditor">Makes an editor with nothing loaded (the composition root builds its collaborators).</param>
        public Homm3Module(Func<Homm3EditorViewModel> createEditor)
        {
            this.createEditor = createEditor;
        }

        /// <inheritdoc/>
        public string Name => "Heroes of Might and Magic III";

        /// <inheritdoc/>
        public string OpenPattern => "*.CGM;*.GM*";

        /// <inheritdoc/>
        public string SavePattern => "*.CGM;*.GM*";

        /// <inheritdoc/>
        public int HeadLength => GzipSavefileCodec.MagicLength;

        /// <inheritdoc/>
        public bool IsOwnSavefile(string filePath, ReadOnlySpan<byte> head) => GzipSavefileCodec.IsGzip(head);

        /// <inheritdoc/>
        public Homm3EditorViewModel CreateEditor() => this.createEditor();
    }
}
