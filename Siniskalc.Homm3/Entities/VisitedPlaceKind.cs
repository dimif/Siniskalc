namespace Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Data;

public sealed class VisitedPlaceKind : CatalogEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <param name="definition">The definition.</param>
    internal VisitedPlaceKind(EntityCatalog catalog, VisitablePlaceDefinition definition)
        : base(catalog, definition)
    {
        this.ObjectType = definition.ObjectType;
        this.Slot = definition.Slot;
        this.ToolTip = definition.Description;
        this.NumberMask = definition.NumberMask;
    }

    /// <summary>
    /// Gets the number of the type of the object of the map that is the place, or <see langword="null"/> when the map does not list it.
    /// </summary>
    public int? ObjectType { get; }

    /// <summary>
    /// Gets where the visit of the place is kept in the record of a hero.
    /// </summary>
    public VisitSlot Slot { get; }

    /// <summary>
    /// Gets what the place gives.
    /// </summary>
    public string ToolTip { get; }

    /// <summary>
    /// Gets the bits of the number of an object that tell which place of the kind it is.
    /// </summary>
    public byte NumberMask { get; }

    /// <summary>
    /// Gets whether the number of an object says which place of the kind it is.
    /// </summary>
    public bool CellHasFlags => this.NumberMask != byte.MaxValue;

    /// <summary>
    /// Gets whether only Horn of the Abyss has the place.
    /// </summary>
    public bool IsHotaOnly => this.Slot is VisitSlot.HotaExperience or VisitSlot.HotaPrimarySkills;

    /// <summary>
    /// Gets the number of the kind, which is the number of its slot.
    /// </summary>
    public int Index => (int)this.Slot;
}
