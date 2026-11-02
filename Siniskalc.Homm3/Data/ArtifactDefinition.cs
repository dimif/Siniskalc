namespace Siniskalc.Homm3.Data;

using Siniskalc.Homm3.Entities;

public sealed record ArtifactDefinition : EntityDefinition
{
    /// <summary>
    /// Gets the slot of the hero the artifact is worn in.
    /// </summary>
    public required ArtifactSlots Slot { get; init; }

    /// <summary>
    /// Gets the slots the artifact takes up besides its own.
    /// </summary>
    public required ArtifactSlots BlockedSlots { get; init; }

    /// <summary>
    /// Gets what the artifact does for its owner.
    /// </summary>
    public required ArtifactEffectDefinition EffectInfo { get; init; }

    /// <summary>
    /// Gets the text that says what the artifact does.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets whether the artifact is a combination, put together from other artifacts.
    /// </summary>
    public required bool IsCombination { get; init; }

    /// <summary>
    /// Gets the rarity class of the artifact: special, treasure, minor, major or relic.
    /// </summary>
    public required ArtifactClass Class { get; init; }

    /// <summary>
    /// Gets the price of the artifact in gold.
    /// </summary>
    public required int Cost { get; init; }

    /// <summary>
    /// Gets the identifier of the combination this artifact is a part of, or <see langword="null"/> when it is a part of none.
    /// </summary>
    public required string? CombinesInto { get; init; }

    /// <summary>
    /// Gets what the artifact is for the editor: an ordinary one, the spell book or a scroll.
    /// </summary>
    public ArtifactRole Role { get; init; }

    /// <summary>
    /// Gets whether a hero can wear the artifact.
    /// </summary>
    public bool Wearable { get; init; } = true;

    /// <summary>
    /// Gets whether a hero can carry the artifact in the backpack.
    /// </summary>
    public bool Backpack { get; init; } = true;
}
