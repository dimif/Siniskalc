using Siniskalc.Core.Runtime;

namespace Siniskalc.Wpf.Tests.ViewModels
{
    /// <summary>
    /// A game that does not exist: its savefiles start with a byte of its own, and its editor is a <see cref="FakeGameEditor"/>.
    /// </summary>
    internal sealed class FakeGameModule : IGameModule
    {
        private readonly byte marker;

        public FakeGameModule(string name, byte marker, string pattern)
        {
            this.Name = name;
            this.marker = marker;
            this.OpenPattern = pattern;
            this.SavePattern = pattern;
        }

        public FakeGameEditor Editor { get; } = new();

        public int Created { get; private set; }

        public string Name { get; }

        public string OpenPattern { get; }

        public string SavePattern { get; }

        public int HeadLength => 1;

        public bool Recognizes(string filePath, ReadOnlySpan<byte> head) => head.Length >= 1 && head[0] == this.marker;

        public IGameEditor CreateEditor()
        {
            this.Created++;
            return this.Editor;
        }
    }
}
