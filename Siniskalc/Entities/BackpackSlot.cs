namespace Siniskalc.Entities
{
    /// <summary>
    /// Represents a slot in the backpack.
    /// </summary>
    /// <param name="Slot">The zero-based slot.</param>
    /// <param name="Art">The artifact in the slot.</param>
    /// <param name="Spell">The spell a Spell Scroll holds; null for other artifacts, and for a scroll whose spell is not known.</param>
    public record BackpackSlot(int Slot, Artifact Art, Spell? Spell = null);
}
