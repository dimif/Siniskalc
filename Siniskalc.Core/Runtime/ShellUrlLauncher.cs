namespace Siniskalc.Core.Runtime
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Opens a web address with the program the operating system has for it.
    /// </summary>
    public sealed class ShellUrlLauncher : IUrlLauncher
    {
        private static readonly ILogger<ShellUrlLauncher> Logger = LogFactory.For<ShellUrlLauncher>();

        private readonly Action<string> start;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellUrlLauncher"/> class.
        /// </summary>
        /// <param name="start">Hands an address or a folder path to the operating system.</param>
        public ShellUrlLauncher(Action<string> start)
        {
            this.start = start;
        }

        /// <inheritdoc/>
        public bool TryOpen(string url)
        {
            // Only web addresses: the shell would also run a file path or any other scheme it is given.
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            {
                Logger.LogDebug("Did not open {Url}: it is no http or https address", url);
                return false;
            }

            try
            {
                this.start(uri.AbsoluteUri);
                Logger.LogInformation("Opened the address {Url}", uri.AbsoluteUri);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Could not open the address {Url}", url);
                return false;
            }
        }

        /// <inheritdoc/>
        public bool TryOpenFolder(string folder)
        {
            // Only a folder that is there: the shell would also run a file or a program it is given.
            if (!Directory.Exists(folder))
            {
                Logger.LogWarning("Could not open the folder {Folder}: it does not exist", folder);
                return false;
            }

            try
            {
                this.start(Path.GetFullPath(folder));
                Logger.LogInformation("Opened the folder {Folder}", folder);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Could not open the folder {Folder}", folder);
                return false;
            }
        }
    }
}
