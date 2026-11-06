namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
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
    public sealed class SafeHeroSerializer : ISerializer<HeroChunk, HeroModel>
    {
        private static readonly ILogger<SafeHeroSerializer> Logger = LogFactory.For<SafeHeroSerializer>();

        private readonly HeroOffsets heroOffsets;
        private readonly VersionSpecificEntitiesProvider entities;

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeHeroSerializer"/> class.
        /// </summary>
        /// <param name="heroOffsets">The hero offsets.</param>
        /// <param name="entities">The entities.</param>
        public SafeHeroSerializer(HeroOffsets heroOffsets, VersionSpecificEntitiesProvider entities)
        {
            this.heroOffsets = heroOffsets;
            this.entities = entities;
        }

        /// <inheritdoc/>
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
            chunk = this.SerializeVisitBonus(chunk, hero.MoraleBonus, hero.LuckBonus);
            chunk = this.SerializeVisitEffects(chunk, hero.VisitEffects);
            chunk = this.SerializeIsAsleep(chunk, hero.IsAsleep);
            chunk = this.SerializeHasSpellBook(chunk, hero.SpellBook.HasSpellBook);
            chunk = this.SerializeCreatures(chunk, hero.Army);
            chunk = this.SerializeSpellBook(chunk, hero.SpellBook);
            chunk = this.SerializePlaceVisits(chunk, hero.PlaceVisits);

            return chunk;
        }

        /// <summary>
        /// Writes the places the hero has visited into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="visits">The visits.</param>
        /// <returns>The chunk with the places the hero has visited written; the chunk given is not changed.</returns>
        public HeroChunk SerializePlaceVisits(HeroChunk chunk, PlaceVisits visits)
        {
            return Edit(chunk, buffer =>
            {
                foreach (var kind in this.entities.Catalog.VisitablePlaces.All)
                {
                    // A kind the savegame has no room for, or that does not exist in this version, is left alone.
                    var offset = VisitedPlaceOffsets.For(kind.Slot);
                    if (offset + sizeof(uint) <= buffer.Length && (!kind.IsHotaOnly || this.heroOffsets.GameVersion == GameVersion.HornOfTheAbyss)
                        && ByteBufferHelper.ReadUInt32(buffer, offset) != visits.Of(kind))
                    {
                        ByteBufferHelper.WriteUInt32(buffer, offset, visits.Of(kind));
                    }
                }
            });
        }

        /// <summary>
        /// Writes the army of the hero into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="creatures">The army.</param>
        /// <returns>The chunk with the army of the hero written; the chunk given is not changed.</returns>
        public HeroChunk SerializeCreatures(HeroChunk chunk, IArmy<Creature> creatures)
        {
            return Edit(chunk, buffer => ArmyWriter.Write(buffer, this.heroOffsets.Army, creatures, this.entities.CreatureCodes, nameof(creatures)));
        }

        /// <summary>
        /// Writes the movement points of the hero into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="movementPoints">The movement points.</param>
        /// <returns>The chunk with the movement points of the hero written; the chunk given is not changed.</returns>
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

        /// <summary>
        /// Writes the secondary skills of the hero into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="secondarySkills">The skills.</param>
        /// <returns>The chunk with the secondary skills of the hero written; the chunk given is not changed.</returns>
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

        /// <summary>
        /// Writes where the hero is going into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="destination">The destination, or <see cref="MapPosition.None"/> for none.</param>
        /// <returns>The chunk with where the hero is going written; the chunk given is not changed.</returns>
        public HeroChunk SerializeDestination(HeroChunk chunk, MapPosition destination)
        {
            return Edit(chunk, buffer =>
            {
                // The marker of a hero that has none reads as no destination at all, whatever its level byte says: it is left alone then.
                var x = this.heroOffsets[HeroOffsetName.DestinationX];
                var y = this.heroOffsets[HeroOffsetName.DestinationY];
                var z = this.heroOffsets[HeroOffsetName.DestinationZ];
                var none = BitConverter.ToInt32(buffer, x) < 0 || BitConverter.ToInt32(buffer, y) < 0;
                if (destination == MapPosition.None)
                {
                    if (!none)
                    {
                        // The game leaves x and y at -1 and the level at 0 for a hero with no marker.
                        ByteBufferHelper.WriteInt32(buffer, x, -1);
                        ByteBufferHelper.WriteInt32(buffer, y, -1);
                        ByteBufferHelper.WriteInt16(buffer, z, 0);
                    }

                    return;
                }

                ByteBufferHelper.WriteInt32(buffer, x, Coordinate(destination.X, nameof(destination)));
                ByteBufferHelper.WriteInt32(buffer, y, Coordinate(destination.Y, nameof(destination)));
                ByteBufferHelper.WriteInt16(buffer, z, Coordinate(destination.Level, nameof(destination)));
            });
        }

        /// <summary>
        /// Writes the class of the hero into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="heroClass">The class.</param>
        /// <returns>The chunk with the class of the hero written; the chunk given is not changed.</returns>
        public HeroChunk SerializeClass(HeroChunk chunk, HeroClass heroClass)
        {
            return Edit(chunk, buffer =>
            {
                // A class this editor does not know reads as Knight, and is left alone while the hero stays a Knight.
                var offset = this.heroOffsets[HeroOffsetName.Class];
                if (this.entities.Catalog.HeroClasses.Find(buffer[offset]) == null && heroClass == this.entities.Catalog.FallbackHeroClass)
                {
                    Logger.LogTrace("Class byte {Code:X2} is kept while the hero stays {StandIn}", buffer[offset], heroClass.Id);
                    return;
                }

                buffer[offset] = heroClass.Code;
            });
        }

        /// <summary>
        /// Writes the level and the experience of the hero into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="level">The level and the experience.</param>
        /// <returns>The chunk with the level and the experience of the hero written; the chunk given is not changed.</returns>
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

        /// <summary>
        /// Writes where the hero is on the map into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="position">The position.</param>
        /// <returns>The chunk with where the hero is on the map written; the chunk given is not changed.</returns>
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

        /// <summary>
        /// Writes the primary skills of the hero into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="primarySkills">The skills.</param>
        /// <returns>The chunk with the primary skills of the hero written; the chunk given is not changed.</returns>
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

        /// <summary>
        /// Writes the war machines of the hero into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="warMachines">The war machines the hero has.</param>
        /// <returns>The chunk with the war machines of the hero written; the chunk given is not changed.</returns>
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

        /// <summary>
        /// Writes the spell points of the hero into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="spellPoints">The spell points.</param>
        /// <returns>The chunk with the spell points of the hero written; the chunk given is not changed.</returns>
        public HeroChunk SerializeSpellPoints(HeroChunk chunk, int spellPoints)
        {
            RangeGuard.ThrowIfOutOfRange<long>(spellPoints, 0, ushort.MaxValue, nameof(spellPoints));
            return Edit(chunk, buffer =>
                ByteBufferHelper.WriteUInt16(buffer, this.heroOffsets[HeroOffsetName.CurrentSpellPoints], (ushort)spellPoints));
        }

        /// <summary>
        /// Writes the backpack of the hero into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="backpack">The backpack.</param>
        /// <returns>The chunk with the backpack of the hero written; the chunk given is not changed.</returns>
        public HeroChunk SerializeBackpack(HeroChunk chunk, Backpack backpack)
        {
            return Edit(chunk, buffer =>
            {
                for (var i = 0; i < Constants.BackpackSlotCount; i++)
                {
                    var offset = this.heroOffsets[HeroOffsetName.Backpack] + (i * Constants.BackpackSlotSize);
                    var currentId = ByteBufferHelper.ReadUInt32(buffer, offset);
                    var currentlyPresent = currentId <= byte.MaxValue && this.entities.Catalog.Artifacts.Find((byte)currentId) is { CanBeInBackpack: true };

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
                    else if (artifact.IsScroll && slot!.Spell is { } spell
                        && ByteBufferHelper.ReadUInt32(buffer, offset + 4) != spell.Code)
                    {
                        // The same scroll holding another spell. A scroll without a known spell keeps the bytes it has.
                        WriteSlot(buffer, offset, artifact.Code, spell.Code);
                    }
                }
            });
        }

        /// <summary>
        /// Writes the artifacts the hero wears into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="equippedArtifacts">The artifacts, by slot.</param>
        /// <returns>The chunk with the artifacts the hero wears written; the chunk given is not changed.</returns>
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
                        var scrollSpell = artifact.IsScroll ? equippedArtifacts.GetScrollSpell(slot) : null;
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

        /// <summary>
        /// Writes the player the hero serves into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="owner">The colour of the player.</param>
        /// <returns>The chunk with the player the hero serves written; the chunk given is not changed.</returns>
        public HeroChunk SerializeOwner(HeroChunk chunk, PlayerColor owner)
        {
            return Edit(chunk, buffer =>
            {
                var offset = this.heroOffsets[HeroOffsetName.Owner];
                buffer[offset] = ByteEnumerations.WriteKeeping(owner, buffer[offset], PlayerColor.Neutral);
            });
        }

        /// <summary>
        /// Writes the adventure spells that are active on the hero into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="active">The active spells.</param>
        /// <returns>The chunk with the adventure spells that are active on the hero written; the chunk given is not changed.</returns>
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

        /// <summary>
        /// Writes the morale and the luck that visited buildings gave the hero until its next battle, each a signed byte. A byte is only
        /// written when it reads as something else.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="morale">The morale the visits give.</param>
        /// <param name="luck">The luck the visits give.</param>
        /// <returns>The chunk with the bonus written; the chunk given is not changed.</returns>
        public HeroChunk SerializeVisitBonus(HeroChunk chunk, int morale, int luck)
        {
            RangeGuard.ThrowIfOutOfRange(morale, sbyte.MinValue, sbyte.MaxValue, nameof(morale));
            RangeGuard.ThrowIfOutOfRange(luck, sbyte.MinValue, sbyte.MaxValue, nameof(luck));
            return Edit(chunk, buffer =>
            {
                foreach (var (offset, value) in new[] { (this.heroOffsets[HeroOffsetName.MoraleBonus], morale), (this.heroOffsets[HeroOffsetName.LuckBonus], luck) })
                {
                    if ((sbyte)buffer[offset] != value)
                    {
                        buffer[offset] = unchecked((byte)(sbyte)value);
                    }
                }
            });
        }

        /// <summary>
        /// Sets or clears the bit of each visited building the editor knows (<see cref="HeroOffsets.VisitEffectBits"/>). The other bits of
        /// those bytes are not known and are left as they are.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="effects">The effects the hero remembers.</param>
        /// <returns>The chunk with the effects written; the chunk given is not changed.</returns>
        public HeroChunk SerializeVisitEffects(HeroChunk chunk, VisitedBuildingEffects effects)
        {
            return Edit(chunk, buffer =>
            {
                foreach (var (effect, offset, mask) in this.heroOffsets.VisitEffectBits)
                {
                    var wanted = effects.HasFlag(effect);
                    if (((buffer[offset] & mask) != 0) != wanted)
                    {
                        buffer[offset] = (byte)(wanted ? buffer[offset] | mask : buffer[offset] & ~mask);
                    }
                }
            });
        }

        /// <summary>
        /// Writes whether the hero is asleep into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="isAsleep">Whether the hero is asleep.</param>
        /// <returns>The chunk with whether the hero is asleep written; the chunk given is not changed.</returns>
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

        /// <summary>
        /// Writes whether the hero has moved today into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="movedToday">Whether the hero has moved.</param>
        /// <returns>The chunk with whether the hero has moved today written; the chunk given is not changed.</returns>
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

        /// <summary>
        /// Writes the gender of the hero into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="gender">The gender, or <see langword="null"/> to leave it as it is.</param>
        /// <returns>The chunk with the gender of the hero written; the chunk given is not changed.</returns>
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

        /// <summary>
        /// Writes whether the hero has the spell book into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="hasSpellBook">Whether the hero has the spell book.</param>
        /// <returns>The chunk with whether the hero has the spell book written; the chunk given is not changed.</returns>
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

        /// <summary>
        /// Writes the spells of the spell book of the hero into the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="spellBook">The spell book.</param>
        /// <returns>The chunk with the spells of the spell book of the hero written; the chunk given is not changed.</returns>
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
