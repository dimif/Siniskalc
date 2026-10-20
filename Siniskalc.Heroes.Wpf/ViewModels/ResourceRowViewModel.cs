namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One resource of a player: its amount, editable, and how far it is from what the savefile had.
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
        }

        public Resource Resource { get; }

        public bool IsGold => this.Resource == Resource.Gold;
    }
}
