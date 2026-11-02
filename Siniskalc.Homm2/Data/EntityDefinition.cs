namespace Siniskalc.Homm2.Data
{
    public record EntityDefinition
    {
        /// <summary>
        /// Gets the identifier of the entity, which the other definitions refer to.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Gets the name the editor shows.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Gets the byte the savegame holds for the entity.
        /// </summary>
        public required byte Code { get; init; }
    }
}
