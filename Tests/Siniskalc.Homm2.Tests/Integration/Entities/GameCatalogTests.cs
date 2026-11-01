namespace Siniskalc.Homm2.Tests.Integration.Entities
{
    using Siniskalc.Homm2.Entities;

    /// <summary>Golden expectations captured independently from commit 190d2051, not from shipped JSON.</summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class GameCatalogTests
    {
        [Test]
        public void EveryCreatureKeepsItsOriginalIdentityStatsCostsAbilitiesAndDwellingRelationships()
        {
            var actual = TestCatalog.Standard.Creatures.Select(c =>
            {
                var s = c.Info;
                Assert.That(s.AiValue, Is.Zero, "The original HoMM2 table has no known AI values.");
                Assert.That(s.FightValue, Is.Zero, "The original HoMM2 table has no known fight values.");
                var stats = string.Join(",", new object[] { s.Atk, s.Def, s.MinDamage, s.MaxDamage, s.Health, s.Speed, s.Growth, s.Gold, s.Wood, s.Ore, s.Mercury, s.Sulfur, s.Crystal, s.Gems, s.Shots });
                return $"{c.Id}|{c.Name}|{c.Code}|{c.Faction?.Id}|{c.Tier}|{c.BaseCreatureCode}|{c.Dwelling}|{stats}|{string.Join(",", s.Special!.Cast<CreatureAbility>().Select(a => a.Id))}";
            });

            Matches("creatures", actual);
        }

        [Test]
        public void EveryArtifactKeepsItsOriginalIdentityVersionAndPrimarySkillEffects()
        {
            var actual = TestCatalog.Standard.Artifacts.Select(a =>
            {
                var bonus = a.PrimarySkillBonus;
                return $"{a.Id}|{a.Name}|{a.Code}|{a.GameVersion}|{bonus.Attack},{bonus.Defense},{bonus.SpellPower},{bonus.Knowledge}";
            });

            Matches("artifacts", actual);
        }

        [Test]
        public void EverySpellKeepsItsOriginalIdentityCostLevelKindAndGroup() =>
            Matches("spells", TestCatalog.Standard.Spells.Select(s => $"{s.Id}|{s.Name}|{s.Code}|{s.SpellPoints}|{s.Type}|{s.Level}|{s.Group.Id}"));

        [Test]
        public void EveryCreatureAbilityKeepsItsOriginalPresentationAndRangedBehaviour() =>
            Matches("abilities", TestCatalog.Standard.Abilities.Select(a => $"{a.Id}|{a.Name}|{a.Code}|{a.IsRanged.ToString().ToLowerInvariant()}"));

        [Test]
        public void EveryFactionKeepsItsOriginalIdentityAndSaveCode() =>
            Matches("factions", TestCatalog.Standard.Factions.Select(f => $"{f.Id}|{f.Name}|{f.Code}"));

        [Test]
        public void EveryHeroKeepsItsOriginalFactionAndOutOfTableFallback()
        {
            string[] factions = ["Knight", "Barbarian", "Sorceress", "Warlock", "Wizard", "Necromancer"];
            foreach (var id in Enumerable.Range(0, 54))
            {
                Assert.That(TestCatalog.Standard.FactionForHero(id).Id, Is.EqualTo(factions[id / 9]), $"Hero {id}");
            }

            Assert.That(TestCatalog.Standard.FactionForHero(54).Id, Is.EqualTo("Necromancer"));
            Assert.That(TestCatalog.Standard.FactionForHero(int.MaxValue).Id, Is.EqualTo("Necromancer"));
        }

        [Test]
        public void TheOriginalLibraryCapabilityGuildCountsAndLibraryDescriptionArePreserved()
        {
            var catalog = TestCatalog.Standard;

            Assert.That(catalog.Factions.Where(f => f.HasLibrary).Select(f => f.Id), Is.EqualTo(new[] { "Wizard" }));
            Assert.That(catalog.Factions["Wizard"].SpecialBuildingDescription, Is.EqualTo("Gives each level of the Mage Guild a fourth spell."));
            foreach (var faction in catalog.Factions)
            {
                Assert.That(faction.GuildSpellCounts, Is.EqualTo(new[] { 3, 3, 2, 2, 1 }), faction.Name);
            }
        }

        [Test]
        public void EverySkillKeepsItsOriginalIdentityAndSaveCode() =>
            Matches("skills", TestCatalog.Standard.Skills.Select(s => $"{s.Id}|{s.Name}|{s.Code}"));

        [Test]
        public void EveryFactionKeepsTheOriginalNamesOfItsGrowthAndSpecialBuildings()
        {
            var factions = TestCatalog.Standard.Factions.Where(f => !f.IsNeutral).ToArray();
            Matches("faction-buildings", factions.Select(f => $"{f.Id}|{f.GrowthBuildingName}").Concat(factions.Select(f => $"{f.Id}|{f.SpecialBuildingName}")));
        }

        [Test]
        public void EverySupportedVisitablePlaceKeepsItsOriginalEncoding() =>
            Matches("places", TestCatalog.Standard.Places.Select(p => $"{p.Kind}|{p.Code}"));

        private static void Matches(string domain, IEnumerable<string> actual) =>
            Assert.That(actual, Is.EqualTo(File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "TestDefinitions", "baseline", domain + ".txt"))));
    }
}
