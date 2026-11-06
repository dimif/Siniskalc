namespace Siniskalc.Homm3.Models
{
    using System.Collections.Generic;
    using Siniskalc.Core;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Immutable view of a hero. Edit it with <c>with</c> expressions or the <c>With...</c> reducers, and write it back through
    /// <see cref="Data.HeroEditSession.Update"/>.
    /// What the equipped artifacts add (spells the hero can cast, primary skill bonuses, the effects as a whole) is worked out
    /// here, every time the artifacts, the spell book or the primary skills are set, so it can never be stale.
    /// </summary>
    public sealed record HeroModel : IHeroModel<SecondarySkill, SecondarySkills, Creature, PlayerColor>
    {
        private readonly IGrantedSpellsResolver grantedSpellsResolver;
        private PrimarySkills primarySkills;
        private EquippedArtifacts equippedArtifacts;
        private SpellBook spellBook;
        private ArtifactEffectInfo artifactEffects;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeroModel"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="name">The name.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="position">The position.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="movementPoints">The movement points.</param>
        /// <param name="heroLevel">The hero level.</param>
        /// <param name="spellPoints">The spell points.</param>
        /// <param name="primarySkills">The primary skills.</param>
        /// <param name="secondarySkills">The secondary skills.</param>
        /// <param name="army">The army.</param>
        /// <param name="grantedSpellsResolver">The granted spells resolver.</param>
        /// <param name="heroClass">The hero class.</param>
        /// <param name="backpack">The backpack.</param>
        /// <param name="equippedArtifacts">The equipped artifacts.</param>
        /// <param name="warMachines">The war machines.</param>
        /// <param name="spellBook">The spell book.</param>
        /// <param name="biography">The biography.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="movedToday">The moved today.</param>
        /// <param name="activeSpells">The active spells.</param>
        /// <param name="isAsleep">The is asleep.</param>
        /// <param name="moraleBonus">The morale bonus.</param>
        /// <param name="luckBonus">The luck bonus.</param>
        /// <param name="visitEffects">The visit effects.</param>
        /// <param name="placeVisits">The place visits.</param>
        public HeroModel(
            int id,
            string name,
            PlayerColor owner,
            HeroPosition position,
            MapPosition destination,
            MovementPoints movementPoints,
            HeroLevel heroLevel,
            int spellPoints,
            PrimarySkills primarySkills,
            SecondarySkills secondarySkills,
            IArmy<Creature> army,
            IGrantedSpellsResolver grantedSpellsResolver,
            HeroClass heroClass,
            Backpack backpack,
            EquippedArtifacts equippedArtifacts,
            IReadOnlyList<WarMachine> warMachines,
            SpellBook spellBook,
            string? biography,
            HeroGender? gender,
            bool movedToday,
            ActiveAdventureSpells activeSpells,
            bool isAsleep,
            int moraleBonus,
            int luckBonus,
            VisitedBuildingEffects visitEffects,
            PlaceVisits placeVisits)
        {
            this.Name = name;
            this.Id = id;
            this.Biography = biography;
            this.Gender = gender;
            this.MovedToday = movedToday;
            this.ActiveSpells = activeSpells;
            this.IsAsleep = isAsleep;
            this.MoraleBonus = moraleBonus;
            this.LuckBonus = luckBonus;
            this.VisitEffects = visitEffects;
            this.PlaceVisits = placeVisits;
            this.grantedSpellsResolver = grantedSpellsResolver;
            this.Owner = owner;
            this.primarySkills = primarySkills;
            this.HeroClass = heroClass;
            this.HeroLevel = heroLevel;
            this.Position = position;
            this.Destination = destination;
            this.MovementPoints = movementPoints;
            this.SecondarySkills = secondarySkills;
            this.Backpack = backpack;
            this.equippedArtifacts = equippedArtifacts;
            this.WarMachines = warMachines;
            this.SpellPoints = spellPoints;
            this.Army = army;
            this.spellBook = spellBook;
            this.Reconcile(keepSkills: true);
        }

        /// <inheritdoc/>
        public string Name { get; init; }

        /// <summary>
        /// Gets the id of the hero: its place in the table of heroes of the savefile (0 Orrin, 1 Valeska, ...). A castle holds the
        /// ids of the heroes in it. It is read-only: it is never written back.
        /// </summary>
        public int Id { get; init; }

        /// <inheritdoc/>
        public PlayerColor Owner { get; init; }

        /// <summary>
        /// Gets the primary skills: the values the savefile holds, which have what the equipped artifacts add in them. Setting the
        /// skills takes them as they are; setting the equipped artifacts moves them by what the artifacts add or no longer add.
        /// </summary>
        public PrimarySkills PrimarySkills
        {
            get => this.primarySkills;
            init
            {
                this.primarySkills = value;
                this.Reconcile(keepSkills: true);
            }
        }

        /// <summary>
        /// Gets the class of the hero.
        /// </summary>
        public HeroClass HeroClass { get; init; }

        /// <inheritdoc/>
        public HeroLevel HeroLevel { get; init; }

        /// <inheritdoc/>
        public HeroPosition Position { get; init; }

        /// <inheritdoc/>
        public MapPosition Destination { get; init; }

        /// <inheritdoc/>
        public MovementPoints MovementPoints { get; init; }

        /// <inheritdoc/>
        public SecondarySkills SecondarySkills { get; init; }

        /// <summary>
        /// Gets the backpack of the hero.
        /// </summary>
        public Backpack Backpack { get; init; }

        /// <summary>
        /// Gets the equipped artifacts. Setting them updates the spells they grant and the primary skills (the game saves the skills with
        /// the bonuses of the artifacts in them, so they go up or down by what the artifacts add); use
        /// <see cref="WithEquippedArtifacts"/> to also give the hero the spell book Titan's Thunder brings.
        /// </summary>
        public EquippedArtifacts EquippedArtifacts
        {
            get => this.equippedArtifacts;
            init
            {
                this.equippedArtifacts = value;
                this.Reconcile();
            }
        }

        /// <summary>
        /// Gets what all the equipped artifacts do together.
        /// </summary>
        public ArtifactEffectInfo ArtifactEffects => this.artifactEffects;

        /// <summary>
        /// Gets the war machines the hero has.
        /// </summary>
        public IReadOnlyList<WarMachine> WarMachines { get; init; }

        /// <inheritdoc/>
        public int SpellPoints { get; init; }

        /// <inheritdoc/>
        public IArmy<Creature> Army { get; init; }

        /// <summary>
        /// Gets the hero's spell book: whether it has one, the spells it learned and the ones artifacts let it cast.
        /// </summary>
        public SpellBook SpellBook
        {
            get => this.spellBook;
            init
            {
                this.spellBook = value;
                this.Reconcile();
            }
        }

        /// <summary>
        /// Gets the biography stored with the hero in the savefile, or null if it has none. It is read-only.
        /// </summary>
        public string? Biography { get; init; }

        /// <summary>
        /// Gets the gender of the hero, or null if the savefile holds a value this editor does not know.
        /// </summary>
        public HeroGender? Gender { get; init; }

        /// <summary>
        /// Gets whether the hero has moved today: set when the hero moves, cleared on a new day.
        /// Only Horn of the Abyss savefiles have the flag; for the other versions it is always false (Shadow of Death has no such byte).
        /// </summary>
        public bool MovedToday { get; init; }

        /// <summary>
        /// Gets the adventure spells the hero has cast today. Confirmed for Horn of the Abyss and Shadow of Death savefiles.
        /// </summary>
        public ActiveAdventureSpells ActiveSpells { get; init; }

        /// <summary>
        /// Gets whether the hero has been put to sleep (it does not move until it is woken, or until it is moved, which wakes it).
        /// Only confirmed for Shadow of Death and Horn of the Abyss savefiles; for the other versions it is always false.
        /// </summary>
        public bool IsAsleep { get; init; }

        /// <summary>
        /// Gets the morale that the buildings the hero visited gave it, until the next battle (-3 to +3). Read-only: it is never written back.
        /// </summary>
        public int MoraleBonus { get; init; }

        /// <summary>
        /// Gets the luck that the buildings the hero visited gave it, until the next battle (-3 to +3). Read-only: it is never written back.
        /// </summary>
        public int LuckBonus { get; init; }

        /// <summary>
        /// Gets the visited buildings whose effect is still on the hero (until the next battle, or the end of the week for the stables).
        /// Read-only: it is never written back.
        /// </summary>
        public VisitedBuildingEffects VisitEffects { get; init; }

        /// <summary>
        /// Gets the places of the permanent bonuses (arenas, schools, stars axes...) the hero visited, which give nothing a second time.
        /// </summary>
        public PlaceVisits PlaceVisits { get; init; }

        /// <inheritdoc/>
        public bool HasSpellBook => this.SpellBook.HasSpellBook;

        /// <inheritdoc/>
        public IReadOnlyCollection<ISpell> LearnedSpells => [.. this.SpellBook.Learned];

        /// <summary>
        /// Returns the hero at <paramref name="level"/>, with the experience that level starts at.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>The new hero; this one is not changed.</returns>
        public HeroModel WithLevel(int level)
        {
            RangeGuard.ThrowIfOutOfRange<long>(level, Constants.Hero.MinLevel, Constants.Hero.MaxLevel, nameof(level));
            return this with { HeroLevel = HeroLevel.AtLevel(LevelCalculator.Table, level) };
        }

        /// <summary>
        /// Returns the hero with <paramref name="experience"/> points, at the level the game gives that much experience.
        /// </summary>
        /// <param name="experience">The experience points.</param>
        /// <returns>The new hero; this one is not changed.</returns>
        public HeroModel WithExperience(long experience)
        {
            RangeGuard.ThrowIfOutOfRange(experience, Constants.Hero.MinExperience, Constants.Hero.MaxExperience, nameof(experience));
            return this with { HeroLevel = HeroLevel.AtExperience(LevelCalculator.Table, experience) };
        }

        /// <summary>
        /// Returns the hero with <paramref name="movement"/> movement points left today; the movement of a full day stays.
        /// </summary>
        /// <param name="movement">The movement points left.</param>
        /// <returns>The new hero; this one is not changed.</returns>
        public HeroModel WithMovementLeft(long movement) => this with { MovementPoints = this.MovementPoints with { Current = movement } };

        /// <summary>
        /// Returns the hero as the game leaves it after it visited <paramref name="effect"/> (<paramref name="visited"/> is true), or as it was
        /// before the visit. The bit that remembers the visit and what the visit gave (morale, luck, and the movement of the stables, to the
        /// points left and to the most of the day) change together, so the hero is never left with the bit and not the bonus.
        /// A hero that already is in the state asked for is returned as it is.
        /// </summary>
        /// <param name="effect">What the visit gives.</param>
        /// <param name="visited">True for a hero that visited the building, false for one that did not.</param>
        /// <returns>The new hero; this one is not changed.</returns>
        public HeroModel WithVisitedBuilding(Data.VisitEffectDefinition effect, bool visited)
        {
            if (this.VisitEffects.HasFlag(effect.Effect) == visited)
            {
                return this;
            }

            var sign = visited ? 1 : -1;
            return this with
            {
                VisitEffects = visited ? this.VisitEffects | effect.Effect : this.VisitEffects & ~effect.Effect,
                MoraleBonus = RangeGuard.Clamp(this.MoraleBonus + (sign * effect.Morale), -Constants.Hero.MaxMoraleOrLuck, Constants.Hero.MaxMoraleOrLuck),
                LuckBonus = RangeGuard.Clamp(this.LuckBonus + (sign * effect.Luck), -Constants.Hero.MaxMoraleOrLuck, Constants.Hero.MaxMoraleOrLuck),
                MovementPoints = new MovementPoints(
                    RangeGuard.Clamp(this.MovementPoints.Current + (sign * effect.Movement), Constants.Hero.MinMovementPoints, Constants.Hero.MaxMovementPoints),
                    RangeGuard.Clamp(this.MovementPoints.Max + (sign * effect.Movement), Constants.Hero.MinMovementPoints, Constants.Hero.MaxMovementPoints)),
            };
        }

        /// <summary>
        /// Returns the hero with <paramref name="spellPoints"/> spell points.
        /// </summary>
        /// <param name="spellPoints">The spell points.</param>
        /// <returns>The new hero; this one is not changed.</returns>
        public HeroModel WithSpellPoints(int spellPoints) => this with { SpellPoints = spellPoints };

        /// <summary>
        /// Returns the hero in the service of <paramref name="owner"/>. A hero that serves nobody leaves the map: its position and
        /// destination become -1, -1, -1 (as the game stores a hero in a tavern) and it faces east.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The new hero; this one is not changed.</returns>
        public HeroModel WithOwner(PlayerColor owner) => owner.IsNeutral
            ? this with
            {
                Owner = owner,
                Position = new HeroPosition(MapPosition.None.X, MapPosition.None.Y, MapPosition.None.Level, FacingDirection.East),
                Destination = MapPosition.None,
            }
            : this with { Owner = owner };

        /// <summary>
        /// Returns a copy with <paramref name="artifacts"/> equipped. Besides what setting <see cref="EquippedArtifacts"/> does, an
        /// artifact that gives a spell book (Titan's Thunder) that was just put on gives the hero one. Taking it off does not take
        /// the book away, since there is no telling whether the hero had one before.
        /// </summary>
        /// <param name="artifacts">The artifacts.</param>
        /// <returns>The new hero; this one is not changed.</returns>
        public HeroModel WithEquippedArtifacts(EquippedArtifacts artifacts)
        {
            var updated = this with { EquippedArtifacts = artifacts };
            return !this.EquippedArtifacts.GrantsSpellBook && artifacts.GrantsSpellBook && !updated.SpellBook.HasSpellBook
                ? updated with { SpellBook = updated.SpellBook.WithHasSpellBook(true) }
                : updated;
        }

        /// <summary>
        /// Brings what depends on the equipped artifacts in line with them. Runs whenever one of the inputs is set, and does the
        /// same every time, so the order in which a <c>with</c> expression sets them does not matter.
        /// </summary>
        private void Reconcile(bool keepSkills = false)
        {
            this.artifactEffects = this.equippedArtifacts.GetTotalEffects();
            this.spellBook = this.spellBook.WithGrants(this.grantedSpellsResolver.ResolveGrantedSpells(this.equippedArtifacts), this.equippedArtifacts.GrantsSpellBook);
            var bonus = new PrimarySkillBonus(this.artifactEffects.Atk, this.artifactEffects.Def, this.artifactEffects.Pow, this.artifactEffects.Kno);

            // Skills that are set, and the ones the savefile held, already have their bonus in them: only the bonus is updated to follow.
            this.primarySkills = keepSkills ? this.primarySkills with { Bonus = bonus } : this.primarySkills.WithBonus(bonus);
        }
    }
}
