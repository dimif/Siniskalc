namespace Siniskalc.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Entities;

    /// <summary>
    /// Immutable set of the artifacts a hero has equipped. Edits return a new instance.
    /// </summary>
    public sealed class EquippedArtifacts
    {
        private readonly IReadOnlyDictionary<ArtifactSlots, Artifact> slotsToArtifacts;
        private readonly IReadOnlyDictionary<ArtifactSlots, Spell> scrollSpells;

        /// <param name="slotsToArtifacts">What is worn in each slot.</param>
        /// <param name="scrollSpells">The spell of each Spell Scroll that is worn, by slot. A scroll missing here holds no known spell.</param>
        public EquippedArtifacts(IDictionary<ArtifactSlots, Artifact> slotsToArtifacts, IDictionary<ArtifactSlots, Spell>? scrollSpells = null)
        {
            this.slotsToArtifacts = new Dictionary<ArtifactSlots, Artifact>(slotsToArtifacts);
            this.scrollSpells = (scrollSpells ?? new Dictionary<ArtifactSlots, Spell>())
                .Where(kv => slotsToArtifacts.TryGetValue(kv.Key, out var a) && a == Artifact.SpellScroll)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>
        /// Gets whether one of the artifacts gives the hero a spell book, as Titan's Thunder does.
        /// </summary>
        public bool GrantsSpellBook => this.slotsToArtifacts.Values.Any(a => a.EffectInfo.GrantsSpellBook);

        public static bool IsSingleSlot(ArtifactSlots slot)
        {
            return slot != ArtifactSlots.None && ((int)slot & ((int)slot - 1)) == 0;
        }

        public Artifact? GetArtifact(ArtifactSlots slot)
        {
            return this.slotsToArtifacts.TryGetValue(slot, out var artifact) ? artifact : null;
        }

        /// <summary>
        /// Gets the spell of the Spell Scroll worn in <paramref name="slot"/>; null when the slot holds no scroll or its spell is not known.
        /// </summary>
        public Spell? GetScrollSpell(ArtifactSlots slot)
        {
            return this.scrollSpells.TryGetValue(slot, out var spell) ? spell : null;
        }

        /// <summary>
        /// Returns a copy with <paramref name="artifact"/> equipped in <paramref name="slot"/>.
        /// A combination artifact also empties the other slots it blocks; it is only stored in its own slot.
        /// </summary>
        /// <param name="spell">The spell of a Spell Scroll; ignored for any other artifact.</param>
        public EquippedArtifacts WithEquipped(ArtifactSlots slot, Artifact artifact, Spell? spell = null)
        {
            if (!IsSingleSlot(slot))
            {
                throw new ArgumentException($"'{slot}' is not a single equipment slot.", nameof(slot));
            }

            if (artifact == null)
            {
                return this.WithUnequipped(slot);
            }

            var slotsToArtifacts = new Dictionary<ArtifactSlots, Artifact>(this.slotsToArtifacts)
            {
                [slot] = artifact,
            };
            var scrollSpells = new Dictionary<ArtifactSlots, Spell>(this.scrollSpells);
            scrollSpells.Remove(slot);
            if (artifact == Artifact.SpellScroll && spell != null)
            {
                scrollSpells[slot] = spell;
            }

            if (artifact.IsCombination)
            {
                foreach (var blockedSlot in Enum.GetValues<ArtifactSlots>())
                {
                    if (blockedSlot != slot && IsSingleSlot(blockedSlot) && artifact.BlockedSlots.HasFlag(blockedSlot))
                    {
                        slotsToArtifacts.Remove(blockedSlot);
                        scrollSpells.Remove(blockedSlot);
                    }
                }
            }

            return new EquippedArtifacts(slotsToArtifacts, scrollSpells);
        }

        public EquippedArtifacts WithUnequipped(ArtifactSlots slot)
        {
            var slotsToArtifacts = new Dictionary<ArtifactSlots, Artifact>(this.slotsToArtifacts);
            slotsToArtifacts.Remove(slot);
            var scrollSpells = new Dictionary<ArtifactSlots, Spell>(this.scrollSpells);
            scrollSpells.Remove(slot);
            return new EquippedArtifacts(slotsToArtifacts, scrollSpells);
        }

        public IEnumerable<Tuple<ArtifactSlots, Artifact>> GetEquippedArtifacts()
        {
            return this.slotsToArtifacts.Where(kv => kv.Value != null).Select(kv => Tuple.Create(kv.Key, kv.Value));
        }

        public ArtifactEffectInfo GetTotalEffects()
        {
            ArtifactEffectInfo totalEffects = default;

            foreach (var artifact in this.slotsToArtifacts.Values)
            {
                if (artifact != null)
                {
                    var hasOther = !string.IsNullOrEmpty(artifact.EffectInfo.Other);
                    var needsSeparator = !string.IsNullOrEmpty(totalEffects.Other) && hasOther;
                    var separator = needsSeparator ? "; " : string.Empty;
                    var other = hasOther ? artifact.EffectInfo.Other : string.Empty;

                    totalEffects = totalEffects with
                    {
                        Atk = (short)(totalEffects.Atk + artifact.EffectInfo.Atk),
                        Def = (short)(totalEffects.Def + artifact.EffectInfo.Def),
                        Pow = (short)(totalEffects.Pow + artifact.EffectInfo.Pow),
                        Kno = (short)(totalEffects.Kno + artifact.EffectInfo.Kno),
                        Luck = (short)(totalEffects.Luck + artifact.EffectInfo.Luck),
                        Morale = (short)(totalEffects.Morale + artifact.EffectInfo.Morale),
                        LandMovement = (short)(totalEffects.LandMovement + artifact.EffectInfo.LandMovement),
                        WaterMovement = (short)(totalEffects.WaterMovement + artifact.EffectInfo.WaterMovement),
                        SpellGrants = [.. totalEffects.SpellGrants ?? [], .. artifact.EffectInfo.SpellGrants ?? []],
                        GrantsSpellBook = totalEffects.GrantsSpellBook || artifact.EffectInfo.GrantsSpellBook,
                        Gold = (short)(totalEffects.Gold + artifact.EffectInfo.Gold),
                        Ore = (short)(totalEffects.Ore + artifact.EffectInfo.Ore),
                        Wood = (short)(totalEffects.Wood + artifact.EffectInfo.Wood),
                        Crystal = (short)(totalEffects.Crystal + artifact.EffectInfo.Crystal),
                        Gems = (short)(totalEffects.Gems + artifact.EffectInfo.Gems),
                        Mercury = (short)(totalEffects.Mercury + artifact.EffectInfo.Mercury),
                        Sulfur = (short)(totalEffects.Sulfur + artifact.EffectInfo.Sulfur),
                        Other = $"{totalEffects.Other}{separator}{other}",
                    };
                }
            }

            return totalEffects;
        }
    }
}
