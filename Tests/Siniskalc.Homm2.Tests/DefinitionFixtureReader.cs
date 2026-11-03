namespace Siniskalc.Homm2.Tests
{
    using Siniskalc.Core.Data;

    /// <summary>Overlays only the checked-in JSON fixture; standard parent files remain the shipped files.</summary>
    internal sealed class DefinitionFixtureReader : IGameDefinitionReader
    {
        private readonly IGameDefinitionReader reader = new JsonGameDefinitionReader();

        public bool Locked { get; set; }

        public int Reads { get; private set; }

        public T Read<T>(string relativePath)
            where T : class
        {
            if (this.Locked)
            {
                throw new InvalidOperationException("Definitions were read after startup.");
            }

            this.Reads++;
            var fixture = relativePath == @"GameData\Homm2\manifest.json" || relativePath.StartsWith(@"GameData\Homm2\synthetic\", StringComparison.Ordinal)
                ? relativePath.Replace(@"GameData\Homm2\", @"TestDefinitions\", StringComparison.Ordinal)
                : relativePath;
            return this.reader.Read<T>(fixture);
        }
    }
}
