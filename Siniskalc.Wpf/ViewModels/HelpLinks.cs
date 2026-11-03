namespace Siniskalc.Wpf.ViewModels
{
    /// <summary>
    /// Where the Help menu of an application leads.
    /// </summary>
    /// <param name="UserGuideUrl">The user guide on the web.</param>
    /// <param name="IssuesUrl">Where problems are reported.</param>
    /// <param name="LogFolder">The folder the log files are written to, or <c>null</c> when the application does not offer to show it.</param>
    public sealed record HelpLinks(string UserGuideUrl, string IssuesUrl, string? LogFolder = null);
}
