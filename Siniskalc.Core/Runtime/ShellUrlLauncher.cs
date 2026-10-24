namespace Siniskalc.Core.Runtime
{
    using System;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Opens a web address with the program the operating system has for it.
    /// </summary>
    public sealed class ShellUrlLauncher : IUrlLauncher
    {
        private static readonly ILogger<ShellUrlLauncher> Logger = LogFactory.For<ShellUrlLauncher>();

        private readonly Action<string> start;

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
                return false;
            }

            try
            {
                this.start(uri.AbsoluteUri);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Could not open the address {Url}", url);
                return false;
            }
        }
    }
}
