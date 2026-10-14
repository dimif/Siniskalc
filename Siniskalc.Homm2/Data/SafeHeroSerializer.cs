namespace Siniskalc.Homm2.Data
{
    using System;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Writes a hero model into the record of a hero, as it is given. A field whose bytes already read as the model says is left alone
    /// (so a value the editor does not know, or a byte it does not model, is not changed by saving), and so is everything the model
    /// does not hold: the name, the facing and the rest of the bytes of the record, and the 14 bytes the Price of Loyalty adds.
    /// </summary>
    public sealed class SafeHeroSerializer : IHeroSerializer
    {
        /// <inheritdoc/>
        public HeroChunk Serialize(HeroChunk chunk, HeroModel model)
        {
            var bytes = chunk.ToArray();

            bytes[HeroLayout.Owner] = Bytes.ColorByte(model.Owner, bytes[HeroLayout.Owner]);
            WriteNumber(bytes, HeroLayout.SpellPoints, 2, model.SpellPoints, Constants.Hero.MinSpellPoints, Constants.Hero.MaxSpellPoints, nameof(model.SpellPoints));
            WriteNumber(bytes, HeroLayout.X, 4, model.Position.X, 0, int.MaxValue, nameof(model.Position));
            WriteNumber(bytes, HeroLayout.Y, 4, model.Position.Y, 0, int.MaxValue, nameof(model.Position));
            WriteDestination(bytes, model.Destination);
            WriteNumber(bytes, HeroLayout.MaxMovement, 4, model.MaxMovement, Constants.Hero.MinMovementPoints, Constants.Hero.MaxMovementPoints, nameof(model.MaxMovement));
            WriteNumber(bytes, HeroLayout.MovementLeft, 4, model.MovementLeft, Constants.Hero.MinMovementPoints, Constants.Hero.MaxMovementPoints, nameof(model.MovementLeft));
            WriteNumber(bytes, HeroLayout.Experience, 4, model.Experience, Constants.Hero.MinExperience, Constants.Hero.MaxExperience, nameof(model.Experience));
            WriteNumber(bytes, HeroLayout.Level, 1, model.Level, Constants.Hero.MinLevel, Constants.Hero.MaxLevel, nameof(model.Level));
            WriteNumber(bytes, HeroLayout.PrimarySkills, 1, model.PrimarySkills.Attack, Constants.Hero.MinPrimarySkill, Constants.Hero.MaxPrimarySkill, nameof(model.PrimarySkills));
            WriteNumber(bytes, HeroLayout.PrimarySkills + 1, 1, model.PrimarySkills.Defense, Constants.Hero.MinPrimarySkill, Constants.Hero.MaxPrimarySkill, nameof(model.PrimarySkills));
            WriteNumber(bytes, HeroLayout.PrimarySkills + 2, 1, model.PrimarySkills.SpellPower, Constants.Hero.MinPrimarySkill, Constants.Hero.MaxPrimarySkill, nameof(model.PrimarySkills));
            WriteNumber(bytes, HeroLayout.PrimarySkills + 3, 1, model.PrimarySkills.Knowledge, Constants.Hero.MinPrimarySkill, Constants.Hero.MaxPrimarySkill, nameof(model.PrimarySkills));
            WriteSecondarySkills(bytes, model.SecondarySkills);
            WriteArmy(bytes, model.Army);

            for (var slot = 0; slot < Constants.Hero.ArtifactSlotCount; slot++)
            {
                var offset = HeroLayout.Artifacts + slot;
                bytes[offset] = Bytes.ArtifactByte(model.Artifacts.Slots[slot], bytes[offset]);
            }

            foreach (var spell in Spell.All)
            {
                var offset = HeroLayout.Spells + spell.Code;
                var marked = bytes[offset] != 0;
                if (marked != model.Spells.Contains(spell))
                {
                    bytes[offset] = marked ? (byte)0 : (byte)1;
                }
            }

            return chunk.WithBytes(bytes);
        }

        private static void WriteNumber(byte[] bytes, int offset, int size, long value, long min, long max, string name)
        {
            long current = size switch
            {
                1 => bytes[offset],
                2 => BitConverter.ToUInt16(bytes, offset),
                _ => BitConverter.ToUInt32(bytes, offset),
            };
            if (current == value)
            {
                return;
            }

            RangeGuard.ThrowIfOutOfRange(value, min, max, name);
            switch (size)
            {
                case 1:
                    bytes[offset] = (byte)value;
                    break;
                case 2:
                    ByteBufferHelper.WriteUInt16(bytes, offset, (ushort)value);
                    break;
                default:
                    ByteBufferHelper.WriteUInt32(bytes, offset, (uint)value);
                    break;
            }
        }

        private static void WriteDestination(byte[] bytes, Position? destination)
        {
            var current = Bytes.OptionalPosition(ByteBufferHelper.ReadUInt32(bytes, HeroLayout.Destination), ByteBufferHelper.ReadUInt32(bytes, HeroLayout.Destination + 4));
            if (current == destination)
            {
                return;
            }

            ByteBufferHelper.WriteUInt32(bytes, HeroLayout.Destination, destination is { } x ? (uint)x.X : uint.MaxValue);
            ByteBufferHelper.WriteUInt32(bytes, HeroLayout.Destination + 4, destination is { } y ? (uint)y.Y : uint.MaxValue);
        }

        private static void WriteSecondarySkills(byte[] bytes, SecondarySkills skills)
        {
            var levels = bytes.AsSpan(HeroLayout.SkillLevels, Constants.Hero.SecondarySkillCount).ToArray();
            var order = bytes.AsSpan(HeroLayout.SkillOrder, Constants.Hero.SecondarySkillCount).ToArray();
            if (Bytes.SecondarySkills(levels, order).Equals(skills))
            {
                return;
            }

            levels.AsSpan().Clear();
            order.AsSpan().Clear();
            for (var i = 0; i < skills.Entries.Count; i++)
            {
                levels[skills.Entries[i].Skill.Code] = (byte)skills.Entries[i].Level;
                order[skills.Entries[i].Skill.Code] = (byte)(i + 1);
            }

            levels.CopyTo(bytes, HeroLayout.SkillLevels);
            order.CopyTo(bytes, HeroLayout.SkillOrder);
            bytes[HeroLayout.SkillCount] = (byte)skills.Count;
        }

        private static void WriteArmy(byte[] bytes, Army army)
        {
            for (var slot = 0; slot < Constants.ArmySlotCount; slot++)
            {
                var typeOffset = HeroLayout.ArmyTypes + slot;
                var countOffset = HeroLayout.ArmyCounts + (slot * 2);
                var wanted = army.Slots[slot];
                var currentType = Bytes.ReadCreature(bytes[typeOffset]);
                var currentCount = BitConverter.ToUInt16(bytes, countOffset);

                if (wanted is null)
                {
                    if (currentType is not null)
                    {
                        bytes[typeOffset] = Bytes.Empty;
                        ByteBufferHelper.WriteUInt16(bytes, countOffset, 0);
                    }

                    continue;
                }

                if (currentType == wanted.Creature && currentCount == wanted.Count)
                {
                    continue;
                }

                RangeGuard.ThrowIfOutOfRange<long>(wanted.Count, 0, Constants.MaxCreatureCount, nameof(army));
                bytes[typeOffset] = Bytes.CreatureByte(wanted.Creature, bytes[typeOffset]);
                ByteBufferHelper.WriteUInt16(bytes, countOffset, (ushort)wanted.Count);
            }
        }
    }
}
