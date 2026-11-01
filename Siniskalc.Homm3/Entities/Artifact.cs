namespace Siniskalc.Homm3.Entities;

using System.Collections.Generic;
using System.Linq;
using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;

public sealed class Artifact : CatalogEntity, IArtifact
{
    private readonly string? combinesInto;

    internal Artifact(EntityCatalog catalog, ArtifactDefinition definition) : base(catalog, definition)
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
        this.EffectInfo = new ArtifactEffectInfo(e.Atk, e.Def, e.Pow, e.Kno, e.Luck, e.Morale, e.LandMovement, e.WaterMovement,
            e.SpellGrants.Select(g => new SpellGrantProvider(catalog, g)).ToArray().AsReadOnly(), e.GrantsSpellBook,
            e.Gold, e.Ore, e.Wood, e.Crystal, e.Gems, e.Mercury, e.Sulfur, e.Other);
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
