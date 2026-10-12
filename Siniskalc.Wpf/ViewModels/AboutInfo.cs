namespace Siniskalc.Wpf.ViewModels
{
    using System.Collections.Generic;

    /// <summary>
    /// What the About window says about an application.
    /// </summary>
    /// <param name="Name">The name of the application.</param>
    /// <param name="Version">The version of the application.</param>
    /// <param name="Description">One line about what it does.</param>
    /// <param name="RepositoryUrl">The home of the project on the web.</param>
    /// <param name="ReleasesUrl">Where new versions are published.</param>
    /// <param name="License">The license of the application, in one line.</param>
    /// <param name="Notices">Disclaimers and third-party notices, one paragraph per entry.</param>
    public sealed record AboutInfo(
        string Name,
        string Version,
        string Description,
        string RepositoryUrl,
        string ReleasesUrl,
        string License,
        IReadOnlyList<string> Notices);
}
