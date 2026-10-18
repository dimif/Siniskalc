namespace Siniskalc.Homm2.Data
{
    using System;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
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
            WriteNumber(bytes, HeroLayout.X, 4, model.Position.X, Constants.Hero.MinCoordinate, Constants.Hero.MaxCoordinate, nameof(model.Position));
            WriteNumber(bytes, HeroLayout.Y, 4, model.Position.Y, Constants.Hero.MinCoordinate, Constants.Hero.MaxCoordinate, nameof(model.Position));
            WriteDestination(bytes, model.Destination);
            if (Bytes.Facing(bytes[HeroLayout.Facing]) != model.Facing)
            {
                bytes[HeroLayout.Facing] = (byte)model.Facing;
            }

            WriteNumber(bytes, HeroLayout.MaxMovement, 4, model.MaxMovement, Constants.Hero.MinMovementPoints, Constants.Hero.MaxMovementPoints, nameof(model.MaxMovement));
            WriteNumber(bytes, HeroLayout.MovementLeft, 4, model.MovementLeft, Constants.Hero.MinMovementPoints, Constants.Hero.MaxMovementPoints, nameof(model.MovementLeft));
            WriteNumber(bytes, HeroLayout.Experience, 4, model.Experience, Constants.Hero.MinExperience, Constants.Hero.MaxExperience, nameof(model.Experience));
            WriteNumber(bytes, HeroLayout.Level, 1, model.Level, Constants.Hero.MinLevel, Constants.Hero.MaxLevel, nameof(model.Level));
            WriteNumber(bytes, HeroLayout.PrimarySkills, 1, model.PrimarySkills.Attack, Constants.Hero.MinPrimarySkill, Constants.Hero.MaxPrimarySkill, nameof(model.PrimarySkills));
            WriteNumber(bytes, HeroLayout.PrimarySkills + 1, 1, model.PrimarySkills.Defense, Constants.Hero.MinPrimarySkill, Constants.Hero.MaxPrimarySkill, nameof(model.PrimarySkills));
            WriteNumber(bytes, HeroLayout.PrimarySkills + 2, 1, model.PrimarySkills.SpellPower, Constants.Hero.MinPrimarySkill, Constants.Hero.MaxPrimarySkill, nameof(model.PrimarySkills));
            WriteNumber(bytes, HeroLayout.PrimarySkills + 3, 1, model.PrimarySkills.Knowledge, Constants.Hero.MinPrimarySkill, Constants.Hero.MaxPrimarySkill, nameof(model.PrimarySkills));
            WriteBattleBonus(bytes, HeroLayout.BattleMorale, model.NextBattle.Morale, nameof(model.NextBattle));
            WriteBattleBonus(bytes, HeroLayout.BattleLuck, model.NextBattle.Luck, nameof(model.NextBattle));
            WriteVisited(bytes, model.Visited);
            if (ByteBufferHelper.ReadUInt32(bytes, HeroLayout.Effects) != (uint)model.Effects)
            {
                ByteBufferHelper.WriteUInt32(bytes, HeroLayout.Effects, (uint)model.Effects);
            }

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

            // A 32 bit number the model cannot hold reads as the largest it can (see Bytes.ReadCoordinate): it is left alone while the model says that.
            if (current == value || (max == Constants.Hero.MaxCoordinate && value == max && current > max))
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

        private static void WriteBattleBonus(byte[] bytes, int offset, int value, string name)
        {
            if ((sbyte)bytes[offset] == value)
            {
                return;
            }

            RangeGuard.ThrowIfOutOfRange(value, Constants.Hero.MinBattleBonus, Constants.Hero.MaxBattleBonus, name);
            bytes[offset] = (byte)(sbyte)value;
        }

        private static void WriteVisited(byte[] bytes, VisitedPlaces visited)
        {
            uint[] sets = [visited.Gazebos, visited.Forts, visited.WitchDoctorHuts, visited.MercenaryCamps, visited.StandingStones];
            for (var i = 0; i < sets.Length; i++)
            {
                if (ByteBufferHelper.ReadUInt32(bytes, HeroLayout.VisitedPlaces + (i * 4)) != sets[i])
                {
                    ByteBufferHelper.WriteUInt32(bytes, HeroLayout.VisitedPlaces + (i * 4), sets[i]);
                }
            }

            if (ByteBufferHelper.ReadUInt32(bytes, HeroLayout.VisitedXanadu) != visited.Xanadu)
            {
                ByteBufferHelper.WriteUInt32(bytes, HeroLayout.VisitedXanadu, visited.Xanadu);
            }
        }

        private static void WriteDestination(byte[] bytes, MapPosition destination)
        {
            var current = Bytes.Destination(ByteBufferHelper.ReadUInt32(bytes, HeroLayout.Destination), ByteBufferHelper.ReadUInt32(bytes, HeroLayout.Destination + 4));
            if (current.X == destination.X && current.Y == destination.Y)
            {
                return;
            }

            // A part that is below 0 is the game's "none".
            RangeGuard.ThrowIfOutOfRange(destination.X, Siniskalc.Heroes.Constants.Map.NoCoordinate, Constants.Hero.MaxCoordinate, nameof(destination));
            RangeGuard.ThrowIfOutOfRange(destination.Y, Siniskalc.Heroes.Constants.Map.NoCoordinate, Constants.Hero.MaxCoordinate, nameof(destination));
            ByteBufferHelper.WriteUInt32(bytes, HeroLayout.Destination, destination.X < 0 ? uint.MaxValue : (uint)destination.X);
            ByteBufferHelper.WriteUInt32(bytes, HeroLayout.Destination + 4, destination.Y < 0 ? uint.MaxValue : (uint)destination.Y);
        }

        private static void WriteSecondarySkills(byte[] bytes, IHomm2SecondarySkills skills)
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
                levels[skills.Entries[i].Skill.Code] = skills.Entries[i].Level.Code;
                order[skills.Entries[i].Skill.Code] = (byte)(i + 1);
            }

            levels.CopyTo(bytes, HeroLayout.SkillLevels);
            order.CopyTo(bytes, HeroLayout.SkillOrder);
            bytes[HeroLayout.SkillCount] = (byte)skills.SlotCount;
        }

        private static void WriteArmy(byte[] bytes, Army<Creature> army)
        {
            for (var slot = 0; slot < Constants.ArmySlotCount; slot++)
            {
                Bytes.WriteArmySlot(bytes, HeroLayout.ArmyTypes + slot, HeroLayout.ArmyCounts + (slot * 2), army.Slots[slot], nameof(army));
            }
        }
    }
}
