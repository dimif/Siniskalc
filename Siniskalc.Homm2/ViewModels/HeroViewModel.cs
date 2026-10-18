namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The pages of a hero: what the hero is and has (general), the secondary skills, the army, the spell book and the artifacts.
    /// </summary>
    public sealed class HeroViewModel : PageViewModel, IOwnerChoice
    {
        private readonly Action<int> openCastle;
        private HeroModel hero = null!;
        private readonly HeroEditSession session;
        private readonly IReadOnlyList<AmountRowViewModel> primarySkills;
        private readonly IReadOnlyList<AmountRowViewModel> battleBonus;
        private readonly IReadOnlyList<VisitedPlaceRowViewModel> visitedPlaces;
        private readonly IReadOnlyList<ToggleRowViewModel> battleSources;
        private readonly IReadOnlyList<ToggleRowViewModel> usedOnce;
        private readonly IReadOnlyList<ToggleRowViewModel> usedWeekly;
        private bool visionsActive;
        private string castleName = string.Empty;
        private int castleIndex = -1;

        public HeroViewModel(Game game, int heroId, Action<int> openCastle, IMaxSpellPointsCalculator<SecondarySkill, IHomm2SecondarySkills> spellPoints, IByteEditTargetFactory? byteEditTargets = null)
            : base(game, Homm2ChunkKinds.Hero, heroId, byteEditTargets)
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
            this.battleBonus =
            [
                this.Row("Morale", Constants.Hero.MinBattleBonus, Constants.Hero.MaxBattleBonus, 1, h => h.NextBattle.Morale, (h, v) => h with { NextBattle = h.NextBattle with { Morale = (int)v } }, () => "next battle only"),
                this.Row("Luck", Constants.Hero.MinBattleBonus, Constants.Hero.MaxBattleBonus, 1, h => h.NextBattle.Luck, (h, v) => h with { NextBattle = h.NextBattle with { Luck = (int)v } }, () => "next battle only"),
            ];
            this.battleSources =
            [
                this.Source("Temple", HeroEffects.Temple, 2, 0, "+2 morale for the next battle"),
                this.Source("Fountain", HeroEffects.Fountain, 0, 1, "+1 luck for the next battle"),
                this.Source("Oasis", HeroEffects.Oasis, 1, 0, "+1 morale for the next battle"),
                this.Source("Faerie ring", HeroEffects.FaerieRing, 0, 1, "+1 luck for the next battle"),
                this.Source("Graveyard visited before", HeroEffects.Graveyard, -1, 0, "-1 morale for the next battle"),
            ];
            this.usedOnce =
            [
                this.Remembered("Arena used", HeroEffects.Arena, "The hero used the arena: it gives a point of skill once per hero"),
            ];
            this.usedWeekly =
            [
                this.Remembered("Stables used this week", HeroEffects.Stables, "The stables give more movement points once a week for each hero; the game clears this at the start of a week"),
            ];
            this.visitedPlaces =
            [
                this.Visited(game, PlaceKind.Gazebo, "Gazebos", "A thousand experience points, once per hero"),
                this.Visited(game, PlaceKind.Fort, "Forts", "A point of defense, once per hero"),
                this.Visited(game, PlaceKind.WitchDoctorHut, "Witch doctor's huts", "A point of knowledge or a skill, once per hero"),
                this.Visited(game, PlaceKind.MercenaryCamp, "Mercenary camps", "A point of attack, once per hero"),
                this.Visited(game, PlaceKind.StandingStones, "Standing stones", "A point of spell power, once per hero"),
                this.Visited(game, PlaceKind.Xanadu, "Xanadu", "A point of every primary skill, once per hero"),
            ];
            this.ArmyEditor = new ArmyViewModel(
                game.GameVersion,
                () => this.session.Current.Army,
                edit => this.session.Update(h => h with { Army = edit(h.Army) }));
            this.SkillsEditor = new SecondarySkillsViewModel(
                game.GameVersion,
                () => this.session.Current.SecondarySkills,
                edit => this.session.Update(h => h with { SecondarySkills = edit(h.SecondarySkills) }));
            this.SpellsEditor = new SpellsViewModel(
                game.GameVersion,
                () => this.session.Current,
                edit => this.session.Update(edit));
            this.ArtifactsEditor = new ArtifactsViewModel(
                game.GameVersion,
                () => this.session.Current,
                edit => this.session.Update(edit));
            // TODO:LOG warning: the active spell is kept in the view model only, as the savegames do not say where the game keeps it
            this.ActiveSpells = new ActiveSpellsCardViewModel(
            [
                new ToggleRowViewModel(
                    "Visions",
                    () => this.visionsActive,
                    active => this.visionsActive = active,
                    toolTip: "The editor does not know yet where the game keeps this: the box is not read from the savegame and not saved."),
            ]);
            this.OwnerField = new ChoiceFieldViewModel(
                "Owner",
                OwnerNames<Color>.Choices,
                () => OwnerNames<Color>.Of(this.session.Current.Owner),
                this.ChangeOwner,
                toolTip: "Which player the hero belongs to. The hero is added to the list of heroes of its new owner and taken from the old one; a neutral hero has no move to finish.");
            this.MovementAndSpellPoints = new MovementAndSpellPointsCardViewModel(
                () => this.session.Current.MovementLeft,
                value => this.session.Update(h => h with { MovementLeft = value }),
                () => this.session.Current.MaxMovement,
                new NumberRange(Constants.Hero.MinMovementPoints, Constants.Hero.MaxMovementPoints, Constants.Hero.MovementStep),
                () => this.session.Current.SpellPoints,
                value => this.session.Update(h => h with { SpellPoints = value }),
                new NumberRange(Constants.Hero.MinSpellPoints, Constants.Hero.MaxSpellPoints),
                () => spellPoints.GetMaximum(this.session.Current));
            this.LevelAndClass = new LevelAndClassCardViewModel(
                () => this.session.Current.Level,
                value => this.session.Update(h => h with { Level = value, Experience = HeroLevels.ExperienceOf(value) }),
                new NumberRange(Constants.Hero.MinLevel, Constants.Hero.MaxLevel),
                () => this.session.Current.Experience,
                value => this.session.Update(h => h with { Experience = value, Level = HeroLevels.LevelOf(value) }),
                new NumberRange(Constants.Hero.MinExperience, Constants.Hero.MaxExperience, Constants.Hero.ExperienceStep),
                this.NextLevelNote,
                Enum.GetNames<Faction>(),
                () => this.session.Current.Faction.ToString());
            this.Position = MapPointCardViewModel.ForPosition(
                () => this.session.Current.Position,
                position => this.session.Update(h => h with { Position = position }),
                new NumberRange(Constants.Hero.MinCoordinate, Constants.Hero.MaxCoordinate),
                getFacing: () => this.session.Current.Facing,
                setFacing: facing => this.session.Update(h => h with { Facing = facing }));
            this.Destination = MapPointCardViewModel.ForDestination(
                () => this.session.Current.Destination,
                destination => this.session.Update(h => h with { Destination = destination }),
                new NumberRange(Siniskalc.Heroes.Constants.Map.NoCoordinate, Constants.Hero.MaxCoordinate));
            this.OpenCastleCommand = new DelegateCommand(_ => this.openCastle(this.castleIndex), _ => this.castleIndex >= 0);
            this.Refresh();
        }

        public int HeroId { get; }

        public ChoiceFieldViewModel OwnerField { get; }

        /// <inheritdoc/>
        public IReadOnlyList<string> OwnerChoices => this.OwnerField.Choices;

        /// <summary>
        /// Gets or sets the owner, as "Neutral" or the name of a player colour.
        /// </summary>
        public string SelectedOwner
        {
            get => this.OwnerField.Selected;
            set => this.OwnerField.Selected = value;
        }

        /// <summary>
        /// Gets a sentence saying why the owner could not be changed, or nothing.
        /// </summary>
        public string Notice { get; private set; } = string.Empty;

        public bool HasNotice => this.Notice.Length > 0;

        public string Name => this.hero.Name;

        public string ColorHex => PlayerPalette.Of(PlayerColors<Color>.Of(this.hero.Owner));

        public string FactionName => this.hero.Faction.ToString();

        public int Level => this.hero.Level;

        public string OwnerText => OwnerNames<Color>.Of(this.hero.Owner);

        /// <summary>
        /// Gets where the hero is on the map, or nothing for a hero that serves nobody.
        /// </summary>
        public string PositionText => this.hero.Owner == null ? string.Empty : MapLevelText.Coordinates(this.hero.Position.X, this.hero.Position.Y);

        /// <summary>
        /// Gets the line under the name of the hero: its faction, its level and where it stands, with a dot between them, such as "Barbarian · level 1 · X: 3, Y: 4".
        /// </summary>
        public string SubtitleText => string.Join(" · ", new[] { this.FactionName, $"Level {this.Level}", this.PositionText }.Where(t => t.Length > 0));

        /// <summary>
        /// Gets the name of the castle the hero is in, or nothing.
        /// </summary>
        public string CastleName => this.castleName;

        public ICommand OpenCastleCommand { get; }

        /// <summary>
        /// Gets the card of the movement points and the spell points.
        /// </summary>
        public MovementAndSpellPointsCardViewModel MovementAndSpellPoints { get; }

        /// <summary>
        /// Gets the card of the level, the experience and the faction (which is the class of a hero of this game).
        /// </summary>
        public LevelAndClassCardViewModel LevelAndClass { get; }

        /// <summary>
        /// Gets the card of where the hero stands and which way it faces.
        /// </summary>
        public MapPointCardViewModel Position { get; }

        /// <summary>
        /// Gets the card of the marker of the hero.
        /// </summary>
        public MapPointCardViewModel Destination { get; }

        /// <summary>
        /// Gets the card of the adventure spells that are active on the hero this turn.
        /// </summary>
        public ActiveSpellsCardViewModel ActiveSpells { get; }

        public IReadOnlyList<AmountRowViewModel> PrimarySkills => this.primarySkills;

        /// <summary>
        /// Gets the morale and the luck the hero has for its next battle only (a temple, a fountain, a faerie ring).
        /// </summary>
        public IReadOnlyList<AmountRowViewModel> BattleBonus => this.battleBonus;

        /// <summary>
        /// Gets the kinds of places the hero has visited and cannot get anything from a second time.
        /// </summary>
        public IReadOnlyList<VisitedPlaceRowViewModel> VisitedPlaces => this.visitedPlaces;

        /// <summary>
        /// Gets the places that gave the hero luck or morale for the next battle, with the bonus each gave: ticking one gives the hero the bonus,
        /// clearing it takes it away.
        /// </summary>
        public IReadOnlyList<ToggleRowViewModel> BattleSources => this.battleSources;

        /// <summary>
        /// Gets the places that give something once to a hero (the arena).
        /// </summary>
        public IReadOnlyList<ToggleRowViewModel> UsedOnce => this.usedOnce;

        /// <summary>
        /// Gets the places that give something once a week to a hero (the stables).
        /// </summary>
        public IReadOnlyList<ToggleRowViewModel> UsedWeekly => this.usedWeekly;

        /// <summary>
        /// Gets the card of the places that give something once to a hero, which HoMM3 could show too once its savefiles say which ones a hero visited.
        /// </summary>
        public VisitedPlacesCardViewModel PlacesVisited => new("Visited once per hero", this.visitedPlaces, this.usedOnce);

        /// <summary>
        /// Gets the card of the buildings that give something once a week: the game clears what they remember at the start of a week.
        /// </summary>
        public VisitedPlacesCardViewModel PlacesUsedWeekly => new("Used once a week", [], this.usedWeekly);

        /// <summary>
        /// Gets the secondary skills of the hero: the slots of the hero screen and the list of every skill.
        /// </summary>
        public SecondarySkillsViewModel SkillsEditor { get; }

        /// <summary>
        /// Gets the army of the hero: its slots and the finder of creatures.
        /// </summary>
        public ArmyViewModel ArmyEditor { get; }

        /// <summary>
        /// Gets the spells of the hero: the spell book and the list of every spell.
        /// </summary>
        public SpellsViewModel SpellsEditor { get; }

        /// <summary>
        /// Gets the artifacts of the hero: its fourteen slots and the finder of artifacts.
        /// </summary>
        public ArtifactsViewModel ArtifactsEditor { get; }

        public bool HasSpellBook => this.hero.HasSpellBook;

        protected override Siniskalc.Core.Data.IByteEditTarget CreateByteEditTarget(IByteEditTargetFactory factory) => factory.ForHero(this.session);

        protected override void Read()
        {
            var document = this.Game.History.Current;
            var hero = document.Heroes().GetModel(this.HeroId);
            this.hero = hero;

            foreach (var row in this.primarySkills.Concat(this.battleBonus))
            {
                row.Refresh();
            }

            foreach (var row in this.visitedPlaces)
            {
                row.Refresh();
            }

            foreach (var row in this.battleSources.Concat(this.usedOnce).Concat(this.usedWeekly))
            {
                row.Refresh();
            }

            this.MovementAndSpellPoints?.Refresh();
            this.LevelAndClass?.Refresh();
            this.Position?.Refresh();
            this.Destination?.Refresh();
            this.ActiveSpells?.Refresh();
            this.OwnerField?.Refresh();
            this.ArmyEditor?.Refresh();
            this.SkillsEditor?.Refresh();
            this.SpellsEditor?.Refresh();

            this.ArtifactsEditor?.Refresh();

            var castle = document.Castles().GetModels().FirstOrDefault(c => c.IsUsed && c.VisitingHero == this.HeroId);
            this.castleIndex = castle?.Index ?? -1;
            this.castleName = castle?.Name ?? string.Empty;
        }

        private string NextLevelNote()
        {
            var level = this.session.Current.Level;
            if (level >= Constants.Hero.MaxLevel)
            {
                return string.Empty;
            }

            var next = HeroLevels.ExperienceOf(level + 1);
            return string.Create(
                System.Globalization.CultureInfo.CurrentCulture,
                $"{next - this.session.Current.Experience:N0} XP to level {level + 1} (at {next:N0})");
        }

        private void ChangeOwner(string name)
        {
            this.Notice = string.Empty;
            try
            {
                this.session.ChangeOwner(OwnerNames<Color>.Parse(name));
            }
            catch (GameRuleException)
            {
                this.Notice = $"{name} has the most heroes a player can have.";
            }

            this.RaisePropertyChanged(nameof(this.Notice));
            this.RaisePropertyChanged(nameof(this.HasNotice));
            this.OwnerField.Refresh();
        }

        private ToggleRowViewModel Source(string name, HeroEffects effect, int morale, int luck, string toolTip) => new(
            name,
            () => this.session.Current.Effects.HasFlag(effect),
            on => this.session.Update(h => h with
            {
                Effects = on ? h.Effects | effect : h.Effects & ~effect,
                NextBattle = new NextBattleBonus(
                    (int)Math.Clamp(h.NextBattle.Morale + (on ? morale : -morale), Constants.Hero.MinBattleBonus, Constants.Hero.MaxBattleBonus),
                    (int)Math.Clamp(h.NextBattle.Luck + (on ? luck : -luck), Constants.Hero.MinBattleBonus, Constants.Hero.MaxBattleBonus)),
            }),
            toolTip: toolTip + ". Sets what the game remembers and its morale or luck together. Not tried in the game.");

        private ToggleRowViewModel Remembered(string name, HeroEffects effect, string toolTip) => new(
            name,
            () => this.session.Current.Effects.HasFlag(effect),
            on => this.session.Update(h => h with { Effects = on ? h.Effects | effect : h.Effects & ~effect }),
            toolTip: toolTip + ". Not tried in the game.");

        private VisitedPlaceRowViewModel Visited(Game game, PlaceKind kind, string name, string toolTip)
        {
            // The first place of a kind on the map is bit 1 of the hero's set (bit 0 is not used).
            var places = game.Places.Of(kind)
                .Select((position, index) => new ToggleRowViewModel(
                    $"At {MapLevelText.Coordinates(position.X, position.Y)}",
                    () => (this.session.Current.Visited.Of(kind) & (1u << (index + 1))) != 0,
                    visited => this.session.Update(h => h with { Visited = h.Visited.With(kind, visited ? h.Visited.Of(kind) | (1u << (index + 1)) : h.Visited.Of(kind) & ~(1u << (index + 1))) }),
                    toolTip: "Tick it when the hero has visited this one: it gives nothing a second time. Not tried in the game."))
                .ToList();
            var allOnMap = places.Aggregate(0u, (mask, _) => (mask << 1) | 1u) << 1;
            return new(
                name,
                () => System.Numerics.BitOperations.PopCount(this.session.Current.Visited.Of(kind)),
                () => this.session.Update(h => h with { Visited = h.Visited.With(kind, 0) }),
                () => this.session.Update(h => h with { Visited = h.Visited.With(kind, h.Visited.Of(kind) | allOnMap) }),
                toolTip,
                places);
        }

        private AmountRowViewModel Row(string name, long minimum, long maximum, long step, Func<HeroModel, long> get, Func<HeroModel, long, HeroModel> set, Func<string>? note = null) => new(
            name,
            PlayerPalette.Of(PlayerColors<Color>.Of(this.hero?.Owner)),
            minimum,
            maximum,
            step,
            () => get(this.session.Current),
            () => get(this.Game.History.Original.Heroes().GetModel(this.HeroId)),
            value => this.session.Update(h => set(h, value)),
            note);
    }
}
