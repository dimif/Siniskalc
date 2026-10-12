namespace Siniskalc.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Windows.Input;
    using Siniskalc.Core.Runtime;

    public sealed class AboutViewModel : AbstractViewModelBase
    {
        private readonly AboutInfo info;
        private readonly IUrlLauncher launcher;

        public AboutViewModel(AboutInfo info, IUrlLauncher launcher)
        {
            this.info = info;
            this.launcher = launcher;
            this.OpenRepositoryCommand = new DelegateCommand(_ => this.launcher.TryOpen(this.info.RepositoryUrl));
            this.OpenReleasesCommand = new DelegateCommand(_ => this.launcher.TryOpen(this.info.ReleasesUrl));
        }

        public string Name => this.info.Name;

        public string VersionText => $"Version {this.info.Version}";

        public string Description => this.info.Description;

        public string RepositoryUrl => this.info.RepositoryUrl;

        public string License => this.info.License;

        public IReadOnlyList<string> Notices => this.info.Notices;

        public ICommand OpenRepositoryCommand { get; }

        public ICommand OpenReleasesCommand { get; }
    }
}
