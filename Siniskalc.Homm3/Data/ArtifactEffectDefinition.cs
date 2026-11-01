namespace Siniskalc.Homm3.Data
{
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    public sealed record ArtifactEffectDefinition
    {
        public AttributeEffect[] Modifiers { get; init; } = [];

        public SpellGrantDefinition[] SpellGrants { get; init; } = [];

        public bool GrantsSpellBook { get; init; }

        public string Other { get; init; } = string.Empty;

        internal short Amount(ArtifactAttribute attribute) => this.Modifiers.FirstOrDefault(effect => effect.Attribute == attribute)?.Amount ?? 0;
    }
}
