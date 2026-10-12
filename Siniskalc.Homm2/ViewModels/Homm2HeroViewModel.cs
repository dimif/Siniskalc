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
    /// The pages of a hero: what the hero is and has (general), the secondary skills, the army, the spell book and the artifacts.
    /// </summary>
    public sealed class Homm2HeroViewModel : Homm2PageViewModel
    {
        private readonly Action<int> openCastle;
        private Homm2HeroModel hero = null!;
        private IReadOnlyList<Homm2FactRow> primarySkills = [];
        private IReadOnlyList<Homm2FactRow> facts = [];
        private IReadOnlyList<Homm2FactRow> skills = [];
        private IReadOnlyList<Homm2FactRow> army = [];
        private IReadOnlyList<Homm2FactRow> spells = [];
        private IReadOnlyList<Homm2FactRow> artifacts = [];
        private string castleName = string.Empty;
        private int castleIndex = -1;

        public Homm2HeroViewModel(Homm2Game game, int heroId, Action<int> openCastle)
            : base(game, Homm2ChunkKinds.Hero, heroId)
        {
            this.HeroId = heroId;
            this.openCastle = openCastle;
            this.OpenCastleCommand = new DelegateCommand(_ => this.openCastle(this.castleIndex), _ => this.castleIndex >= 0);
            this.Refresh();
        }

        public int HeroId { get; }

        public string Name => this.hero.Name;

        public string ColorHex => Homm2Palette.Of(this.hero.Owner);

        public string RaceName => this.hero.Race.ToString();

        public int Level => this.hero.Level;

        public string OwnerText => this.hero.Owner?.ToString() ?? "Nobody";

        /// <summary>
        /// Gets where the hero is on the map, or nothing for a hero that serves nobody.
        /// </summary>
        public string PositionText => this.hero.Owner == null ? string.Empty : $"at {this.hero.Position}";

        /// <summary>
        /// Gets the name of the castle the hero is in, or nothing.
        /// </summary>
        public string CastleName => this.castleName;

        public ICommand OpenCastleCommand { get; }

        public IReadOnlyList<Homm2FactRow> PrimarySkills => this.primarySkills;

        public IReadOnlyList<Homm2FactRow> Facts => this.facts;

        public IReadOnlyList<Homm2FactRow> Skills => this.skills;

        public IReadOnlyList<Homm2FactRow> Army => this.army;

        public IReadOnlyList<Homm2FactRow> Spells => this.spells;

        public IReadOnlyList<Homm2FactRow> Artifacts => this.artifacts;

        public bool HasSkills => this.hero.SecondarySkills.Count > 0;

        public bool HasArmy => this.hero.Army.Slots.Any(s => s != null);

        public bool HasSpellBook => this.hero.HasSpellBook;

        public bool HasSpells => this.hero.Spells.Count > 0;

        public bool HasArtifacts => this.hero.Artifacts.Slots.Any(a => a != null);

        protected override void Read()
        {
            var document = this.Game.History.Current;
            var hero = document.Heroes().GetModel(this.HeroId);
            this.hero = hero;

            this.primarySkills =
            [
                new Homm2FactRow("Attack", hero.PrimarySkills.Attack.ToString()),
                new Homm2FactRow("Defense", hero.PrimarySkills.Defense.ToString()),
                new Homm2FactRow("Spell power", hero.PrimarySkills.SpellPower.ToString()),
                new Homm2FactRow("Knowledge", hero.PrimarySkills.Knowledge.ToString()),
            ];

            this.facts =
            [
                new Homm2FactRow("Level", hero.Level.ToString()),
                new Homm2FactRow("Experience", hero.Experience.ToString("N0")),
                new Homm2FactRow("Spell points", hero.SpellPoints.ToString("N0")),
                new Homm2FactRow("Movement", hero.MovementLeft.ToString("N0"), $"of {hero.MaxMovement:N0} a day"),
            ];

            this.skills = hero.SecondarySkills.Entries
                .Select(e => new Homm2FactRow(e.Skill.Name, e.Level.ToString()))
                .ToList();

            this.army = hero.Army.Slots
                .Select((slot, i) => slot == null
                    ? new Homm2FactRow($"Slot {i + 1}", "empty")
                    : new Homm2FactRow($"Slot {i + 1}", slot.Creature.Name, $"× {slot.Count:N0}"))
                .ToList();

            this.spells = hero.Spells.Spells
                .Select(s => new Homm2FactRow(s.Name, $"{s.SpellPoints} spell points"))
                .ToList();

            this.artifacts = hero.Artifacts.Slots
                .Where(a => a != null)
                .Select((a, i) => new Homm2FactRow($"Slot {i + 1}", a!.Name))
                .ToList();

            var castle = document.Castles().GetModels().FirstOrDefault(c => c.IsUsed && c.VisitingHero == this.HeroId);
            this.castleIndex = castle?.Index ?? -1;
            this.castleName = castle?.Name ?? string.Empty;
        }
    }
}
