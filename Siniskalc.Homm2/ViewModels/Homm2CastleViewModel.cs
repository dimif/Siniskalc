namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Entities;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The pages of a castle: what it is and who owns it (general), the buildings and the dwellings with the creatures that wait in them,
    /// the army that defends it and the Mage Guild.
    /// </summary>
    public sealed class Homm2CastleViewModel : Homm2PageViewModel
    {
        private const string Nobody = "Nobody";
        private const string NoSpell = "(none)";
        private const string NotBuilt = "Not built";
        
        private readonly Action<int> openHero;
        private readonly Homm2CastleEditSession session;
        private readonly IReadOnlyList<ToggleRowViewModel> buildings;
        private readonly IReadOnlyList<Homm2DwellingRowViewModel> dwellings;
        private readonly IReadOnlyList<Homm2GuildLevelViewModel> guildLevels;
        private Homm2CastleModel castle = null!;
        private IReadOnlyList<Homm2FactRow> facts = [];
        private string visitingHeroName = string.Empty;
        private string notice = string.Empty;

        public Homm2CastleViewModel(Homm2Game game, int castleIndex, Action<int> openHero, IHomm2ByteEditTargetFactory? byteEditTargets = null)
            : base(game, Homm2ChunkKinds.Castle, castleIndex, byteEditTargets)
        {
            this.CastleIndex = castleIndex;
            this.openHero = openHero;
            this.session = game.History.CreateCastleSession(castleIndex);
            this.castle = this.session.Current;

            this.GarrisonEditor = new Homm2ArmyViewModel(
                game.GameVersion,
                () => this.session.Current.Garrison,
                edit => this.session.Update(c => c with { Garrison = edit(c.Garrison) }));
            this.OpenHeroCommand = new DelegateCommand(_ => this.openHero(this.castle.VisitingHero!.Value), _ => this.castle?.VisitingHero != null);

            this.OwnerField = new ChoiceFieldViewModel(
                "Owner",
                [Nobody, .. Enum.GetValues<Homm2Color>().Select(c => c.ToString())],
                () => this.session.Current.Owner?.ToString() ?? Nobody,
                this.ChangeOwner,
                toolTip: "Who owns the castle. The castle is added to the list of castles of its new owner and taken from the old one.");

            this.buildings =
            [
                this.Building("Castle", Homm2TownBuildings.Castle, "A town becomes a castle when it is built."),
                this.Building("Tavern", Homm2TownBuildings.Tavern),
                this.Building("Thieves' Guild", Homm2TownBuildings.ThievesGuild),
                this.Building("Shipyard", Homm2TownBuildings.Shipyard),
                this.Building("Well", Homm2TownBuildings.Well),
                this.Building("Statue", Homm2TownBuildings.Statue),
            ];

            if (this.castle.Race == Homm2Race.Wizard)
            {
                this.buildings = [.. this.buildings, new ToggleRowViewModel(
                    "Library",
                    () => this.session.Current.HasLibrary,
                    built => this.session.Update(c => c.WithLibrary(built)),
                    toolTip: "Gives each level of the Mage Guild a fourth spell. Not tried in the game: found by comparing the castles of the savegames.")];
            }

            this.GuildLevelField = new ChoiceFieldViewModel(
                "Level",
                ["None", .. Enumerable.Range(1, Constants.Castle.MageGuildLevels).Select(l => $"Level {l}")],
                () => this.session.Current.MageGuildLevel == 0 ? "None" : $"Level {this.session.Current.MageGuildLevel}",
                name => this.session.Update(c => c.WithMageGuildLevel(name == "None" ? 0 : name[^1] - '0')),
                toolTip: "Builds the Mage Guild to this level, or takes it down. Each level shows 3, 3, 2, 2 and 1 spells (one more with the Library of a Wizard).");

            var spellNames = new[] { NoSpell }.Concat(Homm2Spell.All.Where(s => s.IsAvailableIn(game.GameVersion)).OrderBy(s => s.Name).Select(s => s.Name)).ToList();
            this.guildLevels = Enumerable.Range(1, Constants.Castle.MageGuildLevels)
                .Select(level => new Homm2GuildLevelViewModel(
                    level,
                    Enumerable.Range(0, Constants.Castle.MageGuildSlotsPerLevel)
                        .Select(slot => new ChoiceFieldViewModel(
                            $"Spell {slot + 1}",
                            spellNames,
                            () => this.session.Current.GuildSpell(level, slot)?.Name ?? NoSpell,
                            name => this.session.Update(c => c.WithGuildSpell(level, slot, name == NoSpell ? null : AbstractEnumeration.FromName<Homm2Spell>(name))),
                            () => this.session.Current.MageGuildLevel >= level && slot < this.session.Current.GuildSpellCounts[level - 1],
                            "The guild shows only the spells counted for its level: three at the first levels, and a fourth only with the Library of a Wizard."))
                        .ToList()))
                .ToList();

            this.dwellings = Enumerable.Range(1, Constants.Castle.DwellingCount).Select(this.CreateDwelling).ToList();

            this.Refresh();
        }

        public int CastleIndex { get; }

        public string Name => this.castle.Name;

        public string ColorHex => Homm2Palette.Of(this.castle.Owner);

        public string RaceName => this.castle.Race?.ToString() ?? "Unknown race";

        /// <summary>
        /// Gets what the settlement is: a castle, or a town that has none built.
        /// </summary>
        public string KindText => this.castle.HasCastle ? "Castle" : "Town";

        /// <summary>
        /// Gets the name of the hero that is in the castle, or nothing.
        /// </summary>
        public string VisitingHeroName => this.visitingHeroName;

        /// <summary>
        /// Gets why an edit of the owner was refused, or nothing.
        /// </summary>
        public string Notice => this.notice;

        public bool HasNotice => this.notice.Length > 0;

        public ICommand OpenHeroCommand { get; }

        public ChoiceFieldViewModel OwnerField { get; }

        public IReadOnlyList<Homm2FactRow> Facts => this.facts;

        public IReadOnlyList<ToggleRowViewModel> Buildings => this.buildings;

        public IReadOnlyList<Homm2DwellingRowViewModel> Dwellings => this.dwellings;

        /// <summary>
        /// Gets the army that defends the castle when no hero is in it: its slots and the finder of creatures.
        /// </summary>
        public Homm2ArmyViewModel GarrisonEditor { get; }

        public ChoiceFieldViewModel GuildLevelField { get; }

        public IReadOnlyList<Homm2GuildLevelViewModel> GuildLevels => this.guildLevels;

        protected override Siniskalc.Core.Data.IByteEditTarget CreateByteEditTarget(IHomm2ByteEditTargetFactory factory) => factory.ForCastle(this.session);

        protected override void Read()
        {
            var document = this.Game.History.Current;
            var castle = this.session.Current;
            this.castle = castle;
            var heroes = document.Heroes().GetModels();

            this.visitingHeroName = castle.VisitingHero is { } id && id >= 0 && id < heroes.Count ? heroes[id].Name : string.Empty;

            this.facts =
            [
                new Homm2FactRow("Position", castle.Position.ToString()),
                new Homm2FactRow("Boat appears at", castle.Boat?.ToString() ?? "no coast"),
            ];

            this.OwnerField?.Refresh();
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

        private ToggleRowViewModel Building(string name, Homm2TownBuildings building, string toolTip = "") => new(
            name,
            () => this.session.Current.Town.HasFlag(building),
            built => this.session.Update(c => c.WithBuilding(building, built)),
            toolTip: toolTip);

        private Homm2DwellingRowViewModel CreateDwelling(int dwelling)
        {
            // The creatures of the level, in the order of the table: the dwelling, then what its upgrades make.
            var creatures = this.castle.Race is { } race ? Homm2Creature.ForRace(race).Where(c => c.Level == dwelling).Select(c => c.Name).ToList() : [];
            var levels = new List<string> { NotBuilt };
            levels.AddRange(Enumerable.Range(0, Homm2CastleModel.MaxDwellingLevel(this.castle.Race, dwelling)).Select(i => i < creatures.Count ? creatures[i] : $"Dwelling {dwelling}"));
            return new Homm2DwellingRowViewModel(
                new ChoiceFieldViewModel(
                    $"Level {dwelling}",
                    levels,
                    () => levels[this.session.Current.DwellingLevel(dwelling)],
                    name => this.session.Update(c => c.WithDwellingLevel(dwelling, levels.IndexOf(name))),
                    toolTip: "Build the dwelling, upgrade it (the creature changes), or take it down. The creatures that wait in it stay."),
                new AmountRowViewModel(
                    "Waiting",
                    Homm2Palette.NoPlayer,
                    Constants.Castle.MinCreatures,
                    Constants.Castle.MaxWaiting,
                    1,
                    () => this.session.Current.Waiting(dwelling),
                    () => this.Game.History.Original.Castles().GetModel(this.CastleIndex).Waiting(dwelling),
                    count => this.session.Update(c => c.WithWaiting(dwelling, (int)count))),
                () => this.session.Current.DwellingLevel(dwelling) > 0);
        }

        private void ChangeOwner(string name)
        {
            this.notice = string.Empty;
            try
            {
                this.session.ChangeOwner(name == Nobody ? null : Enum.Parse<Homm2Color>(name));
            }
            catch (InvalidOperationException)
            {
                this.notice = $"{name} has the most castles a player can have.";
            }

            this.RaisePropertyChanged(nameof(this.Notice));
            this.RaisePropertyChanged(nameof(this.HasNotice));
            this.OwnerField.Refresh();
        }
    }
}
