namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;
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
    public sealed class CastleViewModel : PageViewModel, ICastlePage
    {
        private const string NoSpell = "(none)";
        private const string NotBuilt = "Not built";

        private readonly Action<int> openHero;
        private readonly CastleEditSession session;
        private readonly MapObjectEditSession<BuiltTodayModel> builtToday;
        private readonly IReadOnlyList<ToggleRowViewModel> buildings;
        private readonly IReadOnlyList<DwellingRowViewModel> dwellings;
        private readonly IReadOnlyList<GuildLevelViewModel> guildLevels;
        private CastleModel castle = null!;
        private IReadOnlyList<CastleHeroRowViewModel> heroes = [];
        private string visitingHeroName = string.Empty;
        private string notice = string.Empty;

        public CastleViewModel(Game game, int castleIndex, Action<int> openHero, IByteEditTargetFactory? byteEditTargets = null)
            : base(game, Homm2ChunkKinds.Castle, castleIndex, byteEditTargets)
        {
            this.CastleIndex = castleIndex;
            this.openHero = openHero;
            this.session = game.History.CreateCastleSession(castleIndex);
            this.castle = this.session.Current;
            this.builtToday = game.History.CreateBuiltTodaySession();
            this.CanBuildThisTurn = new ToggleRowViewModel(
                "Can build this turn",
                () => !this.builtToday.Current.Has(this.CastleIndex),
                canBuild => this.builtToday.Update(set => set.With(this.CastleIndex, !canBuild)),
                toolTip: "Whether a building can still be built in this town today. The game allows one building per town per day.");

            this.GarrisonEditor = new ArmyViewModel(
                game.GameVersion,
                () => this.session.Current.Garrison,
                edit => this.session.Update(c => c with { Garrison = edit(c.Garrison) }));
            this.OpenHeroCommand = new DelegateCommand(_ => this.openHero(this.castle.VisitingHeroId!.Value), _ => this.castle?.VisitingHeroId != null);

            this.OwnerField = new ChoiceFieldViewModel(
                "Owner",
                OwnerNames<PlayerColor>.Choices,
                () => OwnerNames<PlayerColor>.Of(this.session.Current.Owner),
                this.ChangeOwner,
                toolTip: "Who owns the castle. The castle is added to the list of castles of its new owner and taken from the old one.");

            var faction = this.castle.Faction;
            this.buildings =
            [
                this.Building("Castle", TownBuildings.Castle, "A town becomes a castle when it is built."),
                this.Building("Tavern", TownBuildings.Tavern),
                this.Building("Thieves' Guild", TownBuildings.ThievesGuild),
                this.Building("Shipyard", TownBuildings.Shipyard),
                this.Building("Well", TownBuildings.Well),
                this.Building("Statue", TownBuildings.Statue),
                this.Building("Marketplace", TownBuildings.Marketplace),
                this.Building("Left turret", TownBuildings.LeftTurret),
                this.Building("Right turret", TownBuildings.RightTurret),
                this.Building("Moat", TownBuildings.Moat),
                this.Building("Captain's quarters", TownBuildings.CaptainsQuarters),
                this.Building(TownBuildingNames.SecondWellOf(faction), TownBuildings.GrowthBuilding, "More creatures every week. Found in the savegames of Threedom; not tried in the game."),
            ];

            this.buildings = faction == Faction.Wizard
                ?
                [
                    .. this.buildings,
                    new ToggleRowViewModel(
                        TownBuildingNames.SpecialOf(faction),
                        () => this.session.Current.HasLibrary,
                        built => this.session.Update(c => c.WithLibrary(built)),
                        toolTip: "Gives each level of the Mage Guild a fourth spell."),
                ]
                : [.. this.buildings, this.Building(TownBuildingNames.SpecialOf(faction), TownBuildings.Special, "The special building of the faction. Found in the savegames of Threedom; not tried in the game.")];

            this.GuildLevelField = new ChoiceFieldViewModel(
                "Level",
                ["None", .. Enumerable.Range(1, Constants.Castle.MageGuildLevels).Select(l => $"Level {l}")],
                () => this.session.Current.MageGuildLevel == 0 ? "None" : $"Level {this.session.Current.MageGuildLevel}",
                name => this.session.Update(c => c.WithMageGuildLevel(name == "None" ? 0 : name[^1] - '0')),
                toolTip: "Builds the Mage Guild to this level, or takes it down. Each level shows 3, 3, 2, 2 and 1 spells (one more with the Library of a Wizard).");

            var spellNames = new[] { NoSpell }.Concat(Spell.All.Where(s => s.IsAvailableIn(game.GameVersion)).OrderBy(s => s.Name).Select(s => s.Name)).ToList();
            this.guildLevels = Enumerable.Range(1, Constants.Castle.MageGuildLevels)
                .Select(level => new GuildLevelViewModel(
                    level,
                    Enumerable.Range(0, Constants.Castle.MageGuildSlotsPerLevel)
                        .Select(slot => new ChoiceFieldViewModel(
                            $"Spell {slot + 1}",
                            spellNames,
                            () => this.session.Current.MageGuildSpell(level, slot)?.Name ?? NoSpell,
                            name => this.session.Update(c => c.WithMageGuildSpell(level, slot, name == NoSpell ? null : AbstractEnumeration.FromName<Spell>(name))),
                            () => this.session.Current.MageGuildLevel >= level && slot < this.session.Current.MageGuildSpellCounts[level - 1],
                            "The guild shows only the spells counted for its level: three at the first levels, and a fourth only with the Library of a Wizard."))
                        .ToList()))
                .ToList();

            this.dwellings = Enumerable.Range(1, Constants.Castle.DwellingCount).Select(this.CreateDwelling).ToList();

            this.Refresh();
        }

        public int CastleIndex { get; }

        public string Name => this.castle.Name;

        public string ColorHex => PlayerPalette.Of(this.castle.Owner);

        public string FactionName => this.castle.Faction == Faction.Neutral ? "Unknown faction" : this.castle.Faction.ToString();

        /// <summary>
        /// Gets what the settlement is: a castle, or a town that has none built.
        /// </summary>
        public string KindText => this.castle.HasCastle ? "Castle" : "Town";

        /// <summary>
        /// Gets the name of the hero that is in the castle, or nothing.
        /// </summary>
        public string VisitingHeroName => this.visitingHeroName;

        /// <inheritdoc/>
        public string Notice => this.notice;

        public bool HasNotice => this.notice.Length > 0;

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

        IReadOnlyList<ILink> ICastlePage.Heroes => this.Heroes;

        public IReadOnlyList<ToggleRowViewModel> Buildings => this.buildings;

        /// <summary>
        /// Gets whether the castle can still build something today: the opposite of the bit the game sets when it has built (one building a day;
        /// the game clears the bit when a new day starts).
        /// </summary>
        public ToggleRowViewModel CanBuildThisTurn { get; }

        public IReadOnlyList<DwellingRowViewModel> Dwellings => this.dwellings;

        /// <summary>
        /// Gets the army that defends the castle when no hero is in it: its slots and the finder of creatures.
        /// </summary>
        public ArmyViewModel GarrisonEditor { get; }

        public ChoiceFieldViewModel GuildLevelField { get; }

        public IReadOnlyList<GuildLevelViewModel> GuildLevels => this.guildLevels;

        protected override Siniskalc.Core.Data.IByteEditTarget CreateByteEditTarget(IByteEditTargetFactory factory) => factory.ForCastle(this.session);

        protected override void Read()
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
            var creatures = this.castle.Faction != Faction.Neutral ? Creature.ForFaction(this.castle.Faction).Where(c => c.Tier == dwelling).Select(c => c.Name).ToList() : [];
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
            this.notice = string.Empty;
            try
            {
                this.session.ChangeOwner(OwnerNames<PlayerColor>.Parse(name));
            }
            catch (GameRuleException)
            {
                this.notice = $"{name} has the most castles a player can have.";
            }

            this.RaisePropertyChanged(nameof(this.Notice));
            this.RaisePropertyChanged(nameof(this.HasNotice));
            this.OwnerField.Refresh();
        }
    }
}
