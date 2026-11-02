namespace Siniskalc.Homm3.Data;

public sealed record MapMetadataDefinition
{
    /// <summary>
    /// Gets the name of the objects of the map that are merchants.
    /// </summary>
    public required string MerchantName { get; init; }

    /// <summary>
    /// Gets the name of the objects of the map that are dwellings.
    /// </summary>
    public required string DwellingName { get; init; }
}
