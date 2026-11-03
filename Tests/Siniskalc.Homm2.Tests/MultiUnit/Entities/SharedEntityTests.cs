namespace Siniskalc.Homm2.Tests.MultiUnit.Entities
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Tests that the colours, factions, skills and skill levels of the game are what the editor's shared pages ask for.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class SharedEntityTests
    {
        [Test]
        public void EveryColourOfTheGameIsAnIPlayerColorWithItsNameAndCodeAndItsOwnHexValue()
        {
            Assert.That(PlayerColor.All.Cast<IPlayerColor>().Select(c => (c.Name, c.Code)), Is.EqualTo(PlayerColor.All.Select(c => (c.ToString(), c.Code))));
            Assert.That(PlayerColor.All.Select(PlayerPalette.Of), Is.Unique.And.None.EqualTo(PlayerPalette.NoPlayer));
            Assert.That(PlayerPalette.Of(null), Is.EqualTo(PlayerPalette.NoPlayer));
        }

        [Test]
        public void AColourIsFoundByItsCodeAndANumberThatIsNoColourIsNeutral()
        {
            Assert.That(PlayerColor.FromByte(PlayerColor.All[1].Code), Is.SameAs(PlayerColor.All[1]));
            Assert.That(PlayerColor.FromByte(200), Is.SameAs(PlayerColor.Neutral));
        }

        [Test]
        public void EveryFactionOfTheGameIsAnIFactionWithItsNameAndCode()
        {
            Assert.That(TestCatalog.Standard.Factions.Cast<IFaction>().Select(f => (f.Name, f.Code)), Is.EqualTo(TestCatalog.Standard.Factions.Select(f => (f.ToString(), f.Code))));
            Assert.That(TestCatalog.Standard.Factions.Find(4), Is.SameAs(TestCatalog.Standard.Factions["Wizard"]));
        }

        [Test]
        public void EveryCreatureExposesItsCatalogFactionOrTheSameCatalogsNeutralFactionToSharedPickers()
        {
            foreach (var creature in TestCatalog.Standard.Creatures)
            {
                var shared = (ICreature)creature;
                Assert.That(shared.Faction, Is.SameAs(creature.Faction ?? TestCatalog.Standard.NeutralFaction));
                Assert.That(new CreatureChoiceViewModel(shared, creature.Info).Faction, Is.EqualTo(shared.Faction.Name));
            }
        }

        [Test]
        public void AnArtifactTellsWhatItAddsToThePrimarySkills()
        {
            Assert.That(((IArtifact)TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"]).PrimarySkillBonus, Is.EqualTo(new PrimarySkillBonus(12, 0, 0, 0)));
            Assert.That(TestCatalog.Standard.Artifacts["UltimateSwordOfDominion"].PrimarySkillBonus, Is.EqualTo(new PrimarySkillBonus(12, 0, 0, 0)));
            Assert.That(TestCatalog.Standard.Artifacts["AmmoCart"].PrimarySkillBonus, Is.EqualTo(PrimarySkillBonus.None));
        }

        [Test]
        public void EverySecondarySkillOfTheGameIsAnISecondarySkill()
        {
            Assert.That(TestCatalog.Standard.Skills.Cast<ISecondarySkill>().Select(s => s.Code), Is.EqualTo(TestCatalog.Standard.Skills.Select(s => s.Code)));
        }
    }
}
