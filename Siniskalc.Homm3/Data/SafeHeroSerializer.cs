namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Writes a <see cref="HeroModel"/> back into a <see cref="HeroChunk"/>, preserving unknown bits.
    /// </summary>
    /// <remarks>
    /// Safe because it patches a copy of the chunk and and bytes that failed to read/model, stay as they are.
    /// This guarantees some basic forward compatibility, for instance an unknown creature would fail to deserialize (and therefore resolve to an empty slot),
    /// but the serializer would leave it as it was.
    /// </remarks>
    public sealed class SafeHeroSerializer : IHeroSerializer
    {
        private readonly HeroOffsets heroOffsets;
        private readonly VersionSpecificEntitiesProvider entities;

        public SafeHeroSerializer(HeroOffsets heroOffsets, VersionSpecificEntitiesProvider entities)
        {
            this.heroOffsets = heroOffsets;
            this.entities = entities;
        }

        public HeroChunk Serialize(HeroChunk original, HeroModel hero)
        {
            var chunk = this.SerializePrimarySkills(original, hero.PrimarySkills);
            chunk = this.SerializeClass(chunk, hero.HeroClass);
            chunk = this.SerializeLevel(chunk, hero.HeroLevel);
            chunk = this.SerializePosition(chunk, hero.Position);
            chunk = this.SerializeDestination(chunk, hero.Destination);
            chunk = this.SerializeMovementPoints(chunk, hero.MovementPoints);
            chunk = this.SerializeSecondarySkills(chunk, hero.SecondarySkills);
            chunk = this.SerializeBackpack(chunk, hero.Backpack);
            chunk = this.SerializeEquippedArtifacts(chunk, hero.EquippedArtifacts);
            chunk = this.SerializeWarMachines(chunk, hero.WarMachines);
            chunk = this.SerializeSpellPoints(chunk, hero.SpellPoints);
            chunk = this.SerializeOwner(chunk, hero.Owner);
            chunk = this.SerializeGender(chunk, hero.Gender);
            chunk = this.SerializeMovedToday(chunk, hero.MovedToday);
            chunk = this.SerializeActiveSpells(chunk, hero.ActiveSpells);
            chunk = this.SerializeIsAsleep(chunk, hero.IsAsleep);
            chunk = this.SerializeHasSpellBook(chunk, hero.SpellBook.HasSpellBook);
            chunk = this.SerializeCreatures(chunk, hero.Army);
            chunk = this.SerializeSpellBook(chunk, hero.SpellBook);

            return chunk;
        }

        public HeroChunk SerializeCreatures(HeroChunk chunk, IArmy<Creature> creatures)
        {
            return Edit(chunk, buffer => ArmyWriter.Write(buffer, this.heroOffsets.Army, creatures, CreatureCodes.Instance, nameof(creatures)));
        }

        public HeroChunk SerializeMovementPoints(HeroChunk chunk, MovementPoints movementPoints)
        {
            return Edit(chunk, buffer =>
            {
                // The savefile holds the movement points in 32 bits; more is refused and not cut.
                RangeGuard.ThrowIfOutOfRange<long>(movementPoints.Current, 0, uint.MaxValue, nameof(movementPoints));
                RangeGuard.ThrowIfOutOfRange<long>(movementPoints.Max, 0, uint.MaxValue, nameof(movementPoints));
                ByteBufferHelper.WriteUInt32(buffer, this.heroOffsets[HeroOffsetName.CurrentMovementPoints], (uint)movementPoints.Current);
                ByteBufferHelper.WriteUInt32(buffer, this.heroOffsets[HeroOffsetName.MaxMovementPoints], (uint)movementPoints.Max);
            });
        }

        public HeroChunk SerializeSecondarySkills(HeroChunk chunk, SecondarySkills secondarySkills)
        {
            return Edit(chunk, buffer =>
            {
                var skills = this.entities.SecondarySkills;
                var levelsOffset = this.heroOffsets[HeroOffsetName.Skills];
                var slotsOffset = this.heroOffsets[HeroOffsetName.SkillSlots];
                var previousSlotCount = buffer[this.heroOffsets[HeroOffsetName.NumOfSkills]];

                var slotsBySkill = new Dictionary<SecondarySkill, int>();
                for (var i = 0; i < secondarySkills.SlotCount; i++)
                {
                    var skillInSlot = secondarySkills.GetSkillInSlot(i);
                    if (skillInSlot != null)
                    {
                        // Slots are stored 1 based; 0 means no slot.
                        slotsBySkill[skillInSlot] = i + 1;
                    }
                }

                foreach (var skill in skills)
                {
                    var desiredLevel = secondarySkills.GetSkillLevel(skill);
                    var levelOffset = levelsOffset + skill.Code;
                    var currentLevel = SkillLevel.Of(buffer[levelOffset]);

                    // A byte that is no level reads as none, so it is left alone while the hero has no level for the skill.
                    if (currentLevel != desiredLevel)
                    {
                        buffer[levelOffset] = desiredLevel.Code;
                    }

                    var slotOffset = slotsOffset + skill.Code;
                    var currentSlot = buffer[slotOffset];

                    if (slotsBySkill.TryGetValue(skill, out var desiredSlot))
                    {
                        buffer[slotOffset] = (byte)desiredSlot;
                    }
                    else if (currentSlot >= 1 && currentSlot <= previousSlotCount)
                    {
                        // Only clear slots the deserializer would have read; leave anything else alone.
                        buffer[slotOffset] = 0;
                    }
                }

                buffer[this.heroOffsets[HeroOffsetName.NumOfSkills]] = (byte)secondarySkills.SlotCount;
            });
        }

        public HeroChunk SerializeDestination(HeroChunk chunk, MapPosition destination)
        {
            return Edit(chunk, buffer =>
            {
                ByteBufferHelper.WriteInt16(buffer, this.heroOffsets[HeroOffsetName.DestinationX], Coordinate(destination.X, nameof(destination)));
                ByteBufferHelper.WriteInt16(buffer, this.heroOffsets[HeroOffsetName.DestinationY], Coordinate(destination.Y, nameof(destination)));
                ByteBufferHelper.WriteInt16(buffer, this.heroOffsets[HeroOffsetName.DestinationZ], Coordinate(destination.Level, nameof(destination)));
            });
        }

        public HeroChunk SerializeClass(HeroChunk chunk, HeroClass heroClass)
        {
            return Edit(chunk, buffer =>
            {
                // A class this editor does not know reads as Knight, and is left alone while the hero stays a Knight.
                var offset = this.heroOffsets[HeroOffsetName.Class];
                if (HeroClass.FromCode<HeroClass>(buffer[offset]) == null && heroClass == HeroClass.Knight)
                {
                    return;
                }

                buffer[offset] = heroClass.Code;
            });
        }

        public HeroChunk SerializeLevel(HeroChunk chunk, HeroLevel level)
        {
            return Edit(chunk, buffer =>
            {
                RangeGuard.ThrowIfOutOfRange<long>(level.Experience, int.MinValue, int.MaxValue, nameof(level));
                ByteBufferHelper.WriteInt32(buffer, this.heroOffsets[HeroOffsetName.ExperiencePoints], (int)level.Experience);
                RangeGuard.ThrowIfOutOfRange<long>(level.Level, 0, Constants.Hero.MaxStoredLevel, nameof(level));
                buffer[this.heroOffsets[HeroOffsetName.Level]] = (byte)level.Level;
            });
        }

        public HeroChunk SerializePosition(HeroChunk chunk, HeroPosition position)
        {
            return Edit(chunk, buffer =>
            {
                // The coordinates of a hero with a biography are not in the chunk, and what is at its start is
                // biography text, so they are left alone.
                if (chunk.OutsidePositionBytes == null)
                {
                    ByteBufferHelper.WriteInt16(buffer, this.heroOffsets[HeroOffsetName.PositionX], Coordinate(position.X, nameof(position)));
                    ByteBufferHelper.WriteInt16(buffer, this.heroOffsets[HeroOffsetName.PositionY], Coordinate(position.Y, nameof(position)));
                    ByteBufferHelper.WriteInt16(buffer, this.heroOffsets[HeroOffsetName.PositionZ], Coordinate(position.Level, nameof(position)));
                }

                // A byte that is no direction reads as North, and is left alone while the hero keeps facing North.
                var facingOffset = this.heroOffsets[HeroOffsetName.Facing];
                if (ByteEnum.TryRead<FacingDirection>(buffer[facingOffset], out _) || position.FacingDirection != FacingDirection.North)
                {
                    buffer[facingOffset] = (byte)position.FacingDirection;
                }
            });
        }

        public HeroChunk SerializePrimarySkills(HeroChunk chunk, PrimarySkills primarySkills)
        {
            return Edit(chunk, buffer =>
            {
                buffer[this.heroOffsets[HeroOffsetName.AttributeAtk]] = SkillByte(primarySkills.Attack);
                buffer[this.heroOffsets[HeroOffsetName.AttributeDef]] = SkillByte(primarySkills.Defense);
                buffer[this.heroOffsets[HeroOffsetName.AttributePow]] = SkillByte(primarySkills.SpellPower);
                buffer[this.heroOffsets[HeroOffsetName.AttributeKno]] = SkillByte(primarySkills.Knowledge);
            });
        }

        public HeroChunk SerializeWarMachines(HeroChunk chunk, IReadOnlyList<WarMachine> warMachines)
        {
            return Edit(chunk, buffer =>
            {
                // Machines can share an offset (ballista / cannon), so remove first, then add. Presence is the first byte, as the
                // deserializer reads it, so a machine that is already there is not rewritten.
                foreach (var warMachine in this.entities.WarMachines)
                {
                    var offset = this.heroOffsets[WarMachineOffsets.For(warMachine)];
                    if (buffer[offset] == warMachine.Code && !warMachines.Contains(warMachine))
                    {
                        ByteBufferHelper.WriteUInt32(buffer, offset, Constants.BlankId);
                    }
                }

                foreach (var warMachine in warMachines)
                {
                    var offset = this.heroOffsets[WarMachineOffsets.For(warMachine)];
                    if (buffer[offset] != warMachine.Code)
                    {
                        ByteBufferHelper.WriteUInt32(buffer, offset, warMachine.Code);
                    }
                }
            });
        }

        public HeroChunk SerializeSpellPoints(HeroChunk chunk, int spellPoints)
        {
            RangeGuard.ThrowIfOutOfRange<long>(spellPoints, 0, ushort.MaxValue, nameof(spellPoints));
            return Edit(chunk, buffer =>
                ByteBufferHelper.WriteUInt16(buffer, this.heroOffsets[HeroOffsetName.CurrentSpellPoints], (ushort)spellPoints));
        }

        public HeroChunk SerializeBackpack(HeroChunk chunk, Backpack backpack)
        {
            return Edit(chunk, buffer =>
            {
                for (var i = 0; i < Constants.BackpackSlotCount; i++)
                {
                    var offset = this.heroOffsets[HeroOffsetName.Backpack] + (i * Constants.BackpackSlotSize);
                    var currentId = ByteBufferHelper.ReadUInt32(buffer, offset);
                    var currentlyPresent = currentId <= byte.MaxValue && Artifact.CarriedInBackpack.ContainsKey((byte)currentId);

                    var slot = i < backpack.All.Count ? backpack.All[i] : null;
                    var artifact = slot?.Art;

                    if (artifact == null)
                    {
                        if (currentlyPresent)
                        {
                            WriteSlot(buffer, offset, Constants.BlankId);
                        }
                    }
                    else if (!currentlyPresent || currentId != artifact.Code)
                    {
                        WriteSlot(buffer, offset, artifact.Code, slot!.Spell?.Code ?? Constants.BlankId);
                    }
                    else if (artifact == Artifact.SpellScroll && slot!.Spell is { } spell
                        && ByteBufferHelper.ReadUInt32(buffer, offset + 4) != spell.Code)
                    {
                        // The same scroll holding another spell. A scroll without a known spell keeps the bytes it has.
                        WriteSlot(buffer, offset, artifact.Code, spell.Code);
                    }
                }
            });
        }

        public HeroChunk SerializeEquippedArtifacts(HeroChunk chunk, EquippedArtifacts equippedArtifacts)
        {
            var slotsToArtifacts = equippedArtifacts
                .GetEquippedArtifacts()
                .ToDictionary(t => t.Item1, t => t.Item2);

            return Edit(chunk, buffer =>
            {
                foreach (var (slot, offset) in this.heroOffsets.EquipmentSlots)
                {
                    var currentCode = buffer[offset];

                    if (slotsToArtifacts.TryGetValue(slot, out var artifact))
                    {
                        var scrollSpell = artifact == Artifact.SpellScroll ? equippedArtifacts.GetScrollSpell(slot) : null;
                        if (currentCode != artifact.Code)
                        {
                            WriteSlot(buffer, offset, artifact.Code, scrollSpell?.Code ?? Constants.BlankId);
                        }
                        else if (scrollSpell != null && ByteBufferHelper.ReadUInt32(buffer, offset + 4) != scrollSpell.Code)
                        {
                            // The same scroll holding another spell. A scroll without a known spell keeps the bytes it has.
                            WriteSlot(buffer, offset, artifact.Code, scrollSpell.Code);
                        }
                    }
                    else if (currentCode != Constants.ByteOff)
                    {
                        WriteSlot(buffer, offset, Constants.BlankId);
                    }
                }

                this.WriteReservedSlotCounters(buffer, slotsToArtifacts);
            });
        }

        public HeroChunk SerializeOwner(HeroChunk chunk, PlayerColor owner)
        {
            return Edit(chunk, buffer =>
            {
                var offset = this.heroOffsets[HeroOffsetName.Owner];
                buffer[offset] = ByteEnumerations.WriteKeeping(owner, buffer[offset], PlayerColor.Neutral);
            });
        }

        public HeroChunk SerializeActiveSpells(HeroChunk chunk, ActiveAdventureSpells active)
        {
            return Edit(chunk, buffer =>
            {
                foreach (var (spell, offset) in this.heroOffsets.ActiveSpellBytes)
                {
                    if (offset >= buffer.Length)
                    {
                        continue;
                    }

                    // A spell that stays active keeps whatever value the game wrote.
                    var isActive = buffer[offset] != Constants.ByteOff;
                    if (isActive != active.HasFlag(spell))
                    {
                        buffer[offset] = active.HasFlag(spell) ? (byte)0 : (byte)Constants.ByteOff;
                    }
                }
            });
        }

        public HeroChunk SerializeIsAsleep(HeroChunk chunk, bool isAsleep)
        {
            // A flag that has any non-zero value counts as set, and is left alone while the hero stays asleep.
            if (this.heroOffsets.AsleepOffset is not { } offset || offset >= chunk.Length)
            {
                return chunk;
            }

            return Edit(chunk, buffer =>
            {
                if ((buffer[offset] != 0) != isAsleep)
                {
                    buffer[offset] = (byte)(isAsleep ? 1 : 0);
                }
            });
        }

        public HeroChunk SerializeMovedToday(HeroChunk chunk, bool movedToday)
        {
            // A flag that has any non-zero value counts as set, and is left alone while the hero stays "moved".
            if (this.heroOffsets[HeroOffsetName.MovedToday] >= chunk.Length)
            {
                return chunk;
            }

            return Edit(chunk, buffer =>
            {
                var offset = this.heroOffsets[HeroOffsetName.MovedToday];
                if ((buffer[offset] != 0) != movedToday)
                {
                    buffer[offset] = (byte)(movedToday ? 1 : 0);
                }
            });
        }

        public HeroChunk SerializeGender(HeroChunk chunk, HeroGender? gender)
        {
            // A gender byte this editor does not know is kept as it is.
            var offset = this.heroOffsets[HeroOffsetName.Gender];
            if (!gender.HasValue || chunk.GetHeaderByteAt(offset) == (byte)gender.Value)
            {
                return chunk;
            }

            return chunk.WithHeaderByte(offset, (byte)gender.Value);
        }

        public HeroChunk SerializeHasSpellBook(HeroChunk chunk, bool hasSpellBook)
        {
            return Edit(chunk, buffer =>
            {
                var offset = this.heroOffsets[HeroOffsetName.SpellBook];
                var currentlyHasSpellBook = ByteBufferHelper.ReadUInt32(buffer, offset) == Constants.ByteOn;

                if (currentlyHasSpellBook != hasSpellBook)
                {
                    WriteSlot(buffer, offset, hasSpellBook ? Constants.ByteOn : Constants.BlankId);
                }
            });
        }

        public HeroChunk SerializeSpellBook(HeroChunk chunk, SpellBook spellBook)
        {
            return Edit(chunk, buffer =>
            {
                // The file only ever holds 0 or 1 here (the loader regex enforces it), so writing is lossless.
                foreach (var spell in this.entities.Spells)
                {
                    buffer[this.heroOffsets[HeroOffsetName.SpellBookSpells] + spell.Code] = spellBook.IsLearned(spell) ? (byte)1 : (byte)0;
                    buffer[this.heroOffsets[HeroOffsetName.AvailableSpells] + spell.Code] = spellBook.IsAvailable(spell) ? (byte)1 : (byte)0;
                }
            });
        }

        // The savefile holds a coordinate in 16 bits; one that does not fit is refused and not cut.
        private static short Coordinate(int value, string name)
        {
            RangeGuard.ThrowIfOutOfRange<long>(value, short.MinValue, short.MaxValue, name);
            return (short)value;
        }

        // The savefile holds a skill, with the bonus of the artifacts in it, in a byte; more is refused and not cut.
        private static byte SkillByte(int value)
        {
            RangeGuard.ThrowIfOutOfRange<long>(value, Constants.Hero.MinPrimarySkill, Constants.Hero.MaxSavedPrimarySkill, "primarySkills");
            return (byte)value;
        }

        private static int BitCount(int value) => System.Numerics.BitOperations.PopCount((uint)value);

        private static HeroChunk Edit(HeroChunk chunk, Action<byte[]> write)
        {
            var buffer = chunk.ToArray();
            write(buffer);
            return chunk.WithBytes(buffer);
        }

        /// <summary>
        /// Writes an 8-byte artifact slot: the id, followed by the spell id of a Spell Scroll or a blank one. A blank slot is all 0xFF.
        /// </summary>
        private static void WriteSlot(byte[] buffer, int offset, uint id, uint spellId = Constants.BlankId)
        {
            ByteBufferHelper.WriteUInt32(buffer, offset, id);
            ByteBufferHelper.WriteUInt32(buffer, offset + 4, spellId);
        }

        /// <summary>
        /// Marks the slots taken by combination artifacts, like h3sed: for every combination artifact each blocked slot
        /// except the one it is stored in adds 1 to the counter of that slot's group. Counters above 5 are left alone
        /// when nothing reserves them, because Horn of the Abyss uses them for an unknown purpose.
        /// </summary>
        private void WriteReservedSlotCounters(byte[] buffer, IReadOnlyDictionary<ArtifactSlots, Artifact> equipped)
        {
            var combinations = equipped.Where(e => e.Value.IsCombination).ToList();

            foreach (var (group, offset) in this.heroOffsets.ReservedSlotCounters)
            {
                if (offset >= buffer.Length)
                {
                    continue;
                }

                var count = combinations.Sum(e => BitCount((int)(e.Value.BlockedSlots & ~e.Key & group)));
                if (count == 0 && buffer[offset] > 5)
                {
                    continue;
                }

                buffer[offset] = (byte)count;
            }
        }
    }
}
