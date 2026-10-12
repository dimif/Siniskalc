namespace Siniskalc.ViewModels
{
    using System.Collections.Generic;
    using System.Windows.Input;
    using Siniskalc.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A page that lists artifacts and shows what the one the pointer rests on does, in an info card.
    /// </summary>
    public abstract class ArtifactInfoViewModelBase : PageViewModelBase
    {
        private ArtifactEffectInfo selectedArtifactInfo;
        private Artifact? selectedArtifact;

        protected ArtifactInfoViewModelBase(string title, EffectLineProvider effectLines)
            : base(title)
        {
            this.EffectLines = effectLines;
            this.ShowArtifactInfoCommand = new DelegateCommand(p => this.ShowArtifactInfoByName(p as string));
            this.ClearArtifactInfoCommand = new DelegateCommand(_ => this.ClearArtifactInfo());
        }

        public short Attack => this.selectedArtifactInfo.Atk;

        public short Defense => this.selectedArtifactInfo.Def;

        public short Power => this.selectedArtifactInfo.Pow;

        public short Knowledge => this.selectedArtifactInfo.Kno;

        public short Morale => this.selectedArtifactInfo.Morale;

        public short Luck => this.selectedArtifactInfo.Luck;

        public string Effects => this.selectedArtifactInfo.Other;

        /// <summary>
        /// Gets the class and gold cost of the artifact the pointer is over, or empty when there is none.
        /// </summary>
        public string ClassAndCost => this.selectedArtifact is { Cost: > 0 } a ? $"{a.Class} · {a.Cost:N0} gold" : string.Empty;

        /// <summary>
        /// Gets the numeric effects of the artifact the pointer is over, one line each; empty when it is over none.
        /// </summary>
        public IReadOnlyList<EffectLine> SelectedEffectLines => this.EffectLines.Describe(this.selectedArtifactInfo);

        /// <summary>
        /// Gets whether the pointer is over an artifact that does something, so the info card has anything to show.
        /// </summary>
        public bool HasSelectedInfo => this.SelectedEffectLines.Count > 0 || !string.IsNullOrEmpty(this.Effects) || !string.IsNullOrEmpty(this.ClassAndCost);

        public ICommand ShowArtifactInfoCommand { get; }

        public ICommand ClearArtifactInfoCommand { get; }

        /// <summary>
        /// Gets what words the effects of artifacts.
        /// </summary>
        protected EffectLineProvider EffectLines { get; }

        public void ShowArtifactInfoByName(string? name)
        {
            this.ShowArtifactInfo(string.IsNullOrWhiteSpace(name) ? null : Artifact.FindByName(name));
        }

        public void ClearArtifactInfo()
        {
            this.ShowArtifactInfo(null);
        }

        /// <summary>
        /// Shows what <paramref name="artifact"/> does in the info card; null empties the card.
        /// </summary>
        protected void ShowArtifactInfo(Artifact? artifact)
        {
            this.selectedArtifact = artifact;
            this.selectedArtifactInfo = artifact?.EffectInfo ?? default;
            this.RaisePropertyChanged(nameof(this.Attack));
            this.RaisePropertyChanged(nameof(this.Defense));
            this.RaisePropertyChanged(nameof(this.Power));
            this.RaisePropertyChanged(nameof(this.Knowledge));
            this.RaisePropertyChanged(nameof(this.Morale));
            this.RaisePropertyChanged(nameof(this.Luck));
            this.RaisePropertyChanged(nameof(this.Effects));
            this.RaisePropertyChanged(nameof(this.SelectedEffectLines));
            this.RaisePropertyChanged(nameof(this.ClassAndCost));
            this.RaisePropertyChanged(nameof(this.HasSelectedInfo));
        }
    }
}
