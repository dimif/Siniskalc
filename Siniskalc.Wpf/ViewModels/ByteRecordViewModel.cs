namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using Siniskalc.Core.Data;

    /// <summary>
    /// A record of a savefile whose raw bytes a Hex tab shows, such as a mine, a shrine, a merchant or a creature bank.
    /// </summary>
    /// <param name="Name">What the record is, as the list shows it.</param>
    /// <param name="Create">Makes the target the hex editor works on; the caller disposes it.</param>
    public sealed record ByteRecordViewModel(string Name, Func<IByteEditTarget> Create);
}
