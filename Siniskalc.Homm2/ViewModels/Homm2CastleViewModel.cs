namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The pages of a castle: what it is and who is in it (general), the buildings and dwellings, the army that defends it and the
    /// Mage Guild.
    /// </summary>
    public sealed class Homm2CastleViewModel : Homm2PageViewModel
    {
        private const string Built = "Built";
        private const string NotBuilt = "–";

        private readonly Action<int> openHero;
        private Homm2CastleModel castle = null!;
        private IReadOnlyList<Homm2FactRow> facts = [];
        private IReadOnlyList<Homm2FactRow> buildings = [];
        private IReadOnlyList<Homm2FactRow> dwellings = [];
        private IReadOnlyList<Homm2FactRow> guild = [];
        private string visitingHeroName = string.Empty;

        public Homm2CastleViewModel(Homm2Game game, int castleIndex, Action<int> openHero)
            : base(game, Homm2ChunkKinds.Castle, castleIndex)
        {
            this.CastleIndex = castleIndex;
            this.openHero = openHero;
            var session = game.History.CreateCastleSession(castleIndex);
            this.GarrisonEditor = new Homm2ArmyViewModel(
                game.GameVersion,
                () => session.Current.Garrison,
                edit => session.Update(c => c with { Garrison = edit(c.Garrison) }));
            this.OpenHeroCommand = new DelegateCommand(_ => this.openHero(this.castle.VisitingHero!.Value), _ => this.castle?.VisitingHero != null);
            this.Refresh();
        }

        public int CastleIndex { get; }

        public string Name => this.castle.Name;

        public string ColorHex => Homm2Palette.Of(this.castle.Owner);

        public string RaceName => this.castle.Race?.ToString() ?? "Unknown race";

        public string OwnerText => this.castle.Owner?.ToString() ?? "Nobody";

        /// <summary>
        /// Gets what the settlement is: a castle, or a town that has none built.
        /// </summary>
        public string KindText => this.castle.HasCastle ? "Castle" : "Town";

        /// <summary>
        /// Gets the name of the hero that is in the castle, or nothing.
        /// </summary>
        public string VisitingHeroName => this.visitingHeroName;

        public ICommand OpenHeroCommand { get; }

        public IReadOnlyList<Homm2FactRow> Facts => this.facts;

        public IReadOnlyList<Homm2FactRow> Buildings => this.buildings;

        public IReadOnlyList<Homm2FactRow> Dwellings => this.dwellings;

        /// <summary>
        /// Gets the army that defends the castle when no hero is in it: its slots and the finder of creatures.
        /// </summary>
        public Homm2ArmyViewModel GarrisonEditor { get; }

        public IReadOnlyList<Homm2FactRow> Guild => this.guild;

        public bool HasMageGuild => this.castle.MageGuildLevel > 0;

        protected override void Read()
        {
            var document = this.Game.History.Current;
            var castle = document.Castles().GetModel(this.CastleIndex);
            this.castle = castle;
            var heroes = document.Heroes().GetModels();

            this.visitingHeroName = castle.VisitingHero is { } id && id >= 0 && id < heroes.Count ? heroes[id].Name : string.Empty;

            this.facts =
            [
                new Homm2FactRow("Position", castle.Position.ToString()),
                new Homm2FactRow("Boat appears at", castle.Boat?.ToString() ?? "no coast"),
            ];

            this.buildings =
            [
                new Homm2FactRow("Castle", castle.HasCastle ? Built : NotBuilt),
                new Homm2FactRow("Mage Guild", castle.MageGuildLevel > 0 ? $"Level {castle.MageGuildLevel}" : NotBuilt),
                new Homm2FactRow("Tavern", Has(castle, Homm2TownBuildings.Tavern)),
                new Homm2FactRow("Thieves' Guild", Has(castle, Homm2TownBuildings.ThievesGuild)),
                new Homm2FactRow("Shipyard", Has(castle, Homm2TownBuildings.Shipyard)),
                new Homm2FactRow("Well", Has(castle, Homm2TownBuildings.Well)),
                new Homm2FactRow("Statue", Has(castle, Homm2TownBuildings.Statue)),
            ];

            IReadOnlyList<Homm2Creature> creatures = castle.Race is { } race ? Homm2Creature.ForRace(race) : [];
            this.dwellings = Enumerable.Range(1, Constants.Castle.DwellingCount)
                .Select(level =>
                {
                    var name = creatures.FirstOrDefault(c => c.Level == level)?.Name ?? $"Dwelling {level}";
                    var waiting = castle.Creatures[level - 1];
                    return new Homm2FactRow($"Level {level}", castle.HasDwelling(level) ? name : NotBuilt, castle.HasDwelling(level) ? $"{waiting:N0} waiting" : string.Empty);
                })
                .ToList();

            this.GarrisonEditor?.Refresh();

            this.guild = Enumerable.Range(1, castle.MageGuildLevel)
                .Select(level => new Homm2FactRow(
                    $"Level {level}",
                    string.Join(", ", Enumerable.Range(0, Math.Min(Constants.Castle.MageGuildSlotsPerLevel, castle.GuildSpellCounts[level - 1]))
                        .Select(slot => castle.GuildSpell(level, slot)?.Name ?? "none"))))
                .ToList();
        }

        private static string Has(Homm2CastleModel castle, Homm2TownBuildings building) => castle.Town.HasFlag(building) ? Built : NotBuilt;
    }
}
