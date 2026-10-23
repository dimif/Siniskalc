namespace Siniskalc.Core.Runtime
{
    using System;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Opens a web address with the program the operating system has for it.
    /// </summary>
    public sealed class ShellUrlLauncher : IUrlLauncher
    {
        private readonly Action<string> start;
        private readonly ILogger<ShellUrlLauncher> logger;

        public ShellUrlLauncher(Action<string> start, ILogger<ShellUrlLauncher> logger)
        {
            this.start = start;
            this.logger = logger;
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
                this.logger.LogWarning(ex, "Could not open the address {Url}", url);
                return false;
            }
        }
    }
}
