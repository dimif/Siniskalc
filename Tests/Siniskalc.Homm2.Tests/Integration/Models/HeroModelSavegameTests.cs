namespace Siniskalc.Homm2.Tests.Integration.Models
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// What a hero model holds when it is read from a savegame.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroModelSavegameTests
    {
        [Test]
        public void TheFactionOfAHeroFollowsItsNumberNineToAFaction()
        {
            var heroes = TestServices.LoadBrokenAlliance(10).History.Current.Heroes().GetModels();

            Assert.That(heroes.Select(h => h.Faction), Is.EqualTo(heroes.Select(h => TestCatalog.Standard.Factions[Math.Min(h.Id / 9, TestCatalog.Standard.Factions.Count - 1)])));
            Assert.That((heroes[0].Faction, heroes[9].Faction, heroes[53].Faction), Is.EqualTo((TestCatalog.Standard.Factions["Knight"], TestCatalog.Standard.Factions["Barbarian"], TestCatalog.Standard.Factions["Necromancer"])));
        }

        [Test]
        public void GivingTheUltimateSwordToAnotherHeroMovesItsTwelveAttackPointsAsTheGameDidInSaveE13ArtSw()
        {
            // In the game Maximus (attack 15 with the sword) gave it to Sir Gallant (attack 2): they ended on 3 and 14.
            var heroes = TestServices.LoadBrokenAlliance(10).History.Current.Heroes();
            var maximus = (heroes.GetModel(7) with { PrimarySkills = new PrimarySkills(15, 15, 15, 15) }).WithArtifact(TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"]);
            var gallant = heroes.GetModel(1);

            var maximusAfter = maximus.WithoutArtifact(TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"]);
            var gallantAfter = gallant.WithArtifact(TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"]);

            Assert.That(gallantAfter.PrimarySkills.Attack, Is.EqualTo(gallant.PrimarySkills.Attack + 12));
            Assert.That(maximusAfter.PrimarySkills.Attack, Is.EqualTo(maximus.PrimarySkills.Attack - 12));
        }
    }
}
