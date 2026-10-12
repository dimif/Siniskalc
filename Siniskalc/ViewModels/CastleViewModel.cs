namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Data;
    using Siniskalc.Entities;
    using Siniskalc.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// ViewModel of the page of a castle (a town): its owner, position, garrison and buildings. It reads from a
    /// <see cref="CastleEditSession"/>, writes every edit back through it, and refreshes when the history changes, so edits show up in
    /// the pending changes and follow undo and redo.
    /// </summary>
    public sealed class CastleViewModel : PageViewModelBase, IDisposable
    {
        private static readonly DelegateCommand NoCommand = new(_ => { }, _ => false);

        private readonly SaveDocumentHistory history;
        private readonly CastleEditSession session;
        private readonly IByteEditTargetFactory byteEditTargets;
        private readonly CastleBuildMessageProvider buildMessages;
        private readonly int castleIndex;
        private readonly Action<int>? openHero;
        private string notice = string.Empty;

        public CastleViewModel(SaveDocumentHistory history, int castleIndex, IByteEditTargetFactory byteEditTargets, CastleBuildMessageProvider buildMessages, Action<int>? openHero = null)
            : base(history.Current.Castles().GetName(castleIndex))
        {
            this.history = history;
            this.byteEditTargets = byteEditTargets;
            this.buildMessages = buildMessages;
            this.session = history.CreateCastleSession(castleIndex);
            this.castleIndex = castleIndex;
            this.openHero = openHero;
            this.Castle = this.session.Current;
            this.ResetCommand = new DelegateCommand(_ => this.session.Update(_ => this.Original), _ => this.HasPendingChanges);
            this.Refresh();

            // The garrison is an army like a hero's, so it is edited with the same view model. While a hero is garrisoned its army takes
            // the place of the castle's: the editor shows that army and cannot change anything.
            this.GarrisonEditor = new ArmyViewModel(
                history.Current.GameVersion(),
                () => this.GarrisonedHeroModel?.Army ?? this.Castle.Garrison,
                edit => this.Edit(castle => castle.WithGarrison(edit(castle.Garrison))),
                () => this.HasGarrisonedHero);

            this.history.Changed += this.OnHistoryChanged;
        }

        public int CastleIndex => this.castleIndex;

        public string Name => this.Castle.Name;

        /// <summary>
        /// Gets the name of the faction, or "Unknown faction".
        /// </summary>
        public string FactionText => this.Castle.Faction?.Name ?? "Unknown faction";

        /// <summary>
        /// Gets a line such as "Mage Guild level 4 · built this turn".
        /// </summary>
        public string StatusText => string.Join(" · ", new[]
        {
            this.Castle.MageGuildLevel == 0 ? "No Mage Guild" : $"Mage Guild level {this.Castle.MageGuildLevel}",
            this.Castle.BuiltThisTurn ? "built this turn" : "nothing built this turn",
        });

        public string OwnerText => OwnerNames.Of(this.Castle.Owner);

        /// <summary>
        /// Gets the owners to pick from: "Neutral" and the eight player colours.
        /// </summary>
        public IReadOnlyList<string> OwnerChoices { get; } = OwnerNames.Choices;

        /// <summary>
        /// Gets or sets the owner, as "Neutral" or the name of a player colour.
        /// </summary>
        public string SelectedOwner
        {
            get => this.OwnerText;
            set
            {
                var owner = OwnerNames.Parse(value);
                this.Edit(castle => castle with { Owner = owner });
            }
        }

        /// <summary>
        /// Gets or sets whether something was built in the castle this turn. A plain flag: building and removing buildings
        /// neither change it nor depend on it.
        /// </summary>
        public bool BuiltThisTurn
        {
            get => this.Castle.BuiltThisTurn;
            set => this.Edit(castle => castle with { BuiltThisTurn = value });
        }

        /// <summary>
        /// Gets or sets whether a building can still be built in the castle today; the opposite of <see cref="BuiltThisTurn"/>.
        /// </summary>
        public bool CanBuildThisTurn
        {
            get => !this.Castle.BuiltThisTurn;
            set => this.BuiltThisTurn = !value;
        }

        public string ColorHex => PlayerPalette.Of(this.Castle.Owner);

        /// <summary>
        /// Gets where the town is on the map, such as "X: 33, Y: 23, Surface (0)".
        /// </summary>
        public string PositionText => $"{MapLevelText.Coordinates(this.Castle.Position.X, this.Castle.Position.Y, this.Castle.Position.Z)}";

        public string Garrisoned => this.HasGarrisonedHero ? $"Garrisoned by {this.GarrisonedHeroName}"
            : this.Castle.Garrison.Slots.Length == 0 ? "No garrison" : $"{this.Castle.Garrison.Slots.Length} of {Constants.CreatureSlotCount} slots in use";

        /// <summary>
        /// Gets whether a hero is garrisoned in the castle. Its army then takes the place of the castle's own creatures.
        /// </summary>
        public bool HasGarrisonedHero => this.Castle.GarrisonedHeroId != null;

        /// <summary>
        /// Opens the page of the garrisoned hero.
        /// </summary>
        public ICommand OpenGarrisonedHeroCommand => this.GarrisonedHeroRow?.OpenCommand ?? NoCommand;

        /// <summary>
        /// Gets the name of the garrisoned hero, shown as a link on the garrison tab.
        /// </summary>
        public string GarrisonedHeroName => this.GarrisonedHeroRow?.Name ?? $"Unknown hero {this.Castle.GarrisonedHeroId}";

        public IReadOnlyList<GarrisonSlotRowViewModel> Garrison { get; private set; } = [];

        /// <summary>
        /// Gets the editor of the garrison: its seven slots and a finder to add creatures.
        /// </summary>
        public ArmyViewModel GarrisonEditor { get; }

        /// <summary>
        /// Gets the heroes in the castle: the garrisoned one and the one at the gate, if there are any.
        /// </summary>
        public IReadOnlyList<CastleHeroRowViewModel> Heroes { get; private set; } = [];

        public bool HasHeroes => this.Heroes.Count > 0;

        public IReadOnlyList<BuildingGroupViewModel> BuildingGroups { get; private set; } = [];

        /// <summary>
        /// Gets the seven creature levels with the dwelling that serves each and the creatures waiting in it.
        /// </summary>
        public IReadOnlyList<DwellingRowViewModel> Dwellings { get; private set; } = [];

        /// <summary>
        /// Gets a line such as "123 Total".
        /// </summary>
        public string DwellingsSummary { get; private set; } = string.Empty;

        public IReadOnlyList<MageGuildLevelRowViewModel> MageGuildLevels { get; private set; } = [];

        /// <summary>
        /// Gets the levels the Mage Guild can have in this castle, from 0 (none) up.
        /// </summary>
        public IReadOnlyList<int> MageGuildLevelChoices { get; private set; } = [0];

        /// <summary>
        /// Gets or sets the level of the Mage Guild. Setting it builds the levels up to it, or removes the ones above.
        /// </summary>
        public int SelectedMageGuildLevel
        {
            get => this.Castle.MageGuildLevel;
            set => this.Edit(castle => CastleBuilder.SetMageGuildLevel(castle, value));
        }

        /// <summary>
        /// Gets a line such as "Level 4 of 5".
        /// </summary>
        public string MageGuildSummary { get; private set; } = string.Empty;

        /// <summary>
        /// Gets where the boats of the castle appear, or that the castle is not on the coast.
        /// </summary>
        public string ShipyardText => this.Castle.ShipyardLocation is { } tile ? $"Boats appear at X {tile.X}, Y {tile.Y}" : "Not on the coast, so no boats";

        /// <summary>
        /// Gets a line about the buildings, such as "9 of 40 buildings built".
        /// </summary>
        public string BuildingsSummary { get; private set; } = string.Empty;

        /// <summary>
        /// Gets why the last edit was refused, such as "Fort: Needed by Citadel.", or nothing. Cleared by the next edit that works.
        /// </summary>
        public string Notice
        {
            get => this.notice;
            private set
            {
                this.notice = value;
                this.RaisePropertyChanged(nameof(this.Notice));
                this.RaisePropertyChanged(nameof(this.HasNotice));
            }
        }

        public bool HasNotice => this.notice.Length > 0;

        /// <summary>
        /// Gets the command that puts the whole castle back to what the savegame had.
        /// </summary>
        public ICommand ResetCommand { get; }

        private CastleModel Castle { get; set; }

        /// <summary>
        /// Gets the castle as the savegame had it when it was loaded or last saved.
        /// </summary>
        private CastleModel Original => this.history.Original.Castles().GetModel(this.castleIndex);

        public void Dispose()
        {
            this.history.Changed -= this.OnHistoryChanged;
        }

        /// <summary>
        /// Creates a target for the hex editor onto the raw bytes of the castle. The caller disposes it.
        /// </summary>
        public IByteEditTarget CreateByteEditTarget() => this.byteEditTargets.ForCastle(this.history, this.castleIndex);

        /// <summary>
        /// Shows the castle again from the current document.
        /// </summary>
        public void Refresh()
        {
            this.Castle = this.session.Current;
            this.UpdateIsDirty();

            this.Garrison = Enumerable.Range(1, Constants.CreatureSlotCount)
                .Select(slot => new GarrisonSlotRowViewModel(slot, this.Castle.Garrison.FindSlot(slot)))
                .ToList();

            this.Heroes = CastleHeroRowViewModel.For(this.Castle, this.history.Current.Heroes(), this.openHero);

            // The upgraded horde buildings (bits 19 and 25) are no buildings of the game: it sets the bits by itself when a growth building and the
            // upgrade of its dwelling both exist, and the growth building works on both dwellings. They are not listed or counted.
            IReadOnlyList<Building> available = this.Castle.Faction == null ? [] : Building.ForFaction(this.Castle.Faction).Where(b => !CastleBuilder.IsAutomatic(b)).ToList();
            BuildingGroupViewModel Group(string title, Func<Building, bool> filter) => new(
                title,
                available.Where(filter).Select(this.CreateBuildingRow(available)).ToList());

            this.BuildingGroups = new[]
                {
                    Group("Town", b => b.Faction == null),
                    Group("Special buildings", b => b.Faction != null && b.Code < 30),
                    this.DwellingGroup(available),
                }
                .Where(g => g.Rows.Count > 0)
                .ToList();
            var built = available.Count(this.Castle.HasBuilt);
            var disabled = available.Count(b => !this.Castle.HasBuilt(b) && !this.Castle.EnabledBuildings.Contains(b));
            this.BuildingsSummary = $"{built} of {available.Count} buildings built. {disabled} not allowed";

            this.Dwellings = this.CreateDwellings(available);
            var waiting = Enumerable.Range(0, Building.DwellingLevelCount).Sum(level => CastleBuilder.CreaturesWaiting(this.Castle, level) ?? 0);
            this.DwellingsSummary = $"{waiting:N0} Total";

            var maximum = available.Count(b => b.IsMageGuild);
            // The rows stay while the castle is edited, so that the list a spell is being picked from is not replaced under the user.
            if (this.MageGuildLevels.Count != maximum || this.MageGuildLevels.Any(row => row.Slots.Count != this.Castle.MageGuildSpells[row.Level - 1].Count))
            {
                this.MageGuildLevels = Enumerable.Range(1, maximum)
                    .Select(level => new MageGuildLevelRowViewModel(level, level <= this.Castle.MageGuildLevel, this.Castle.SpellCounts[level - 1], this.CreateSpellSlots(level)))
                    .ToList();
            }
            else
            {
                foreach (var row in this.MageGuildLevels)
                {
                    row.Update(row.Level <= this.Castle.MageGuildLevel, this.Castle.SpellCounts[row.Level - 1]);
                }
            }

            this.MageGuildLevelChoices = Enumerable.Range(0, maximum + 1).ToList();
            this.MageGuildSummary = this.Castle.MageGuildLevel == 0 ? $"No Mage Guild, up to level {maximum}" : $"Level {this.Castle.MageGuildLevel} of {maximum}";

            this.GarrisonEditor?.Refresh();
            this.RaiseAllPropertiesChanged();
            CommandManager.InvalidateRequerySuggested();
        }

        private CastleHeroRowViewModel? GarrisonedHeroRow => this.Heroes.FirstOrDefault(h => h.Kind == CastleHeroRole.Garrisoned);

        private HeroModel? GarrisonedHeroModel
        {
            get
            {
                if (this.Castle.GarrisonedHeroId is not { } id)
                {
                    return null;
                }

                var heroes = this.history.Current.Heroes();
                var index = heroes.IndexOfId(id);
                return index < 0 ? null : heroes.GetModel(index);
            }
        }

        /// <summary>
        /// Applies an edit to the castle. An edit the rules of the game do not allow is not made; <see cref="Notice"/> says why.
        /// </summary>
        private void Edit(Func<CastleModel, CastleModel> edit)
        {
            try
            {
                this.session.Update(edit);
                this.Notice = string.Empty;
            }
            catch (GameRuleException ex)
            {
                // Only the rules of the game are told to the user; a fault (a bad index, say) is not made to look like one.
                this.Notice = ex.Message;
                this.Refresh();
            }
        }

        /// <summary>
        /// Lists each dwelling followed by its upgraded dwelling, which is shown as its child.
        /// </summary>
        private BuildingGroupViewModel DwellingGroup(IReadOnlyList<Building> available)
        {
            var rows = new List<BuildingRowViewModel>();
            for (var level = 0; level < Building.DwellingLevelCount; level++)
            {
                var dwelling = available.FirstOrDefault(b => b.Type == BuildingType.Dwelling1 + level);
                var upgraded = available.FirstOrDefault(b => b.Type == BuildingType.UpgradedDwelling1 + level);
                if (dwelling != null)
                {
                    rows.Add(this.CreateBuildingRow(available)(dwelling));
                }

                if (upgraded != null)
                {
                    rows.Add(this.CreateBuildingRow(available, isChild: dwelling != null)(upgraded));
                }
            }

            return new BuildingGroupViewModel("Creature dwellings", rows);
        }

        private Func<Building, BuildingRowViewModel> CreateBuildingRow(IReadOnlyList<Building> available, bool isChild = false)
        {
            return b =>
            {
                var isBuilt = this.Castle.HasBuilt(b);
                return new BuildingRowViewModel(
                    b,
                    isBuilt,
                    this.Castle.Buildings,
                    this.Castle.EnabledBuildings.Contains(b),
                    this.CreaturesAvailable(b, available),
                    isBuilt && !this.Castle.CurrentBuildings.Contains(b),
                    build => this.Edit(castle => build ? CastleBuilder.Build(castle, b) : CastleBuilder.Unbuild(castle, b)),
                    isBuilt ? this.buildMessages.DescribeUnbuild(this.Castle, b) : this.buildMessages.DescribeBuild(this.Castle, b),
                    isChild,
                    allow => this.Edit(castle => CastleBuilder.SetAllowed(castle, b, allow)),
                    this.buildMessages.DescribeSetAllowed(this.Castle, b));
            };
        }

        private List<MageGuildSpellSlotViewModel> CreateSpellSlots(int level)
        {
            var choices = Spell.All.Where(s => s.Level == level && s.CanBeInMageGuild).OrderBy(s => s.Name).ToList();
            return Enumerable.Range(0, this.Castle.MageGuildSpells[level - 1].Count)
                .Select(slot => new MageGuildSpellSlotViewModel(
                    () => this.Castle.MageGuildSpells[level - 1][slot],
                    choices,
                    picked => this.Edit(castle => CastleBuilder.SetMageGuildSpell(castle, level, slot, picked))))
                .ToList();
        }

        private List<DwellingRowViewModel> CreateDwellings(IReadOnlyList<Building> available)
        {
            var rows = new List<DwellingRowViewModel>();
            for (var level = 0; level < Building.DwellingLevelCount; level++)
            {
                var dwelling = available.FirstOrDefault(b => b.Faction != null && b.Code == 30 + level);
                if (dwelling == null)
                {
                    continue;
                }

                var index = level;
                var upgraded = available.FirstOrDefault(b => b.Faction != null && b.Code == 37 + level);
                if (upgraded != null && this.Castle.HasBuilt(upgraded))
                {
                    var count = this.Castle.UpgradedCreaturesAvailable[level];
                    rows.Add(new DwellingRowViewModel(level + 1, upgraded.Name, "Upgraded", count.ToString(CultureInfo.CurrentCulture), true, count, n => this.SetCreaturesWaiting(index, n), DwellingCreatures.NamesOf(this.Castle.Faction!, level, hasUpgrade: true)));
                }
                else if (this.Castle.HasBuilt(dwelling))
                {
                    var count = this.Castle.CreaturesAvailable[level];
                    rows.Add(new DwellingRowViewModel(level + 1, dwelling.Name, "Built", count.ToString(CultureInfo.CurrentCulture), true, count, n => this.SetCreaturesWaiting(index, n), DwellingCreatures.NamesOf(this.Castle.Faction!, level, hasUpgrade: false)));
                }
                else
                {
                    var state = this.Castle.EnabledBuildings.Contains(dwelling) ? "Not built" : "Not allowed by the map";
                    rows.Add(new DwellingRowViewModel(level + 1, dwelling.Name, state, string.Empty, false, creatureNames: DwellingCreatures.NamesOf(this.Castle.Faction!, level, hasUpgrade: false)));
                }
            }

            return rows;
        }

        private void SetCreaturesWaiting(int level, int count)
        {
            this.Edit(castle => CastleBuilder.SetCreaturesWaiting(castle, level, count));
        }

        /// <summary>
        /// Returns how many creatures wait in <paramref name="building"/> if it is a dwelling that is built and has not been
        /// replaced by its upgrade, otherwise null.
        /// </summary>
        private int? CreaturesAvailable(Building building, IReadOnlyList<Building> available)
        {
            if (!this.Castle.HasBuilt(building))
            {
                return null;
            }

            if (building.IsDwelling)
            {
                var upgraded = Building.FromDwellingLevel(this.Castle.Faction!, building.DwellingLevel!.Value, upgraded: true);
                return upgraded != null && this.Castle.HasBuilt(upgraded) ? null : this.Castle.CreaturesAvailable[building.DwellingLevel!.Value];
            }

            return building.IsUpgradedDwelling ? this.Castle.UpgradedCreaturesAvailable[building.DwellingLevel!.Value] : null;
        }

        private void UpdateIsDirty()
        {
            this.HasPendingChanges = this.session.IsDirty;
        }

        private void OnHistoryChanged(object? sender, EventArgs e)
        {
            this.Refresh();
        }
    }
}
