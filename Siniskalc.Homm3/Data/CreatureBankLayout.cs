namespace Siniskalc.Homm3.Data
{
    using System;

    /// <summary>The variable-length creature-bank record and its separate HotA entrance state.</summary>
    public static class CreatureBankLayout
    {
        public const int ObjectType = 25;
        public const int BankObjectType = 16;
        public const int Counts = 28;
        public const int Resources = 56;
        public const int Gold = 80;
        public const int RewardCreature = 84;
        public const int RewardCreatureCount = 88;
        public const int ArtifactCount = 89;
        public const int Artifacts = 91;
        public const int UnderHeroType = 11;
        public const int UnderHeroState = 16;
        public const uint EmptyFlag = 0x20;
        public const uint ResetMask = 0x0200003F;
        public const uint ReadyState = 0x1F;
        public const int BankIndexShift = 13;
        public const uint BankIndexMask = 0xFFF;

        public static int BankIndex(uint state) => (int)((state >> BankIndexShift) & BankIndexMask);

        public static uint HeroValue(HeroChunk hero, int offset) => BitConverter.ToUInt32(
            new[] { hero.GetHeaderByteAt(offset), hero.GetHeaderByteAt(offset + 1), hero.GetHeaderByteAt(offset + 2), hero.GetHeaderByteAt(offset + 3) });
    }
}
