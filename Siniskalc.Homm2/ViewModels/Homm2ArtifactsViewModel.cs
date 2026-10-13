namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Entities;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The artifacts page of a hero of Heroes of Might and Magic II: fourteen slots, which are not tied to a part of the body, and a
    /// finder that puts an artifact in the first free one. An artifact that adds to the skills moves its bonus with it (see
    /// <see cref="Homm2HeroModel.WithArtifact(int, Homm2Artifact?)"/>). Call <see cref="Refresh"/> after the hero changed.
    /// </summary>
    public sealed class Homm2ArtifactsViewModel : PageViewModelBase
    {
        private readonly Func<Homm2HeroModel> getHero;
        private readonly Action<Func<Homm2HeroModel, Homm2HeroModel>> update;
        private readonly Homm2GameVersion gameVersion;
        private IReadOnlyList<Homm2FactRow> bonuses = [];

        public Homm2ArtifactsViewModel(Homm2GameVersion gameVersion, Func<Homm2HeroModel> getHero, Action<Func<Homm2HeroModel, Homm2HeroModel>> update)
            : base("Artifacts")
        {
            this.gameVersion = gameVersion;
            this.getHero = getHero;
            this.update = update;

            this.Slots = Enumerable.Range(0, Constants.Hero.ArtifactSlotCount)
                .Select(slot => new ArtifactPickerViewModel(
                    $"Slot {slot + 1}",
                    onPick => new Homm2ArtifactFinderViewModel(gameVersion, string.Empty, "Choose", artifact => onPick(artifact.Name)),
                    name => this.Put(slot, name)))
                .ToList();
            this.Finder = new Homm2ArtifactFinderViewModel(gameVersion, "Find artifacts", "Equip", this.Equip, () => !this.getHero().Artifacts.IsFull);

            this.Refresh();
        }

        public IReadOnlyList<ArtifactPickerViewModel> Slots { get; }

        /// <summary>
        /// Gets the first half of the slots, which the page shows in its left column.
        /// </summary>
        public IEnumerable<ArtifactPickerViewModel> LeftSlots => this.Slots.Take(this.Slots.Count / 2);

        /// <summary>
        /// Gets the second half of the slots, which the page shows in its right column.
        /// </summary>
        public IEnumerable<ArtifactPickerViewModel> RightSlots => this.Slots.Skip(this.Slots.Count / 2);

        /// <summary>
        /// Gets the finder that puts an artifact in the first free slot.
        /// </summary>
        public Homm2ArtifactFinderViewModel Finder { get; }

        /// <summary>
        /// Gets what the artifacts of the hero add to its skills, one line for each skill they change.
        /// </summary>
        public IReadOnlyList<Homm2FactRow> Bonuses => this.bonuses;

        public bool HasBonuses => this.bonuses.Count > 0;

        /// <summary>
        /// Shows the hero's artifacts again. Call after they changed.
        /// </summary>
        public void Refresh()
        {
            var hero = this.getHero();
            for (var i = 0; i < this.Slots.Count; i++)
            {
                this.Slots[i].UpdateFromModel(hero.Artifacts.Slots[i]?.Name);
            }

            var total = hero.Artifacts.Slots.OfType<Homm2Artifact>()
                .Select(Homm2ArtifactBonuses.Of)
                .Aggregate(new Homm2PrimarySkills(0, 0, 0, 0), (sum, b) => new Homm2PrimarySkills(sum.Attack + b.Attack, sum.Defense + b.Defense, sum.SpellPower + b.SpellPower, sum.Knowledge + b.Knowledge));
            this.bonuses = new (string Name, int Amount)[] { ("Attack", total.Attack), ("Defense", total.Defense), ("Spell power", total.SpellPower), ("Knowledge", total.Knowledge) }
                .Where(b => b.Amount != 0)
                .Select(b => new Homm2FactRow(b.Name, $"{(b.Amount > 0 ? "+" : string.Empty)}{b.Amount}"))
                .ToList();

            this.RaiseAllPropertiesChanged();
            this.Finder.Refresh();
        }

        private void Equip(Homm2Artifact artifact) => this.update(hero => hero.Artifacts.IsFull ? hero : hero.WithArtifact(artifact));

        private void Put(int slot, string? name)
        {
            var artifact = string.IsNullOrEmpty(name) ? null : AbstractEnumeration.FromName<Homm2Artifact>(name);
            if (name == null || artifact != null)
            {
                this.update(hero => hero.WithArtifact(slot, artifact));
            }
        }
    }
}
