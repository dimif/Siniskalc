namespace Siniskalc.Core.Data
{
    using System;

    /// <summary>
    /// Tells that the history of a savefile changed, which is all a page needs to know to show its record again.
    /// </summary>
    public interface IHistoryEvents
    {
        /// <summary>
        /// Raised whenever the current or the original document of the history changes, whichever record the change was made to.
        /// </summary>
        event EventHandler? Changed;
    }
}
