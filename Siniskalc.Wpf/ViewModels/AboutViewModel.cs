namespace Siniskalc.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Windows.Input;
    using Siniskalc.Core.Runtime;

    public sealed class AboutViewModel : AbstractViewModelBase
    {
        private readonly AboutInfo info;
        private readonly IUrlLauncher launcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutViewModel"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="launcher">The launcher.</param>
        public AboutViewModel(AboutInfo info, IUrlLauncher launcher)
        {
            this.info = info;
            this.launcher = launcher;
            this.OpenRepositoryCommand = new DelegateCommand(_ => this.launcher.TryOpen(this.info.RepositoryUrl));
            this.OpenReleasesCommand = new DelegateCommand(_ => this.launcher.TryOpen(this.info.ReleasesUrl));
        }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        public string Name => this.info.Name;

        /// <summary>
        /// Gets the version of the application as the About window shows it, such as "Version 1.2.0".
        /// </summary>
        public string VersionText => $"Version {this.info.Version}";

        /// <summary>
        /// Gets the sentence that says what the application is for.
        /// </summary>
        public string Description => this.info.Description;

        /// <summary>
        /// Gets the address of the project on the web.
        /// </summary>
        public string RepositoryUrl => this.info.RepositoryUrl;

        /// <summary>
        /// Gets the copyright and license line.
        /// </summary>
        public string License => this.info.License;

        /// <summary>
        /// Gets the notices the window lists below the license, such as trademarks and third-party credits.
        /// </summary>
        public IReadOnlyList<string> Notices => this.info.Notices;

        /// <summary>
        /// Gets the command that opens the project page in the browser.
        /// </summary>
        public ICommand OpenRepositoryCommand { get; }

        /// <summary>
        /// Gets the command that opens the page of the releases in the browser.
        /// </summary>
        public ICommand OpenReleasesCommand { get; }
    }
}
