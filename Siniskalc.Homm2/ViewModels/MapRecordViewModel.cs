namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using Siniskalc.Core.Data;

    /// <summary>
    /// A record of the map whose raw bytes the Bytes tab of the map page shows: a mine, a shrine, a sphinx, whose turn it is, the castles that
    /// built today or the obelisks.
    /// </summary>
    /// <param name="Name">What the record is, as the list shows it.</param>
    /// <param name="Create">Makes the target the hex editor works on; the caller disposes it.</param>
    public sealed record MapRecordViewModel(string Name, Func<IByteEditTarget> Create);
}
