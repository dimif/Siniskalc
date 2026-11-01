namespace Siniskalc.Homm2.Tests
{
    using Siniskalc.Core.Data;

    internal sealed class DefinitionMutationReader : IGameDefinitionReader
    {
        private readonly IGameDefinitionReader reader = new JsonGameDefinitionReader();
        private readonly Dictionary<string, object> replacements = [];

        public void Replace<T>(string domain, Func<T, T> replace)
            where T : class
        {
            var path = @"GameData\Homm2\standard\" + domain + ".json";
            this.replacements[path] = replace(this.reader.Read<T>(path));
        }

        public void Set<T>(string relativePath, T value)
            where T : class => this.replacements[relativePath] = value;

        public T Read<T>(string relativePath)
            where T : class => this.replacements.TryGetValue(relativePath, out var value) ? (T)value : this.reader.Read<T>(relativePath);
    }
}
