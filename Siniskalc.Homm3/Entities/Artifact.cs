namespace Siniskalc.Homm3.Entities;

using System.Collections.Generic;
using System.Linq;
using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;

public sealed class Artifact : CatalogEntity, IArtifact
{
    private readonly string? combinesInto;

    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <param name="definition">The definition.</param>
    internal Artifact(EntityCatalog catalog, ArtifactDefinition definition)
        : base(catalog, definition)
    {
        this.Slot = definition.Slot;
        this.BlockedSlots = definition.BlockedSlots;
        this.IsCombination = definition.IsCombination;
        this.combinesInto = definition.CombinesInto;
        this.Description = definition.Description;
        this.Class = definition.Class;
        this.Cost = definition.Cost;
        this.Role = definition.Role;
        this.Wearable = definition.Wearable;
        this.CanBeInBackpack = definition.Backpack;
        var e = definition.EffectInfo;
        this.EffectInfo = new ArtifactEffectInfo(
            e.Amount(ArtifactAttribute.Attack),
            e.Amount(ArtifactAttribute.Defense),
            e.Amount(ArtifactAttribute.SpellPower),
            e.Amount(ArtifactAttribute.Knowledge),
            e.Amount(ArtifactAttribute.Luck),
            e.Amount(ArtifactAttribute.Morale),
            e.Amount(ArtifactAttribute.LandMovement),
            e.Amount(ArtifactAttribute.WaterMovement),
            e.SpellGrants.Select(g => new SpellGrantProvider(catalog, g)).ToArray().AsReadOnly(),
            e.GrantsSpellBook,
            e.Amount(ArtifactAttribute.Gold),
            e.Amount(ArtifactAttribute.Ore),
            e.Amount(ArtifactAttribute.Wood),
            e.Amount(ArtifactAttribute.Crystal),
            e.Amount(ArtifactAttribute.Gems),
            e.Amount(ArtifactAttribute.Mercury),
            e.Amount(ArtifactAttribute.Sulfur),
            e.Other);
    }

    /// <summary>
    /// Gets what the artifact adds to the four primary skills.
    /// </summary>
    public PrimarySkillBonus PrimarySkillBonus => new(this.EffectInfo.Atk, this.EffectInfo.Def, this.EffectInfo.Pow, this.EffectInfo.Kno);

    /// <summary>
    /// Gets the slot of the hero the artifact is worn in.
    /// </summary>
    public ArtifactSlots Slot { get; }

    /// <summary>
    /// Gets the slots the artifact takes up besides its own.
    /// </summary>
    public ArtifactSlots BlockedSlots { get; }

    /// <summary>
    /// Gets what the artifact does for its owner.
    /// </summary>
    public ArtifactEffectInfo EffectInfo { get; }

    /// <summary>
    /// Gets the text that says what the artifact does.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets whether the artifact is a combination, put together from other artifacts.
    /// </summary>
    public bool IsCombination { get; }

    /// <summary>
    /// Gets the rarity class of the artifact.
    /// </summary>
    public ArtifactClass Class { get; }

    /// <summary>
    /// Gets the price of the artifact in gold.
    /// </summary>
    public int Cost { get; }

    /// <summary>
    /// Gets what the artifact is for the editor: an ordinary one, the spell book or a scroll.
    /// </summary>
    public ArtifactRole Role { get; }

    /// <summary>
    /// Gets whether a hero can wear the artifact.
    /// </summary>
    public bool Wearable { get; }

    /// <summary>
    /// Gets whether a hero can carry the artifact in the backpack.
    /// </summary>
    public bool CanBeInBackpack { get; }

    /// <summary>
    /// Gets whether the artifact is a spell scroll.
    /// </summary>
    public bool IsScroll => this.Role == ArtifactRole.Scroll;

    /// <summary>
    /// Gets the combination this artifact is a part of, or <see langword="null"/> when it is a part of none.
    /// </summary>
    public Artifact? CombinesInto => this.combinesInto is null ? null : this.Catalog.Artifacts.ById(this.combinesInto);

    /// <summary>
    /// Gets the spells the artifact lets its wearer cast.
    /// </summary>
    /// <param name="version">The version of the game.</param>
    /// <returns>The spells.</returns>
    public IReadOnlyCollection<Spell> GrantedSpells(GameVersion version) => this.EffectInfo.GrantedSpells(version);
}
