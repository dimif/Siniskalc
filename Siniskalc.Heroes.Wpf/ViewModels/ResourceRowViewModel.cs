namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Windows.Input;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One resource of a player: its amount, editable, a button that adds a fixed amount to it, and how far it is from what the savefile had.
    /// </summary>
    public sealed class ResourceRowViewModel : AmountRowViewModel
    {
        /// <param name="resource">The resource.</param>
        /// <param name="limits">What the player may hold of it.</param>
        /// <param name="current">Gets the resources of the player now.</param>
        /// <param name="original">Gets the resources of the player as the savefile had them.</param>
        /// <param name="update">Changes the resources of the player.</param>
        /// <param name="note">Tells what is shown beside the amount, such as the income of the resource; nothing when omitted.</param>
        public ResourceRowViewModel(
            Resource resource,
            ResourceLimits limits,
            Func<ResourceSet> current,
            Func<ResourceSet> original,
            Action<Func<ResourceSet, ResourceSet>> update,
            Func<Resource, string>? note = null)
            : base(
                resource.ToString(),
                ResourcePalette.Of(resource.ToString()),
                limits.Minimum,
                resource == Resource.Gold ? limits.MaxGold : limits.MaxOther,
                resource == Resource.Gold ? limits.GoldStep : limits.OtherStep,
                () => current()[resource],
                () => original()[resource],
                value => update(resources => resources.With(resource, value)),
                note == null ? null : () => note(resource))
        {
            this.Resource = resource;
            var preset = resource == Resource.Gold ? limits.GoldPreset : limits.OtherPreset;
            var maximum = this.Maximum;
            this.AddPresetLabel = AmountText.Plus(preset);
            this.AddPresetToolTip = $"Add {preset:N0} {resource.ToString().ToLowerInvariant()}";
            this.AddPresetCommand = new DelegateCommand(
                _ => update(resources => resources.With(resource, Math.Min(resources[resource] + preset, maximum))),
                _ => current()[resource] < maximum);
        }

        /// <summary>
        /// Gets the resource the row is about.
        /// </summary>
        public Resource Resource { get; }

        /// <summary>
        /// Gets a value indicating whether the resource is gold, which has its own presets.
        /// </summary>
        public bool IsGold => this.Resource == Resource.Gold;

        /// <summary>
        /// Gets the text of the button that adds a fixed amount to the resource, such as "+10" or "+10k" for gold.
        /// </summary>
        public string AddPresetLabel { get; }

        /// <summary>
        /// Gets the tooltip of the button that adds the preset amount.
        /// </summary>
        public string AddPresetToolTip { get; }

        /// <summary>
        /// Gets the command that adds the fixed amount, up to the limit of the resource.
        /// </summary>
        public ICommand AddPresetCommand { get; }
    }
}
