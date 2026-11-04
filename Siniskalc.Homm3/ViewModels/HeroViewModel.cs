namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// ViewModel that wraps a hero for UI binding.
    /// Reads the hero from a <see cref="IHeroSession"/> and writes every change back through it, so edits land in
    /// the savefile's undo/redo history. Refreshes itself whenever that history changes.
    /// </summary>
    public class HeroViewModel : SessionPageViewModelBase<HeroModel>, IHeroPage
    {
        private readonly IHeroSession session;
        private readonly bool hasUnderground;
        private readonly IByteEditTargetFactory byteEditTargets;
        private readonly VisitedPlacesViewModel visitedPlaces;
        private object shownRevision;

        /// <summary>
        /// Initializes a new instance of HeroViewModel for the hero edited by <paramref name="session"/>.
        /// </summary>
        public HeroViewModel(IHeroSession session, EffectLineProvider effectLines, IMaxSpellPointsCalculator<SecondarySkill, SecondarySkills> spellPoints, IByteEditTargetFactory byteEditTargets, MapPlaces places, bool hasUnderground = true, Action<int>? openCastle = null)
            : base(session.Current.Name, session)
        {
            this.session = session;
            this.hasUnderground = hasUnderground;
            this.byteEditTargets = byteEditTargets;
            this.OpenCastleCommand = new DelegateCommand(
                _ => openCastle?.Invoke(this.GarrisonCastleIndex),
                _ => openCastle != null && this.GarrisonCastleIndex >= 0);
            this.shownRevision = session.Revision;
            this.GeneralInfoViewModel = new GeneralInfoPageViewModel(() => this.HeroModel, this.Update, spellPoints, session.GameVersion, hasUnderground, () => this.CanSleep, this.DestinationsToSendTo, () => this.session.SupportsMovedToday);
            this.visitedPlaces = new VisitedPlacesViewModel(() => this.HeroModel, this.Update, places, session.GameVersion);
            this.SecondarySkillsViewModel = new SecondarySkillsViewModel(
                this.HeroModel.HeroClass.Catalog,
                () => this.HeroModel.SecondarySkills,
                edit => this.Update(h => h with { SecondarySkills = edit(h.SecondarySkills) }));
            this.SpellsViewModel = new SpellsViewModel(
                this.HeroModel.HeroClass.Catalog,
                () => this.HeroModel.SpellBook,
                edit => this.Update(h => h with { SpellBook = edit(h.SpellBook) }));
            this.ArmyViewModel = new ArmyViewModel(
                this.HeroModel.HeroClass.Catalog,
                () => this.HeroModel.Army,
                edit => this.Update(h => h with { Army = edit(h.Army) }));
            this.BackpackViewModel = new BackpackViewModel(
                effectLines,
                this.HeroModel.Backpack,
                this.HeroModel.HeroClass.Catalog,
                backpack => this.Update(h => h with { Backpack = backpack }));
            this.ArtifactsViewModel = new EquippedArtifactsViewModel(
                effectLines,
                this.HeroModel.EquippedArtifacts,
                this.HeroModel.HeroClass.Catalog,
                artifacts => this.Update(h => h.WithEquippedArtifacts(artifacts)),
                artifact => this.BackpackViewModel.Add(artifact),
                () => this.BackpackViewModel.HasFreeSlot);

            this.UpdateIsDirty();
        }

        /// <summary>
        /// Gets the cached general-info view model for this hero.
        /// </summary>
        public GeneralInfoPageViewModel GeneralInfoViewModel { get; }

        /// <summary>
        /// Gets the card of the places of the permanent bonuses the hero visited: the Places tab.
        /// </summary>
        public VisitedPlacesCardViewModel PlacesVisited => this.visitedPlaces.PlacesVisited;

        /// <summary>
        /// Gets the cached creatures view model for this hero.
        /// </summary>
        public ArmyViewModel ArmyViewModel { get; }

        /// <summary>
        /// Gets the secondary skills view model for this hero.
        /// </summary>
        public SecondarySkillsViewModel SecondarySkillsViewModel { get; }

        /// <summary>
        /// Gets the spells view model for this hero.
        /// </summary>
        public SpellsViewModel SpellsViewModel { get; }

        /// <summary>
        /// Gets the cached equipped-artifacts view model for this hero.
        /// </summary>
        public EquippedArtifactsViewModel ArtifactsViewModel { get; }

        /// <summary>
        /// Gets the cached backpack view model for this hero.
        /// </summary>
        public BackpackViewModel BackpackViewModel { get; }

        /// <summary>
        /// Gets the edit session for this hero.
        /// </summary>
        public new IHeroSession Session => this.session;

        /// <summary>
        /// Gets the hero as it is in the current document.
        /// </summary>
        public HeroModel HeroModel => this.session.Current;

        /// <summary>
        /// Gets the command that puts the whole hero back to what the savegame had when it was loaded or last saved.
        /// </summary>

        /// <summary>
        /// Gets where the hero is on the map, such as "at 5, 28, Surface", or nothing for a hero that is not on the map.
        /// </summary>
        public string PositionText => this.HeroModel.Position.X < 0 || this.HeroModel.Position.Y < 0
            ? string.Empty
            : MapLevelText.Coordinates(this.HeroModel.Position.X, this.HeroModel.Position.Y, this.HeroModel.Position.Level);

        /// <summary>
        /// Gets the line under the name of the hero: its class, its level and where it stands, with a dot between them, such as "Barbarian · level 1 · at 3, 4, Surface".
        /// </summary>
        public string SubtitleText => string.Join(" · ", new[] { this.HeroModel.HeroClass.Name, $"Level {this.LevelNumber}", this.PositionText }.Where(t => t.Length > 0));

        /// <inheritdoc/>
        public string Name => this.HeroModel.Name;

        /// <inheritdoc/>
        public string ColorHex => PlayerPalette.Of(this.HeroModel.Owner);

        /// <summary>
        /// Gets the owners to pick from: "Neutral" and the eight player colours.
        /// </summary>
        public IReadOnlyList<string> OwnerChoices { get; } = OwnerNames<PlayerColor>.Choices;

        /// <summary>
        /// Gets or sets the owner, as "Neutral" or the name of a player colour. The lists of heroes of the players and the table of hero owners
        /// change with it (<see cref="PlayerListsReconciler"/>). A hero that is made neutral leaves the map: its position and destination
        /// become -1, -1, -1 and it faces east. A change the editor cannot make as the game would is not made; <see cref="Notice"/> says why.
        /// </summary>
        public string SelectedOwner
        {
            get => OwnerNames<PlayerColor>.Of(this.HeroModel.Owner);
            set
            {
                var owner = OwnerNames<PlayerColor>.Parse(value);
                if (owner == this.HeroModel.Owner)
                {
                    return;
                }

                if (!this.TryUpdate(h => h.WithOwner(owner)))
                {
                    this.RaisePropertyChanged(nameof(this.SelectedOwner));
                }
            }
        }

        /// <summary>
        /// Gets the name of the castle that holds this hero inside it, otherwise an empty text.
        /// </summary>
        public string GarrisonCastleName
        {
            get
            {
                var index = this.GarrisonCastleIndex;
                return index < 0 ? string.Empty : this.session.CastleName(index);
            }
        }

        /// <summary>
        /// Gets the command that opens the castle that holds this hero.
        /// </summary>
        public ICommand OpenCastleCommand { get; }

        /// <summary>
        /// Gets the level and the experience of the hero.
        /// </summary>
        public HeroLevel Level => this.HeroModel.HeroLevel;

        /// <summary>
        /// Gets the hero's level as a number, for display.
        /// </summary>
        public short LevelNumber => (short)this.HeroModel.HeroLevel.Level;

        /// <summary>
        /// Gets or sets the class of the hero.
        /// </summary>
        public HeroClass HeroClass
        {
            get => this.HeroModel.HeroClass;
            set => this.Update(h => h with { HeroClass = value });
        }

        /// <summary>
        /// Gets or sets where the hero is.
        /// </summary>
        public HeroPosition Position
        {
            get => this.HeroModel.Position;
            set => this.Update(h => h with { Position = value });
        }

        /// <summary>
        /// Gets or sets where the hero is going.
        /// </summary>
        public MapPosition Destination
        {
            get => this.HeroModel.Destination;
            set => this.Update(h => h with { Destination = value });
        }

        /// <summary>
        /// Gets or sets the primary skills of the hero.
        /// </summary>
        public PrimarySkills PrimarySkills
        {
            get => this.HeroModel.PrimarySkills;
            set => this.Update(h => h with { PrimarySkills = value });
        }

        /// <summary>
        /// Gets or sets the secondary skills of the hero.
        /// </summary>
        public SecondarySkills SecondarySkills
        {
            get => this.HeroModel.SecondarySkills;
            set => this.Update(h => h with { SecondarySkills = value });
        }

        /// <summary>
        /// Gets or sets the spell book of the hero.
        /// </summary>
        public SpellBook SpellBook
        {
            get => this.HeroModel.SpellBook;
            set => this.Update(h => h with { SpellBook = value });
        }

        /// <summary>
        /// Gets or sets whether the hero has the spell book.
        /// </summary>
        public bool HasSpellBook
        {
            get => this.HeroModel.SpellBook.HasSpellBook;
            set => this.Update(h => h with { SpellBook = h.SpellBook.WithHasSpellBook(value) });
        }

        /// <summary>
        /// Gets or sets the spell points of the hero.
        /// </summary>
        public int CurrentSpellPoints
        {
            get => this.HeroModel.SpellPoints;
            set => this.Update(h => h with { SpellPoints = value });
        }

        /// <summary>
        /// Gets or sets the army of the hero.
        /// </summary>
        public IArmy<Creature> Army
        {
            get => this.HeroModel.Army;
            set => this.Update(h => h with { Army = value });
        }

        /// <summary>
        /// Gets or sets the war machines of the hero.
        /// </summary>
        public IReadOnlyList<WarMachine> WarMachines
        {
            get => this.HeroModel.WarMachines;
            set
            {
                // Lists compare by reference in the record, so compare the contents here.
                if (value == null || this.HeroModel.WarMachines.SequenceEqual(value))
                {
                    return;
                }

                this.Update(h => h with { WarMachines = value });
            }
        }

        /// <summary>
        /// Gets or sets the movement points of the hero.
        /// </summary>
        public MovementPoints MovementPoints
        {
            get => this.HeroModel.MovementPoints;
            set => this.Update(h => h with { MovementPoints = value });
        }

        /// <summary>
        /// Gets the backpack of the hero.
        /// </summary>
        public Backpack Backpack => this.HeroModel.Backpack;

        /// <summary>
        /// Gets the artifacts the hero wears.
        /// </summary>
        public EquippedArtifacts EquippedArtifacts => this.HeroModel.EquippedArtifacts;

        /// <summary>
        /// Gets the game version that wrote the savefile the hero is in.
        /// </summary>
        public GameVersion GameVersion => this.session.GameVersion;

        /// <summary>
        /// Gets whether the savegame is from Horn of the Abyss.
        /// </summary>
        public bool IsHota => this.GameVersion == GameVersion.HornOfTheAbyss;

        /// <summary>
        /// Gets whether the savefile's game version stores that a hero was put to sleep. The checkbox is only shown when it does.
        /// </summary>
        public bool CanSleep => this.session.SupportsAsleep;

        /// <summary>
        /// Gets or sets whether the hero is asleep. A hero that moves wakes up, so this is mostly for waking one that would not move.
        /// </summary>
        public bool IsAsleep
        {
            get => this.HeroModel.IsAsleep;
            set => this.Update(h => h with { IsAsleep = value });
        }

        private int GarrisonCastleIndex
        {
            get
            {
                // Castles hold the id of the hero, which is not its place among the heroes.
                return this.session.GarrisonCastleIndex;
            }
        }

        /// <summary>
        /// Creates a target for the hex editor onto the raw bytes of the hero. The caller disposes it.
        /// </summary>
        /// <returns>The target.</returns>
        public IByteEditTarget CreateByteEditTarget() => this.session.CreateByteEditTarget(this.byteEditTargets);

        /// <inheritdoc/>
        protected override void OnHistoryChanged()
        {
            // Also raised after saving, which moves the baseline without touching the chunk.
            this.UpdateIsDirty();

            // A castle edit can move this hero in or out of a garrison without touching the hero.
            this.RaisePropertyChanged(nameof(this.GarrisonCastleName));

            // Raised for edits to any hero; unchanged chunks keep their instance, so this skips other heroes' edits.
            var revision = this.session.Revision;
            if (ReferenceEquals(revision, this.shownRevision))
            {
                return;
            }

            this.shownRevision = revision;

            var hero = this.HeroModel;
            this.ArtifactsViewModel.Refresh(hero.EquippedArtifacts);
            this.BackpackViewModel.Refresh(hero.Backpack);
            this.GeneralInfoViewModel.Refresh();
            this.visitedPlaces.Refresh();
            this.ArmyViewModel.Refresh();
            this.SecondarySkillsViewModel.Refresh();
            this.SpellsViewModel.Refresh();

            this.RaiseAllPropertiesChanged();
        }

        /// <summary>
        /// Gives the castles, the mines and the dwellings of the player of the hero. The hero at the gate of a castle stands on the castle's own
        /// tile, so that is the entrance, and a castle is taken when another hero is at its gate. A dwelling is sent to by the tile a hero stands
        /// on to flag it, and a mine by the tile of its record, which is only near its entrance. A place is taken when another hero stands on its
        /// tile. Nothing for a hero that serves nobody.
        /// </summary>
        private IReadOnlyList<MapDestination> DestinationsToSendTo()
        {
            var hero = this.HeroModel;
            if (hero.Owner.IsNeutral)
            {
                return [];
            }

            var document = this.session.History.Current;
            var hasLevels = this.hasUnderground;
            var castles = document.Castles();
            var others = document.Heroes().GetModels().Where(h => h.Id != hero.Id && !h.Owner.IsNeutral).ToList();
            bool HeroStandsOn(MapPosition tile) => others.Any(h => h.Position.X == tile.X && h.Position.Y == tile.Y && h.Position.Level == tile.Level);

            var castleDestinations = Enumerable.Range(0, castles.Count)
                .Select(castles.GetModel)
                .Where(c => c.IsUsed && c.Owner == hero.Owner)
                .Select(c => new MapDestination(c.Name, "Castle", c.Position, MapLevelText.Place(c.Position, hasLevels), c.VisitingHeroId is { } visiting && visiting != hero.Id, c.Faction?.Name ?? string.Empty));
            var mineDestinations = document.Mines().GetModels()
                .Where(m => m.Owner == hero.Owner)
                .Select(m => new MapDestination(m.Catalog.MapMetadata.Mine(m.Kind).Name, "Mine", m.Position, MapLevelText.Place(m.Position, hasLevels), HeroStandsOn(m.Position), IsApproximate: true));
            var dwellingDestinations = document.Dwellings().GetModels()
                .Where(d => d.Owner == hero.Owner)
                .Select(d => new MapDestination($"{d.Creature?.Name ?? $"Creature {d.CreatureCode}"} dwelling", "Dwelling", d.Position, MapLevelText.Place(d.Position, hasLevels), HeroStandsOn(d.Position)));
            return castleDestinations.Concat(mineDestinations).Concat(dwellingDestinations).ToList();
        }

        private void Update(Func<HeroModel, HeroModel> edit)
        {
            // History.Changed refreshes the properties.
            this.session.Update(edit);
        }
    }
}
