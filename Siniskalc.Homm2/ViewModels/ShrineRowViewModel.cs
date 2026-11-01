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

        public int Index => this.session.Index;

        public int CircleNumber => this.session.Current.Circle;

        public string Circle => this.session.Current.Circle switch { 1 => "First circle", 2 => "Second circle", _ => "Third circle" };

        public string SpellName => this.session.Current.Spell?.Name ?? string.Empty;

        public int X => this.session.Current.Position.X;

        public int Y => this.session.Current.Position.Y;

        public string PositionText => MapLevelText.Coordinates(this.session.Current.Position.X, this.session.Current.Position.Y);

        /// <summary>
        /// Gets what is in the way of editing the spell: a hero standing on the shrine hides it, or the tile holds a spell the editor does not know.
        /// </summary>
        public string Note => this.session.Current.SpellCode < 0 ? "A hero stands on the shrine: its spell is hidden until the hero moves." : this.session.Current.Spell is null ? $"The tile holds spell number {this.session.Current.SpellCode}, which the editor does not know." : string.Empty;

        public bool HasNote => this.Note.Length > 0;

        public ChoiceFieldViewModel SpellField { get; }

        public void Refresh()
        {
            this.SpellField.Refresh();
            this.RaiseAllPropertiesChanged();
        }
    }
}
