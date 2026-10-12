using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Entities;

namespace Siniskalc.Tests.Data
{
    [TestFixture]
    internal sealed class HeroOffsetsTests
    {
        [TestCase(GameVersion.RestorationOfErathia, 236)]
        [TestCase(GameVersion.ShadowOfDeath, 236)]
        [TestCase(GameVersion.HornOfTheAbyss, 1118)]
        public void SkillSlotsDependOnVersion(GameVersion version, int expected)
        {
            Assert.That(new HeroOffsets(version)[HeroOffsetName.SkillSlots], Is.EqualTo(expected));
        }

        [TestCase(GameVersion.RestorationOfErathia, false, 552)]
        [TestCase(GameVersion.ArmageddonsBlade, false, 560)]
        [TestCase(GameVersion.ShadowOfDeath, true, 560)]
        [TestCase(GameVersion.HornOfTheAbyss, true, 560)]
        public void FifthSideSlotAndBackpackDependOnVersion(GameVersion version, bool hasItem5, int backpack)
        {
            var offsets = new HeroOffsets(version);

            Assert.That(offsets.HasItem5, Is.EqualTo(hasItem5));
            Assert.That(offsets.EquipmentSlots.ContainsKey(ArtifactSlots.Misc5), Is.EqualTo(hasItem5));
            Assert.That(offsets[HeroOffsetName.Backpack], Is.EqualTo(backpack));
        }
    }
}
