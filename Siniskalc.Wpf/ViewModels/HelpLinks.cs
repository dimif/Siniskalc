namespace Siniskalc.Wpf.ViewModels
{
    /// <summary>
    /// Where the Help menu of an application leads.
    /// </summary>
    /// <param name="UserGuideUrl">The user guide on the web.</param>
    /// <param name="IssuesUrl">Where problems are reported.</param>
    public sealed record HelpLinks(string UserGuideUrl, string IssuesUrl);
}
