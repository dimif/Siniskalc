namespace Siniskalc.Homm2.Entities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public sealed class EntityCatalog<T> : IReadOnlyList<T>
        where T : CatalogEntity
    {
        private readonly ReadOnlyCollection<T> values;
        private readonly IReadOnlyDictionary<string, T> ids;
        private readonly IReadOnlyDictionary<byte, T> codes;

        internal EntityCatalog(IEnumerable<T> values)
        {
            this.values = System.Array.AsReadOnly(values.ToArray());
            this.ids = new ReadOnlyDictionary<string, T>(this.values.ToDictionary(v => v.Id));
            this.codes = new ReadOnlyDictionary<byte, T>(this.values.ToDictionary(v => v.Code));
        }

        public int Count => this.values.Count;

        public T this[int index] => this.values[index];

        public T this[string id] => this.ids[id];

        public T? Find(byte code) => this.codes.GetValueOrDefault(code);

        public T? FindName(string name) => this.values.FirstOrDefault(e => string.Equals(e.Name, name, System.StringComparison.OrdinalIgnoreCase));

        public IEnumerator<T> GetEnumerator() => this.values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
