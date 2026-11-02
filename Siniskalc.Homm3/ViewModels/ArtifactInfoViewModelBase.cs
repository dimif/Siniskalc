namespace Siniskalc.Homm3.ViewModels
{
    using System.Collections.Generic;
    using System.Windows.Input;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A page that lists artifacts and shows what the one the pointer rests on does, in an info card.
    /// </summary>
    public abstract class ArtifactInfoViewModelBase : PageViewModelBase, IArtifactInfoSource
    {
        private ArtifactEffectInfo selectedArtifactInfo;
        private Artifact? selectedArtifact;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactInfoViewModelBase"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="effectLines">The effect lines.</param>
        /// <param name="catalog">The catalog.</param>
        protected ArtifactInfoViewModelBase(string title, EffectLineProvider effectLines, EntityCatalog catalog)
            : base(title)
        {
            this.EffectLines = effectLines;
            this.Catalog = catalog;
            this.ShowArtifactInfoCommand = new DelegateCommand(p => this.ShowArtifactInfoByName(p as string));
            this.ClearArtifactInfoCommand = new DelegateCommand(_ => this.ClearArtifactInfo());
        }

        /// <summary>
        /// Gets the attack the selected artifact adds.
        /// </summary>
        public short Attack => this.selectedArtifactInfo.Atk;

        /// <summary>
        /// Gets the defense the selected artifact adds.
        /// </summary>
        public short Defense => this.selectedArtifactInfo.Def;

        /// <summary>
        /// Gets the spell power the selected artifact adds.
        /// </summary>
        public short Power => this.selectedArtifactInfo.Pow;

        /// <summary>
        /// Gets the knowledge the selected artifact adds.
        /// </summary>
        public short Knowledge => this.selectedArtifactInfo.Kno;

        /// <summary>
        /// Gets the morale the selected artifact adds.
        /// </summary>
        public short Morale => this.selectedArtifactInfo.Morale;

        /// <summary>
        /// Gets the luck the selected artifact adds.
        /// </summary>
        public short Luck => this.selectedArtifactInfo.Luck;

        /// <summary>
        /// Gets the text of the other things the selected artifact does.
        /// </summary>
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

        /// <summary>
        /// Gets the command that shows the card of an artifact; its parameter is the name of the artifact.
        /// </summary>
        public ICommand ShowArtifactInfoCommand { get; }

        /// <summary>
        /// Gets the command that hides the card.
        /// </summary>
        public ICommand ClearArtifactInfoCommand { get; }

        /// <summary>
        /// Gets what words the effects of artifacts.
        /// </summary>
        protected EffectLineProvider EffectLines { get; }

        /// <summary>
        /// Gets the catalog of the artifacts.
        /// </summary>
        protected EntityCatalog Catalog { get; }

        /// <summary>
        /// Shows the card of an artifact.
        /// </summary>
        /// <param name="name">The name of the artifact; <see langword="null"/> or blank hides the card.</param>
        public void ShowArtifactInfoByName(string? name)
        {
            this.ShowArtifactInfo(string.IsNullOrWhiteSpace(name) ? null : this.Catalog.FindArtifact(name));
        }

        /// <summary>
        /// Hides the card of the artifact.
        /// </summary>
        public void ClearArtifactInfo()
        {
            this.ShowArtifactInfo(null);
        }

        /// <summary>
        /// Shows what <paramref name="artifact"/> does in the info card; null empties the card.
        /// </summary>
        /// <param name="artifact">The artifact, or <see langword="null"/> to hide the card.</param>
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
