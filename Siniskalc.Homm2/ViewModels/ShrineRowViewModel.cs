namespace Siniskalc.Homm2.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A shrine of the map: its circle, where it is, and the spell it teaches (picked from a list).
    /// </summary>
    public sealed class ShrineRowViewModel : AbstractViewModelBase
    {
        private readonly IEditSession<ShrineModel> session;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShrineRowViewModel"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="session">The session.</param>
        /// <param name="spellNames">The spell names.</param>
        public ShrineRowViewModel(GameCatalog catalog, IEditSession<ShrineModel> session, IReadOnlyList<string> spellNames)
        {
            this.session = session;
            this.SpellField = new ChoiceFieldViewModel(
                "Spell",
                spellNames,
                () => this.session.Current.Spell?.Name ?? string.Empty,
                name => this.session.Update(shrine => shrine.WithSpell(catalog.Spells.First(s => s.Name == name))),
                () => this.session.Current.SpellCode >= 0,
                "The spell the hero learns when it visits the shrine (the circle says which level of spell the game gives, but any spell can be put here). Not tried in the game.");
        }

        /// <summary>
        /// Gets the place of the shrine in the list, from 0.
        /// </summary>
        public int Index => this.session.Index;

        /// <summary>
        /// Gets the circle of the shrine, from 1 to 3.
        /// </summary>
        public int CircleNumber => this.session.Current.Circle;

        /// <summary>
        /// Gets the circle of the shrine as text.
        /// </summary>
        public string Circle => this.session.Current.Circle switch { 1 => "First circle", 2 => "Second circle", _ => "Third circle" };

        /// <summary>
        /// Gets the name of the spell, or nothing when it is hidden or not known.
        /// </summary>
        public string SpellName => this.session.Current.Spell?.Name ?? string.Empty;

        /// <summary>
        /// Gets the column of the shrine.
        /// </summary>
        public int X => this.session.Current.Position.X;

        /// <summary>
        /// Gets the row of the shrine.
        /// </summary>
        public int Y => this.session.Current.Position.Y;

        /// <summary>
        /// Gets the position of the shrine as text.
        /// </summary>
        public string PositionText => MapLevelText.Coordinates(this.session.Current.Position.X, this.session.Current.Position.Y);

        /// <summary>
        /// Gets what is in the way of editing the spell: a hero standing on the shrine hides it, or the tile holds a spell the editor does not know.
        /// </summary>
        public string Note => this.session.Current.SpellCode < 0 ? "A hero stands on the shrine: its spell is hidden until the hero moves." : this.session.Current.Spell is null ? $"The tile holds spell number {this.session.Current.SpellCode}, which the editor does not know." : string.Empty;

        /// <summary>
        /// Gets whether there is something to say about the spell.
        /// </summary>
        public bool HasNote => this.Note.Length > 0;

        /// <summary>
        /// Gets the field that sets the spell.
        /// </summary>
        public ChoiceFieldViewModel SpellField { get; }

        /// <summary>
        /// Reads the model again and tells the view that every value may have changed.
        /// </summary>
        public void Refresh()
        {
            this.SpellField.Refresh();
            this.RaiseAllPropertiesChanged();
        }
    }
}
