namespace Siniskalc.Homm3.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Models;
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
    using Siniskalc.Heroes.Wpf.ViewModels;
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

            this.MovementAndSpellPoints = new MovementAndSpellPointsCardViewModel(
                () => this.Hero.MovementPoints.Current,
                value => this.Update(h => h with { MovementPoints = h.MovementPoints with { Current = (uint)value } }),
                () => this.Hero.MovementPoints.Max,
                new NumberRange(Constants.Hero.MinMovementPoints, Constants.Hero.MaxMovementPoints, Constants.Hero.MovementStep),
                () => this.Hero.SpellPoints,
                value => this.Update(h => h with { SpellPoints = (ushort)value }),
                new NumberRange(Constants.Hero.MinSpellPoints, Constants.Hero.MaxSpellPoints, Constants.Hero.SpellPointsStep),
                () => this.spellPoints.GetMaximum(this.gameVersion, this.Hero),
                () => this.ArtifactEffects.LandMovement == 0 ? string.Empty : $"{this.ArtifactEffects.LandMovement:+0;-0} from artifacts");
            this.LevelAndClass = new LevelAndClassCardViewModel(
                () => this.Hero.HeroLevel.Level,
                level => this.SetLevelAndExperience((short)level, LevelCalculator.GetXpForLevel((short)level)),
                new NumberRange(Constants.Hero.MinLevel, Constants.Hero.MaxLevel),
                () => this.Hero.HeroLevel.Experience,
                experience => this.SetLevelAndExperience(LevelCalculator.GetLevelFromXp((int)experience), (int)experience),
                new NumberRange(Constants.Hero.MinExperience, Constants.Hero.MaxExperience, Constants.Hero.ExperienceStep),
                this.NextLevelNote,
                Entities.HeroClass.All.Where(c => c.IsAvailableIn(gameVersion)).Select(c => c.Name).ToList(),
                () => this.Hero.HeroClass.Name,
                this.SetHeroClass,
                Enum.GetNames<Entities.HeroGender>(),
                () => this.Hero.Gender?.ToString() ?? string.Empty,
                this.SetGender);
            var coordinates = new NumberRange(Constants.Map.MinCoordinate, Constants.Map.MaxCoordinate);
            this.Position = MapPointCardViewModel.ForPosition(
                () => this.Hero.Position,
                position => this.Update(h => h with { Position = (HeroPosition)position }),
                coordinates,
                this.LevelChoices,
                () => this.Hero.Position.FacingDirection,
                facing => this.Update(h => h with { Position = h.Position with { FacingDirection = facing } }));
            this.Destination = MapPointCardViewModel.ForDestination(
                () => this.Hero.Destination,
                destination => this.Update(h => h with { Destination = destination }),
                coordinates,
                this.LevelChoices);
            this.SelectAllWarMachinesCommand = new DelegateCommand(_ => this.SelectAllWarMachines());

            this.WarMachinesChecked = new ObservableCollection<bool>(new bool[WarMachineCheckBoxCount]);

            this.RefreshWarMachines();
        }

        public string HeroName => this.Hero.Name;

        /// <summary>
        /// Gets the card of the movement points and the spell points.
        /// </summary>
        public MovementAndSpellPointsCardViewModel MovementAndSpellPoints { get; }

        /// <summary>
        /// Gets the card of the level, the experience, the class and the gender.
        /// </summary>
        public LevelAndClassCardViewModel LevelAndClass { get; }

        /// <summary>
        /// Gets the card of where the hero stands and which way it faces.
        /// </summary>
        public MapPointCardViewModel Position { get; }

        /// <summary>
        /// Gets the card of the destination of the hero: the tile it is going to.
        /// </summary>
        public MapPointCardViewModel Destination { get; }

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
                this.MovementAndSpellPoints.Refresh();
                this.LevelAndClass.Refresh();
                this.Position.Refresh();
                this.Destination.Refresh();

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

        /// <summary>
        /// Gets how much experience the hero needs for the next level, such as "2,440 XP to level 8 (at 10,000)". Empty at the highest level.
        /// </summary>
        private string NextLevelNote()
        {
            var next = LevelCalculator.GetNextLevel((int)this.Hero.HeroLevel.Experience);
            return next == null
                ? string.Empty
                : string.Create(
                    System.Globalization.CultureInfo.CurrentCulture,
                    $"{next.Value.Experience - this.Hero.HeroLevel.Experience:N0} XP to level {next.Value.Level} (at {next.Value.Experience:N0})");
        }

        private void SetHeroClass(string value)
        {
            var heroClass = AbstractEnumeration.FromName<Entities.HeroClass>(value);
            if (heroClass != null && heroClass != this.Hero.HeroClass)
            {
                this.Update(h => h with { HeroClass = heroClass });
            }
        }

        private void SetGender(string value)
        {
            if (Enum.TryParse<Entities.HeroGender>(value, out var gender) && gender != this.Hero.Gender)
            {
                this.Update(h => h with { Gender = gender });
            }
        }

        private void SetLevelAndExperience(short level, int experience)
        {
            this.Update(h => h with { HeroLevel = new HeroLevel { Level = level, Experience = experience } });
        }

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
