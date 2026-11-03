namespace Siniskalc.Homm2.Data
{
    /// <summary>
    /// The definition of a group of spells, as it is read from <c>spellTypes.json</c>.
    /// </summary>
    public sealed record SpellTypeDefinition : EntityDefinition
    {
        /// <summary>
        /// Gets whether the group is the one of the spells the editor does not list.
        /// </summary>
        public required bool Hidden { get; init; }
    }
}
