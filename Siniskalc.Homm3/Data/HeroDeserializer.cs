namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    public sealed class HeroDeserializer : IHeroDeserializer
    {
        private static readonly ILogger<HeroDeserializer> Logger = LogFactory.For<HeroDeserializer>();

        private readonly HeroOffsets heroOffsets;
        private readonly VersionSpecificEntitiesProvider entities;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeroDeserializer"/> class.
        /// </summary>
        /// <param name="heroOffsets">The hero offsets.</param>
        /// <param name="entities">The entities.</param>
        public HeroDeserializer(HeroOffsets heroOffsets, VersionSpecificEntitiesProvider entities)
        {
            this.heroOffsets = heroOffsets;
            this.entities = entities;
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Reads the places the hero has visited from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The visits.</returns>
        public PlaceVisits DeserializePlaceVisits(HeroChunk chunk)
        {
            var masks = new uint[Constants.Catalog.PlaceCapacity];
            foreach (var kind in this.entities.Catalog.VisitablePlaces.All)
            {
                masks[kind.Index] = this.HasPlaces(chunk, kind) ? chunk.GetUInt32At(VisitedPlaceOffsets.For(kind.Slot)) : 0u;
            }

            return new PlaceVisits(masks);
        }

        /// <summary>
        /// Reads the morale the visits to places give the hero for its next battle from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The morale bonus; it can be negative.</returns>
        public int DeserializeMoraleBonus(HeroChunk chunk) => (sbyte)chunk.GetByteAt(this.heroOffsets[HeroOffsetName.MoraleBonus]);

        /// <summary>
        /// Reads the luck the visits to places give the hero for its next battle from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The luck bonus; it can be negative.</returns>
        public int DeserializeLuckBonus(HeroChunk chunk) => (sbyte)chunk.GetByteAt(this.heroOffsets[HeroOffsetName.LuckBonus]);

        /// <summary>
        /// Reads the effects of the buildings the hero visited from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The effects the hero remembers.</returns>
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

        /// <summary>
        /// Reads the adventure spells that are active on the hero from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The active spells.</returns>
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

        /// <summary>
        /// Reads the number of the hero from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The number.</returns>
        public int DeserializeHeroId(HeroChunk chunk)
        {
            return chunk.GetByteAt(this.heroOffsets[HeroOffsetName.HeroId]);
        }

        /// <summary>
        /// Reads whether the hero is asleep from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns><see langword="true"/> if the hero is asleep; any value but 0 counts as set.</returns>
        public bool DeserializeIsAsleep(HeroChunk chunk)
        {
            return this.heroOffsets.AsleepOffset is { } offset && offset < chunk.Length && chunk.GetByteAt(offset) != 0;
        }

        /// <summary>
        /// Reads whether the hero has moved today from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns><see langword="true"/> if the hero has moved; any value but 0 counts as set.</returns>
        public bool DeserializeMovedToday(HeroChunk chunk)
        {
            return this.heroOffsets[HeroOffsetName.MovedToday] < chunk.Length
                && chunk.GetByteAt(this.heroOffsets[HeroOffsetName.MovedToday]) != 0;
        }

        /// <summary>
        /// Reads the army of the hero from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The army.</returns>
        public IArmy<Creature> DeserializeCreatures(HeroChunk chunk) => ArmyReader.Read(chunk, this.heroOffsets.Army, this.entities.CreatureCodes);

        /// <summary>
        /// Reads the movement points of the hero from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The movement points left and the most the hero has.</returns>
        public MovementPoints DeserializeMovementPoints(HeroChunk chunk)
        {
            return new MovementPoints(
                chunk.GetUInt32At(this.heroOffsets[HeroOffsetName.CurrentMovementPoints]),
                chunk.GetUInt32At(this.heroOffsets[HeroOffsetName.MaxMovementPoints]));
        }

        /// <summary>
        /// Reads the secondary skills of the hero from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The skills; a skill of the savefile that the editor does not know is left out.</returns>
        public SecondarySkills DeserializeSecondarySkills(HeroChunk chunk)
        {
            using var subject = Logger.BeginSubject("Hero", chunk.Name);
            var skills = this.entities.SecondarySkills;
            var allSkillsToLevels = new Dictionary<SecondarySkill, SkillLevel>();

            // We read all the skills, rather than just the ones in the 8 slots, because the game supports more skills
            // being set than just the 8, and their effects will apply. This can be interesting for custom maps or mods.
            foreach (var skill in skills)
            {
                var skillLevelByte = chunk.GetByteAt(this.heroOffsets[HeroOffsetName.Skills] + skill.Code);

                if (skillLevelByte > SkillLevel.Expert.Code)
                {
                    Logger.LogTrace("Skill level byte {Code:X2} for {Skill} read as no level and kept", skillLevelByte, skill.Name);
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
                    Logger.LogTrace("Secondary skill slot value {Slot} is out of range for {Count} skills, skill {Skill} ignored", slot, slotCount, skill.Name);
                }
            }

            return new SecondarySkills(allSkillsToLevels, slots);
        }

        /// <summary>
        /// Reads where the hero was going when its movement ran out from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The destination, or <see cref="MapPosition.None"/> for none.</returns>
        public MapPosition DeserializeDestination(HeroChunk chunk)
        {
            // Two 32 bit numbers, x and y (FFFFFFFF for none), and then the level in 16 bits, which is 0 when there is no marker.
            var destinationX = chunk.GetInt32At(this.heroOffsets[HeroOffsetName.DestinationX]);
            var destinationY = chunk.GetInt32At(this.heroOffsets[HeroOffsetName.DestinationY]);
            var destinationZ = chunk.GetInt16At(this.heroOffsets[HeroOffsetName.DestinationZ]);

            return destinationX < 0 || destinationY < 0 ? MapPosition.None : new MapPosition(destinationX, destinationY, destinationZ);
        }

        /// <summary>
        /// Reads the class of the hero from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The class; a byte that is no class reads as the first one.</returns>
        public HeroClass DeserializeClass(HeroChunk chunk)
        {
            using var subject = Logger.BeginSubject("Hero", chunk.Name);
            var code = chunk.GetByteAt(this.heroOffsets[HeroOffsetName.Class]);
            if (this.entities.Catalog.HeroClasses.Find(code) is null)
            {
                Logger.LogTrace("Class byte {Code:X2} read as {StandIn} and kept until the class is changed", code, this.entities.Catalog.FallbackHeroClass.Name);
            }

            return this.entities.Catalog.HeroClasses.Find(code) ?? this.entities.Catalog.FallbackHeroClass;
        }

        /// <summary>
        /// Reads the level and the experience of the hero from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The level and the experience.</returns>
        public HeroLevel DeserializeLevel(HeroChunk chunk)
        {
            return new HeroLevel(
                chunk.GetInt32At(this.heroOffsets[HeroOffsetName.ExperiencePoints]),
                chunk.GetByteAt(this.heroOffsets[HeroOffsetName.Level]));
        }

        /// <summary>
        /// Reads where the hero is on the map from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The position, with the level of the map.</returns>
        public HeroPosition DeserializePosition(HeroChunk chunk)
        {
            using var subject = Logger.BeginSubject("Hero", chunk.Name);
            var outside = chunk.OutsidePositionBytes;
            var x = outside != null ? BitConverter.ToInt16(outside, 0) : chunk.GetInt16At(this.heroOffsets[HeroOffsetName.PositionX]);
            var y = outside != null ? BitConverter.ToInt16(outside, 2) : chunk.GetInt16At(this.heroOffsets[HeroOffsetName.PositionY]);
            var z = outside != null ? BitConverter.ToInt16(outside, 4) : chunk.GetInt16At(this.heroOffsets[HeroOffsetName.PositionZ]);

            // A byte that is no direction reads as the first one, North, and is left alone by the serializer while the hero keeps facing it.
            var facingByte = chunk.GetByteAt(this.heroOffsets[HeroOffsetName.Facing]);
            if (!ByteEnum.TryRead<FacingDirection>(facingByte, out var direction))
            {
                direction = FacingDirection.North;
                Logger.LogTrace("Facing byte {Code:X2} read as North and kept", facingByte);
            }

            var facing = direction;

            return new HeroPosition(x, y, z, facing);
        }

        /// <summary>
        /// Reads the primary skills of the hero from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The skills, with what the artifacts add.</returns>
        public PrimarySkills DeserializePrimarySkills(HeroChunk chunk)
        {
            return new PrimarySkills(
                chunk.GetByteAt(this.heroOffsets[HeroOffsetName.AttributeAtk]),
                chunk.GetByteAt(this.heroOffsets[HeroOffsetName.AttributeDef]),
                chunk.GetByteAt(this.heroOffsets[HeroOffsetName.AttributePow]),
                chunk.GetByteAt(this.heroOffsets[HeroOffsetName.AttributeKno]));
        }

        /// <summary>
        /// Reads the war machines of the hero from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The war machines the hero has.</returns>
        public IReadOnlyList<WarMachine> DeserializeWarMachines(HeroChunk chunk)
        {
            return this.entities.WarMachines
                .Where(warMachine => chunk.GetByteAt(this.heroOffsets[WarMachineOffsets.For(warMachine)]) == warMachine.Code)
                .ToList();
        }

        /// <summary>
        /// Reads the spell points of the hero from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The spell points.</returns>
        public ushort DeserializeSpellPoints(HeroChunk chunk)
        {
            return chunk.GetUInt16At(this.heroOffsets[HeroOffsetName.CurrentSpellPoints]);
        }

        /// <summary>
        /// Reads the backpack of the hero from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The backpack.</returns>
        public Backpack DeserializeBackpack(HeroChunk chunk)
        {
            using var subject = Logger.BeginSubject("Hero", chunk.Name);
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

                if (id <= byte.MaxValue && this.entities.Catalog.Artifacts.Find((byte)id) is { CanBeInBackpack: true } artifact)
                {
                    var spellId = chunk.GetUInt32At(offset + 4);
                    var spell = artifact.IsScroll && spellId <= byte.MaxValue ? this.entities.Catalog.Spells.Find((byte)spellId) : null;
                    artifacts[i] = new BackpackSlot(i, artifact, spell);
                }
                else
                {
                    Logger.LogTrace("Artifact id {Code:X} in backpack slot {Slot} is unknown, the slot reads as empty and its bytes are kept", id, i);
                }
            }

            return new Backpack(artifacts);
        }

        /// <summary>
        /// Reads the artifacts the hero wears from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The artifacts, by slot.</returns>
        public EquippedArtifacts DeserializeEquippedArtifacts(HeroChunk chunk)
        {
            using var subject = Logger.BeginSubject("Hero", chunk.Name);
            var slotsToArtifacts = new Dictionary<ArtifactSlots, Artifact>();
            var scrollSpells = new Dictionary<ArtifactSlots, Spell>();

            foreach (var (slot, offset) in this.heroOffsets.EquipmentSlots)
            {
                var code = chunk.GetByteAt(offset);
                if (code == Constants.ByteOff)
                {
                    continue;
                }

                if (this.entities.Catalog.Artifacts.Find(code) is { Wearable: true } artifact)
                {
                    slotsToArtifacts[slot] = artifact;

                    // Like a backpack slot, a worn scroll keeps its spell id in the 4 bytes after the artifact id.
                    var spellId = chunk.GetUInt32At(offset + 4);
                    if (artifact.IsScroll && spellId <= byte.MaxValue && this.entities.Catalog.Spells.Find((byte)spellId) is { } spell)
                    {
                        scrollSpells[slot] = spell;
                    }
                }
                else
                {
                    Logger.LogTrace("Artifact id {Code:X} in slot {Slot} is unknown, the slot reads as empty and its bytes are kept", code, slot);
                }
            }

            return new EquippedArtifacts(slotsToArtifacts, scrollSpells);
        }

        /// <summary>
        /// Reads the player the hero serves from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The colour of the player; the neutral colour when the byte is no colour.</returns>
        public PlayerColor DeserializeOwner(HeroChunk chunk)
        {
            var playerByte = chunk.GetByteAt(this.heroOffsets[HeroOffsetName.Owner]);

            return PlayerColor.FromByte(playerByte);
        }

        /// <summary>
        /// Reads the gender of the hero from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns>The gender, or <see langword="null"/> when the savefile does not say or says something the editor does not know.</returns>
        public HeroGender? DeserializeGender(HeroChunk chunk)
        {
            return ByteEnum.ReadOrNull<HeroGender>(chunk.GetHeaderByteAt(this.heroOffsets[HeroOffsetName.Gender]));
        }

        /// <summary>
        /// Reads whether the hero has the spell book from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <returns><see langword="true"/> if the hero has the spell book.</returns>
        public bool DeserializeHasSpellBook(HeroChunk chunk)
        {
            // The spellbook slot holds the Spell Book artifact, whose id is 0.
            return chunk.GetUInt32At(this.heroOffsets[HeroOffsetName.SpellBook]) == Constants.ByteOn;
        }

        /// <summary>
        /// Reads the spell book of the hero. The spells its <paramref name="equippedArtifacts"/> grant are told apart from the
        /// other available spells, which are kept as the savefile has them.
        /// </summary>
        /// <param name="chunk">The chunk of the hero.</param>
        /// <param name="equippedArtifacts">The artifacts the hero wears, which can grant the book and spells.</param>
        /// <returns>The spell book.</returns>
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
            VisitedPlaceOffsets.For(kind.Slot) + sizeof(uint) <= chunk.Length && (!kind.IsHotaOnly || this.heroOffsets.GameVersion == GameVersion.HornOfTheAbyss);
    }
}
