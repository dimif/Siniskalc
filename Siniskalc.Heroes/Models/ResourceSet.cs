namespace Siniskalc.Heroes.Models
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// An amount of each of the seven resources: what a player owns, or what a player earns in a day. The limits of an amount are
    /// the game's own and are not checked here.
    /// </summary>
    public sealed record ResourceSet(long Wood, long Mercury, long Ore, long Sulfur, long Crystal, long Gems, long Gold)
    {
        /// <summary>
        /// Gets the resources, in the order of the savegame's tables.
        /// </summary>
        public static IReadOnlyList<Resource> All { get; } = Enum.GetValues<Resource>();

        /// <summary>
        /// Gets none of each resource.
        /// </summary>
        public static ResourceSet None { get; } = new(0, 0, 0, 0, 0, 0, 0);

        /// <summary>
        /// Gets the amount of a resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        public long this[Resource resource] => resource switch
        {
            Resource.Wood => this.Wood,
            Resource.Mercury => this.Mercury,
            Resource.Ore => this.Ore,
            Resource.Sulfur => this.Sulfur,
            Resource.Crystal => this.Crystal,
            Resource.Gems => this.Gems,
            Resource.Gold => this.Gold,
            _ => throw new ArgumentOutOfRangeException(nameof(resource), resource, null),
        };

        /// <summary>
        /// Returns the resources with one of them replaced.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="amount">The new amount.</param>
        /// <returns>The new set; this one is not changed.</returns>
        public ResourceSet With(Resource resource, long amount) => resource switch
        {
            Resource.Wood => this with { Wood = amount },
            Resource.Mercury => this with { Mercury = amount },
            Resource.Ore => this with { Ore = amount },
            Resource.Sulfur => this with { Sulfur = amount },
            Resource.Crystal => this with { Crystal = amount },
            Resource.Gems => this with { Gems = amount },
            Resource.Gold => this with { Gold = amount },
            _ => throw new ArgumentOutOfRangeException(nameof(resource), resource, null),
        };
    }
}
