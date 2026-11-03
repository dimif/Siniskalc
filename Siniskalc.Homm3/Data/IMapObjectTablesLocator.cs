namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Homm3.Entities;

    /// <summary>Locates complete counted map-object tables without depending on their editable subsets.</summary>
    public interface IMapObjectTablesLocator
    {
        /// <summary>Returns validated table boundaries, or null when none or competing tables are found.</summary>
        MapObjectTables? Locate(byte[] bytes, GameVersion version);
    }
}
