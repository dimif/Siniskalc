namespace Siniskalc.Core.Models
{
    /// <summary>
    /// The part of a struct (a hero, a player, a castle) that a <see cref="Change"/> belongs to. A game defines its areas as values of this type.
    /// </summary>
    /// <param name="DisplayName">The words for the area, e.g. "Primary skills".</param>
    public sealed record ChangeArea(string DisplayName)
    {
        /// <inheritdoc/>
        public override string ToString() => this.DisplayName;
    }
}
