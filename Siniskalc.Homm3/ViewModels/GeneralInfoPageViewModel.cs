namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// ViewModel for the general info page of a hero: class, level, movement, position, primary skills and war machines.
    /// Secondary skills and spells have their own pages.
    /// Reads from the hero it is given and writes every edit back through <c>update</c>, so nothing is cached here.
    /// Call <see cref="Refresh"/> after the hero changed.
    /// </summary>
    public sealed class GeneralInfoPageViewModel : PageViewModelBase
    {
        private const int WarMachineCheckBoxCount = 5;
        private const uint HighestMovement = (uint)Constants.Hero.MaxMovementPoints;
        private const ushort HighestSpellPoints = (ushort)Constants.Hero.MaxSpellPoints;

        private readonly Func<HeroModel> getHero;
        private readonly Action<Func<HeroModel, HeroModel>> update;
        private readonly ISpellPointsCalculator spellPoints;
        private readonly GameVersion gameVersion;
        private readonly bool hasUnderground;
        private readonly Func<bool> canSleep;
        private bool isRefreshing;

        public GeneralInfoPageViewModel(Func<HeroModel> getHero, Action<Func<HeroModel, HeroModel>> update, ISpellPointsCalculator spellPoints, GameVersion gameVersion, bool hasUnderground = true, Func<bool>? canSleep = null)
            : base("General Info")
        {
            this.getHero = getHero;
            this.update = update;
            this.spellPoints = spellPoints;
            this.gameVersion = gameVersion;
            this.hasUnderground = hasUnderground;
            this.canSleep = canSleep ?? (() => false);

            this.RestoreMovementCommand = new DelegateCommand(
                _ => this.CurrentMovement = this.MaxMovement,
                _ => this.CurrentMovement != this.MaxMovement);
            this.MaxMovementCommand = new DelegateCommand(
                _ => this.CurrentMovement = HighestMovement,
                _ => this.CurrentMovement != HighestMovement);
            this.MaxSpellPointsCommand = new DelegateCommand(
                _ => this.CurrentSpellPoints = HighestSpellPoints,
                _ => this.CurrentSpellPoints != HighestSpellPoints);
            this.ResetSpellPointsCommand = new DelegateCommand(
                _ => this.CurrentSpellPoints = (ushort)Math.Min(this.MaxSpellPoints, HighestSpellPoints),
                _ => this.CurrentSpellPoints != Math.Min(this.MaxSpellPoints, HighestSpellPoints));
            this.RotateLeftCommand = new DelegateCommand(_ => this.Rotate(-1));
            this.RotateRightCommand = new DelegateCommand(_ => this.Rotate(1));
            this.RemoveMarkerCommand = new DelegateCommand(
                _ => this.Update(h => h with { MarkerPosition = MapPosition.None }),
                _ => this.Hero.MarkerPosition != MapPosition.None);
            this.SelectAllWarMachinesCommand = new DelegateCommand(_ => this.SelectAllWarMachines());

            this.WarMachinesChecked = new ObservableCollection<bool>(new bool[WarMachineCheckBoxCount]);

            this.RefreshWarMachines();
        }

        public string HeroName => this.Hero.Name;

        public short Level
        {
            get => (short)this.Hero.HeroLevel.Level;
            set
            {
                if (value != this.Level && RangeGuard.IsInRange<long>(value, Constants.Hero.MinLevel, Constants.Hero.MaxLevel))
                {
                    this.SetLevelAndExperience(value, LevelCalculator.GetXpForLevel(value));
                }
            }
        }

        public int Experience
        {
            get => (int)this.Hero.HeroLevel.Experience;
            set
            {
                value = (int)RangeGuard.Clamp<long>(value, Constants.Hero.MinExperience, Constants.Hero.MaxExperience);
                if (value != this.Experience)
                {
                    this.SetLevelAndExperience(LevelCalculator.GetLevelFromXp(value), value);
                }
            }
        }

        public string HeroClass
        {
            get => this.Hero.HeroClass.Name;
            set
            {
                var heroClass = AbstractEnumeration.FromName<Entities.HeroClass>(value);
                if (heroClass != null && heroClass != this.Hero.HeroClass)
                {
                    this.Update(h => h with { HeroClass = heroClass });
                }
            }
        }

        public string Gender
        {
            get => this.Hero.Gender?.ToString() ?? string.Empty;
            set
            {
                if (Enum.TryParse<Entities.HeroGender>(value, out var gender) && gender != this.Hero.Gender)
                {
                    this.Update(h => h with { Gender = gender });
                }
            }
        }

        public uint CurrentMovement
        {
            get => (uint)this.Hero.MovementPoints.Current;
            set
            {
                value = (uint)RangeGuard.Clamp<long>(value, Constants.Hero.MinMovementPoints, Constants.Hero.MaxMovementPoints);
                if (value != this.CurrentMovement)
                {
                    this.Update(h => h with { MovementPoints = h.MovementPoints with { Current = value } });
                }
            }
        }

        /// <summary>
        /// Gets the command that gives the hero 99999 movement points, more than the game ever grants.
        /// </summary>
        public ICommand MaxMovementCommand { get; }

        /// <summary>
        /// Gets the command that gives the hero 999 spell points, more than the game ever grants.
        /// </summary>
        public ICommand MaxSpellPointsCommand { get; }

        public uint MaxMovement => (uint)this.Hero.MovementPoints.Max;

        /// <summary>
        /// Gets the command that gives the hero all of its movement points back.
        /// </summary>
        public ICommand RestoreMovementCommand { get; }

        /// <summary>
        /// Gets the most spell points the hero can have, from knowledge and Intelligence.
        /// </summary>
        public int MaxSpellPoints => this.spellPoints.GetMaximum(this.gameVersion, this.Hero);

        public string MaxSpellPointsText => $"Maximum {this.MaxSpellPoints}";

        /// <summary>
        /// Gets the command that fills the hero's spell points up to the maximum.
        /// </summary>
        public ICommand ResetSpellPointsCommand { get; }

        public ushort CurrentSpellPoints
        {
            get => this.Hero.SpellPoints;
            set
            {
                value = (ushort)RangeGuard.Clamp<long>(value, Constants.Hero.MinSpellPoints, Constants.Hero.MaxSpellPoints);
                if (value != this.CurrentSpellPoints)
                {
                    this.Update(h => h with { SpellPoints = value });
                }
            }
        }

        public short PositionX
        {
            get => Coordinate(this.Hero.Position.X);
            set => this.SetPosition(this.PositionX, ClampCoordinate(value), p => p with { X = ClampCoordinate(value) });
        }

        public short PositionY
        {
            get => Coordinate(this.Hero.Position.Y);
            set => this.SetPosition(this.PositionY, ClampCoordinate(value), p => p with { Y = ClampCoordinate(value) });
        }

        public int PositionZ
        {
            get => this.Hero.Position.Level;
            set => this.SetPosition(this.PositionZ, value, p => p with { Level = value });
        }

        public FacingDirection FacingDirection
        {
            get => this.Hero.Position.FacingDirection;
            set => this.SetPosition(this.FacingDirection, value, p => p with { FacingDirection = value });
        }

        public short MarkerX
        {
            get => Coordinate(this.Hero.MarkerPosition.X);
            set => this.SetMarker(this.MarkerX, ClampCoordinate(value), p => p with { X = ClampCoordinate(value) });
        }

        public short MarkerY
        {
            get => Coordinate(this.Hero.MarkerPosition.Y);
            set => this.SetMarker(this.MarkerY, ClampCoordinate(value), p => p with { Y = ClampCoordinate(value) });
        }

        public int MarkerZ
        {
            get => this.Hero.MarkerPosition.Level;
            set => this.SetMarker(this.MarkerZ, value, p => p with { Level = value });
        }

        /// <summary>
        /// Gets the levels the hero's position can be on. Has "None" too when the position is -1, as it is for a hero that is not on the map.
        /// </summary>
        public IReadOnlyList<MapLevelChoice> PositionLevelChoices => this.LevelChoices(this.Hero.Position.Level);

        public IReadOnlyList<MapLevelChoice> MarkerLevelChoices => this.LevelChoices(this.Hero.MarkerPosition.Level);

        public ICommand RotateLeftCommand { get; }

        public ICommand RotateRightCommand { get; }

        /// <summary>
        /// Gets the command that sets the marker's X, Y and level to -1, which is how the savefile says there is none.
        /// </summary>
        public ICommand RemoveMarkerCommand { get; }

        /// <summary>
        /// Gets the command that gives the hero every war machine it can have.
        /// </summary>
        public ICommand SelectAllWarMachinesCommand { get; }

        public byte Attack
        {
            get => ClampSkill(this.Hero.PrimarySkills.BaseAttack);
            set => this.SetPrimarySkills(this.Attack, ClampSkill(value), s => s.WithBaseAttack(ClampSkill(value)));
        }

        public byte Defense
        {
            get => ClampSkill(this.Hero.PrimarySkills.BaseDefense);
            set => this.SetPrimarySkills(this.Defense, ClampSkill(value), s => s.WithBaseDefense(ClampSkill(value)));
        }

        public byte SpellPower
        {
            get => ClampSkill(this.Hero.PrimarySkills.BaseSpellPower);
            set => this.SetPrimarySkills(this.SpellPower, ClampSkill(value), s => s.WithBaseSpellPower(ClampSkill(value)));
        }

        public byte Knowledge
        {
            get => ClampSkill(this.Hero.PrimarySkills.BaseKnowledge);
            set => this.SetPrimarySkills(this.Knowledge, ClampSkill(value), s => s.WithBaseKnowledge(ClampSkill(value)));
        }

        /// <summary>
        /// Gets the attack skill with what the hero's artifacts add, which is what counts in the game and what the savefile holds.
        /// <see cref="Attack"/> is what is left without the artifacts.
        /// </summary>
        public int AttackTotal => this.Hero.PrimarySkills.Attack;

        public int DefenseTotal => this.Hero.PrimarySkills.Defense;

        public int SpellPowerTotal => this.Hero.PrimarySkills.SpellPower;

        public int KnowledgeTotal => this.Hero.PrimarySkills.Knowledge;

        /// <summary>
        /// Gets what the hero's artifacts add to the attack skill, such as "+2 from artifacts". Empty when they add nothing.
        /// </summary>
        public string AttackArtifactNote => SkillNote(this.ArtifactEffects.Atk);

        public string DefenseArtifactNote => SkillNote(this.ArtifactEffects.Def);

        public string SpellPowerArtifactNote => SkillNote(this.ArtifactEffects.Pow);

        public string KnowledgeArtifactNote => SkillNote(this.ArtifactEffects.Kno);

        public string? Biography => this.Hero.Biography;

        public bool HasBiography => !string.IsNullOrEmpty(this.Hero.Biography);

        /// <summary>
        /// Gets how much experience the hero needs for the next level, such as "2,440 XP to level 8 (at 10,000)".
        /// Empty at the highest level.
        /// </summary>
        public string NextLevelNote
        {
            get
            {
                var next = LevelCalculator.GetNextLevel((int)this.Hero.HeroLevel.Experience);
                return next == null
                    ? string.Empty
                    : string.Create(
                        System.Globalization.CultureInfo.CurrentCulture,
                        $"{next.Value.Experience - this.Hero.HeroLevel.Experience:N0} XP to level {next.Value.Level} (at {next.Value.Experience:N0})");
            }
        }

        /// <summary>
        /// Gets what the hero's artifacts add to the movement on land, such as "+600 from artifacts". Empty when they add nothing.
        /// </summary>
        public string MovementArtifactNote => this.ArtifactEffects.LandMovement == 0
            ? string.Empty
            : $"{this.ArtifactEffects.LandMovement:+0;-0} from artifacts";

        public string MaxMovementText => $"Maximum {this.MaxMovement}";

        /// <summary>
        /// Gets which war machine check boxes are ticked. Order: ballista, ballista (HotA), cannon, ammo cart, first aid tent.
        /// </summary>
        public ObservableCollection<bool> WarMachinesChecked { get; }

        /// <summary>
        /// Gets or sets whether the hero has moved today. Only Horn of the Abyss savefiles have the flag; it does nothing for the others.
        /// </summary>
        public bool MovedToday
        {
            get => this.Hero.MovedToday;
            set => this.Update(h => h with { MovedToday = value });
        }

        /// <summary>
        /// Gets whether the savefile's game version stores that a hero was put to sleep. The check box is only shown when it does.
        /// </summary>
        public bool CanSleep => this.canSleep();

        /// <summary>
        /// Gets or sets whether the hero is asleep. A hero that moves wakes up, so this is mostly for waking one that would not move.
        /// </summary>
        public bool IsAsleep
        {
            get => this.Hero.IsAsleep;
            set => this.Update(h => h with { IsAsleep = value });
        }

        public bool DisguiseActive
        {
            get => this.Hero.ActiveSpells.HasFlag(ActiveAdventureSpells.Disguise);
            set => this.SetActive(ActiveAdventureSpells.Disguise, value);
        }

        public bool FlyActive
        {
            get => this.Hero.ActiveSpells.HasFlag(ActiveAdventureSpells.Fly);
            set => this.SetActive(ActiveAdventureSpells.Fly, value);
        }

        public bool WaterWalkActive
        {
            get => this.Hero.ActiveSpells.HasFlag(ActiveAdventureSpells.WaterWalk);
            set => this.SetActive(ActiveAdventureSpells.WaterWalk, value);
        }

        public bool VisionsActive
        {
            get => this.Hero.ActiveSpells.HasFlag(ActiveAdventureSpells.Visions);
            set => this.SetActive(ActiveAdventureSpells.Visions, value);
        }

        public bool StablesVisited => this.Hero.VisitEffects.HasFlag(VisitedBuildingEffects.Stables);

        public bool SwanPondVisited => this.Hero.VisitEffects.HasFlag(VisitedBuildingEffects.SwanPond);

        public bool WateringHoleVisited => this.Hero.VisitEffects.HasFlag(VisitedBuildingEffects.WateringHole);

        public bool CryptVisited => this.Hero.VisitEffects.HasFlag(VisitedBuildingEffects.Crypt);

        public bool FaerieRingVisited => this.Hero.VisitEffects.HasFlag(VisitedBuildingEffects.FaerieRing);

        public bool FountainOfFortuneVisited => this.Hero.VisitEffects.HasFlag(VisitedBuildingEffects.FountainOfFortune);

        public bool IdolOfFortuneVisited => this.Hero.VisitEffects.HasFlag(VisitedBuildingEffects.IdolOfFortune);

        /// <summary>
        /// Gets the hero's morale such as "+2": what Leadership, the artifacts and the visited buildings add, between -3 and +3. The game
        /// also counts how mixed the army is, which is not worked out here.
        /// </summary>
        public string MoraleText => FormatSigned(this.Clamp(this.LeadershipBonus + this.ArtifactEffects.Morale + this.Hero.MoraleBonus));

        /// <summary>
        /// Gets what changes the morale, such as "Leadership +1, buildings +1": only what is not 0, and nothing when nothing is.
        /// </summary>
        public string MoraleBreakdown => Modifiers(("Leadership", this.LeadershipBonus), ("artifacts", this.ArtifactEffects.Morale), ("buildings", this.Hero.MoraleBonus));

        /// <summary>
        /// Gets the hero's luck such as "+1": what the Luck skill, the artifacts and the visited buildings add, between -3 and +3.
        /// </summary>
        public string LuckText => FormatSigned(this.Clamp(this.LuckSkillBonus + this.ArtifactEffects.Luck + this.Hero.LuckBonus));

        /// <summary>
        /// Gets what changes the luck, such as "Luck +2, artifacts +1": only what is not 0, and nothing when nothing is.
        /// </summary>
        public string LuckBreakdown => Modifiers(("Luck", this.LuckSkillBonus), ("artifacts", this.ArtifactEffects.Luck), ("buildings", this.Hero.LuckBonus));

        public bool IsHota => this.gameVersion == GameVersion.HornOfTheAbyss;

        /// <summary>
        /// Gets whether the ballista is shown as a plain check box; in Horn of the Abyss it shares its place with the cannon, so it is a radio button.
        /// </summary>
        public bool HasSingleBallista => !this.IsHota;

        public IEnumerable<string> AvailableHeroClasses =>
            Entities.HeroClass.All.Where(c => c.IsAvailableIn(this.gameVersion)).Select(c => c.Name).ToList();

        public IEnumerable<string> AvailableGenders => Enum.GetNames<Entities.HeroGender>();

        public Array AvailableFacingDirections => Enum.GetValues(typeof(FacingDirection));

        private HeroModel Hero => this.getHero();

        private ArtifactEffectInfo ArtifactEffects => this.Hero.ArtifactEffects;

        /// <summary>
        /// Shows the current hero again. Call after the hero changed.
        /// </summary>
        public void Refresh()
        {
            this.isRefreshing = true;
            try
            {
                this.RefreshWarMachines();

                this.RaiseAllPropertiesChanged();
            }
            finally
            {
                this.isRefreshing = false;
            }
        }

        public void UpdateWarMachine(string warMachineName, bool isChecked)
        {
            if (this.isRefreshing)
            {
                return;
            }

            var warMachine = GetWarMachine(warMachineName);
            if (warMachine == null)
            {
                return;
            }

            var current = this.Hero.WarMachines;
            if (current.Contains(warMachine) == isChecked)
            {
                return;
            }

            // Ballista and cannon share one place in the savefile, so only one of them can be kept.
            IReadOnlyList<WarMachine> updated = isChecked
                ? [.. current.Where(w => w.Slot != warMachine.Slot), warMachine]
                : [.. current.Where(w => w != warMachine)];

            this.Update(h => h with { WarMachines = updated });
        }

        private static short ClampCoordinate(short value) => (short)RangeGuard.Clamp<long>(value, Constants.Map.MinCoordinate, Constants.Map.MaxCoordinate);

        private static byte ClampSkill(int value) => (byte)RangeGuard.Clamp<long>(value, Constants.Hero.MinPrimarySkill, Constants.Hero.MaxPrimarySkill);

        private static string SkillNote(short fromArtifacts) => fromArtifacts == 0
            ? string.Empty
            : $"{fromArtifacts:+0;-0} from artifacts";

        private static WarMachine? GetWarMachine(string name) => name switch
        {
            "Ballista" => WarMachine.Ballista,
            "Cannon" => WarMachine.Cannon,
            "Ammo_Cart" => WarMachine.AmmoCart,
            "First_Aid_Tent" => WarMachine.FirstAidTent,
            _ => null,
        };

        private IReadOnlyList<MapLevelChoice> LevelChoices(int current)
        {
            var choices = new List<MapLevelChoice> { new(MapPosition.Above, MapLevelText.Of(MapPosition.Above)) };
            if (this.hasUnderground || current == MapPosition.Underground)
            {
                choices.Add(new(MapPosition.Underground, MapLevelText.Of(MapPosition.Underground)));
            }

            if (current != MapPosition.Above && current != MapPosition.Underground)
            {
                choices.Add(new(current, MapLevelText.Of(current)));
            }

            return choices;
        }

        private static string Modifiers(params (string Name, int Value)[] parts) =>
            string.Join(", ", parts.Where(p => p.Value != 0).Select(p => $"{p.Name} {FormatSigned(p.Value)}"));

        private static string FormatSigned(int value) => value.ToString("+0;-0;+0", System.Globalization.CultureInfo.InvariantCulture);

        private int LeadershipBonus => (int)(this.Hero.SecondarySkills.GetSkillLevel(SecondarySkill.Leadership) ?? 0);

        private int LuckSkillBonus => (int)(this.Hero.SecondarySkills.GetSkillLevel(SecondarySkill.Luck) ?? 0);

        private int Clamp(int value) => Math.Clamp(value, -Constants.Hero.MaxMoraleOrLuck, Constants.Hero.MaxMoraleOrLuck);

        private void SetActive(ActiveAdventureSpells spell, bool active)
        {
            this.Update(h => h with { ActiveSpells = active ? h.ActiveSpells | spell : h.ActiveSpells & ~spell });
        }

        private void Rotate(int turns)
        {
            var count = Enum.GetValues<FacingDirection>().Length;
            this.FacingDirection = (FacingDirection)(((int)this.FacingDirection + turns + count) % count);
        }

        private void SelectAllWarMachines()
        {
            // Ballista and cannon share a place; a hero that has the cannon keeps it, any other gets the ballista.
            var artillery = this.Hero.WarMachines.Contains(WarMachine.Cannon) ? WarMachine.Cannon : WarMachine.Ballista;
            IReadOnlyList<WarMachine> all = [artillery, WarMachine.AmmoCart, WarMachine.FirstAidTent];
            if (!this.Hero.WarMachines.OrderBy(w => w.Slot).SequenceEqual(all.OrderBy(w => w.Slot)))
            {
                this.Update(h => h with { WarMachines = all });
            }
        }

        private void SetLevelAndExperience(short level, int experience)
        {
            this.Update(h => h with { HeroLevel = new HeroLevel { Level = level, Experience = experience } });
        }

        private static short Coordinate(int value) => (short)RangeGuard.Clamp(value, short.MinValue, short.MaxValue);

        private void SetPosition<T>(T current, T value, Func<HeroPosition, HeroPosition> edit)
        {
            if (!EqualityComparer<T>.Default.Equals(current, value))
            {
                this.Update(h => h with { Position = edit(h.Position) });
            }
        }

        private void SetMarker(int current, int value, Func<MapPosition, MapPosition> edit)
        {
            if (current != value)
            {
                this.Update(h => h with { MarkerPosition = edit(h.MarkerPosition) });
            }
        }

        private void SetPrimarySkills(byte current, byte value, Func<PrimarySkills, PrimarySkills> edit)
        {
            if (current != value)
            {
                this.Update(h => h with { PrimarySkills = edit(h.PrimarySkills) });
            }
        }

        private void RefreshWarMachines()
        {
            var machines = this.Hero.WarMachines;
            var hasBallista = machines.Contains(WarMachine.Ballista);
            this.WarMachinesChecked[0] = hasBallista;
            this.WarMachinesChecked[1] = hasBallista;
            this.WarMachinesChecked[2] = machines.Contains(WarMachine.Cannon);
            this.WarMachinesChecked[3] = machines.Contains(WarMachine.AmmoCart);
            this.WarMachinesChecked[4] = machines.Contains(WarMachine.FirstAidTent);
        }

        private void Update(Func<HeroModel, HeroModel> edit)
        {
            // The history raises Changed, which makes the HeroViewModel refresh this one.
            this.update(edit);
        }
    }
}
