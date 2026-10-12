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
        private readonly Homm2HeroEditSession session;
        private readonly IReadOnlyList<AmountRowViewModel> primarySkills;
        private readonly IReadOnlyList<AmountRowViewModel> facts;
        private IReadOnlyList<Homm2FactRow> skills = [];
        private IReadOnlyList<Homm2FactRow> spells = [];
        private IReadOnlyList<Homm2FactRow> artifacts = [];
        private string castleName = string.Empty;
        private int castleIndex = -1;

        public Homm2HeroViewModel(Homm2Game game, int heroId, Action<int> openCastle)
            : base(game, Homm2ChunkKinds.Hero, heroId)
        {
            this.HeroId = heroId;
            this.openCastle = openCastle;
            this.session = game.History.CreateHeroSession(heroId);
            this.primarySkills =
            [
                this.Row("Attack", Constants.Hero.MinPrimarySkill, Constants.Hero.MaxPrimarySkill, 1, h => h.PrimarySkills.Attack, (h, v) => h with { PrimarySkills = h.PrimarySkills with { Attack = (int)v } }),
                this.Row("Defense", Constants.Hero.MinPrimarySkill, Constants.Hero.MaxPrimarySkill, 1, h => h.PrimarySkills.Defense, (h, v) => h with { PrimarySkills = h.PrimarySkills with { Defense = (int)v } }),
                this.Row("Spell power", Constants.Hero.MinPrimarySkill, Constants.Hero.MaxPrimarySkill, 1, h => h.PrimarySkills.SpellPower, (h, v) => h with { PrimarySkills = h.PrimarySkills with { SpellPower = (int)v } }),
                this.Row("Knowledge", Constants.Hero.MinPrimarySkill, Constants.Hero.MaxPrimarySkill, 1, h => h.PrimarySkills.Knowledge, (h, v) => h with { PrimarySkills = h.PrimarySkills with { Knowledge = (int)v } }),
            ];
            this.facts =
            [
                this.Row("Level", Constants.Hero.MinLevel, Constants.Hero.MaxLevel, 1, h => h.Level, (h, v) => h with { Level = (int)v }),
                this.Row("Experience", Constants.Hero.MinExperience, Constants.Hero.MaxExperience, Constants.Hero.ExperienceStep, h => h.Experience, (h, v) => h with { Experience = v }),
                this.Row("Spell points", Constants.Hero.MinSpellPoints, Constants.Hero.MaxSpellPoints, 1, h => h.SpellPoints, (h, v) => h with { SpellPoints = (int)v }),
                this.Row("Movement", Constants.Hero.MinMovementPoints, Constants.Hero.MaxMovementPoints, Constants.Hero.MovementStep, h => h.MovementLeft, (h, v) => h with { MovementLeft = v }, () => $"of {this.hero.MaxMovement:N0} a day"),
            ];
            this.ArmyEditor = new Homm2ArmyViewModel(
                game.GameVersion,
                () => this.session.Current.Army,
                edit => this.session.Update(h => h with { Army = edit(h.Army) }));
            this.RestoreMovementCommand = new DelegateCommand(_ => this.session.Update(h => h with { MovementLeft = h.MaxMovement }), _ => this.hero.MovementLeft != this.hero.MaxMovement);
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

        /// <summary>
        /// Gets the command that gives the hero all the movement points of a day.
        /// </summary>
        public ICommand RestoreMovementCommand { get; }

        public IReadOnlyList<AmountRowViewModel> PrimarySkills => this.primarySkills;

        public IReadOnlyList<AmountRowViewModel> Facts => this.facts;

        public IReadOnlyList<Homm2FactRow> Skills => this.skills;

        /// <summary>
        /// Gets the army of the hero: its slots and the finder of creatures.
        /// </summary>
        public Homm2ArmyViewModel ArmyEditor { get; }

        public IReadOnlyList<Homm2FactRow> Spells => this.spells;

        public IReadOnlyList<Homm2FactRow> Artifacts => this.artifacts;

        public bool HasSkills => this.hero.SecondarySkills.Count > 0;

        public bool HasSpellBook => this.hero.HasSpellBook;

        public bool HasSpells => this.hero.Spells.Count > 0;

        public bool HasArtifacts => this.hero.Artifacts.Slots.Any(a => a != null);

        protected override void Read()
        {
            var document = this.Game.History.Current;
            var hero = document.Heroes().GetModel(this.HeroId);
            this.hero = hero;

            foreach (var row in this.primarySkills.Concat(this.facts))
            {
                row.Refresh();
            }

            this.skills = hero.SecondarySkills.Entries
                .Select(e => new Homm2FactRow(e.Skill.Name, e.Level.ToString()))
                .ToList();

            this.ArmyEditor?.Refresh();

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

        private AmountRowViewModel Row(string name, long minimum, long maximum, long step, Func<Homm2HeroModel, long> get, Func<Homm2HeroModel, long, Homm2HeroModel> set, Func<string>? note = null) => new(
            name,
            Homm2Palette.Of(this.hero?.Owner),
            minimum,
            maximum,
            step,
            () => get(this.session.Current),
            () => get(this.Game.History.Original.Heroes().GetModel(this.HeroId)),
            value => this.session.Update(h => set(h, value)),
            note);
    }
}
