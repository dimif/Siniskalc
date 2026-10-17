namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Heroes.Models;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Writes a <see cref="CastleModel"/> back into a <see cref="CastleChunk"/>, preserving unknown bits.
    /// </summary>
    /// <remarks>
    /// Safe because it patches a copy of the chunk and and bytes that failed to read/model, stay as they are.
    /// This guarantees some basic forward compatibility, for instance an unknown creature would fail to deserialize (and therefore resolve to an empty slot),
    /// but the serializer would leave it as it was.
    /// </remarks>
    public sealed class SafeCastleSerializer : ICastleSerializer
    {
        /// <inheritdoc>
        public CastleChunk Serialize(CastleChunk chunk, CastleModel castle)
        {
            var bytes = chunk.ToArray();
            var nameEnd = chunk.NameEnd;

            SerializeOwner(bytes, castle.Owner);
            SerializeBuiltThisTurn(bytes, castle.BuiltThisTurn);
            RangeGuard.ThrowIfOutOfRange<long>(castle.MageGuildLevel, 0, Constants.Castle.MaxMageGuildLevel, nameof(castle.MageGuildLevel));
            bytes[CastleLayout.MageGuildLevel] = (byte)castle.MageGuildLevel;
            bytes[CastleLayout.GarrisonedHero] = HeroByte(castle.GarrisonedHeroId, nameof(castle.GarrisonedHeroId));
            bytes[CastleLayout.VisitingHero] = HeroByte(castle.VisitingHeroId, nameof(castle.VisitingHeroId));
            SerializeGarrison(bytes, castle.Garrison);
            SerializeCreaturesAvailable(bytes, nameEnd + CastleLayout.CreaturesAvailableFromNameEnd, castle.CreaturesAvailable);
            SerializeCreaturesAvailable(bytes, nameEnd + CastleLayout.UpgradedCreaturesAvailableFromNameEnd, castle.UpgradedCreaturesAvailable);

            for (var level = 0; level < CastleLayout.SpellLevelCount && level < castle.SpellCounts.Count; level++)
            {
                RangeGuard.ThrowIfOutOfRange<long>(castle.SpellCounts[level], 0, Constants.Castle.MaxSpellsOfALevel, nameof(castle.SpellCounts));
                bytes[nameEnd + CastleLayout.SpellCountsFromNameEnd + level] = (byte)castle.SpellCounts[level];
            }

            SerializeMageGuildSpells(chunk, bytes, castle);
            SerializeBuildings(chunk, bytes, castle);

            return chunk.WithBytes(bytes);
        }

        /// <summary>
        /// Writes the spells of the guild into the slots that hold a spell the editor knows, in order, and leaves the other slots
        /// (a level the faction cannot build, a code that is no spell) alone. The model lists only the slots that read as a spell, so a
        /// level whose list is not as long as that was not made by this reader and is left as it is.
        /// </summary>
        private static void SerializeMageGuildSpells(CastleChunk chunk, byte[] bytes, CastleModel castle)
        {
            for (var level = 1; level <= CastleLayout.SpellLevelCount && level <= castle.MageGuildSpells.Count; level++)
            {
                var slots = new List<int>();
                for (var slot = 0; slot < MageGuildSpellSlots.Count(chunk, level); slot++)
                {
                    var offset = MageGuildSpellSlots.Offset(chunk, level, slot);
                    var code = offset < 0 ? -1 : chunk.GetInt32At(offset);
                    if (code is >= 0 and <= byte.MaxValue && AbstractEnumeration.FromCode<Spell>((byte)code) != null)
                    {
                        slots.Add(offset);
                    }
                }

                var spells = castle.MageGuildSpells[level - 1];
                if (spells.Count != slots.Count)
                {
                    // TODO:LOG warning: the spells of level {level} of the Mage Guild of {castle.Name} were not written: the model lists {spells.Count} of them and the savefile has {slots.Count} slots that hold a known spell.
                    continue;
                }

                for (var i = 0; i < slots.Count; i++)
                {
                    BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(slots[i], 4), spells[i].Code);
                }
            }
        }

        private static byte HeroByte(int? heroId, string name)
        {
            if (heroId is not { } id)
            {
                return Constants.ByteOff;
            }

            RangeGuard.ThrowIfOutOfRange<long>(id, 0, Constants.Player.MaxHeroId, name);
            return (byte)id;
        }

        private static void SerializeOwner(byte[] bytes, PlayerColor? owner)
        {
            // A byte that is no player's colour reads as no owner, and is left alone while the owner stays none.
            var hasOwner = ByteEnum.TryRead<PlayerColor>(bytes[CastleLayout.Owner], out var current);
            if (owner == null ? hasOwner : !hasOwner || current != owner.Value)
            {
                bytes[CastleLayout.Owner] = owner == null ? Constants.ByteOff : (byte)owner.Value;
            }
        }

        private static void SerializeBuiltThisTurn(byte[] bytes, bool builtThisTurn)
        {
            // Any non-zero byte reads as built, and is left alone while the castle stays built.
            if ((bytes[CastleLayout.BuiltThisTurn] != 0) != builtThisTurn)
            {
                bytes[CastleLayout.BuiltThisTurn] = (byte)(builtThisTurn ? 1 : 0);
            }
        }

        private static void SerializeGarrison(byte[] bytes, Army<Creature> garrison)
        {
            for (var i = 0; i < Constants.CreatureSlotCount; i++)
            {
                var slot = garrison.Slots[i];
                var typeOffset = CastleLayout.GarrisonTypes + (i * 4);
                var countOffset = CastleLayout.GarrisonCounts + (i * 4);

                if (slot != null)
                {
                    RangeGuard.ThrowIfOutOfRange<long>(slot.Count, 0, uint.MaxValue, nameof(garrison));
                    ByteBufferHelper.WriteInt32(bytes, typeOffset, slot.Creature.Code);
                    ByteBufferHelper.WriteUInt32(bytes, countOffset, (uint)slot.Count);
                }
                else if (HoldsKnownCreature(bytes, typeOffset))
                {
                    // Emptied. A slot that does not hold a creature this editor knows is empty or holds an unknown one: leave it.
                    ByteBufferHelper.WriteInt32(bytes, typeOffset, -1);
                    ByteBufferHelper.WriteUInt32(bytes, countOffset, 0u);
                }
            }
        }

        private static bool HoldsKnownCreature(byte[] bytes, int typeOffset)
        {
            var type = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(typeOffset));
            return type >= 0 && type <= byte.MaxValue && Creature.FromByte((byte)type) != null;
        }

        private static void SerializeCreaturesAvailable(byte[] bytes, int offset, IReadOnlyList<int> counts)
        {
            for (var level = 0; level < CastleLayout.CreatureLevelCount && level < counts.Count; level++)
            {
                RangeGuard.ThrowIfOutOfRange<long>(counts[level], Constants.Castle.MinDwellingCreatures, Constants.Castle.MaxDwellingCreatures, nameof(counts));
                ByteBufferHelper.WriteUInt16(bytes, offset + (level * 2), (ushort)counts[level]);
            }
        }

        private static void SerializeBuildings(CastleChunk chunk, byte[] bytes, CastleModel castle)
        {
            if (castle.Faction == null)
            {
                return;
            }

            var nameEnd = chunk.NameEnd;
            var builtOffset = nameEnd + CastleLayout.BuiltBuildingsFromNameEnd;
            var currentOffset = nameEnd + CastleLayout.CurrentBuildingsFromNameEnd;
            var enabledOffset = nameEnd + CastleLayout.EnabledBuildingsFromNameEnd;

            var built = ByteBufferHelper.ReadUInt64(bytes, builtOffset);
            var current = ByteBufferHelper.ReadUInt64(bytes, currentOffset);
            var enabled = ByteBufferHelper.ReadUInt64(bytes, enabledOffset);
            var hadBuilt = built;

            foreach (var building in Building.ForFaction(castle.Faction))
            {
                if (building.Code >= 64)
                {
                    // TODO:LOG warning: the building {building.Name} has a code that does not fit the 64-bit masks and was not written.
                    continue;
                }

                built = WithBit(built, building.Code, castle.Buildings.Contains(building));
                current = WithBit(current, building.Code, castle.CurrentBuildings.Contains(building));
                enabled = WithBit(enabled, building.Code, castle.EnabledBuildings.Contains(building));
            }

            foreach (var (trigger, bit) in CastleLayout.CompanionBits(castle.Faction))
            {
                var had = (hadBuilt & (1UL << trigger.Code)) != 0;
                var has = castle.Buildings.Contains(trigger);
                if (had != has)
                {
                    built = WithBit(built, bit, has);
                    current = WithBit(current, bit, has);
                }
            }

            ByteBufferHelper.WriteUInt64(bytes, builtOffset, built);
            ByteBufferHelper.WriteUInt64(bytes, currentOffset, current);
            ByteBufferHelper.WriteUInt64(bytes, enabledOffset, enabled);
        }

        private static ulong WithBit(ulong mask, int bit, bool value) => value ? mask | (1UL << bit) : mask & ~(1UL << bit);
    }
}
