namespace Siniskalc.Homm2.Models
{
    using System;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// An amount of each of the seven resources: the stock of a player, or what the player earns in a day.
    /// </summary>
    public sealed record Homm2Resources(long Wood, long Mercury, long Ore, long Sulfur, long Crystal, long Gems, long Gold)
    {
        public static Homm2Resources None { get; } = new(0, 0, 0, 0, 0, 0, 0);

        public long this[Homm2Resource resource] => resource switch
        {
            Homm2Resource.Wood => this.Wood,
            Homm2Resource.Mercury => this.Mercury,
            Homm2Resource.Ore => this.Ore,
            Homm2Resource.Sulfur => this.Sulfur,
            Homm2Resource.Crystal => this.Crystal,
            Homm2Resource.Gems => this.Gems,
            Homm2Resource.Gold => this.Gold,
            _ => throw new ArgumentOutOfRangeException(nameof(resource), resource, null),
        };

        public Homm2Resources With(Homm2Resource resource, long amount) => resource switch
        {
            Homm2Resource.Wood => this with { Wood = amount },
            Homm2Resource.Mercury => this with { Mercury = amount },
            Homm2Resource.Ore => this with { Ore = amount },
            Homm2Resource.Sulfur => this with { Sulfur = amount },
            Homm2Resource.Crystal => this with { Crystal = amount },
            Homm2Resource.Gems => this with { Gems = amount },
            Homm2Resource.Gold => this with { Gold = amount },
            _ => throw new ArgumentOutOfRangeException(nameof(resource), resource, null),
        };
    }
}
