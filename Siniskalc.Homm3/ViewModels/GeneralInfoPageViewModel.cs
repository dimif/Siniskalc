namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
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
        private readonly Func<HeroModel> getHero;
        private readonly Action<Func<HeroModel, HeroModel>> update;
        private readonly IMaxSpellPointsCalculator<SecondarySkill, SecondarySkills> spellPoints;
        private readonly GameVersion gameVersion;
        private readonly bool hasUnderground;
        private readonly Func<bool> canSleep;
        private readonly Func<bool> canMoveToday;
        private bool isRefreshing;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralInfoPageViewModel"/> class.
        /// </summary>
        /// <param name="getHero">Gets the hero from the model.</param>
        /// <param name="update">Changes the hero in the model.</param>
        /// <param name="spellPoints">Works out the most spell points of the hero.</param>
        /// <param name="gameVersion">The version of the game.</param>
        /// <param name="hasUnderground">Whether the map has an underground level.</param>
        /// <param name="canSleep">Says whether the hero can be put to sleep; <see langword="null"/> means it can.</param>
        /// <param name="destinationsToSendTo">Lists the places the hero can be sent to; <see langword="null"/> for none.</param>
        /// <param name="canMoveToday">Says whether the hero can still move today; <see langword="null"/> means it can.</param>
        public GeneralInfoPageViewModel(Func<HeroModel> getHero, Action<Func<HeroModel, HeroModel>> update, IMaxSpellPointsCalculator<SecondarySkill, SecondarySkills> spellPoints, GameVersion gameVersion, bool hasUnderground = true, Func<bool>? canSleep = null, Func<IReadOnlyList<MapDestination>>? destinationsToSendTo = null, Func<bool>? canMoveToday = null)
            : base("General Info")
        {
            this.getHero = getHero;
            this.update = update;
            this.spellPoints = spellPoints;
            this.gameVersion = gameVersion;
            this.hasUnderground = hasUnderground;
            this.canSleep = canSleep ?? (() => false);
            this.canMoveToday = canMoveToday ?? (() => false);

            this.PrimarySkills = new PrimarySkillsCardViewModel(
                () => this.Hero.PrimarySkills,
                edit => this.Update(h => h with { PrimarySkills = edit(h.PrimarySkills) }),
                new NumberRange(Constants.Hero.MinPrimarySkill, Constants.Hero.MaxPrimarySkill));
            this.MovementAndSpellPoints = new MovementAndSpellPointsCardViewModel(
                () => this.Hero.MovementPoints.Current,
                value => this.Update(h => h.WithMovementLeft(value)),
                () => this.Hero.MovementPoints.Max,
                new NumberRange(Constants.Hero.MinMovementPoints, Constants.Hero.MaxMovementPoints, Constants.Hero.MovementStep),
                () => this.Hero.SpellPoints,
                value => this.Update(h => h.WithSpellPoints((int)value)),
                new NumberRange(Constants.Hero.MinSpellPoints, Constants.Hero.MaxSpellPoints, Constants.Hero.SpellPointsStep),
                () => this.spellPoints.GetMaximum(this.Hero.PrimarySkills, this.Hero.SecondarySkills),
                () => this.ArtifactEffects.LandMovement == 0 ? string.Empty : $"{this.ArtifactEffects.LandMovement:+0;-0} from artifacts");
            this.LevelAndClass = new LevelAndClassCardViewModel(
                () => this.Hero.HeroLevel.Level,
                level => this.Update(h => h.WithLevel(level)),
                new NumberRange(Constants.Hero.MinLevel, Constants.Hero.MaxLevel),
                () => this.Hero.HeroLevel.Experience,
                experience => this.Update(h => h.WithExperience(experience)),
                new NumberRange(Constants.Hero.MinExperience, Constants.Hero.MaxExperience, Constants.Hero.ExperienceStep),
                this.NextLevelNote,
                this.Hero.HeroClass.Catalog.HeroClasses.All.Select(c => c.Name).ToList(),
                () => this.Hero.HeroClass.Name,
                this.SetHeroClass,
                Enum.GetNames<Entities.HeroGender>(),
                () => this.Hero.Gender?.ToString() ?? string.Empty,
                this.SetGender);
            var coordinates = new NumberRange(Constants.Map.MinCoordinate, Constants.Map.MaxCoordinate);
            this.ActiveSpells = new ActiveSpellsCardViewModel(
            [
                this.ActiveSpell("Disguise", ActiveAdventureSpells.Disguise),
                this.ActiveSpell("Fly", ActiveAdventureSpells.Fly),
                this.ActiveSpell("Water Walk", ActiveAdventureSpells.WaterWalk),
                this.ActiveSpell("Visions", ActiveAdventureSpells.Visions),
            ]);
            this.Position = MapPointCardViewModel.ForPosition(
                () => this.Hero.Position,
                position => this.Update(h => h with { Position = (HeroPosition)position }),
                coordinates,
                this.LevelChoices,
                () => this.Hero.Position.FacingDirection,
                facing => this.Update(h => h with { Position = h.Position with { FacingDirection = facing } }),
                destinationsToSendTo);
            this.Destination = MapPointCardViewModel.ForDestination(
                () => this.Hero.Destination,
                destination => this.Update(h => h with { Destination = destination }),
                coordinates,
                this.LevelChoices);
            this.SelectAllWarMachinesCommand = new DelegateCommand(_ => this.SelectAllWarMachines());
            this.NextBattle = this.CreateNextBattle();

            this.WarMachineChoices = this.Hero.HeroClass.Catalog.WarMachines.All
                .Select(machine => new ToggleRowViewModel(
                    machine.Name,
                    () => this.Hero.WarMachines.Contains(machine),
                    selected => this.UpdateWarMachine(machine.Name, selected))).ToArray().AsReadOnly();

            this.RefreshWarMachines();
        }

        /// <summary>
        /// Gets the name of the hero.
        /// </summary>
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
        /// Gets the card of the adventure spells that are active on the hero this turn.
        /// </summary>
        public ActiveSpellsCardViewModel ActiveSpells { get; }

        /// <summary>
        /// Gets the command that gives the hero every war machine it can have.
        /// </summary>
        public ICommand SelectAllWarMachinesCommand { get; }

        /// <summary>
        /// Gets the card of the primary skills: the base the hero learned, the total with its artifacts and what they add.
        /// </summary>
        public PrimarySkillsCardViewModel PrimarySkills { get; }

        /// <summary>
        /// Gets the biography of the hero.
        /// </summary>
        public string? Biography => this.Hero.Biography;

        /// <summary>
        /// Gets whether the hero has a biography.
        /// </summary>
        public bool HasBiography => !string.IsNullOrEmpty(this.Hero.Biography);

        /// <summary>
        /// Gets every representable war machine of this catalog; choices sharing a physical slot exclude one another.
        /// </summary>
        public IReadOnlyList<ToggleRowViewModel> WarMachineChoices { get; }

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
        /// Gets whether the savefile has the "moved today" flag of a hero.
        /// </summary>
        public bool CanMoveToday => this.canMoveToday();

        /// <summary>
        /// Gets or sets whether the hero is asleep. A hero that moves wakes up, so this is mostly for waking one that would not move.
        /// </summary>
        public bool IsAsleep
        {
            get => this.Hero.IsAsleep;
            set => this.Update(h => h with { IsAsleep = value });
        }

        /// <summary>
        /// Gets the card of what visited buildings gave the hero until its next battle: the morale and the luck, and a box for each building.
        /// </summary>
        public NextBattleCardViewModel NextBattle { get; }

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

        /// <summary>
        /// Gets whether the savegame is from Horn of the Abyss.
        /// </summary>
        public bool IsHota => this.gameVersion == GameVersion.HornOfTheAbyss;

        private int LeadershipBonus => this.Hero.SecondarySkills.GetSkillLevel(Data.SkillRole.Morale).Code;

        private int LuckSkillBonus => this.Hero.SecondarySkills.GetSkillLevel(Data.SkillRole.Luck).Code;

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
                this.PrimarySkills.Refresh();
                this.MovementAndSpellPoints.Refresh();
                this.LevelAndClass.Refresh();
                this.Position.Refresh();
                this.Destination.Refresh();
                this.ActiveSpells.Refresh();
                this.NextBattle.Refresh();

                this.RaiseAllPropertiesChanged();
            }
            finally
            {
                this.isRefreshing = false;
            }
        }

        /// <summary>
        /// Gives the hero a war machine or takes it away.
        /// </summary>
        /// <param name="warMachineName">The name of the war machine.</param>
        /// <param name="isChecked">Whether the hero has it.</param>
        public void UpdateWarMachine(string warMachineName, bool isChecked)
        {
            if (this.isRefreshing)
            {
                return;
            }

            var warMachine = this.GetWarMachine(warMachineName);
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

        private WarMachine? GetWarMachine(string name) => this.Hero.HeroClass.Catalog.WarMachines.FindByName(name)
            ?? this.Hero.HeroClass.Catalog.WarMachines.FindByName(name.Replace('_', ' '));

        private static string Modifiers(params (string Name, int Value)[] parts) =>
            string.Join(", ", parts.Where(p => p.Value != 0).Select(p => $"{p.Name} {FormatSigned(p.Value)}"));

        private static string FormatSigned(int value) => value.ToString("+0;-0;+0", System.Globalization.CultureInfo.InvariantCulture);

        private int Clamp(int value) => Math.Clamp(value, -Constants.Hero.MaxMoraleOrLuck, Constants.Hero.MaxMoraleOrLuck);

        private void SelectAllWarMachines()
        {
            // Ballista and cannon share a place; a hero that has the cannon keeps it, any other gets the ballista.
            IReadOnlyList<WarMachine> all = this.Hero.HeroClass.Catalog.WarMachines.All.GroupBy(w => w.Slot)
                .Select(g => g.FirstOrDefault(this.Hero.WarMachines.Contains) ?? g.First()).ToList();
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
            var heroClass = this.Hero.HeroClass.Catalog.HeroClasses.FindByName(value);
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

        private ToggleRowViewModel ActiveSpell(string name, ActiveAdventureSpells spell) => new(
            name,
            () => this.Hero.ActiveSpells.HasFlag(spell),
            active => this.Update(h => h with { ActiveSpells = active ? h.ActiveSpells | spell : h.ActiveSpells & ~spell }));

        // The buildings whose morale or luck is known from the saves (TestData/Sod/Hotseat) set their bit and the byte together, as HoMM2 has to
        // (docs/HOMM2_SAVE_FORMAT.md); the stables only have a bit. The faerie ring, the fountain and the idol of fortune give a bonus the
        // saves do not pin down, so their boxes cannot be changed.
        private NextBattleCardViewModel CreateNextBattle()
        {
            var range = new NumberRange(-Constants.Hero.MaxMoraleOrLuck, Constants.Hero.MaxMoraleOrLuck);
            return new NextBattleCardViewModel(
                "Visited buildings",
                [
                    this.BonusRow("Morale", range, h => h.MoraleBonus, (h, value) => h with { MoraleBonus = value }),
                    this.BonusRow("Luck", range, h => h.LuckBonus, (h, value) => h with { LuckBonus = value }),
                ],
                this.Hero.HeroClass.Catalog.MapMetadata.VisitEffects
                    .Select(e => e.Editable ? this.Source(e.Effect, e.Morale, e.Luck, range, e.Description) : this.Shown(e.Effect, e.Description)).ToList(),
                "What the buildings the hero visited gave. A battle takes most of it away; the stables last until the end of the week. Not tried in the game.");
        }

        private AmountRowViewModel BonusRow(string name, NumberRange range, Func<HeroModel, int> get, Func<HeroModel, int, HeroModel> set) => new(
            name,
            PlayerPalette.Of(this.Hero.Owner),
            range.Minimum,
            range.Maximum,
            range.Step,
            () => get(this.Hero),
            () => get(this.Hero),
            value => this.Update(h => set(h, (int)value)),
            () => "next battle only");

        private ToggleRowViewModel Source(VisitedBuildingEffects effect, int morale, int luck, NumberRange range, string toolTip) => new(
            this.Hero.HeroClass.Catalog.MapMetadata.EffectName(effect),
            () => this.Hero.VisitEffects.HasFlag(effect),
            on => this.Update(h => h with
            {
                VisitEffects = on ? h.VisitEffects | effect : h.VisitEffects & ~effect,
                MoraleBonus = (int)Math.Clamp(h.MoraleBonus + (on ? morale : -morale), range.Minimum, range.Maximum),
                LuckBonus = (int)Math.Clamp(h.LuckBonus + (on ? luck : -luck), range.Minimum, range.Maximum),
            }),
            toolTip: toolTip + (morale == 0 && luck == 0 ? ". Sets what the game remembers." : ". Sets what the game remembers and its morale or luck together.") + " Not tried in the game.");

        private ToggleRowViewModel Shown(VisitedBuildingEffects effect, string toolTip) => new(
            this.Hero.HeroClass.Catalog.MapMetadata.EffectName(effect),
            () => this.Hero.VisitEffects.HasFlag(effect),
            _ => { },
            canToggle: () => false,
            toolTip: toolTip + ". Shown only: how much the saves keep for it is not known for sure, so it is not edited.");

        private void RefreshWarMachines()
        {
            foreach (var choice in this.WarMachineChoices)
            {
                choice.Refresh();
            }
        }

        private void Update(Func<HeroModel, HeroModel> edit)
        {
            // The history raises Changed, which makes the HeroViewModel refresh this one.
            this.update(edit);
        }
    }
}
