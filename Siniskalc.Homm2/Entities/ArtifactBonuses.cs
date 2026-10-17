namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// What the artifacts add to the primary skills of the hero that carries them. The savegame keeps the sum (the base skills and
    /// the bonuses): when Maximus gave the Ultimate Sword of Dominion to Sir Gallant in the game, his attack fell from 15 to 3 and
    /// Gallant's rose from 2 to 14 (save E13ARTSW). So an edit that puts an artifact on a hero or takes it off must change the skills
    /// by the same amount, as the game does. Only the +12 of that sword is checked against the game; the other values are the
    /// bonuses of the open source engine fheroes2 (read only, GPL-2.0, no code copied). A curse is a negative number.
    /// </summary>
    public static class ArtifactBonuses
    {
        private static readonly IReadOnlyDictionary<int, PrimarySkillBonus> Table = new Dictionary<int, PrimarySkillBonus>
        {
            [0] = new(0, 0, 0, 12), // ULTIMATE_BOOK
            [1] = new(12, 0, 0, 0), // ULTIMATE_SWORD
            [2] = new(0, 12, 0, 0), // ULTIMATE_CLOAK
            [3] = new(0, 0, 12, 0), // ULTIMATE_WAND
            [4] = new(6, 6, 0, 0), // ULTIMATE_SHIELD
            [5] = new(0, 0, 6, 6), // ULTIMATE_STAFF
            [6] = new(4, 4, 4, 4), // ULTIMATE_CROWN
            [8] = new(0, 0, 4, 0), // ARCANE_NECKLACE
            [9] = new(0, 0, 2, 0), // CASTER_BRACELET
            [10] = new(0, 0, 2, 0), // MAGE_RING
            [11] = new(0, 0, 3, 0), // WITCHES_BROACH
            [17] = new(1, 0, 0, 0), // THUNDER_MACE
            [18] = new(0, 1, 0, 0), // ARMORED_GAUNTLETS
            [19] = new(0, 1, 0, 0), // DEFENDER_HELM
            [20] = new(1, 0, 0, 0), // GIANT_FLAIL
            [22] = new(0, 2, 0, 0), // STEALTH_SHIELD
            [23] = new(3, 0, 0, 0), // DRAGON_SWORD
            [24] = new(2, 0, 0, 0), // POWER_AXE
            [25] = new(0, 3, 0, 0), // DIVINE_BREASTPLATE
            [26] = new(0, 0, 0, 2), // MINOR_SCROLL
            [27] = new(0, 0, 0, 3), // MAJOR_SCROLL
            [28] = new(0, 0, 0, 4), // SUPERIOR_SCROLL
            [29] = new(0, 0, 0, 5), // FOREMOST_SCROLL
            [77] = new(1, 1, 0, 0), // SPIKED_HELM
            [78] = new(2, 2, 0, 0), // SPIKED_SHIELD
            [79] = new(0, 0, 1, 1), // WHITE_PEARL
            [80] = new(0, 0, 2, 2), // BLACK_PEARL
            [87] = new(0, 0, 3, 0), // ARM_MARTYR
            [88] = new(0, 5, 0, 0), // BREASTPLATE_ANDURAN
            [89] = new(0, 0, -2, 0), // BROACH_SHIELDING
            [90] = new(5, 5, 5, 0), // BATTLE_GARB
            [94] = new(0, 0, 5, 0), // HELMET_ANDURAN
            [95] = new(5, 0, 0, 0), // HOLY_HAMMER
            [96] = new(2, 2, 2, 2), // LEGENDARY_SCEPTER
            [99] = new(0, 0, 5, 0), // STAFF_WIZARDRY
            [100] = new(1, 4, 0, 0), // SWORD_BREAKER
            [101] = new(5, 0, 0, 0), // SWORD_ANDURAN
        };

        /// <summary>
        /// Returns what <paramref name="artifact"/> adds to attack, defense, spell power and knowledge (all zero for most artifacts).
        /// </summary>
        public static PrimarySkillBonus Of(Artifact artifact) => Table.TryGetValue(artifact.Code, out var bonus) ? bonus : PrimarySkillBonus.None;
    }
}
