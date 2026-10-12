namespace Siniskalc.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Entities;

    /// <summary>
    /// Resolves the spells artifacts grant for one game version, since spells from later versions do not exist in earlier ones.
    /// Two resolvers for the same version are equal, so heroes of one savefile stay equal to each other.
    /// </summary>
    public sealed record GrantedSpellsResolver(GameVersion GameVersion) : IGrantedSpellsResolver
    {
        public IReadOnlyDictionary<Spell, string> ResolveGrantedSpells(EquippedArtifacts artifacts)
        {
            var sources = new Dictionary<Spell, List<string>>();
            foreach (var (slot, artifact) in artifacts.GetEquippedArtifacts())
            {
                IEnumerable<Spell> granted = artifact.GrantedSpells(this.GameVersion);
                if (artifacts.GetScrollSpell(slot) is { } scrollSpell && scrollSpell.IsAvailableIn(this.GameVersion))
                {
                    granted = granted.Append(scrollSpell);
                }

                foreach (var spell in granted)
                {
                    if (!sources.TryGetValue(spell, out var names))
                    {
                        sources[spell] = names = [];
                    }

                    if (!names.Contains(artifact.Name))
                    {
                        names.Add(artifact.Name);
                    }
                }
            }

            return sources.ToDictionary(kv => kv.Key, kv => string.Join(", ", kv.Value));
        }
    }
}
