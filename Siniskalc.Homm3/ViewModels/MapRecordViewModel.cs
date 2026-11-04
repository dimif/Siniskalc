namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using Siniskalc.Core.Data;

    /// <summary>
    /// A record of the map whose raw bytes the Hex tab of the map page shows: the stock of a merchant or a creature bank.
    /// </summary>
    /// <param name="Name">What the record is, as the list shows it.</param>
    /// <param name="Create">Makes the target the hex editor works on; the caller disposes it.</param>
    public sealed record MapRecordViewModel(string Name, Func<IByteEditTarget> Create);
}
