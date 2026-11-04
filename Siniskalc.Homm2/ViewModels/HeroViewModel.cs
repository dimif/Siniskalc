namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
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
    public sealed class HeroViewModel : SessionPageViewModelBase<HeroModel>, IHeroPage
    {
        private readonly Action<int> openCastle;
        private readonly IEditSession<HeroModel> session;
        private readonly IByteEditTargetFactory byteEditTargets;
        private readonly IReadOnlyList<VisitedPlaceRowViewModel> visitedPlaces;
        private readonly IReadOnlyList<ToggleRowViewModel> usedOnce;
        private readonly IReadOnlyList<ToggleRowViewModel> usedWeekly;
        private HeroModel hero = null!;
        private string castleName = string.Empty;
        private int castleIndex = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeroViewModel"/> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="heroId">The number of the hero.</param>
        /// <param name="openCastle">Opens the page of a castle, by its number.</param>
        /// <param name="spellPoints">Works out the most spell points of the hero.</param>
        /// <param name="byteEditTargets">Makes the targets of the hex editor.</param>
        public HeroViewModel(Game game, int heroId, Action<int> openCastle, IMaxSpellPointsCalculator<SecondarySkill, SecondarySkills> spellPoints, IByteEditTargetFactory byteEditTargets)
            : this(game, game.History.CreateHeroSession(heroId), openCastle, spellPoints, byteEditTargets)
        {
        }

        private HeroViewModel(Game game, IEditSession<HeroModel> session, Action<int> openCastle, IMaxSpellPointsCalculator<SecondarySkill, SecondarySkills> spellPoints, IByteEditTargetFactory byteEditTargets)
            : base("Hero", session)
        {
            var heroId = session.Index;
            this.byteEditTargets = byteEditTargets;
            this.Game = game;
            this.HeroId = heroId;
            this.openCastle = openCastle;
            this.session = session;
            this.PrimarySkills = new PrimarySkillsCardViewModel(
                () => this.session.Current.PrimarySkills,
                edit => this.session.Update(h => h with { PrimarySkills = edit(h.PrimarySkills) }),
                new NumberRange(Constants.Hero.MinPrimarySkill, Constants.Hero.MaxPrimarySkill));
            this.NextBattle = new NextBattleCardViewModel(
                "Next battle",
                [
                    this.Row("Morale", Constants.Hero.MinBattleBonus, Constants.Hero.MaxBattleBonus, 1, h => h.NextBattle.Morale, (h, v) => h with { NextBattle = h.NextBattle with { Morale = (int)v } }, () => "next battle only"),
                    this.Row("Luck", Constants.Hero.MinBattleBonus, Constants.Hero.MaxBattleBonus, 1, h => h.NextBattle.Luck, (h, v) => h with { NextBattle = h.NextBattle with { Luck = (int)v } }, () => "next battle only"),
                ],
                game.Catalog.VisitEffects.Where(e => e.Behaviour == VisitBehaviour.NextBattle).Select(e => this.Source(e.Name, e.Flag, e.Morale, e.Luck, e.Description)).ToList(),
                "What a temple, a fountain, a faerie ring or a graveyard gave. The game takes it away after the next battle. Not tried in the game.",
                () => this.session.Update(h => h with { NextBattle = default, Effects = h.Effects & ~game.Catalog.VisitEffects.Where(e => e.Behaviour == VisitBehaviour.NextBattle).Aggregate(HeroEffects.None, (flags, e) => flags | e.Flag) }));
            this.usedOnce = game.Catalog.VisitEffects.Where(e => e.Behaviour == VisitBehaviour.Once).Select(e => this.Remembered(e.Name, e.Flag, e.Description)).ToList();
            this.usedWeekly = game.Catalog.VisitEffects.Where(e => e.Behaviour == VisitBehaviour.Weekly).Select(e => this.Remembered(e.Name, e.Flag, e.Description)).ToList();
            this.visitedPlaces = game.Catalog.Places.Select(place => this.Visited(game, place.Kind, place.Name, place.Description)).ToList();
            this.ArmyEditor = new ArmyViewModel(
                game.Catalog,
                game.GameVersion,
                () => this.session.Current.Army,
                edit => this.session.Update(h => h with { Army = edit(h.Army) }));
            this.SkillsEditor = new SecondarySkillsViewModel(
                game.Catalog,
                game.GameVersion,
                () => this.session.Current.SecondarySkills,
                edit => this.session.Update(h => h with { SecondarySkills = edit(h.SecondarySkills) }));
            this.SpellsEditor = new SpellsViewModel(
                game.Catalog,
                game.GameVersion,
                () => this.session.Current,
                edit => this.session.Update(edit));
            this.ArtifactsEditor = new ArtifactsViewModel(
                game.Catalog,
                game.GameVersion,
                () => this.session.Current,
                edit => this.session.Update(edit));

            this.OwnerField = new ChoiceFieldViewModel(
                "Owner",
                OwnerNames<PlayerColor>.Choices,
                () => OwnerNames<PlayerColor>.Of(this.session.Current.Owner),
                this.ChangeOwner,
                toolTip: "Which player the hero belongs to. The hero is added to the list of heroes of its new owner and taken from the old one; a neutral hero has no move to finish.");
            this.MovementAndSpellPoints = new MovementAndSpellPointsCardViewModel(
                () => this.session.Current.MovementPoints.Current,
                value => this.session.Update(h => h.WithMovementLeft(value)),
                () => this.session.Current.MovementPoints.Max,
                new NumberRange(Constants.Hero.MinMovementPoints, Constants.Hero.MaxMovementPoints, Constants.Hero.MovementStep),
                () => this.session.Current.SpellPoints,
                value => this.session.Update(h => h.WithSpellPoints((int)value)),
                new NumberRange(Constants.Hero.MinSpellPoints, Constants.Hero.MaxSpellPoints),
                () => spellPoints.GetMaximum(this.session.Current.PrimarySkills, this.session.Current.SecondarySkills));
            this.LevelAndClass = new LevelAndClassCardViewModel(
                () => this.session.Current.HeroLevel.Level,
                value => this.session.Update(h => h.WithLevel((int)value)),
                new NumberRange(Constants.Hero.MinLevel, Constants.Hero.MaxLevel),
                () => this.session.Current.HeroLevel.Experience,
                value => this.session.Update(h => h.WithExperience(value)),
                new NumberRange(Constants.Hero.MinExperience, Constants.Hero.MaxExperience, Constants.Hero.ExperienceStep),
                this.NextLevelNote,
                game.Catalog.Factions.Where(f => !f.IsNeutral).Select(f => f.Name).ToList(),
                () => this.session.Current.Faction.ToString());
            this.Position = MapPointCardViewModel.ForPosition(
                () => this.session.Current.Position,
                position => this.session.Update(h => h with { Position = (HeroPosition)position }),
                new NumberRange(Constants.Hero.MinCoordinate, Constants.Hero.MaxCoordinate),
                getFacing: () => this.session.Current.Position.FacingDirection,
                setFacing: facing => this.session.Update(h => h with { Position = h.Position with { FacingDirection = facing } }),
                destinations: this.DestinationsToSendTo);
            this.Destination = MapPointCardViewModel.ForDestination(
                () => this.session.Current.Destination,
                destination => this.session.Update(h => h with { Destination = destination }),
                new NumberRange(Siniskalc.Heroes.Constants.Map.NoCoordinate, Constants.Hero.MaxCoordinate));
            this.OpenCastleCommand = new DelegateCommand(_ => this.openCastle(this.castleIndex), _ => this.castleIndex >= 0);
            this.Refresh();
        }

        /// <summary>
        /// Gets the number of the hero.
        /// </summary>
        public int HeroId { get; }

        /// <summary>
        /// Gets the field that sets the owner.
        /// </summary>
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

        /// <inheritdoc/>
        public string Name => this.hero.Name;

        /// <inheritdoc/>
        public string ColorHex => PlayerPalette.Of(this.hero.Owner);

        /// <summary>
        /// Gets the name of the faction of the hero.
        /// </summary>
        public string FactionName => this.hero.Faction.ToString();

        /// <summary>
        /// Gets the level of the hero.
        /// </summary>
        public int Level => this.hero.HeroLevel.Level;

        /// <summary>
        /// Gets the owner as text.
        /// </summary>
        public string OwnerText => OwnerNames<PlayerColor>.Of(this.hero.Owner);

        /// <summary>
        /// Gets where the hero is on the map, or nothing for a hero that serves nobody.
        /// </summary>
        public string PositionText => this.hero.Owner.IsNeutral ? string.Empty : MapLevelText.Coordinates(this.hero.Position.X, this.hero.Position.Y);

        /// <summary>
        /// Gets the line under the name of the hero: its faction, its level and where it stands, with a dot between them, such as "Barbarian · level 1 · at 3, 4".
        /// </summary>
        public string SubtitleText => string.Join(" · ", new[] { this.FactionName, $"Level {this.Level}", this.PositionText }.Where(t => t.Length > 0));

        /// <summary>
        /// Gets the name of the castle the hero is in, or nothing.
        /// </summary>
        public string CastleName => this.castleName;

        /// <summary>
        /// Gets the command that opens the page of the castle the hero is in.
        /// </summary>
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
        /// Gets the card of the primary skills: the base the hero learned, the total with its artifacts (what the savegame holds) and what they add.
        /// </summary>
        public PrimarySkillsCardViewModel PrimarySkills { get; }

        /// <summary>
        /// Gets the card of the morale and the luck the hero has for its next battle only, and of the places that gave them (a temple, a
        /// fountain, a faerie ring): ticking one gives the hero the bonus, clearing it takes it away.
        /// </summary>
        public NextBattleCardViewModel NextBattle { get; }

        /// <summary>
        /// Gets the kinds of places the hero has visited and cannot get anything from a second time.
        /// </summary>
        public IReadOnlyList<VisitedPlaceRowViewModel> VisitedPlaces => this.visitedPlaces;

        /// <summary>
        /// Gets the places that give something once to a hero (the arena).
        /// </summary>
        public IReadOnlyList<ToggleRowViewModel> UsedOnce => this.usedOnce;

        /// <summary>
        /// Gets the places that give something once a week to a hero (the stables).
        /// </summary>
        public IReadOnlyList<ToggleRowViewModel> UsedWeekly => this.usedWeekly;

        /// <summary>
        /// Gets the shared card of the places that give something once to a hero.
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

        /// <summary>
        /// Gets whether the hero has the spell book.
        /// </summary>
        public bool HasSpellBook => this.hero.HasSpellBook;

        private Game Game { get; }

        /// <summary>
        /// Creates the target the hex tab of the page works on: the raw bytes of the hero. The caller disposes it. <c>null</c> when the
        /// editor was built without the means to make one, and the page has no hex tab.
        /// </summary>
        /// <returns>The target, or <see langword="null"/> when there is no hex editor.</returns>
        public Siniskalc.Core.Data.IByteEditTarget? CreateByteEditTarget() => this.byteEditTargets.ForHero(this.Game.History, this.HeroId);

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

        private void Read()
        {
            var document = this.Game.History.Current;
            var hero = document.Heroes().GetModel(this.HeroId);
            this.hero = hero;

            this.PrimarySkills.Refresh();
            this.NextBattle.Refresh();

            foreach (var row in this.visitedPlaces)
            {
                row.Refresh();
            }

            foreach (var row in this.usedOnce.Concat(this.usedWeekly))
            {
                row.Refresh();
            }

            this.MovementAndSpellPoints?.Refresh();
            this.LevelAndClass?.Refresh();
            this.Position?.Refresh();
            this.Destination?.Refresh();
            this.OwnerField?.Refresh();
            this.ArmyEditor?.Refresh();
            this.SkillsEditor?.Refresh();
            this.SpellsEditor?.Refresh();

            this.ArtifactsEditor?.Refresh();

            var castle = document.Castles().GetModels().FirstOrDefault(c => c.IsUsed && c.VisitingHeroId == this.HeroId);
            this.castleIndex = castle?.Index ?? -1;
            this.castleName = castle?.Name ?? string.Empty;
        }

        private string NextLevelNote()
        {
            var level = this.session.Current.HeroLevel.Level;
            if (level >= Constants.Hero.MaxLevel)
            {
                return string.Empty;
            }

            var next = HeroLevels.ExperienceOf(level + 1);
            return string.Create(
                System.Globalization.CultureInfo.CurrentCulture,
                $"{next - this.session.Current.HeroLevel.Experience:N0} XP to level {level + 1} (at {next:N0})");
        }

        private void ChangeOwner(string name)
        {
            this.TryUpdate(h => h.WithOwner(OwnerNames<PlayerColor>.Parse(name)));
            this.OwnerField.Refresh();
        }

        /// <summary>
        /// Gives the castles and the mines of the player of the hero. A castle is entered from the tile under its own tile and is taken when
        /// another hero stands there or is in the castle; a mine is sent to by the tile of its record, which is only near its entrance. Nothing
        /// for a hero that serves nobody.
        /// </summary>
        private IReadOnlyList<MapDestination> DestinationsToSendTo()
        {
            var hero = this.session.Current;
            if (hero.Owner.IsNeutral)
            {
                return [];
            }

            var document = this.Game.History.Current;
            var castles = document.Castles();
            var others = document.Heroes().GetModels().Where((h, index) => index != this.HeroId && !h.Owner.IsNeutral).ToList();
            var mines = document.Mines();
            var castleDestinations = castles.UsedIndexes()
                .Select(castles.GetModel)
                .Where(c => c.Owner == hero.Owner)
                .Select(c =>
                {
                    var entrance = c.Position with { Y = c.Position.Y + 1 };
                    var occupied = (c.VisitingHeroId is { } visiting && visiting != this.HeroId) || others.Any(h => h.Position.X == entrance.X && h.Position.Y == entrance.Y);
                    return new MapDestination(c.Name, "Castle", entrance, MapLevelText.Place(entrance.X, entrance.Y), occupied, c.Faction.Name);
                });
            var mineDestinations = Enumerable.Range(0, mines.Count)
                .Select(mines.GetModel)
                .Where(m => m.Owner == hero.Owner)
                .Select(m => new MapDestination(
                    m.Catalog.Mines.Find((byte)m.Kind)!.Name,
                    "Mine",
                    m.Position,
                    MapLevelText.Place(m.Position.X, m.Position.Y),
                    others.Any(h => h.Position.X == m.Position.X && h.Position.Y == m.Position.Y),
                    IsApproximate: true));
            return castleDestinations.Concat(mineDestinations).ToList();
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

        /// <remarks>
        /// The first place of a kind on the map is bit 1 of the hero's set (bit 0 is not used).
        /// </remarks>
        private VisitedPlaceRowViewModel Visited(Game game, PlaceKind kind, string name, string toolTip) =>
            VisitedPlaceRows.Create(
                name,
                toolTip,
                game.Places.Of(kind).Select((position, index) => (index + 1, position)),
                () => this.session.Current.Visited.Of(kind),
                edit => this.session.Update(h => h with { Visited = h.Visited.With(kind, edit(h.Visited.Of(kind))) }));

        private AmountRowViewModel Row(string name, long minimum, long maximum, long step, Func<HeroModel, long> get, Func<HeroModel, long, HeroModel> set, Func<string>? note = null) => new(
            name,
            PlayerPalette.Of(this.hero?.Owner),
            minimum,
            maximum,
            step,
            () => get(this.session.Current),
            () => get(this.Game.History.Original.Heroes().GetModel(this.HeroId)),
            value => this.session.Update(h => set(h, value)),
            note);
    }
}
