namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The pages of a castle: what it is and who owns it (general), the buildings and the dwellings with the creatures that wait in them,
    /// the army that defends it and the Mage Guild.
    /// </summary>
    public sealed class CastleViewModel : SessionPageViewModelBase<CastleModel>, ICastlePage
    {
        private const string NoSpell = "(none)";
        private const string NotBuilt = "Not built";

        private readonly Action<int> openHero;
        private readonly IEditSession<CastleModel> session;
        private readonly IByteEditTargetFactory? byteEditTargets;
        private readonly IEditSession<BuiltTodayModel> builtToday;
        private readonly IReadOnlyList<ToggleRowViewModel> buildings;
        private readonly IReadOnlyList<DwellingRowViewModel> dwellings;
        private readonly IReadOnlyList<GuildLevelViewModel> guildLevels;
        private DwellingStockActions? dwellingActions;
        private CastleModel castle = null!;
        private IReadOnlyList<CastleHeroRowViewModel> heroes = [];
        private string visitingHeroName = string.Empty;

        public CastleViewModel(Game game, int castleIndex, Action<int> openHero, IByteEditTargetFactory? byteEditTargets = null)
            : this(game, game.History.CreateCastleSession(castleIndex), openHero, byteEditTargets)
        {
        }

        private CastleViewModel(Game game, IEditSession<CastleModel> session, Action<int> openHero, IByteEditTargetFactory? byteEditTargets)
            : base("Castle", session)
        {
            var castleIndex = session.Index;
            this.byteEditTargets = byteEditTargets;
            this.Game = game;
            this.CastleIndex = castleIndex;
            this.openHero = openHero;
            this.session = session;
            this.castle = this.session.Current;
            this.builtToday = game.History.CreateBuiltTodaySession();
            this.CanBuildThisTurn = new ToggleRowViewModel(
                "Can build this turn",
                () => !this.builtToday.Current.Has(this.CastleIndex),
                canBuild => this.builtToday.Update(set => set.With(this.CastleIndex, !canBuild)),
                toolTip: "Whether a building can still be built in this town today. The game allows one building per town per day.");

            this.GarrisonEditor = new ArmyViewModel(
                game.Catalog,
                game.GameVersion,
                () => this.session.Current.Garrison,
                edit => this.session.Update(c => c with { Garrison = edit(c.Garrison) }));
            this.GarrisonEditor.SetTitle("Garrison");
            this.OpenHeroCommand = new DelegateCommand(_ => this.openHero(this.castle.VisitingHeroId!.Value), _ => this.castle?.VisitingHeroId != null);

            this.OwnerField = new ChoiceFieldViewModel(
                "Owner",
                OwnerNames<PlayerColor>.Choices,
                () => OwnerNames<PlayerColor>.Of(this.session.Current.Owner),
                this.ChangeOwner,
                toolTip: "Who owns the castle. The castle is added to the list of castles of its new owner and taken from the old one.");

            var faction = this.castle.Faction;
            this.buildings = game.Catalog.Buildings.Select(building =>
                building.Role == BuildingRole.Special && faction.HasLibrary
                    ? new ToggleRowViewModel(
                        building.NameFor(faction),
                        () => this.session.Current.HasLibrary,
                        built => this.session.Update(c => c.WithLibrary(built)),
                        toolTip: building.DescriptionFor(faction))
                    : this.Building(building.NameFor(faction), building.Flag, building.DescriptionFor(faction))).ToList();

            this.GuildLevelField = new ChoiceFieldViewModel(
                "Level",
                ["None", .. Enumerable.Range(1, Constants.Castle.MageGuildLevels).Select(l => $"Level {l}")],
                () => this.session.Current.MageGuildLevel == 0 ? "None" : $"Level {this.session.Current.MageGuildLevel}",
                name => this.session.Update(c => c.WithMageGuildLevel(name == "None" ? 0 : name[^1] - '0')),
                toolTip: "Builds the Mage Guild to this level, or takes it down. Each level shows 3, 3, 2, 2 and 1 spells (one more with the Library of a Wizard).");

            var spells = game.Catalog.Spells.Where(s => s.IsAvailableIn(game.GameVersion)).ToList();
            var (original, current) = (this.session.Original, this.session.Current);
            this.guildLevels = Enumerable.Range(1, Constants.Castle.MageGuildLevels)
                .Select(level => new GuildLevelViewModel(
                    level,
                    Enumerable.Range(0, Constants.Castle.MageGuildSlotsPerLevel)
                        .Select(slot => new ChoiceFieldViewModel(
                            $"Spell {slot + 1}",
                            GuildSpellChoices(spells, level, original.MageGuildSpell(level, slot), current.MageGuildSpell(level, slot)),
                            () => this.session.Current.MageGuildSpell(level, slot)?.Name ?? NoSpell,
                            name => this.session.Update(c => c.WithMageGuildSpell(level, slot, name == NoSpell ? null : game.Catalog.Spells.FindName(name))),
                            () => this.session.Current.MageGuildLevel >= level && slot < this.session.Current.MageGuildSpellCounts[level - 1],
                            $"A guild of level {level} teaches spells of level {level}, as every guild of the test saves does; the spell levels are read from those saves and not all were checked in the game. A spell of another level that the savegame has here is kept. The guild shows only the spells counted for its level: three at the first levels, and a fourth only with the Library of a Wizard."))
                        .ToList()))
                .ToList();

            this.dwellings = Enumerable.Range(1, Constants.Castle.DwellingCount).Select(this.CreateDwelling).ToList();

            this.Refresh();
        }

        public int CastleIndex { get; }

        public string Name => this.castle.Name;

        public string ColorHex => PlayerPalette.Of(this.castle.Owner);

        public string FactionName => this.castle.Faction.IsNeutral ? "Unknown faction" : this.castle.Faction.ToString();

        /// <summary>
        /// Gets what the settlement is: a castle, or a town that has none built.
        /// </summary>
        public string KindText => this.castle.HasCastle ? "Castle" : "Town";

        /// <summary>
        /// Gets the name of the hero that is in the castle, or nothing.
        /// </summary>
        public string VisitingHeroName => this.visitingHeroName;

        public ICommand OpenHeroCommand { get; }

        public ChoiceFieldViewModel OwnerField { get; }

        /// <summary>
        /// Gets the owner picked in the list, which is "Neutral" or the name of a colour.
        /// </summary>
        public string SelectedOwner
        {
            get => this.OwnerField.Selected;
            set => this.OwnerField.Selected = value;
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> OwnerChoices => this.OwnerField.Choices;

        /// <summary>
        /// Gets who owns the castle, as "Neutral" or the name of a colour.
        /// </summary>
        public string OwnerText => OwnerNames<PlayerColor>.Of(this.castle.Owner);

        public string PositionText => MapLevelText.Coordinates(this.castle.Position.X, this.castle.Position.Y);

        /// <summary>
        /// Gets where a boat appears when the castle is on a coast, or says there is no coast.
        /// </summary>
        public string ShipyardText => this.castle.ShipyardPosition is { } boat ? $"Boat appears at {boat}" : "No coast";

        /// <summary>
        /// Gets the hero in the castle, as a link that opens it: none, or one.
        /// </summary>
        public IReadOnlyList<CastleHeroRowViewModel> Heroes => this.heroes;

        public IReadOnlyList<ToggleRowViewModel> Buildings => this.buildings;

        /// <summary>
        /// Gets whether the castle can still build something today: the opposite of the bit the game sets when it has built (one building a day;
        /// the game clears the bit when a new day starts).
        /// </summary>
        public ToggleRowViewModel CanBuildThisTurn { get; }

        public IReadOnlyList<DwellingRowViewModel> Dwellings => this.dwellings;

        /// <summary>
        /// Gets the buttons that fill every dwelling (all +1 week, all +100, all empty).
        /// </summary>
        public DwellingStockActions DwellingActions => this.dwellingActions ??= new DwellingStockActions(() => this.dwellings);

        /// <summary>
        /// Gets the army that defends the castle when no hero is in it: its slots and the finder of creatures.
        /// </summary>
        public ArmyViewModel GarrisonEditor { get; }

        public ChoiceFieldViewModel GuildLevelField { get; }

        public IReadOnlyList<GuildLevelViewModel> GuildLevels => this.guildLevels;

        private Game Game { get; }

        /// <summary>
        /// Creates the target the hex tab of the page works on: the raw bytes of the castle. The caller disposes it. <c>null</c> when the
        /// editor was built without the means to make one, and the page has no hex tab.
        /// </summary>
        public Siniskalc.Core.Data.IByteEditTarget? CreateByteEditTarget() => this.byteEditTargets?.ForCastle(this.Game.History, this.CastleIndex);

        /// <summary>
        /// Reads what the page shows from the current document. Called by the page once it is built, and after every change.
        /// </summary>
        public void Refresh()
        {
            this.Read();
            this.UpdateIsDirty();
            this.RaiseAllPropertiesChanged();
        }

        /// <inheritdoc/>
        protected override void OnHistoryChanged() => this.Refresh();

        /// <summary>
        /// Returns what a slot of the guild of <paramref name="level"/> can be given: no spell, and the spells of that level by name. A spell of
        /// another level that the slot holds as loaded or now (a savegame may have one) is offered too, so it shows and stays.
        /// </summary>
        private static IReadOnlyList<string> GuildSpellChoices(IEnumerable<Spell> spells, int level, Spell? original, Spell? current) =>
            new[] { NoSpell }
                .Concat(spells.Where(s => s.Level == level)
                    .Concat(new[] { original, current }.OfType<Spell>())
                    .Distinct()
                    .OrderBy(s => s.Name)
                    .Select(s => s.Name))
                .ToList();

        private void Read()
        {
            var document = this.Game.History.Current;
            var castle = this.session.Current;
            this.castle = castle;
            this.heroes = CastleHeroRows.For(castle, document.Heroes(), this.openHero);
            this.visitingHeroName = this.heroes.FirstOrDefault()?.Name ?? string.Empty;

            this.OwnerField?.Refresh();
            this.CanBuildThisTurn?.Refresh();
            this.GuildLevelField?.Refresh();
            this.GarrisonEditor?.Refresh();
            foreach (var building in this.buildings ?? [])
            {
                building.Refresh();
            }

            foreach (var dwelling in this.dwellings ?? [])
            {
                dwelling.Refresh();
            }

            foreach (var level in this.guildLevels ?? [])
            {
                level.Refresh();
            }
        }

        private ToggleRowViewModel Building(string name, TownBuildings building, string toolTip = "") => new(
            name,
            () => this.session.Current.Town.HasFlag(building),
            built => this.session.Update(c => c.WithBuilding(building, built)),
            toolTip: toolTip);

        private DwellingRowViewModel CreateDwelling(int dwelling)
        {
            // The creatures of the level, in the order of the table: the dwelling, then what its upgrades make.
            var creatures = !this.castle.Faction.IsNeutral ? this.castle.Faction.Creatures.Where(c => c.Tier == dwelling).Select(c => c.Name).ToList() : [];
            var levels = new List<string> { NotBuilt };
            levels.AddRange(Enumerable.Range(0, CastleModel.MaxDwellingLevel(this.castle.Faction, dwelling)).Select(i => i < creatures.Count ? creatures[i] : $"Dwelling {dwelling}"));
            return new DwellingRowViewModel(
                new ChoiceFieldViewModel(
                    $"Level {dwelling}",
                    levels,
                    () => levels[this.session.Current.DwellingLevel(dwelling)],
                    name => this.session.Update(c => c.WithDwellingLevel(dwelling, levels.IndexOf(name))),
                    toolTip: "Build the dwelling, upgrade it (the creature changes), or take it down. The creatures that wait in it stay."),
                new AmountRowViewModel(
                    "Waiting",
                    PlayerPalette.NoPlayer,
                    Constants.Castle.MinCreatures,
                    Constants.Castle.MaxWaiting,
                    1,
                    () => this.session.Current.CreaturesWaiting(dwelling),
                    () => this.Game.History.Original.Castles().GetModel(this.CastleIndex).CreaturesWaiting(dwelling),
                    count => this.session.Update(c => c.WithCreaturesWaiting(dwelling, (int)count))),
                () => this.session.Current.DwellingLevel(dwelling) > 0,
                () => this.session.Current.WeeklyGrowth(dwelling));
        }

        private void ChangeOwner(string name)
        {
            this.TryUpdate(c => c with { Owner = OwnerNames<PlayerColor>.Parse(name) });
            this.OwnerField.Refresh();
        }
    }
}
