namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The card of the level, the experience and the class of a hero, in any game. A game that has no gender, or whose heroes cannot change
    /// class, leaves those out: the card shows what it is given.
    /// </summary>
    public sealed class LevelAndClassCardViewModel : AbstractViewModelBase
    {
        private readonly Func<int> getLevel;
        private readonly Action<int> setLevel;
        private readonly Func<long> getExperience;
        private readonly Action<long> setExperience;
        private readonly Func<string> nextLevelNote;
        private readonly Func<string> getClass;
        private readonly Action<string>? setClass;
        private readonly Func<string>? getGender;
        private readonly Action<string>? setGender;

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelAndClassCardViewModel"/> class.
        /// </summary>
        /// <param name="getLevel">Reads the level of the hero.</param>
        /// <param name="setLevel">Writes it (the experience of that level goes with it).</param>
        /// <param name="level">The limits of the level.</param>
        /// <param name="getExperience">Reads the experience of the hero.</param>
        /// <param name="setExperience">Writes it (the level of that experience goes with it).</param>
        /// <param name="experience">The limits of the experience.</param>
        /// <param name="nextLevelNote">Reads what the hero needs for the next level.</param>
        /// <param name="classes">The classes to pick from.</param>
        /// <param name="getClass">Reads the class of the hero.</param>
        /// <param name="setClass">Writes it, or <c>null</c> when a hero cannot change class (the class is then only shown).</param>
        /// <param name="genders">The genders to pick from, or <c>null</c> in a game that does not model them.</param>
        /// <param name="getGender">Reads the gender of the hero.</param>
        /// <param name="setGender">Writes it.</param>
        public LevelAndClassCardViewModel(
            Func<int> getLevel,
            Action<int> setLevel,
            NumberRange level,
            Func<long> getExperience,
            Action<long> setExperience,
            NumberRange experience,
            Func<string> nextLevelNote,
            IReadOnlyList<string> classes,
            Func<string> getClass,
            Action<string>? setClass = null,
            IReadOnlyList<string>? genders = null,
            Func<string>? getGender = null,
            Action<string>? setGender = null)
        {
            this.getLevel = getLevel;
            this.setLevel = setLevel;
            this.LevelRange = level;
            this.getExperience = getExperience;
            this.setExperience = setExperience;
            this.ExperienceRange = experience;
            this.nextLevelNote = nextLevelNote;
            this.Classes = classes;
            this.getClass = getClass;
            this.setClass = setClass;
            this.Genders = genders ?? [];
            this.getGender = getGender;
            this.setGender = setGender;
        }

        /// <summary>
        /// Gets the least and greatest level a hero can have, for the stepper.
        /// </summary>
        public NumberRange LevelRange { get; }

        /// <summary>
        /// Gets the least and greatest experience a hero can have, for the stepper.
        /// </summary>
        public NumberRange ExperienceRange { get; }

        /// <summary>
        /// Gets or sets the level of the hero.
        /// </summary>
        public int Level
        {
            get => this.getLevel();
            set
            {
                if (value != this.Level && RangeGuard.IsInRange<long>(value, this.LevelRange.Minimum, this.LevelRange.Maximum))
                {
                    this.setLevel(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the experience points of the hero.
        /// </summary>
        public long Experience
        {
            get => this.getExperience();
            set
            {
                value = RangeGuard.Clamp(value, this.ExperienceRange.Minimum, this.ExperienceRange.Maximum);
                if (value != this.Experience)
                {
                    this.setExperience(value);
                }
            }
        }

        /// <summary>
        /// Gets how much experience the hero needs for the next level, or nothing at the highest level.
        /// </summary>
        public string NextLevelNote => this.nextLevelNote();

        /// <summary>
        /// Gets the names of the classes the user can pick.
        /// </summary>
        public IReadOnlyList<string> Classes { get; }

        /// <summary>
        /// Gets or sets the class of the hero.
        /// </summary>
        public string HeroClass
        {
            get => this.getClass();
            set
            {
                if (this.setClass != null && value != this.HeroClass)
                {
                    this.setClass(value);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the class can be changed (it is only shown otherwise).
        /// </summary>
        public bool CanChangeClass => this.setClass != null;

        /// <summary>
        /// Gets a value indicating whether the game gives heroes a gender.
        /// </summary>
        public bool HasGender => this.getGender != null;

        /// <summary>
        /// Gets the genders the user can pick.
        /// </summary>
        public IReadOnlyList<string> Genders { get; }

        /// <summary>
        /// Gets or sets the gender of the hero.
        /// </summary>
        public string Gender
        {
            get => this.getGender?.Invoke() ?? string.Empty;
            set
            {
                if (this.setGender != null && value != this.Gender)
                {
                    this.setGender(value);
                }
            }
        }

        /// <summary>
        /// Shows the hero again. Call after the hero changed.
        /// </summary>
        public void Refresh() => this.RaiseAllPropertiesChanged();
    }
}
