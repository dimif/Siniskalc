namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The definition of an object of the map the editor lists, as it is read from <c>mapObjects.json</c>.
    /// </summary>
    public sealed record MapObjectDefinition : EntityDefinition
    {
        /// <summary>
        /// Gets what the object is for the editor.
        /// </summary>
        public required MapObjectRole Role { get; init; }

        /// <summary>
        /// Gets the format of the name of the object, which takes its position.
        /// </summary>
        public required string NameFormat { get; init; }
    }
}
