namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc;
    using Siniskalc.Core.Data;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    public sealed class HeroDeserializer : IHeroDeserializer
    {
        private readonly HeroOffsets heroOffsets;
        private readonly VersionSpecificEntitiesProvider entities;

        public HeroDeserializer(HeroOffsets heroOffsets, VersionSpecificEntitiesProvider entities)
        {
            this.heroOffsets = heroOffsets;
            this.entities = entities;
        }

        public HeroModel Deserialize(HeroChunk chunk)
        {
            var equippedArtifacts = this.DeserializeEquippedArtifacts(chunk);
            return new HeroModel(
                chunk.Name,
                this.entities.GrantedSpellsResolver,
                this.DeserializeOwner(chunk),
                this.DeserializePrimarySkills(chunk),
                this.DeserializeClass(chunk),
                this.DeserializeLevel(chunk),
                this.DeserializePosition(chunk),
                this.DeserializeMarkerPosition(chunk),
                this.DeserializeMovementPoints(chunk),
                this.DeserializeSecondarySkills(chunk),
                this.DeserializeBackpack(chunk),
                equippedArtifacts,
                this.DeserializeWarMachines(chunk),
                this.DeserializeSpellPoints(chunk),
                this.DeserializeCreatures(chunk),
                this.DeserializeSpellBook(chunk, equippedArtifacts),
                this.DeserializeHeroId(chunk),
                chunk.Biography,
                this.DeserializeGender(chunk),
                this.DeserializeMovedToday(chunk),
                this.DeserializeActiveSpells(chunk),
                this.DeserializeIsAsleep(chunk),
                this.DeserializeMoraleBonus(chunk),
                this.DeserializeLuckBonus(chunk),
                this.DeserializeVisitEffects(chunk));
        }

        public int DeserializeMoraleBonus(HeroChunk chunk) => (sbyte)chunk.GetByteAt(this.heroOffsets[HeroOffsetName.MoraleBonus]);

        public int DeserializeLuckBonus(HeroChunk chunk) => (sbyte)chunk.GetByteAt(this.heroOffsets[HeroOffsetName.LuckBonus]);

        public VisitedBuildingEffects DeserializeVisitEffects(HeroChunk chunk)
        {
            var effects = VisitedBuildingEffects.None;
            foreach (var (effect, offset, mask) in this.heroOffsets.VisitEffectBits)
            {
                if (offset < chunk.Length && (chunk.GetByteAt(offset) & mask) != 0)
                {
                    effects |= effect;
                }
            }

            return effects;
        }

        public ActiveAdventureSpells DeserializeActiveSpells(HeroChunk chunk)
        {
            var active = ActiveAdventureSpells.None;
            foreach (var (spell, offset) in this.heroOffsets.ActiveSpellBytes)
            {
                if (offset < chunk.Length && chunk.GetByteAt(offset) != Constants.ByteOff)
                {
                    active |= spell;
                }
            }

            return active;
        }

        public int DeserializeHeroId(HeroChunk chunk)
        {
            return chunk.GetByteAt(this.heroOffsets[HeroOffsetName.HeroId]);
        }

        public bool DeserializeIsAsleep(HeroChunk chunk)
        {
            return this.heroOffsets.AsleepOffset is { } offset && offset < chunk.Length && chunk.GetByteAt(offset) != 0;
        }

        public bool DeserializeMovedToday(HeroChunk chunk)
        {
            return this.heroOffsets[HeroOffsetName.MovedToday] < chunk.Length
                && chunk.GetByteAt(this.heroOffsets[HeroOffsetName.MovedToday]) != 0;
        }

        public Army DeserializeCreatures(HeroChunk chunk)
        {
            var slots = new List<ArmySlot>();

            for (var i = 0; i < Constants.CreatureSlotCount; i++)
            {
                var creatureCode = chunk.GetByteAt(this.heroOffsets[HeroOffsetName.Creatures] + (i * 4));
                if (creatureCode == Constants.ByteOff)
                {
                    continue;
                }

                var creature = Creature.FromByte(creatureCode);
                if (creature == null)
                {
                    // TODO:LOG warning: the hero "{chunk.Name}" has creature code {creatureCode} in slot {i + 1}, which this editor does not know; the slot reads as empty and its bytes are kept.
                    continue;
                }

                var creatureCount = chunk.GetUInt32At(this.heroOffsets[HeroOffsetName.CreaturesAmounts] + (i * 4));

                // Slots are 1 - 7.
                slots.Add(new ArmySlot(i + 1, creature.Code, creatureCount));
            }

            return new Army([.. slots]);
        }

        public HeroMovementPoints DeserializeMovementPoints(HeroChunk chunk)
        {
            return new HeroMovementPoints
            {
                CurrentMovementPoints = chunk.GetUInt32At(this.heroOffsets[HeroOffsetName.CurrentMovementPoints]),
                MaxMovementPoints = chunk.GetUInt32At(this.heroOffsets[HeroOffsetName.MaxMovementPoints]),
            };
        }

        public SecondarySkills DeserializeSecondarySkills(HeroChunk chunk)
        {
            var skills = this.entities.SecondarySkills;
            var allSkillsToLevels = new Dictionary<SecondarySkill, SkillLevel?>();

            // We read all the skills, rather than just the ones in the 8 slots, because the game supports more skills
            // being set than just the 8, and their effects will apply. This can be interesting for custom maps or mods.
            foreach (var skill in skills)
            {
                var skillLevelByte = chunk.GetByteAt(this.heroOffsets[HeroOffsetName.Skills] + skill.Code);

                // TODO:LOG info, when the byte is neither 0 nor a skill level: the hero has skill level byte {skillLevelByte} for {skill.Name}, which reads as no level and is kept.
                allSkillsToLevels.Add(skill, ByteEnum.TryRead<SkillLevel>(skillLevelByte, out var skillLevel) ? skillLevel : null);
            }

            // The slots table have one byte per skill. The index represents the skill's byte, and the value is the slot in which the skill is in.
            // E.g. a hero with Pathfinding (byte code 0) in slot 1 and Diplomacy (byte code 4) in slot 2 has table: 1, 0, 0, 0, 2, 0, ...
            var slotCount = chunk.GetByteAt(this.heroOffsets[HeroOffsetName.NumOfSkills]);
            var slots = new SecondarySkill[slotCount];

            foreach (var skill in skills)
            {
                var slot = chunk.GetByteAt(this.heroOffsets[HeroOffsetName.SkillSlots] + skill.Code);
                if (slot >= 1 && slot <= slotCount)
                {
                    slots[slot - 1] = skill;
                }
                else if (slot != 0)
                {
                    // TODO:LOG warning: the hero "{chunk.Name}" has a secondary skill slot value {slot} out of range for the skill count; the skill is ignored.
                }
            }

            return new SecondarySkills(allSkillsToLevels, slots);
        }

        public Position DeserializeMarkerPosition(HeroChunk chunk)
        {
            var markerX = chunk.GetInt16At(this.heroOffsets[HeroOffsetName.MarkerX]);
            var markerY = chunk.GetInt16At(this.heroOffsets[HeroOffsetName.MarkerY]);
            var markerZ = chunk.GetInt16At(this.heroOffsets[HeroOffsetName.MarkerZ]);

            return new Position(markerX, markerY, markerZ);
        }

        public HeroClass DeserializeClass(HeroChunk chunk)
        {
            // TODO:LOG warning, when the byte is no known class: the hero "{chunk.Name}" has class byte {value}, which reads as Knight and is kept until the class is changed.
            return HeroClass.FromByte(chunk.GetByteAt(this.heroOffsets[HeroOffsetName.Class]));
        }

        public HeroLevel DeserializeLevel(HeroChunk chunk)
        {
            return new HeroLevel
            {
                Experience = chunk.GetInt32At(this.heroOffsets[HeroOffsetName.ExperiencePoints]),
                Level = chunk.GetByteAt(this.heroOffsets[HeroOffsetName.Level]),
            };
        }

        public HeroPosition DeserializePosition(HeroChunk chunk)
        {
            var outside = chunk.OutsidePositionBytes;
            var x = outside != null ? BitConverter.ToInt16(outside, 0) : chunk.GetInt16At(this.heroOffsets[HeroOffsetName.PositionX]);
            var y = outside != null ? BitConverter.ToInt16(outside, 2) : chunk.GetInt16At(this.heroOffsets[HeroOffsetName.PositionY]);
            var z = outside != null ? BitConverter.ToInt16(outside, 4) : chunk.GetInt16At(this.heroOffsets[HeroOffsetName.PositionZ]);

            // A byte that is no direction reads as the first one, North, and is left alone by the serializer while the hero keeps facing it.
            // TODO:LOG info, when the byte is no direction: the hero "{chunk.Name}" has facing byte {value}, which reads as North and is kept.
            var facing = ByteEnum.TryRead<FacingDirection>(chunk.GetByteAt(this.heroOffsets[HeroOffsetName.Facing]), out var direction) ? direction : FacingDirection.North;

            return new HeroPosition(x, y, z, facing);
        }

        public HeroPrimarySkills DeserializePrimarySkills(HeroChunk chunk)
        {
            return new HeroPrimarySkills(
                chunk.GetByteAt(this.heroOffsets[HeroOffsetName.AttributeAtk]),
                chunk.GetByteAt(this.heroOffsets[HeroOffsetName.AttributeDef]),
                chunk.GetByteAt(this.heroOffsets[HeroOffsetName.AttributePow]),
                chunk.GetByteAt(this.heroOffsets[HeroOffsetName.AttributeKno]));
        }

        public IReadOnlyList<WarMachine> DeserializeWarMachines(HeroChunk chunk)
        {
            return this.entities.WarMachines
                .Where(warMachine => chunk.GetByteAt(this.heroOffsets[WarMachineOffsets.For(warMachine)]) == warMachine.Code)
                .ToList();
        }

        public ushort DeserializeSpellPoints(HeroChunk chunk)
        {
            return chunk.GetUInt16At(this.heroOffsets[HeroOffsetName.CurrentSpellPoints]);
        }

        public Backpack DeserializeBackpack(HeroChunk chunk)
        {
            var artifacts = new BackpackSlot[Constants.BackpackSlotCount];

            // Each slot is 8 bytes: a 4-byte artifact id followed by 4 bytes (the spell id for Spell Scrolls).
            // A slot whose id bytes are all 0xFF is empty.
            for (var i = 0; i < Constants.BackpackSlotCount; i++)
            {
                var offset = this.heroOffsets[HeroOffsetName.Backpack] + (i * Constants.BackpackSlotSize);
                var id = chunk.GetUInt32At(offset);
                if (id == Constants.BlankId)
                {
                    continue;
                }

                if (id <= byte.MaxValue && Artifact.CarriedInBackpack.TryGetValue((byte)id, out var artifact))
                {
                    var spellId = chunk.GetUInt32At(offset + 4);
                    var spell = artifact == Artifact.SpellScroll && spellId <= byte.MaxValue ? Spell.FromCode<Spell>((byte)spellId) : null;
                    artifacts[i] = new BackpackSlot(i, artifact, spell);
                }
                else
                {
                    // TODO:LOG warning: the hero "{chunk.Name}" has artifact id {id} in slot {slot}, which this editor does not know; the slot reads as empty and its bytes are kept.
                }
            }

            return new Backpack(artifacts);
        }

        public EquippedArtifacts DeserializeEquippedArtifacts(HeroChunk chunk)
        {
            var slotsToArtifacts = new Dictionary<ArtifactSlots, Artifact>();
            var scrollSpells = new Dictionary<ArtifactSlots, Spell>();

            foreach (var (slot, offset) in this.heroOffsets.EquipmentSlots)
            {
                var code = chunk.GetByteAt(offset);
                if (code == Constants.ByteOff)
                {
                    continue;
                }

                if (Artifact.All.TryGetValue(code, out var artifact))
                {
                    slotsToArtifacts[slot] = artifact;

                    // Like a backpack slot, a worn scroll keeps its spell id in the 4 bytes after the artifact id.
                    var spellId = chunk.GetUInt32At(offset + 4);
                    if (artifact == Artifact.SpellScroll && spellId <= byte.MaxValue && Spell.FromCode<Spell>((byte)spellId) is { } spell)
                    {
                        scrollSpells[slot] = spell;
                    }
                }
                else
                {
                    // TODO:LOG warning: the hero "{chunk.Name}" has artifact id {id} in slot {slot}, which this editor does not know; the slot reads as empty and its bytes are kept.
                }
            }

            return new EquippedArtifacts(slotsToArtifacts, scrollSpells);
        }

        public PlayerColor? DeserializeOwner(HeroChunk chunk)
        {
            var playerByte = chunk.GetByteAt(this.heroOffsets[HeroOffsetName.Owner]);

            return ByteEnum.TryRead<PlayerColor>(playerByte, out var player) ? player : null;
        }

        public HeroGender? DeserializeGender(HeroChunk chunk)
        {
            return ByteEnum.TryRead<HeroGender>(chunk.GetHeaderByteAt(this.heroOffsets[HeroOffsetName.Gender]), out var gender) ? gender : null;
        }

        public bool DeserializeHasSpellBook(HeroChunk chunk)
        {
            // The spellbook slot holds the Spell Book artifact, whose id is 0.
            return chunk.GetUInt32At(this.heroOffsets[HeroOffsetName.SpellBook]) == Constants.ByteOn;
        }

        /// <summary>
        /// Reads the spell book of the hero. The spells its <paramref name="equippedArtifacts"/> grant are told apart from the
        /// other available spells, which are kept as the savefile has them.
        /// </summary>
        public SpellBook DeserializeSpellBook(HeroChunk chunk, EquippedArtifacts equippedArtifacts)
        {
            var spellBookSpells = new HashSet<Spell>();
            var availableSpells = new HashSet<Spell>();

            foreach (var spell in this.entities.Spells)
            {
                if (chunk.GetByteAt(this.heroOffsets[HeroOffsetName.SpellBookSpells] + spell.Code) == 1)
                {
                    spellBookSpells.Add(spell);
                }

                if (chunk.GetByteAt(this.heroOffsets[HeroOffsetName.AvailableSpells] + spell.Code) == 1)
                {
                    availableSpells.Add(spell);
                }
            }

            // The savefile has one "available" flag per spell: what the hero has learned or an artifact grants is not "other".
            var granted = this.entities.GrantedSpellsResolver.ResolveGrantedSpells(equippedArtifacts);
            var otherAvailable = availableSpells.Where(s => !spellBookSpells.Contains(s) && !granted.ContainsKey(s)).ToHashSet();

            return new SpellBook(spellBookSpells, otherAvailable, this.DeserializeHasSpellBook(chunk));
        }
    }
}
