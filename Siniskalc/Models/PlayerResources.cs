namespace Siniskalc.Models
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Entities;

    /// <summary>
    /// What a player owns of each resource.
    /// </summary>
    public sealed record PlayerResources(int Wood, int Mercury, int Ore, int Sulfur, int Crystal, int Gems, int Gold)
    {
        /// <summary>
        /// Gets all resources, in savefile order.
        /// </summary>
        public static IReadOnlyList<Resource> All { get; } = Enum.GetValues<Resource>();

        public int this[Resource resource] => resource switch
        {
            Resource.Wood => this.Wood,
            Resource.Mercury => this.Mercury,
            Resource.Ore => this.Ore,
            Resource.Sulfur => this.Sulfur,
            Resource.Crystal => this.Crystal,
            Resource.Gems => this.Gems,
            Resource.Gold => this.Gold,
            _ => throw new ArgumentOutOfRangeException(nameof(resource)),
        };

        /// <summary>
        /// Returns the resources with one of them replaced.
        /// </summary>
        public PlayerResources With(Resource resource, int amount) => resource switch
        {
            Resource.Wood => this with { Wood = amount },
            Resource.Mercury => this with { Mercury = amount },
            Resource.Ore => this with { Ore = amount },
            Resource.Sulfur => this with { Sulfur = amount },
            Resource.Crystal => this with { Crystal = amount },
            Resource.Gems => this with { Gems = amount },
            Resource.Gold => this with { Gold = amount },
            _ => throw new ArgumentOutOfRangeException(nameof(resource)),
        };
    }
}
