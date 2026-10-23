namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    public sealed class HeroDeserializer : IHeroDeserializer
    {
        private readonly HeroOffsets heroOffsets;
        private readonly VersionSpecificEntitiesProvider entities;
        private readonly ILogger<HeroDeserializer> logger;

        public HeroDeserializer(HeroOffsets heroOffsets, VersionSpecificEntitiesProvider entities, ILogger<HeroDeserializer> logger)
        {
            this.heroOffsets = heroOffsets;
            this.entities = entities;
            this.logger = logger;
        }

        public HeroModel Deserialize(HeroChunk chunk)
        {
            var equippedArtifacts = this.DeserializeEquippedArtifacts(chunk);
            return new HeroModel(
                this.DeserializeHeroId(chunk),
                chunk.Name,
                this.DeserializeOwner(chunk),
                this.DeserializePosition(chunk),
                this.DeserializeDestination(chunk),
                this.DeserializeMovementPoints(chunk),
                this.DeserializeLevel(chunk),
                this.DeserializeSpellPoints(chunk),
                this.DeserializePrimarySkills(chunk),
                this.DeserializeSecondarySkills(chunk),
                this.DeserializeCreatures(chunk),
                this.entities.GrantedSpellsResolver,
                this.DeserializeClass(chunk),
                this.DeserializeBackpack(chunk),
                equippedArtifacts,
                this.DeserializeWarMachines(chunk),
                this.DeserializeSpellBook(chunk, equippedArtifacts),
                chunk.Biography,
                this.DeserializeGender(chunk),
                this.DeserializeMovedToday(chunk),
                this.DeserializeActiveSpells(chunk),
                this.DeserializeIsAsleep(chunk),
                this.DeserializeMoraleBonus(chunk),
                this.DeserializeLuckBonus(chunk),
                this.DeserializeVisitEffects(chunk),
                this.DeserializePlaceVisits(chunk));
        }

        public PlaceVisits DeserializePlaceVisits(HeroChunk chunk) => new(VisitedPlaceKind.All.Select(kind => this.HasPlaces(chunk, kind) ? chunk.GetUInt32At(kind.HeroOffset) : 0u));

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

        public IArmy<Creature> DeserializeCreatures(HeroChunk chunk) => ArmyReader.Read(chunk, this.heroOffsets.Army, CreatureCodes.Instance, this.logger);

        public MovementPoints DeserializeMovementPoints(HeroChunk chunk)
        {
            return new MovementPoints(
                chunk.GetUInt32At(this.heroOffsets[HeroOffsetName.CurrentMovementPoints]),
                chunk.GetUInt32At(this.heroOffsets[HeroOffsetName.MaxMovementPoints]));
        }

        public SecondarySkills DeserializeSecondarySkills(HeroChunk chunk)
        {
            using var subject = this.logger.BeginSubject("Hero", chunk.Name);
            var skills = this.entities.SecondarySkills;
            var allSkillsToLevels = new Dictionary<SecondarySkill, SkillLevel>();

            // We read all the skills, rather than just the ones in the 8 slots, because the game supports more skills
            // being set than just the 8, and their effects will apply. This can be interesting for custom maps or mods.
            foreach (var skill in skills)
            {
                var skillLevelByte = chunk.GetByteAt(this.heroOffsets[HeroOffsetName.Skills] + skill.Code);

                if (skillLevelByte > SkillLevel.Expert.Code)
                {
                    this.logger.LogTrace("Skill level byte {Code:X2} for {Skill} read as no level and kept", skillLevelByte, skill.Name);
                }

                allSkillsToLevels.Add(skill, SkillLevel.Of(skillLevelByte));
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
                    this.logger.LogTrace("Secondary skill slot value {Slot} is out of range for {Count} skills, skill {Skill} ignored", slot, slotCount, skill.Name);
                }
            }

            return new SecondarySkills(allSkillsToLevels, slots);
        }

        public MapPosition DeserializeDestination(HeroChunk chunk)
        {
            // Two 32 bit numbers, x and y (FFFFFFFF for none), and then the level in 16 bits, which is 0 when there is no marker.
            var destinationX = chunk.GetInt32At(this.heroOffsets[HeroOffsetName.DestinationX]);
            var destinationY = chunk.GetInt32At(this.heroOffsets[HeroOffsetName.DestinationY]);
            var destinationZ = chunk.GetInt16At(this.heroOffsets[HeroOffsetName.DestinationZ]);

            return destinationX < 0 || destinationY < 0 ? MapPosition.None : new MapPosition(destinationX, destinationY, destinationZ);
        }

        public HeroClass DeserializeClass(HeroChunk chunk)
        {
            using var subject = this.logger.BeginSubject("Hero", chunk.Name);
            var code = chunk.GetByteAt(this.heroOffsets[HeroOffsetName.Class]);
            if (HeroClass.FromCode<HeroClass>(code) is null)
            {
                this.logger.LogTrace("Class byte {Code:X2} read as Knight and kept until the class is changed", code);
            }

            return HeroClass.FromByte(code);
        }

        public HeroLevel DeserializeLevel(HeroChunk chunk)
        {
            return new HeroLevel(
                chunk.GetInt32At(this.heroOffsets[HeroOffsetName.ExperiencePoints]),
                chunk.GetByteAt(this.heroOffsets[HeroOffsetName.Level]));
        }

        public HeroPosition DeserializePosition(HeroChunk chunk)
        {
            using var subject = this.logger.BeginSubject("Hero", chunk.Name);
            var outside = chunk.OutsidePositionBytes;
            var x = outside != null ? BitConverter.ToInt16(outside, 0) : chunk.GetInt16At(this.heroOffsets[HeroOffsetName.PositionX]);
            var y = outside != null ? BitConverter.ToInt16(outside, 2) : chunk.GetInt16At(this.heroOffsets[HeroOffsetName.PositionY]);
            var z = outside != null ? BitConverter.ToInt16(outside, 4) : chunk.GetInt16At(this.heroOffsets[HeroOffsetName.PositionZ]);

            // A byte that is no direction reads as the first one, North, and is left alone by the serializer while the hero keeps facing it.
            var facingByte = chunk.GetByteAt(this.heroOffsets[HeroOffsetName.Facing]);
            if (!ByteEnum.TryRead<FacingDirection>(facingByte, out var direction))
            {
                direction = FacingDirection.North;
                this.logger.LogTrace("Facing byte {Code:X2} read as North and kept", facingByte);
            }

            var facing = direction;

            return new HeroPosition(x, y, z, facing);
        }

        public PrimarySkills DeserializePrimarySkills(HeroChunk chunk)
        {
            return new PrimarySkills(
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
            using var subject = this.logger.BeginSubject("Hero", chunk.Name);
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
                    this.logger.LogTrace("Artifact id {Code:X} in backpack slot {Slot} is unknown, the slot reads as empty and its bytes are kept", id, i);
                }
            }

            return new Backpack(artifacts);
        }

        public EquippedArtifacts DeserializeEquippedArtifacts(HeroChunk chunk)
        {
            using var subject = this.logger.BeginSubject("Hero", chunk.Name);
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
                    this.logger.LogTrace("Artifact id {Code:X} in slot {Slot} is unknown, the slot reads as empty and its bytes are kept", code, slot);
                }
            }

            return new EquippedArtifacts(slotsToArtifacts, scrollSpells);
        }

        public PlayerColor DeserializeOwner(HeroChunk chunk)
        {
            var playerByte = chunk.GetByteAt(this.heroOffsets[HeroOffsetName.Owner]);

            return PlayerColor.FromByte(playerByte);
        }

        public HeroGender? DeserializeGender(HeroChunk chunk)
        {
            return ByteEnum.ReadOrNull<HeroGender>(chunk.GetHeaderByteAt(this.heroOffsets[HeroOffsetName.Gender]));
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

        private bool HasPlaces(HeroChunk chunk, VisitedPlaceKind kind) =>
            kind.HeroOffset + sizeof(uint) <= chunk.Length && (!kind.IsHotaOnly || this.heroOffsets.GameVersion == GameVersion.HornOfTheAbyss);
    }
}
