namespace Siniskalc.Models
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Entities;

    /// <summary>
    /// Immutable view of a hero. Edit it with <c>with</c> expressions or the <c>With...</c> reducers, and write it back through
    /// <see cref="Data.HeroEditSession.Update"/>.
    /// What the equipped artifacts add (spells the hero can cast, primary skill bonuses, the effects as a whole) is worked out
    /// here, every time the artifacts, the spell book or the primary skills are set, so it can never be stale.
    /// </summary>
    public sealed record HeroModel
    {
        private readonly IGrantedSpellsResolver grantedSpellsResolver;
        private HeroPrimarySkills primarySkills;
        private EquippedArtifacts equippedArtifacts;
        private SpellBook spellBook;
        private ArtifactEffectInfo artifactEffects;

        public HeroModel(
            string name,
            IGrantedSpellsResolver grantedSpellsResolver,
            PlayerColor? owner,
            HeroPrimarySkills primarySkills,
            HeroClass heroClass,
            HeroLevel level,
            HeroPosition position,
            Position markerPosition,
            HeroMovementPoints movementPoints,
            SecondarySkills secondarySkills,
            Backpack backpack,
            EquippedArtifacts equippedArtifacts,
            IReadOnlyList<WarMachine> warMachines,
            ushort spellPoints,
            Army army,
            SpellBook spellBook,
            int heroId,
            string? biography,
            HeroGender? gender,
            bool movedToday,
            ActiveAdventureSpells activeSpells,
            bool isAsleep,
            int moraleBonus,
            int luckBonus,
            VisitedBuildingEffects visitEffects)
        {
            this.Name = name;
            this.HeroId = heroId;
            this.Biography = biography;
            this.Gender = gender;
            this.MovedToday = movedToday;
            this.ActiveSpells = activeSpells;
            this.IsAsleep = isAsleep;
            this.MoraleBonus = moraleBonus;
            this.LuckBonus = luckBonus;
            this.VisitEffects = visitEffects;
            this.grantedSpellsResolver = grantedSpellsResolver;
            this.Owner = owner;
            this.primarySkills = primarySkills;
            this.HeroClass = heroClass;
            this.HeroLevel = level;
            this.Position = position;
            this.MarkerPosition = markerPosition;
            this.MovementPoints = movementPoints;
            this.SecondarySkills = secondarySkills;
            this.Backpack = backpack;
            this.equippedArtifacts = equippedArtifacts;
            this.WarMachines = warMachines;
            this.SpellPoints = spellPoints;
            this.Army = army;
            this.spellBook = spellBook;
            this.Reconcile();
        }

        public string Name { get; init; }

        /// <summary>
        /// Gets the id of the hero: its place in the table of heroes of the savefile (0 Orrin, 1 Valeska, ...). A castle holds the
        /// ids of the heroes in it. It is read-only: it is never written back.
        /// </summary>
        public int HeroId { get; init; }

        public PlayerColor? Owner { get; init; }

        /// <summary>
        /// Gets the primary skills: the base values the savefile holds, and what the equipped artifacts add.
        /// </summary>
        public HeroPrimarySkills PrimarySkills
        {
            get => this.primarySkills;
            init
            {
                this.primarySkills = value;
                this.Reconcile();
            }
        }

        public HeroClass HeroClass { get; init; }

        public HeroLevel HeroLevel { get; init; }

        public HeroPosition Position { get; init; }

        public Position MarkerPosition { get; init; }

        public HeroMovementPoints MovementPoints { get; init; }

        public SecondarySkills SecondarySkills { get; init; }

        public Backpack Backpack { get; init; }

        /// <summary>
        /// Gets the equipped artifacts. Setting them updates the spells they grant and the skill bonuses; use
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

        public IReadOnlyList<WarMachine> WarMachines { get; init; }

        public ushort SpellPoints { get; init; }

        public Army Army { get; init; }

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
        /// Returns a copy with <paramref name="artifacts"/> equipped. Besides what setting <see cref="EquippedArtifacts"/> does, an
        /// artifact that gives a spell book (Titan's Thunder) that was just put on gives the hero one. Taking it off does not take
        /// the book away, since there is no telling whether the hero had one before.
        /// </summary>
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
        private void Reconcile()
        {
            this.artifactEffects = this.equippedArtifacts.GetTotalEffects();
            this.spellBook = this.spellBook.WithGrants(this.grantedSpellsResolver.ResolveGrantedSpells(this.equippedArtifacts), this.equippedArtifacts.GrantsSpellBook);
            this.primarySkills = this.primarySkills with
            {
                Bonus = new PrimarySkillBonus(this.artifactEffects.Atk, this.artifactEffects.Def, this.artifactEffects.Pow, this.artifactEffects.Kno),
            };
        }
    }
}
