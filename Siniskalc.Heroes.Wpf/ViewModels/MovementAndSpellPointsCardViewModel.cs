namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Windows.Input;
    using Siniskalc.Core;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The card of the movement points and the spell points a hero has now, in any game: a stepper for each, a button for the most the hero
    /// can have, and a button that gives the points of a full day back. The card reads and writes the hero through the delegates it is given.
    /// </summary>
    public sealed class MovementAndSpellPointsCardViewModel : AbstractViewModelBase
    {
        private readonly Func<long> getMovement;
        private readonly Action<long> setMovement;
        private readonly Func<long> getMaxMovement;
        private readonly Func<int> getSpellPoints;
        private readonly Action<int> setSpellPoints;
        private readonly Func<int>? getMaxSpellPoints;
        private readonly Func<string> movementNote;

        /// <summary>
        /// Initializes a new instance of the <see cref="MovementAndSpellPointsCardViewModel"/> class.
        /// </summary>
        /// <param name="getMovement">Reads the movement points the hero has left today.</param>
        /// <param name="setMovement">Writes them.</param>
        /// <param name="getMaxMovement">Reads the movement points of a full day.</param>
        /// <param name="movement">The limits of the movement points.</param>
        /// <param name="getSpellPoints">Reads the spell points of the hero.</param>
        /// <param name="setSpellPoints">Writes them.</param>
        /// <param name="spellPoints">The limits of the spell points.</param>
        /// <param name="getMaxSpellPoints">Reads the most spell points the hero can have from its knowledge, or <c>null</c> in a game that does not work it out (there is no such button then).</param>
        /// <param name="movementNote">Reads a note under the movement, such as what the artifacts add; nothing when not given.</param>
        public MovementAndSpellPointsCardViewModel(
            Func<long> getMovement,
            Action<long> setMovement,
            Func<long> getMaxMovement,
            NumberRange movement,
            Func<int> getSpellPoints,
            Action<int> setSpellPoints,
            NumberRange spellPoints,
            Func<int>? getMaxSpellPoints = null,
            Func<string>? movementNote = null)
        {
            this.getMovement = getMovement;
            this.setMovement = setMovement;
            this.getMaxMovement = getMaxMovement;
            this.Movement = movement;
            this.getSpellPoints = getSpellPoints;
            this.setSpellPoints = setSpellPoints;
            this.SpellPoints = spellPoints;
            this.getMaxSpellPoints = getMaxSpellPoints;
            this.movementNote = movementNote ?? (() => string.Empty);

            this.RestoreMovementCommand = new DelegateCommand(
                _ => this.CurrentMovement = this.MaxMovement,
                _ => this.CurrentMovement != this.MaxMovement);
            this.MaxMovementCommand = new DelegateCommand(
                _ => this.CurrentMovement = this.Movement.Maximum,
                _ => this.CurrentMovement != this.Movement.Maximum);
            this.ResetSpellPointsCommand = new DelegateCommand(
                _ => this.CurrentSpellPoints = this.MostSpellPoints,
                _ => this.HasSpellPointsMaximum && this.CurrentSpellPoints != this.MostSpellPoints);
            this.MaxSpellPointsCommand = new DelegateCommand(
                _ => this.CurrentSpellPoints = (int)this.SpellPoints.Maximum,
                _ => this.CurrentSpellPoints != this.SpellPoints.Maximum);
        }

        public NumberRange Movement { get; }

        public NumberRange SpellPoints { get; }

        public long CurrentMovement
        {
            get => this.getMovement();
            set
            {
                value = RangeGuard.Clamp(value, this.Movement.Minimum, this.Movement.Maximum);
                if (value != this.CurrentMovement)
                {
                    this.setMovement(value);
                }
            }
        }

        /// <summary>
        /// Gets the movement points of a full day.
        /// </summary>
        public long MaxMovement => this.getMaxMovement();

        public string MaxMovementText => $"Maximum {this.MaxMovement}";

        /// <summary>
        /// Gets a note under the movement, such as "+600 from artifacts", or nothing.
        /// </summary>
        public string MovementNote => this.movementNote();

        /// <summary>
        /// Gets the command that gives the hero all of its movement points back.
        /// </summary>
        public ICommand RestoreMovementCommand { get; }

        /// <summary>
        /// Gets the command that gives the hero the most movement points that can be saved.
        /// </summary>
        public ICommand MaxMovementCommand { get; }

        public int CurrentSpellPoints
        {
            get => this.getSpellPoints();
            set
            {
                value = (int)RangeGuard.Clamp<long>(value, this.SpellPoints.Minimum, this.SpellPoints.Maximum);
                if (value != this.CurrentSpellPoints)
                {
                    this.setSpellPoints(value);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the game works out the most spell points a hero can have, so that the card can fill them up to it.
        /// </summary>
        public bool HasSpellPointsMaximum => this.getMaxSpellPoints != null;

        /// <summary>
        /// Gets the most spell points the hero can have (from its knowledge and the skills and artifacts that add to it), capped to what can be saved.
        /// </summary>
        public int MostSpellPoints => this.getMaxSpellPoints == null ? (int)this.SpellPoints.Maximum : (int)Math.Min(this.getMaxSpellPoints(), this.SpellPoints.Maximum);

        public string MaxSpellPointsText => this.HasSpellPointsMaximum ? $"Maximum {this.MostSpellPoints}" : string.Empty;

        /// <summary>
        /// Gets the command that fills the spell points up to the most the hero can have.
        /// </summary>
        public ICommand ResetSpellPointsCommand { get; }

        /// <summary>
        /// Gets the command that gives the hero the most spell points that can be saved.
        /// </summary>
        public ICommand MaxSpellPointsCommand { get; }

        /// <summary>
        /// Shows the hero again. Call after the hero changed.
        /// </summary>
        public void Refresh() => this.RaiseAllPropertiesChanged();
    }
}
