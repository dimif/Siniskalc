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

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCatalog"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        internal EntityCatalog(IEnumerable<T> values)
        {
            this.values = System.Array.AsReadOnly(values.ToArray());
            this.ids = new ReadOnlyDictionary<string, T>(this.values.ToDictionary(v => v.Id));
            this.codes = new ReadOnlyDictionary<byte, T>(this.values.ToDictionary(v => v.Code));
        }

        /// <summary>
        /// Gets the number of entities in the catalog.
        /// </summary>
        public int Count => this.values.Count;

        /// <summary>
        /// Gets the entity in a place of the catalog.
        /// </summary>
        public T this[int index] => this.values[index];

        /// <summary>
        /// Gets the entity with an identifier.
        /// </summary>
        public T this[string id] => this.ids[id];

        /// <summary>
        /// Finds an entity by the byte the savegame holds for it.
        /// </summary>
        /// <param name="code">The byte.</param>
        /// <returns>The entity, or <see langword="null"/> when no entity has the code.</returns>
        public T? Find(byte code) => this.codes.GetValueOrDefault(code);

        /// <summary>
        /// Finds an entity by its name, ignoring the case.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The entity, or <see langword="null"/> when no entity has the name.</returns>
        public T? FindName(string name) => this.values.FirstOrDefault(e => string.Equals(e.Name, name, System.StringComparison.OrdinalIgnoreCase));

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => this.values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
