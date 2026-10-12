namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Data;
    using Siniskalc.Entities;
    using Siniskalc.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// ViewModel that wraps a hero for UI binding.
    /// Reads the hero from a <see cref="HeroEditSession"/> and writes every change back through it, so edits land in
    /// the savefile's undo/redo history. Refreshes itself whenever that history changes.
    /// </summary>
    public class HeroViewModel : PageViewModelBase, IDisposable
    {
        private readonly HeroEditSession session;
        private readonly IByteEditTargetFactory byteEditTargets;
        private object shownRevision;

        /// <summary>
        /// Initializes a new instance of HeroViewModel for the hero edited by <paramref name="session"/>.
        /// </summary>
        public HeroViewModel(HeroEditSession session, EffectLineProvider effectLines, ISpellPointsCalculator spellPoints, IByteEditTargetFactory byteEditTargets, bool hasUnderground = true, Action<int>? openCastle = null)
            : base(session.Current.Name)
        {
            this.session = session;
            this.byteEditTargets = byteEditTargets;
            this.OpenGarrisonCastleCommand = new DelegateCommand(
                _ => openCastle?.Invoke(this.GarrisonCastleIndex),
                _ => openCastle != null && this.GarrisonCastleIndex >= 0);
            this.ResetCommand = new DelegateCommand(_ => this.session.Update(_ => this.OriginalModel), _ => this.HasPendingChanges);
            this.shownRevision = session.Revision;
            this.GeneralInfoViewModel = new GeneralInfoPageViewModel(() => this.HeroModel, this.Update, spellPoints, session.History.Current.GameVersion(), hasUnderground, () => this.CanSleep);
            this.SecondarySkillsViewModel = new SecondarySkillsViewModel(
                this.GameVersion,
                () => this.HeroModel.SecondarySkills,
                edit => this.Update(h => h with { SecondarySkills = edit(h.SecondarySkills) }));
            this.SpellsViewModel = new SpellsViewModel(
                this.GameVersion,
                () => this.HeroModel.SpellBook,
                edit => this.Update(h => h with { SpellBook = edit(h.SpellBook) }));
            this.ArmyViewModel = new ArmyViewModel(
                this.GameVersion,
                () => this.HeroModel.Army,
                edit => this.Update(h => h with { Army = edit(h.Army) }));
            this.BackpackViewModel = new BackpackViewModel(
                effectLines,
                this.HeroModel.Backpack,
                backpack => this.Update(h => h with { Backpack = backpack }),
                this.GameVersion);
            this.ArtifactsViewModel = new EquippedArtifactsViewModel(
                effectLines,
                this.HeroModel.EquippedArtifacts,
                artifacts => this.Update(h => h.WithEquippedArtifacts(artifacts)),
                this.GameVersion,
                artifact => this.BackpackViewModel.Add(artifact),
                () => this.BackpackViewModel.HasFreeSlot);

            this.session.History.Changed += this.OnHistoryChanged;
            this.UpdateIsDirty();
        }

        /// <summary>
        /// Gets the cached general-info view model for this hero.
        /// </summary>
        public GeneralInfoPageViewModel GeneralInfoViewModel { get; }

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
        public HeroEditSession Session => this.session;

        /// <summary>
        /// Gets the hero as it is in the current document.
        /// </summary>
        public HeroModel HeroModel => this.session.Current;

        /// <summary>
        /// Gets the command that puts the whole hero back to what the savegame had when it was loaded or last saved.
        /// </summary>
        public ICommand ResetCommand { get; }

        /// <summary>
        /// Gets where the hero is on the map, such as "X: 5, Y: 28, Surface (0)", or nothing for a hero that is not on the map.
        /// </summary>
        public string PositionText => this.HeroModel.Position.X < 0 || this.HeroModel.Position.Y < 0
            ? string.Empty
            : MapLevelText.Coordinates(this.HeroModel.Position.X, this.HeroModel.Position.Y, this.HeroModel.Position.Z);

        private HeroModel OriginalModel => this.session.History.Original.Heroes().GetModel(this.session.HeroIndex);

        public string Name => this.HeroModel.Name;

        public string ColorHex => PlayerPalette.Of(this.HeroModel.Owner);

        /// <summary>
        /// Gets the owners to pick from: "Neutral" and the eight player colours.
        /// </summary>
        public IReadOnlyList<string> OwnerChoices { get; } = OwnerNames.Choices;

        /// <summary>
        /// Gets or sets the owner, as "Neutral" or the name of a player colour. A hero that is made neutral leaves the map:
        /// its position and marker become -1, -1, -1 and it faces east.
        /// </summary>
        public string SelectedOwner
        {
            get => OwnerNames.Of(this.HeroModel.Owner);
            set
            {
                var owner = OwnerNames.Parse(value);
                if (owner == this.HeroModel.Owner)
                {
                    return;
                }

                this.Update(h => owner != null
                    ? h with { Owner = owner }
                    : h with
                    {
                        Owner = null,
                        Position = new HeroPosition(Entities.Position.None.X, Entities.Position.None.Y, Entities.Position.None.Z, FacingDirection.East),
                        MarkerPosition = Entities.Position.None,
                    });
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
                return index < 0 ? string.Empty : this.session.History.Current.Castles().GetName(index);
            }
        }

        /// <summary>
        /// Gets the command that opens the castle that holds this hero.
        /// </summary>
        public ICommand OpenGarrisonCastleCommand { get; }

        public HeroLevel Level => this.HeroModel.HeroLevel;

        /// <summary>
        /// Gets the hero's level as a number, for display.
        /// </summary>
        public short LevelNumber => this.HeroModel.HeroLevel.Level;

        public HeroClass HeroClass
        {
            get => this.HeroModel.HeroClass;
            set => this.Update(h => h with { HeroClass = value });
        }

        public HeroPosition Position
        {
            get => this.HeroModel.Position;
            set => this.Update(h => h with { Position = value });
        }

        public Position MarkerPosition
        {
            get => this.HeroModel.MarkerPosition;
            set => this.Update(h => h with { MarkerPosition = value });
        }

        public HeroPrimarySkills PrimarySkills
        {
            get => this.HeroModel.PrimarySkills;
            set => this.Update(h => h with { PrimarySkills = value });
        }

        public SecondarySkills SecondarySkills
        {
            get => this.HeroModel.SecondarySkills;
            set => this.Update(h => h with { SecondarySkills = value });
        }

        public SpellBook SpellBook
        {
            get => this.HeroModel.SpellBook;
            set => this.Update(h => h with { SpellBook = value });
        }

        public bool HasSpellBook
        {
            get => this.HeroModel.SpellBook.HasSpellBook;
            set => this.Update(h => h with { SpellBook = h.SpellBook.WithHasSpellBook(value) });
        }

        public ushort CurrentSpellPoints
        {
            get => this.HeroModel.SpellPoints;
            set => this.Update(h => h with { SpellPoints = value });
        }

        public Army Army
        {
            get => this.HeroModel.Army;
            set => this.Update(h => h with { Army = value });
        }

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

        public HeroMovementPoints MovementPoints
        {
            get => this.HeroModel.MovementPoints;
            set => this.Update(h => h with { MovementPoints = value });
        }

        public Backpack Backpack => this.HeroModel.Backpack;

        public EquippedArtifacts EquippedArtifacts => this.HeroModel.EquippedArtifacts;

        /// <summary>
        /// Gets the game version that wrote the savefile the hero is in.
        /// </summary>
        public GameVersion GameVersion => this.session.History.Current.GameVersion();

        public bool IsHota => this.GameVersion == GameVersion.HornOfTheAbyss;

        /// <summary>
        /// Gets whether the savefile's game version stores that a hero was put to sleep. The checkbox is only shown when it does.
        /// </summary>
        public bool CanSleep => this.session.History.Current.Heroes().SupportsAsleep;

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
                return this.session.History.Current.Castles().FindByGarrisonedHero(this.HeroModel.HeroId);
            }
        }

        /// <summary>
        /// Creates a target for the hex editor onto the raw bytes of the hero. The caller disposes it.
        /// </summary>
        public IByteEditTarget CreateByteEditTarget() => this.byteEditTargets.ForHero(this.session);

        public void Dispose()
        {
            this.session.History.Changed -= this.OnHistoryChanged;
        }

        private void Update(Func<HeroModel, HeroModel> edit)
        {
            // History.Changed refreshes the properties.
            this.session.Update(edit);
        }

        private void OnHistoryChanged(object? sender, EventArgs e)
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
            this.ArmyViewModel.Refresh();
            this.SecondarySkillsViewModel.Refresh();
            this.SpellsViewModel.Refresh();

            this.RaiseAllPropertiesChanged();
        }

        /// <summary>
        /// A hero is edited when its bytes differ from the savegame as last loaded or saved.
        /// </summary>
        private void UpdateIsDirty()
        {
            this.HasPendingChanges = this.session.IsDirty;
        }
    }
}
