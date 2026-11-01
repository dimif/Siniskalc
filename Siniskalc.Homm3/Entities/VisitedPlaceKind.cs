namespace Siniskalc.Homm3.Entities;

using Siniskalc.Homm3.Data;

public sealed class VisitedPlaceKind : CatalogEntity
{
    internal VisitedPlaceKind(EntityCatalog catalog, VisitedPlaceDefinition definition) : base(catalog, definition)
    {
        this.ObjectType = definition.ObjectType;
        this.Slot = definition.Slot;
        this.ToolTip = definition.ToolTip;
        this.NumberMask = definition.NumberMask;
    }

    public int? ObjectType { get; }
    public VisitSlot Slot { get; }
    public string ToolTip { get; }
    public byte NumberMask { get; }
    public bool CellHasFlags => this.NumberMask != byte.MaxValue;
    public bool IsHotaOnly => this.Slot is VisitSlot.HotaExperience or VisitSlot.HotaPrimarySkills;
    public int Index => (int)this.Slot;
}
