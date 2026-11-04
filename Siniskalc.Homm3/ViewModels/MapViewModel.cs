namespace Siniskalc.Homm3.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// What the map keeps that no player owns: merchant stock and creature banks. The mines and dwellings of the map are on the pages of their
    /// owners and on the page of Neutral.
    /// </summary>
    public sealed class MapViewModel : RecordsPageViewModelBase
    {
        private readonly CreatureBankTextProvider bankText;
        private readonly List<CreatureBankKind> bankKinds;
        private CreatureBankViewModel? selectedBank;
        private CreatureBankKind bankKind = CreatureBankKind.DragonUtopia;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapViewModel"/> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="effectLines">The effect lines.</param>
        public MapViewModel(Siniskalc.Homm3.Runtime.Game game, EffectLineProvider effectLines, CreatureBankTextProvider bankText, IByteEditTargetFactory byteEditTargets)
            : base("Map", game.History, Homm3ChunkKinds.Merchants, Homm3ChunkKinds.CreatureBanks)
        {
            this.bankText = bankText;
            var document = game.History.Current;
            this.Merchants = Enumerable.Range(0, document.Merchants().Count).Select(i => new MerchantViewModel(game.History.CreateMerchantSession(i), document.Catalog(), effectLines, LocationOf(game, i))).ToList();
            var models = document.CreatureBanks().GetModels();
            this.Banks = Enumerable.Range(0, models.Count).Select(i =>
                new CreatureBankViewModel(game.History.CreateCreatureBankSession(i), document.Catalog(), effectLines, bankText, models.Take(i + 1).Count(m => m.Kind == models[i].Kind))).ToList();
            this.bankKinds = this.Banks.Select(b => b.Kind).Distinct().OrderBy(k => k).ToList();
            if (this.bankKinds.Count > 0)
            {
                this.bankKind = this.bankKinds[0];
            }

            this.VisibleBanks = this.Banks.Where(b => b.Kind == this.bankKind).ToList();
            this.ByteRecords = new ByteRecordListViewModel(this.Merchants.Select((m, i) => new ByteRecordViewModel(m.Name, () => byteEditTargets.ForMerchant(game.History, i)))
                .Concat(this.Banks.Select(b => new ByteRecordViewModel(b.Name, () => byteEditTargets.ForCreatureBank(game.History, b.Index))))
                .ToList());
            this.Refresh();
        }

        /// <summary>
        /// Gets the records of the map whose raw bytes the Hex tab can show (the merchants and then the creature banks), and the one it shows.
        /// </summary>
        public ByteRecordListViewModel ByteRecords { get; }

        /// <summary>
        /// Gets whether the map has a creature bank that can be edited.
        /// </summary>
        public bool HasBanks => this.Banks.Count > 0;

        /// <summary>
        /// Gets the names of the bank types the map has, in the order of the navigation tree.
        /// </summary>
        public IReadOnlyList<string> BankTypes => this.bankKinds.Select(this.bankText.GroupName).ToList();

        /// <summary>
        /// Gets or sets the place in <see cref="BankTypes"/> of the bank type whose banks are shown.
        /// </summary>
        public int SelectedBankType
        {
            get => this.bankKinds.IndexOf(this.bankKind);
            set
            {
                if (value >= 0 && value < this.bankKinds.Count && this.bankKinds[value] != this.bankKind)
                {
                    this.ShowSection(MapSections.ForBank(this.bankKinds[value]));
                }
            }
        }

        /// <summary>
        /// Gets what the merchants have in store: each of the black markets of the map.
        /// </summary>
        public IReadOnlyList<MerchantViewModel> Merchants { get; }

        /// <summary>
        /// Gets whether the map has a merchant.
        /// </summary>
        public bool HasMerchants => this.Merchants.Count > 0;

        public IReadOnlyList<CreatureBankViewModel> Banks { get; }

        public IReadOnlyList<CreatureBankViewModel> VisibleBanks { get; private set; }

        public string BankGroupName => this.bankText.GroupName(this.bankKind);

        public int BankSection => MapSections.ForBank(this.bankKind);

        public bool HasVisibleBanks => this.VisibleBanks.Count > 0;

        public CreatureBankViewModel? SelectedBank
        {
            get => this.selectedBank;
            set
            {
                if (this.selectedBank != value)
                {
                    this.selectedBank = value;
                    if (value != null)
                    {
                        // The Hex tab follows the bank that is picked.
                        this.ByteRecords.SelectedRecord = this.ByteRecords.Records[this.Merchants.Count + value.Index];
                    }

                    this.RaisePropertyChanged(nameof(this.SelectedBank));
                }
            }
        }

        public string BankSummary => this.bankText.Summary(this.BankGroupName, this.VisibleBanks.Count, this.VisibleBanks.Count(u => u.IsCleared));

        /// <summary>
        /// Gets what the header says: how many merchants the map has.
        /// </summary>
        public string MapSummary => this.bankText.MapSummary(this.Merchants.Count, this.Banks.Count);

        public void ShowSection(int section, int? bankIndex = null)
        {
            if (MapSections.BankKind(section) is { } kind)
            {
                this.SelectedBank = null;
                this.bankKind = kind;
                this.VisibleBanks = this.Banks.Where(b => b.Kind == kind).ToList();
                this.RaiseAllPropertiesChanged();
                this.SelectedTab = MapSections.BankDetails;
            }
            else
            {
                this.SelectedTab = section;
            }

            this.SelectedBank = bankIndex is { } index ? this.VisibleBanks.Single(u => u.Index == index) : null;
        }

        /// <inheritdoc/>
        protected override void RefreshRecords()
        {
            foreach (var merchant in this.Merchants)
            {
                merchant.Refresh();
            }

            foreach (var bank in this.Banks)
            {
                bank.Refresh();
            }
        }

        private static string LocationOf(Siniskalc.Homm3.Runtime.Game game, int merchant) =>
            game.Places.Merchants.Where(m => m.Number == merchant).Select(m => $"At {MapLevelText.Place(m.Position.X, m.Position.Y)}").FirstOrDefault() ?? string.Empty;
    }
}
