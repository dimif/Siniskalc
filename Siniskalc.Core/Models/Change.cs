namespace Siniskalc.Core.Models
{
    /// <summary>
    /// One difference between a hero or player as loaded and as currently edited, described as text.
    /// </summary>
    /// <param name="Area">The part it belongs to, e.g. "Creatures".</param>
    /// <param name="Description">What changed, e.g. "Slot 2".</param>
    /// <param name="Before">The loaded value, as text.</param>
    /// <param name="After">The current value, as text.</param>
    public record Change(ChangeArea Area, string Description, string Before, string After);
}
