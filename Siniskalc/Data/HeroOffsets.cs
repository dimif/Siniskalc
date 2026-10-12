namespace Siniskalc.Data
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Siniskalc.Entities;

    /// <summary>
    /// Byte offsets of hero fields within a <see cref="BytesChunk"/>, for a single game version.
    /// List taken from https://heroescommunity.com/viewthread.php3?TID=37190&PID=0#0, cysun, h3sed and significant additions by me.
    /// </summary>
    public sealed class HeroOffsets
    {
        /// <summary>
        /// Where the id of the hero is in a hero chunk: the position of the hero in the table of heroes of the savefile, which is
        /// also what a castle holds to say which hero is in it. The same in every version.
        /// </summary>
        public const int HeroIdOffset = 36;

        /// <summary>
        /// Where the flag of a hero that was put to sleep is in a Shadow of Death hero chunk: the sixth of the twelve bytes after the hero struct,
        /// which <see cref="HeroBytesLoader"/> includes in the chunk of that version (1 for a sleeping hero, 0 for one that is awake).
        /// </summary>
        public const int ShadowOfDeathAsleepOffset = 1087;

        /// <summary>
        /// Where the same flag is in a Horn of the Abyss hero chunk: also 1087, but inside the chunk there (its struct is longer). Found in the
        /// saves of <c>TestData/Hota/SleepGarrisonVisiting</c>, where Akka and Oidana were put to sleep; every owned hero of every Horn of the Abyss
        /// savefile of the repository has 0 or 1 there.
        /// </summary>
        public const int HotaAsleepOffset = 1087;

        private static readonly IReadOnlyDictionary<HeroOffsetName, int> CommonOffsets = new Dictionary<HeroOffsetName, int>
        {
            [HeroOffsetName.PositionX] = 0,               // XYZ coordinates
            [HeroOffsetName.PositionY] = 2,
            [HeroOffsetName.PositionZ] = 4,
            [HeroOffsetName.Owner] = 26,                  // Owning player
            [HeroOffsetName.Class] = 37,                  // The hero class
            [HeroOffsetName.Facing] = 41,                 // The direction the hero is facing on the map
            [HeroOffsetName.Gender] = 20,                 // Gender (0 male, 1 female), in the hero header: not in the chunk of a hero with a biography
            [HeroOffsetName.HeroId] = HeroIdOffset,       // Which hero this is: 0 Orrin, 1 Valeska, ... 17 Ufretin
            [HeroOffsetName.Portrait] = 38,               // Portrait id: the hero id unless the map gives the hero another portrait
            [HeroOffsetName.MarkerX] = 45,                // Marker XYZ coordinates
            [HeroOffsetName.MarkerY] = 47,
            [HeroOffsetName.MarkerZ] = 49,
            [HeroOffsetName.MaxMovementPoints] = 57,      // Movement points in total
            [HeroOffsetName.CurrentMovementPoints] = 61,  // Movement points remaining
            [HeroOffsetName.ExperiencePoints] = 65,       // Experience points
            [HeroOffsetName.NumOfSkills] = 69,            // Skills count
            [HeroOffsetName.CurrentSpellPoints] = 73,     // Spell points remaining
            [HeroOffsetName.Level] = 75,                  // Hero level (1 byte)
            [HeroOffsetName.Creatures] = 139,             // Creature type IDs
            [HeroOffsetName.CreaturesAmounts] = 167,      // Creature counts
            [HeroOffsetName.Name] = 195,                  // Hero name, null-padded
            [HeroOffsetName.Skills] = 208,                // Secondary skill levels
            [HeroOffsetName.AttributeAtk] = 264,          // Primary skills
            [HeroOffsetName.AttributeDef] = 265,
            [HeroOffsetName.AttributePow] = 266,
            [HeroOffsetName.AttributeKno] = 267,
            [HeroOffsetName.SpellBookSpells] = 268,       // Spells in book (learned)
            [HeroOffsetName.AvailableSpells] = 338,       // Available spells (via artifact)
            [HeroOffsetName.Helmet] = 408,                // Equipment slots, 8 bytes each
            [HeroOffsetName.Cape] = 416,
            [HeroOffsetName.Necklace] = 424,
            [HeroOffsetName.Weapon] = 432,
            [HeroOffsetName.Shield] = 440,
            [HeroOffsetName.Torso] = 448,
            [HeroOffsetName.LeftRing] = 456,
            [HeroOffsetName.RightRing] = 464,
            [HeroOffsetName.Feet] = 472,
            [HeroOffsetName.Item1] = 480,
            [HeroOffsetName.Item2] = 488,
            [HeroOffsetName.Item3] = 496,
            [HeroOffsetName.Item4] = 504,
            [HeroOffsetName.Ballista] = 512,
            [HeroOffsetName.Cannon] = 512,                // HotA cannon is an alternative to the ballista
            [HeroOffsetName.AmmoCart] = 520,
            [HeroOffsetName.FirstAidTent] = 528,
            [HeroOffsetName.SpellBook] = 544,

            // Found by comparing HotA savefiles from before and after a hero moved, and from before and after a new day.
            // Checked in HotA (and again in the saves of TestData/Hota/SleepGarrisonVisiting). Shadow of Death has no such byte: the struct is
            // shorter there, and no byte of its record tells a hero that has moved from one that has not except the movement points.
            // TODO: Armageddon's Blade, Restoration of Erathia and Chronicles were not seen; they are probably like Shadow of Death.
            [HeroOffsetName.MovedToday] = 1095,

            // Five bytes in front of the hero id, 255 for a spell that is not active and 0 for one that is. A new day clears them.
            // Found by comparing HotA savefiles of a hero that cast each spell, and cast all of them.
            // Checked in HotA and in Shadow of Death (Disguise, Fly and Visions in the Restoration of Erathia series, Fly in save 49).
            // TODO: Armageddon's Blade, Restoration of Erathia and Chronicles were not seen; the bytes are in the part of the struct that all versions share.
            [HeroOffsetName.ActiveSpells] = 31,

            // What visited buildings gave the hero, until the next battle: a signed morale and luck bonus, and the bits of the buildings.
            // Found in TestData/Sod/Hotseat (watering hole, swan pond, stables, fountain of fortune, crypt) and the Restoration of Erathia series
            // (idol of fortune, faerie ring). All of them are in the part of the struct that every version shares; only Shadow of Death saves were seen.
            // TODO: Horn of the Abyss, Armageddon's Blade and Restoration of Erathia 1.0 saves of a hero that visited these buildings were not seen.
            [HeroOffsetName.MoraleBonus] = 28,
            [HeroOffsetName.LuckBonus] = 29,
            [HeroOffsetName.VisitEffects] = 135,
        };

        /// <summary>
        /// The bytes and bits of the effects of visited buildings, as offsets from the start of the hero chunk. The bytes are the first,
        /// second and fourth after <see cref="HeroOffsetName.VisitEffects"/> (the third has not been seen to change).
        /// </summary>
        private static readonly (VisitedBuildingEffects Effect, int Delta, byte Mask)[] VisitEffectBitTable =
        [
            (VisitedBuildingEffects.Stables, 0, 0x02),
            (VisitedBuildingEffects.SwanPond, 0, 0x08),
            (VisitedBuildingEffects.WateringHole, 0, 0x40),
            (VisitedBuildingEffects.Crypt, 1, 0x04),
            (VisitedBuildingEffects.FaerieRing, 1, 0x20),
            (VisitedBuildingEffects.FountainOfFortune, 3, 0x08),
            (VisitedBuildingEffects.IdolOfFortune, 3, 0x02),
        ];

        // The skill slots (one byte per secondary skill) come after a longer part of the struct in HotA.
        private static readonly IReadOnlyDictionary<HeroOffsetName, int> ClassicOffsets = new Dictionary<HeroOffsetName, int>
        {
            [HeroOffsetName.SkillSlots] = 236,
        };

        private static readonly IReadOnlyDictionary<HeroOffsetName, int> HotaOffsets = new Dictionary<HeroOffsetName, int>
        {
            [HeroOffsetName.SkillSlots] = 1118,
        };

        // Versions that have a fifth side slot (SoD, HotA) keep it in front of the backpack, and so does Armageddon's Blade,
        // as unused bytes.
        private static readonly IReadOnlyDictionary<HeroOffsetName, int> BackpackAfterFifthSideSlot = new Dictionary<HeroOffsetName, int>
        {
            [HeroOffsetName.Backpack] = 560,
        };

        private static readonly IReadOnlyDictionary<HeroOffsetName, int> BackpackWithoutFifthSideSlot = new Dictionary<HeroOffsetName, int>
        {
            [HeroOffsetName.Backpack] = 552,
        };

        private static readonly IReadOnlyDictionary<HeroOffsetName, int> FifthSideSlotOffsets = new Dictionary<HeroOffsetName, int>
        {
            [HeroOffsetName.Item5] = 552,
        };

        private static readonly IReadOnlyList<(ArtifactSlots Slot, HeroOffsetName Offset)> EquipmentSlotOffsets =
        [
            (ArtifactSlots.Helmet, HeroOffsetName.Helmet),
            (ArtifactSlots.Cape, HeroOffsetName.Cape),
            (ArtifactSlots.Necklace, HeroOffsetName.Necklace),
            (ArtifactSlots.Weapon, HeroOffsetName.Weapon),
            (ArtifactSlots.Shield, HeroOffsetName.Shield),
            (ArtifactSlots.Torso, HeroOffsetName.Torso),
            (ArtifactSlots.Ring1, HeroOffsetName.LeftRing),
            (ArtifactSlots.Ring2, HeroOffsetName.RightRing),
            (ArtifactSlots.Feet, HeroOffsetName.Feet),
            (ArtifactSlots.Misc1, HeroOffsetName.Item1),
            (ArtifactSlots.Misc2, HeroOffsetName.Item2),
            (ArtifactSlots.Misc3, HeroOffsetName.Item3),
            (ArtifactSlots.Misc4, HeroOffsetName.Item4),
            (ArtifactSlots.Misc5, HeroOffsetName.Item5),
        ];

        // h3sed's "reserved" block (1047-1055, shifted by 26): one counter byte per slot group, giving how many
        // slots of the group are taken by a combination artifact. Rings and misc slots share a byte per group.
        private static readonly IReadOnlyList<(ArtifactSlots Group, int Offset)> ReservedCounterOffsets =
        [
            (ArtifactSlots.Helmet, 1073),
            (ArtifactSlots.Cape, 1074),
            (ArtifactSlots.Necklace, 1075),
            (ArtifactSlots.Weapon, 1076),
            (ArtifactSlots.Shield, 1077),
            (ArtifactSlots.Torso, 1078),
            (ArtifactSlots.Ring1 | ArtifactSlots.Ring2, 1079),
            (ArtifactSlots.Feet, 1080),
            (ArtifactSlots.Misc, 1081),
        ];

        private readonly IReadOnlyDictionary<HeroOffsetName, int> offsets;

        /// <param name="gameVersion">The game version the savefile was written by.</param>
        public HeroOffsets(GameVersion gameVersion)
        {
            this.GameVersion = gameVersion;

            // RoE and AB have no fifth side slot. AB keeps its (unused) bytes in front of the backpack; RoE drops them.
            this.HasItem5 = gameVersion is not (GameVersion.RestorationOfErathia or GameVersion.ArmageddonsBlade);
            var backpackHasItem5Gap = this.HasItem5 || gameVersion == GameVersion.ArmageddonsBlade;

            var all = new Dictionary<HeroOffsetName, int>(CommonOffsets);
            AddAll(all, gameVersion == GameVersion.HornOfTheAbyss ? HotaOffsets : ClassicOffsets);
            AddAll(all, backpackHasItem5Gap ? BackpackAfterFifthSideSlot : BackpackWithoutFifthSideSlot);
            if (this.HasItem5)
            {
                AddAll(all, FifthSideSlotOffsets);
            }

            // The fourth byte of the group has been 0 in every savefile, and is left alone.
            var activeSpells = all[HeroOffsetName.ActiveSpells];
            this.ActiveSpellBytes =
            [
                (ActiveAdventureSpells.Disguise, activeSpells),
                (ActiveAdventureSpells.Fly, activeSpells + 1),
                (ActiveAdventureSpells.WaterWalk, activeSpells + 2),
                (ActiveAdventureSpells.Visions, activeSpells + 4),
            ];
            this.VisitEffectBits = [.. VisitEffectBitTable.Select(b => (b.Effect, all[HeroOffsetName.VisitEffects] + b.Delta, b.Mask))];
            this.AreDayFlagsVerified = gameVersion == GameVersion.HornOfTheAbyss;
            this.AsleepOffset = gameVersion switch
            {
                GameVersion.ShadowOfDeath => ShadowOfDeathAsleepOffset,
                GameVersion.HornOfTheAbyss => HotaAsleepOffset,
                _ => null,
            };

            // RoE and AB saves have no combination artifact reservation block.
            this.ReservedSlotCounters = this.HasItem5 ? ReservedCounterOffsets : [];
            this.offsets = new ReadOnlyDictionary<HeroOffsetName, int>(all);
            this.EquipmentSlots = new ReadOnlyDictionary<ArtifactSlots, int>(EquipmentSlotOffsets
                .Where(e => all.ContainsKey(e.Offset))
                .ToDictionary(e => e.Slot, e => all[e.Offset]));
        }

        public GameVersion GameVersion { get; }

        /// <summary>
        /// Gets where each effect of a visited building is: the offset of the byte in the chunk, and the bit of it.
        /// </summary>
        public IReadOnlyList<(VisitedBuildingEffects Effect, int Offset, byte Mask)> VisitEffectBits { get; }

        public bool HasItem5 { get; }

        /// <summary>
        /// Gets whether the "moved today" flag and the active adventure spells were found in savefiles of this game version.
        /// Only Horn of the Abyss has both: Shadow of Death has the active spells at the same place and no "moved today" byte, and the other versions were not seen.
        /// </summary>
        public bool AreDayFlagsVerified { get; }

        /// <summary>
        /// Gets where the "asleep" flag of a hero is in its chunk, or null for the game versions where it has not been found (everything but
        /// Shadow of Death and Horn of the Abyss).
        /// </summary>
        public int? AsleepOffset { get; }

        /// <summary>
        /// Gets the offset of the byte of each active adventure spell.
        /// </summary>
        public IReadOnlyList<(ActiveAdventureSpells Spell, int Offset)> ActiveSpellBytes { get; }

        /// <summary>
        /// Gets the offset of every equipment slot that exists in this game version.
        /// </summary>
        public IReadOnlyDictionary<ArtifactSlots, int> EquipmentSlots { get; }

        /// <summary>
        /// Gets, per slot group, the offset of the byte counting the slots reserved by combination artifacts.
        /// Empty for game versions that do not store reservations.
        /// </summary>
        public IReadOnlyList<(ArtifactSlots Group, int Offset)> ReservedSlotCounters { get; }

        public int this[HeroOffsetName key] => this.offsets[key];

        private static void AddAll(Dictionary<HeroOffsetName, int> target, IReadOnlyDictionary<HeroOffsetName, int> source)
        {
            foreach (var (name, offset) in source)
            {
                target[name] = offset;
            }
        }
    }
}
