namespace Siniskalc.Homm3.Entities;

using System.Collections.Generic;
using System.Linq;
using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;

public sealed class Artifact : CatalogEntity, IArtifact
{
    private readonly string? combinesInto;

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

    public PrimarySkillBonus PrimarySkillBonus => new(this.EffectInfo.Atk, this.EffectInfo.Def, this.EffectInfo.Pow, this.EffectInfo.Kno);

    public ArtifactSlots Slot { get; }

    public ArtifactSlots BlockedSlots { get; }

    public ArtifactEffectInfo EffectInfo { get; }

    public string Description { get; }

    public bool IsCombination { get; }

    public ArtifactClass Class { get; }

    public int Cost { get; }

    public ArtifactRole Role { get; }

    public bool Wearable { get; }

    public bool CanBeInBackpack { get; }

    public bool IsScroll => this.Role == ArtifactRole.Scroll;

    public Artifact? CombinesInto => this.combinesInto is null ? null : this.Catalog.Artifacts.ById(this.combinesInto);

    public IReadOnlyCollection<Spell> GrantedSpells(GameVersion version) => this.EffectInfo.GrantedSpells(version);
}
