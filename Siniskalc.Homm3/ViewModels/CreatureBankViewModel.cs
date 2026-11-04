namespace Siniskalc.Homm3.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>Edits any supported bank through a model session. The Map page refreshes it with the shared history.</summary>
    public sealed class CreatureBankViewModel : AbstractViewModelBase
    {
        private readonly IEditSession<CreatureBankModel> session;
        private readonly CreatureBankTextProvider text;
        private readonly EntityCatalog catalog;
        private readonly int number;
        private bool upgradedCenter;

        public CreatureBankViewModel(IEditSession<CreatureBankModel> session, EntityCatalog catalog, EffectLineProvider effectLines, CreatureBankTextProvider text, int number)
        {
            this.session = session;
            this.text = text;
            this.catalog = catalog;
            this.number = number;
            this.upgradedCenter = session.Original.UpgradedCenter;
            this.StrengthChoices = Enumerable.Range(Constants.CreatureBank.MinStrength, Constants.CreatureBank.MaxStrength).Select(strength => text.Preset(this.Kind, strength, catalog)).ToList();
            this.RestoreCommand = new DelegateCommand(_ => this.Restore(), _ => this.IsCleared);
            this.ResetCommand = new DelegateCommand(_ => this.session.Update(_ => this.session.Original), _ => this.HasPendingChanges);
            this.Artifacts = Enumerable.Range(0, session.Current.Artifacts.Count).Select(slot =>
                new ArtifactSlotViewModel(
                    text.ArtifactSlot(slot),
                    onChoose => new ArtifactFinderViewModel(catalog, effectLines, string.Empty, "Choose", onChoose, null, includeScrolls: false),
                    name => this.session.Update(m => m.WithArtifact(slot, name == null ? null : catalog.FindArtifact(name))))).ToList();
            this.Refresh();
        }

        public string Name => this.text.Name(this.Kind, this.number);

        public int Index => this.session.Index;

        public CreatureBankKind Kind => this.session.Current.Kind;

        public string Heading => this.text.Heading(this.session.Current, this.number);

        public string Status => this.text.Status(this.IsCleared);

        public int Tier => this.session.Current.Strength;

        public string Location => this.text.Location(this.Kind, this.session.Current.Position);

        public string RewardSummary => this.text.RewardSummary(this.session.Current);

        public IReadOnlyList<string> DefeatChoices => this.text.DefeatChoices;

        public int SelectedDefeat
        {
            get => this.IsCleared ? 1 : 0;
            set
            {
                if (value == 0)
                {
                    this.Restore();
                }
                else
                {
                    this.session.Update(m => m with { IsCleared = true });
                }
            }
        }

        public IReadOnlyList<string> Garrison => this.text.Garrison(this.Kind, this.Tier, this.UpgradedCenter, this.catalog);

        public string ResourceReward => this.text.Resources(this.Kind, this.Tier);

        public bool HasCenterUpgrade => this.Kind != CreatureBankKind.DragonUtopia;

        public bool HasUnknownOriginalGuards => this.HasCenterUpgrade && this.session.Original.IsCleared;

        public bool UpgradedCenter
        {
            get => this.upgradedCenter;
            set
            {
                this.upgradedCenter = value;
                if (!this.IsCleared)
                {
                    this.session.Update(m => m with { UpgradedCenter = value });
                }

                this.RaisePropertyChanged(nameof(this.UpgradedCenter));
                this.RaisePropertyChanged(nameof(this.Garrison));
            }
        }

        public bool IsCleared => this.session.Current.IsCleared;

        public bool HasPendingChanges => this.session.IsDirty;

        public IReadOnlyList<string> StrengthChoices { get; }

        public int SelectedStrength
        {
            get => this.session.Current.Strength - Constants.CreatureBank.MinStrength;
            set => this.session.Update(m => m with { Strength = value + Constants.CreatureBank.MinStrength });
        }

        public IReadOnlyList<ArtifactSlotViewModel> Artifacts { get; }

        public ICommand RestoreCommand { get; }

        public ICommand ResetCommand { get; }

        public void Refresh()
        {
            if (!this.IsCleared)
            {
                this.upgradedCenter = this.session.Current.UpgradedCenter;
            }

            for (var slot = 0; slot < this.Artifacts.Count; slot++)
            {
                this.Artifacts[slot].UpdateFromModel(this.session.Current.Artifacts[slot]?.Name);
            }

            this.RaiseAllPropertiesChanged();
            CommandManager.InvalidateRequerySuggested();
        }

        private void Restore() => this.session.Update(m => m with { IsCleared = false, UpgradedCenter = this.UpgradedCenter });
    }
}
