namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The artifacts page of a hero of Heroes of Might and Magic II: fourteen slots, which are not tied to a part of the body, and a
    /// finder that puts an artifact in the first free one. An artifact that adds to the skills moves its bonus with it (see
    /// <see cref="HeroModel.WithArtifact(int, Artifact?)"/>). Call <see cref="Refresh"/> after the hero changed.
    /// </summary>
    public sealed class ArtifactsViewModel : PageViewModelBase
    {
        private readonly Func<HeroModel> getHero;
        private readonly Action<Func<HeroModel, HeroModel>> update;
        private readonly GameVersion gameVersion;
        private IReadOnlyList<FactRow> bonuses = [];

        public ArtifactsViewModel(GameVersion gameVersion, Func<HeroModel> getHero, Action<Func<HeroModel, HeroModel>> update)
            : base("Artifacts")
        {
            this.gameVersion = gameVersion;
            this.getHero = getHero;
            this.update = update;

            this.Slots = Enumerable.Range(0, Constants.Hero.ArtifactSlotCount)
                .Select(slot => new ArtifactPickerViewModel(
                    $"Slot {slot + 1}",
                    onPick => new ArtifactFinderViewModel(gameVersion, string.Empty, "Choose", artifact => onPick(artifact.Name)),
                    name => this.Put(slot, name)))
                .ToList();
            this.Finder = new ArtifactFinderViewModel(gameVersion, "Find artifacts", "Equip", this.Equip, () => !this.getHero().Artifacts.IsFull);

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
        public ArtifactFinderViewModel Finder { get; }

        /// <summary>
        /// Gets what the artifacts of the hero add to its skills, one line for each skill they change.
        /// </summary>
        public IReadOnlyList<FactRow> Bonuses => this.bonuses;

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

            this.bonuses = PrimarySkillBonusText.Facts(hero.Artifacts.TotalBonus);

            this.RaiseAllPropertiesChanged();
            this.Finder.Refresh();
        }

        private void Equip(Artifact artifact) => this.update(hero => hero.Artifacts.IsFull ? hero : hero.WithArtifact(artifact));

        private void Put(int slot, string? name)
        {
            var artifact = string.IsNullOrEmpty(name) ? null : AbstractEnumeration.FromName<Artifact>(name);
            if (name == null || artifact != null)
            {
                this.update(hero => hero.WithArtifact(slot, artifact));
            }
        }
    }
}
