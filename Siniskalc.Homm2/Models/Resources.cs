namespace Siniskalc.Homm2.Models
{
    using System;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// An amount of each of the seven resources: the stock of a player, or what the player earns in a day.
    /// </summary>
    public sealed record Resources(long Wood, long Mercury, long Ore, long Sulfur, long Crystal, long Gems, long Gold)
    {
        public static Resources None { get; } = new(0, 0, 0, 0, 0, 0, 0);

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

        public Resources With(Resource resource, long amount) => resource switch
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
