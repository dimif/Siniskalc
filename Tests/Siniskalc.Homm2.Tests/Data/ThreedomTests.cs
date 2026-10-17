namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// What the saves of Threedom (TestData/Homm2/Threedom, Price of Loyalty) showed about the buildings of a town and about what a hero
    /// remembers of the places it visited.
    /// </summary>
    [TestFixture]
    internal sealed class ThreedomTests
    {
        private const int Charity = 49;
        private const int ColarBlau = 50;
        private const int Hillstone = 0;
        private const int Whiteshield = 3;

        private static HeroModel Hero(int save, int hero) => TestServices.LoadThreedom(save).History.Current.Heroes().GetModel(hero);

        private static CastleModel Castle(int save, int castle) => TestServices.LoadThreedom(save).History.Current.Castles().GetModel(castle);

        [TestCase(2, TownBuildings.Castle)]
        [TestCase(5, TownBuildings.MageGuild)]
        [TestCase(18, TownBuildings.ThievesGuild)]
        [TestCase(23, TownBuildings.Well)]
        [TestCase(39, TownBuildings.SecondWell)]
        [TestCase(47, TownBuildings.Special)]
        [TestCase(51, TownBuildings.Statue)]
        [TestCase(62, TownBuildings.Marketplace)]
        [TestCase(64, TownBuildings.LeftTurret)]
        [TestCase(66, TownBuildings.RightTurret)]
        [TestCase(68, TownBuildings.Moat)]
        [TestCase(71, TownBuildings.CaptainsQuarters)]
        public void TheBuildingTheNecromancerBuiltInThatSaveIsTheBitOfTheTown(int save, TownBuildings building)
        {
            var (before, after) = (Castle(save - 1, Hillstone).Town, Castle(save, Hillstone).Town);

            Assert.That(after & ~before, Is.EqualTo(building));
        }

        [TestCase(35, TownBuildings.Castle)]
        [TestCase(43, TownBuildings.MageGuild)]
        [TestCase(53, TownBuildings.Well)]
        [TestCase(62, TownBuildings.SecondWell)]
        [TestCase(64, TownBuildings.Special)]
        [TestCase(66, TownBuildings.Tavern)]
        [TestCase(68, TownBuildings.Statue)]
        [TestCase(71, TownBuildings.Moat)]
        public void TheBuildingTheWizardBuiltInThatSaveIsTheBitOfTheTown(int save, TownBuildings building)
        {
            var (before, after) = (Castle(save - 1, Whiteshield).Town, Castle(save, Whiteshield).Town);

            Assert.That(after & ~before, Is.EqualTo(building));
            Assert.That(Castle(save, Whiteshield).Faction, Is.EqualTo(Faction.Wizard));
        }

        [Test]
        public void TheLibraryOfTheWizardWasBuiltWithTheFourthSpellOfEachLevel()
        {
            Assert.That(Castle(63, Whiteshield).GuildSpellCounts[0], Is.EqualTo(3));
            Assert.That(Castle(64, Whiteshield).GuildSpellCounts[0], Is.EqualTo(4));
            Assert.That(Castle(64, Whiteshield).HasLibrary, Is.True);
        }

        [Test]
        public void ADwellingIsUpgradedByLosingItsBitAndTheMageGuildIsALevelNumber()
        {
            Assert.That(Castle(40, Hillstone).Dwellings & Dwellings.Dwelling5, Is.EqualTo(Dwellings.None));
            Assert.That(Castle(40, Hillstone).Dwellings.HasFlag(Dwellings.Dwelling5Upgraded), Is.True);
            Assert.That(Castle(14, Hillstone).MageGuildLevel, Is.EqualTo(5));
            Assert.That(Castle(10, Hillstone).MageGuildLevel, Is.EqualTo(3));
        }

        [Test]
        public void AFountainAndAFaerieRingGiveOneLuckAndATempleTwoMoraleForTheNextBattle()
        {
            Assert.That(Hero(17, ColarBlau).NextBattle, Is.EqualTo(new NextBattleBonus(0, 1)));
            Assert.That(Hero(25, ColarBlau).NextBattle, Is.EqualTo(new NextBattleBonus(2, 0)));
            Assert.That(Hero(34, Charity).NextBattle, Is.EqualTo(new NextBattleBonus(2, 1)));
            Assert.That(Hero(60, ColarBlau).NextBattle, Is.EqualTo(new NextBattleBonus(0, 1)));
        }

        [Test]
        public void AGraveyardThatWasVisitedBeforeTakesAPointOfMoraleAndTheBonusGoesAfterABattle()
        {
            Assert.That(Hero(37, ColarBlau).NextBattle.Morale, Is.EqualTo(-1));
            Assert.That(Hero(38, ColarBlau).NextBattle, Is.EqualTo(new NextBattleBonus(0, 0)));
            Assert.That(Hero(19, ColarBlau).NextBattle, Is.EqualTo(new NextBattleBonus(0, 0)), "the battle of the save before took the luck of the fountain away");
        }

        [Test]
        public void TheHeroRemembersTheFortsTheStandingStonesTheHutsTheCampsAndTheGazeboItVisited()
        {
            Assert.That(Hero(25, ColarBlau).Visited.Forts, Is.EqualTo(0u));
            Assert.That(Hero(26, ColarBlau).Visited.Forts, Is.EqualTo(4u));
            Assert.That(Hero(50, ColarBlau).Visited.Forts, Is.EqualTo(6u), "the second fort sets another bit");
            Assert.That(Hero(45, ColarBlau).Visited.StandingStones, Is.EqualTo(2u));
            Assert.That(Hero(52, Charity).Visited.StandingStones, Is.EqualTo(8u));
            Assert.That(Hero(61, ColarBlau).Visited.WitchDoctorHuts, Is.EqualTo(8u));
            Assert.That(Hero(6, Charity).Visited.WitchDoctorHuts, Is.EqualTo(2u));
            Assert.That(Hero(65, ColarBlau).Visited.MercenaryCamps, Is.EqualTo(4u));
            Assert.That(Hero(69, ColarBlau).Visited.Gazebos, Is.EqualTo(4u));
            Assert.That(Hero(71, ColarBlau).Visited.Count, Is.EqualTo(6));
        }

        [Test]
        public void ForgettingTheVisitedPlacesAndChangingTheBonusIsWrittenBackAndNothingElseChanges()
        {
            var game = TestServices.LoadThreedom(71);
            var chunk = game.History.Current.Heroes().Single(h => h.Name == "Colar Blau");
            var model = TestServices.HeroDeserializer.Deserialize(chunk);

            var written = TestServices.HeroSerializer.Serialize(chunk, model with { Visited = default, NextBattle = new NextBattleBonus(-2, 3) });
            var read = TestServices.HeroDeserializer.Deserialize(written);

            Assert.That(read, Is.EqualTo(model with { Visited = default, NextBattle = new NextBattleBonus(-2, 3) }));
            Assert.That(written.ToArray().Where((b, i) => b != chunk.ToArray()[i]).Count(), Is.LessThan(30));
        }

        [Test]
        public void ABonusOutsideWhatTheGameShowsIsRefused()
        {
            var chunk = TestServices.LoadThreedom(71).History.Current.Heroes().Single(h => h.Name == "Colar Blau");
            var model = TestServices.HeroDeserializer.Deserialize(chunk);

            Assert.That(() => TestServices.HeroSerializer.Serialize(chunk, model with { NextBattle = new NextBattleBonus(9, 0) }), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TheArtifactsAddedTheSkillsTheTableOfBonusesSays()
        {
            Assert.That(Hero(28, Charity).PrimarySkills.Attack - Hero(27, Charity).PrimarySkills.Attack, Is.EqualTo(3), "the Dragon Sword of Dominion");
            Assert.That(Hero(58, Charity).PrimarySkills.Defense - Hero(57, Charity).PrimarySkills.Defense, Is.EqualTo(1), "the Defender Helm of Protection");
            Assert.That(Hero(33, ColarBlau).PrimarySkills.SpellPower - Hero(32, ColarBlau).PrimarySkills.SpellPower, Is.EqualTo(2), "the Caster's Bracelet of Magic");
            Assert.That(Hero(28, Charity).PrimarySkills.Bonus.Attack, Is.EqualTo(3));
            Assert.That(Hero(8, ColarBlau).PrimarySkills.Bonus.Defense, Is.EqualTo(5), "the Breastplate of Anduran");
        }

        [TestCase(8, 2500, 3)]
        [TestCase(23, 3624, 4)]
        [TestCase(27, 5339, 5)]
        [TestCase(28, 7399, 6)]
        [TestCase(58, 9999, 8)]
        public void TheLevelsOfCharityAreTheOnesOfTheExperienceTable(int save, long experience, int level)
        {
            var hero = Hero(save, Charity);

            Assert.That((hero.Experience, hero.Level, HeroLevels.LevelOf(hero.Experience)), Is.EqualTo((experience, level, level)));
        }
    }
}
