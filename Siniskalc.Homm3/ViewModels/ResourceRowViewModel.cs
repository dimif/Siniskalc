namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One resource of a player: its amount, editable, and how far it is from what the savegame had.
    /// </summary>
    public sealed class ResourceRowViewModel : AmountRowViewModel
    {
        public ResourceRowViewModel(
            Resource resource,
            Func<ResourceSet> current,
            Func<ResourceSet> original,
            Action<Func<ResourceSet, ResourceSet>> update)
            : base(
                resource.ToString(),
                ResourcePalette.Of(resource.ToString()),
                Constants.Resources.Min,
                resource == Resource.Gold ? Constants.Resources.MaxGold : Constants.Resources.MaxOther,
                resource == Resource.Gold ? Constants.Resources.GoldStep : Constants.Resources.OtherStep,
                () => current()[resource],
                () => original()[resource],
                value => update(resources => resources.With(resource, value)))
        {
            this.Resource = resource;
        }

        public Resource Resource { get; }

        public bool IsGold => this.Resource == Resource.Gold;
    }
}
