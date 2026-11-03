using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Where the hex editor is told each field of a hero's record lives, for each game version, from the offsets alone (no savegame).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class HeroRegionProviderTests
    {
        private readonly HeroRegionProvider provider = new(new VersionSpecificEntitiesProviderFactory(TestCatalog.Catalogs));

        private IReadOnlyList<ByteRegion> Regions(GameVersion version, int chunkLength = int.MaxValue) => this.provider.GetRegions(new HeroOffsets(version), chunkLength);

        private static (string Name, int Start, int Length) Of(ByteRegion r) => (r.Name, r.Start, r.Length);

        [Test]
        public void TheFixedFieldsOfAShadowOfDeathHeroSitAtTheirBytes()
        {
            var regions = this.Regions(GameVersion.ShadowOfDeath).Select(Of).ToList();

            Assert.That(regions, Does.Contain(("Map X", 0, 2)));
            Assert.That(regions, Does.Contain(("Map Y", 2, 2)));
            Assert.That(regions, Does.Contain(("Map level", 4, 2)));
            Assert.That(regions, Does.Contain(("Gender", 20, 1)));
            Assert.That(regions, Does.Contain(("Owner", 26, 1)));
            Assert.That(regions, Does.Contain(("Morale bonus", 28, 1)));
            Assert.That(regions, Does.Contain(("Luck bonus", 29, 1)));
            Assert.That(regions, Does.Contain(("Active adventure spells", 31, 5)));
            Assert.That(regions, Does.Contain(("Hero id", 36, 1)));
            Assert.That(regions, Does.Contain(("Class", 37, 1)));
            Assert.That(regions, Does.Contain(("Portrait", 38, 1)));
            Assert.That(regions, Does.Contain(("Facing", 41, 1)));
            Assert.That(regions, Does.Contain(("Destination X", 45, 4)));
            Assert.That(regions, Does.Contain(("Destination Y", 49, 4)));
            Assert.That(regions, Does.Contain(("Destination level", 53, 2)));
            Assert.That(regions, Does.Contain(("Movement points, maximum", 57, 4)));
            Assert.That(regions, Does.Contain(("Movement points, left", 61, 4)));
            Assert.That(regions, Does.Contain(("Experience", 65, 4)));
            Assert.That(regions, Does.Contain(("Number of skills", 69, 1)));
            Assert.That(regions, Does.Contain(("Spell points", 73, 2)));
            Assert.That(regions, Does.Contain(("Level", 75, 1)));
            Assert.That(regions, Does.Contain(("Visited building effects", 135, 4)));
            Assert.That(regions, Does.Contain(("Creature types", 139, 28)));
            Assert.That(regions, Does.Contain(("Creature counts", 167, 28)));
            Assert.That(regions, Does.Contain(("Name", 195, 13)));
            Assert.That(regions, Does.Contain(("Attack", 264, 1)));
            Assert.That(regions, Does.Contain(("Defence", 265, 1)));
            Assert.That(regions, Does.Contain(("Spell power", 266, 1)));
            Assert.That(regions, Does.Contain(("Knowledge", 267, 1)));
        }

        [Test]
        public void TheSkillsAndSpellsAreOneByteForEachCodeUpToTheHighestOfTheVersion()
        {
            var regions = this.Regions(GameVersion.ShadowOfDeath).Select(Of).ToList();

            Assert.That(regions, Does.Contain(("Skill levels", 208, 28)));
            Assert.That(regions, Does.Contain(("Skill slots", 236, 28)));
            Assert.That(regions, Does.Contain(("Spells in book", 268, 70)));
            Assert.That(regions, Does.Contain(("Spells from artifacts", 338, 70)));
        }

        [Test]
        public void TheSkillTablesNameEachSkillAtItsCode()
        {
            var levels = this.Regions(GameVersion.ShadowOfDeath).Single(r => r.Name == "Skill levels");

            Assert.That(levels.Parts!, Has.Count.EqualTo(28));
            Assert.That(levels.Parts!.Select(p => p.Start), Is.EqualTo(Enumerable.Range(208, 28)));
            Assert.That(levels.Parts!.Select(p => p.Name), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).SecondarySkills.All.Where(s => s.IsAvailableIn(GameVersion.ShadowOfDeath)).OrderBy(s => s.Code).Select(s => s.Name)));
            Assert.That(levels.Parts!.Select(p => p.Group), Has.All.EqualTo("Skill levels"));
        }

        [Test]
        public void TheEquipmentIsEightBytesForEachSlotAndTheBackpackSixtyFourSlotsOfEight()
        {
            var regions = this.Regions(GameVersion.ShadowOfDeath).Select(Of).ToList();

            var slots = new[] { "Helmet", "Cape", "Necklace", "Weapon", "Shield", "Torso", "Left ring", "Right ring", "Feet", "Misc 1", "Misc 2", "Misc 3", "Misc 4" };
            Assert.That(slots.Select(s => regions.Single(r => r.Name == s).Start), Is.EqualTo(new[] { 408, 416, 424, 432, 440, 448, 456, 464, 472, 480, 488, 496, 504 }));
            Assert.That(regions, Does.Contain(("Ballista", 512, 8)));
            Assert.That(regions, Does.Contain(("Ammo cart", 520, 8)));
            Assert.That(regions, Does.Contain(("First aid tent", 528, 8)));
            Assert.That(regions, Does.Contain(("Spell book", 544, 8)));
            Assert.That(regions, Does.Contain(("Misc 5", 552, 8)));
            Assert.That(regions, Does.Contain(("Backpack", 560, 512)));
        }

        [TestCase(GameVersion.RestorationOfErathia, 552)]
        [TestCase(GameVersion.ArmageddonsBlade, 560)]
        public void AVersionWithoutAFifthSideSlotHasNoRegionForItAndTheBackpackMovesWithTheVersion(GameVersion version, int backpack)
        {
            var regions = this.Regions(version);

            Assert.That(regions.Select(r => r.Name), Does.Not.Contain("Misc 5"));
            Assert.That(regions.Single(r => r.Name == "Backpack").Start, Is.EqualTo(backpack));
        }

        [Test]
        public void TheCannonSharesTheBytesOfTheBallistaAndIsOnlyInHornOfTheAbyss()
        {
            var hota = this.Regions(GameVersion.HornOfTheAbyss).Select(Of).ToList();

            Assert.That(hota, Does.Contain(("Ballista", 512, 8)));
            Assert.That(hota, Does.Contain(("Cannon", 512, 8)));
            Assert.That(this.Regions(GameVersion.ShadowOfDeath).Select(r => r.Name), Does.Not.Contain("Cannon"));
        }

        [Test]
        public void HornOfTheAbyssPutsTheSkillSlotsFarAfterTheOtherFieldsAndHasMoreSkillBytes()
        {
            var sod = this.Regions(GameVersion.ShadowOfDeath);
            var hota = this.Regions(GameVersion.HornOfTheAbyss);

            Assert.That(hota.Single(r => r.Name == "Skill slots").Start, Is.EqualTo(1118));
            Assert.That(hota.Single(r => r.Name == "Skill levels").Length, Is.GreaterThan(sod.Single(r => r.Name == "Skill levels").Length));
            Assert.That(hota.Single(r => r.Name == "Skill slots").Length, Is.EqualTo(hota.Single(r => r.Name == "Skill levels").Length));
        }

        [Test]
        public void TheAsleepByteIsKnownForShadowOfDeathAndHornOfTheAbyssOnly()
        {
            Assert.That(this.Regions(GameVersion.ShadowOfDeath).Select(Of), Does.Contain(("Asleep", 1087, 1)));
            Assert.That(this.Regions(GameVersion.HornOfTheAbyss).Select(Of), Does.Contain(("Asleep", 1087, 1)));
            Assert.That(this.Regions(GameVersion.RestorationOfErathia).Select(r => r.Name), Does.Not.Contain("Asleep"));
            Assert.That(this.Regions(GameVersion.ArmageddonsBlade).Select(r => r.Name), Does.Not.Contain("Asleep"));
        }

        [Test]
        public void TheReservedSlotCountersAreNineBytesInOneRegionInVersionsWithAFifthSideSlot()
        {
            var reserved = this.Regions(GameVersion.ShadowOfDeath).Single(r => r.Name == "Reserved slots");

            Assert.That((reserved.Start, reserved.Length), Is.EqualTo((1073, 9)));
            Assert.That(reserved.Parts!.Select(p => p.Start), Is.EqualTo(Enumerable.Range(1073, 9)));
            Assert.That(this.Regions(GameVersion.ArmageddonsBlade).Select(r => r.Name), Does.Not.Contain("Reserved slots"));
        }

        [Test]
        public void TheActiveSpellsAndTheVisitedBuildingsAreNamedByteByByte()
        {
            var regions = this.Regions(GameVersion.ShadowOfDeath);

            var spells = regions.Single(r => r.Name == "Active adventure spells");
            Assert.That(spells.Parts!.Select(p => (p.Name, p.Start)), Is.EqualTo(new[]
            {
                ("Disguise", 31), ("Fly", 32), ("Water Walk", 33), ("Unknown, always 0", 34), ("Visions", 35),
            }));

            var visits = regions.Single(r => r.Name == "Visited building effects");
            Assert.That(visits.Parts!.Select(p => p.Start), Is.EqualTo(new[] { 135, 136, 137, 138 }));
        }

        [Test]
        public void TheRegionsComeInTheOrderOfTheirBytes()
        {
            var starts = this.Regions(GameVersion.HornOfTheAbyss).Select(r => r.Start).ToList();

            Assert.That(starts, Is.Ordered);
        }

        [Test]
        public void AChunkCutShortLeavesOutTheFieldsThatWouldRunPastItsEnd()
        {
            var regions = this.Regions(GameVersion.ShadowOfDeath, 76);

            Assert.That(regions.Max(r => r.End), Is.LessThanOrEqualTo(76));
            Assert.That(regions.Select(r => r.Name), Does.Contain("Level"));
            Assert.That(regions.Select(r => r.Name), Does.Not.Contain("Creature types"));
        }

        [Test]
        public void ARegionEndingExactlyAtTheEndOfTheChunkIsKeptAndOneByteLessLeavesItOut()
        {
            Assert.That(this.Regions(GameVersion.ShadowOfDeath, 76).Select(r => r.Name), Does.Contain("Level"));
            Assert.That(this.Regions(GameVersion.ShadowOfDeath, 75).Select(r => r.Name), Does.Not.Contain("Level"));
        }

        [Test]
        public void TheAsleepByteAndTheReservedSlotsAreLeftOutOfAChunkThatEndsBeforeThem()
        {
            var cut = this.Regions(GameVersion.ShadowOfDeath, 1082);

            Assert.That(cut.Select(r => r.Name), Does.Contain("Reserved slots"));
            Assert.That(cut.Select(r => r.Name), Does.Not.Contain("Asleep"));
            Assert.That(this.Regions(GameVersion.ShadowOfDeath, 1081).Select(r => r.Name), Does.Not.Contain("Reserved slots"));
            Assert.That(this.Regions(GameVersion.ShadowOfDeath, 1087).Select(r => r.Name), Does.Not.Contain("Asleep"));
            Assert.That(this.Regions(GameVersion.ShadowOfDeath, 1088).Select(r => r.Name), Does.Contain("Asleep"));
        }

        [Test]
        public void TheHeaderInFrontOfAHeroWithABiographyHoldsItsPositionGenderAndTheLengthOfTheBiography()
        {
            var header = this.provider.GetHeaderRegions(new HeroOffsets(GameVersion.ShadowOfDeath));

            Assert.That(header.Select(Of), Is.EqualTo(new[]
            {
                ("Map X", 0, 2), ("Map Y", 2, 2), ("Map level", 4, 2), ("Gender", 20, 1), ("Biography length", 22, 4),
            }));
        }
    }
}
