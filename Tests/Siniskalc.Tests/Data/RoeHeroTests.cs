using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The heroes of the Restoration of Erathia series (saves 32 to 42): Tazar of Rampart gets Boots of Levitation, a spell book, the four tomes
    /// and learns and casts spells, Crag Hack picks up spell scrolls, Damacon picks up Angel Wings and Tazar ends up garrisoned in the Dungeon.
    /// </summary>
    [TestFixture]
    internal sealed class RoeHeroTests
    {
        private static Game Load(string save) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Roe", save + ".GM1"));

        private static HeroModel Hero(string save, string name)
        {
            var game = Load(save);
            return game.History.CreateHeroSession(game.History.Current.Heroes().FindByName(name)).Current;
        }

        private static IEnumerable<string> Spells(HeroModel hero) => hero.SpellBook.GetAll.Select(s => s.Name).OrderBy(n => n);

        [Test]
        public void TazarStartsWithEquestriansGlovesAndNoSpellBook()
        {
            var tazar = Hero("roe_00_baseline", "Tazar");

            Assert.That(tazar.EquippedArtifacts.GetArtifact(ArtifactSlots.Ring2), Is.EqualTo(Artifact.EquestriansGloves));
            Assert.That(tazar.SpellBook.HasSpellBook, Is.False);
            Assert.That(tazar.Owner, Is.EqualTo(PlayerColor.Red));
        }

        [Test]
        public void TheBootsOfLevitationAreInTheFeetSlot()
        {
            var tazar = Hero("roe_32_tazar_boots_of_levitation", "Tazar");

            Assert.That(tazar.EquippedArtifacts.GetArtifact(ArtifactSlots.Feet), Is.EqualTo(Artifact.BootsOfLevitation));
            Assert.That(tazar.EquippedArtifacts.GetArtifact(ArtifactSlots.Ring2), Is.EqualTo(Artifact.EquestriansGloves));
        }

        [Test]
        public void TheSpellBookComesWithNineLearnedSpells()
        {
            var tazar = Hero("roe_33_tazar_gets_spell_book", "Tazar");

            Assert.That(tazar.SpellBook.HasSpellBook, Is.True);
            Assert.That(Spells(tazar), Is.EqualTo(new[] { "Cure", "Dispel", "Haste", "Ice Bolt", "Precision", "Remove Obstacle", "Scuttle Boat", "Shield", "Summon Boat" }));
        }

        [Test]
        public void TheFourTomesGrantEverySpellOfTheirSchoolAndMagicArrowFromAllOfThem()
        {
            var tazar = Hero("roe_34_tazar_all_spell_tomes", "Tazar");

            Assert.That(tazar.EquippedArtifacts.GetArtifact(ArtifactSlots.Misc1), Is.EqualTo(Artifact.TomeOfEarthMagic));
            Assert.That(tazar.EquippedArtifacts.GetArtifact(ArtifactSlots.Misc2), Is.EqualTo(Artifact.TomeOfWaterMagic));
            Assert.That(tazar.EquippedArtifacts.GetArtifact(ArtifactSlots.Misc3), Is.EqualTo(Artifact.TomeOfAirMagic));
            Assert.That(tazar.EquippedArtifacts.GetArtifact(ArtifactSlots.Misc4), Is.EqualTo(Artifact.TomeOfFireMagic));
            Assert.That(tazar.SpellBook.IsLearned(Spell.Fly), Is.False);
            Assert.That(tazar.SpellBook.IsAvailable(Spell.Fly), Is.True);
            Assert.That(tazar.SpellBook.Source(Spell.Fly), Is.EqualTo("Tome of Wind Magic"));
            Assert.That(tazar.SpellBook.Source(Spell.MagicArrow), Does.Contain("Tome of Earth Magic").And.Contain("Tome of Fire Magic"));
        }

        [Test]
        public void CastingTheAdventureSpellsActivatesDisguiseFlyAndVisionsAndCostsSpellPoints()
        {
            var tazar = Hero("roe_41_tazar_adventure_spells", "Tazar");

            Assert.That(tazar.ActiveSpells, Is.EqualTo(ActiveAdventureSpells.Disguise | ActiveAdventureSpells.Fly | ActiveAdventureSpells.Visions));
            Assert.That(tazar.SpellPoints, Is.EqualTo((ushort)12));
        }

        [Test]
        public void TazarLearnedMoreSpellsInTheMageGuildsOnTheWay()
        {
            var tazar = Hero("roe_41_tazar_adventure_spells", "Tazar");

            Assert.That(tazar.SpellBook.IsLearned(Spell.FireWall), Is.True);
            Assert.That(tazar.SpellBook.IsLearned(Spell.LightningBolt), Is.True);
            Assert.That(tazar.SpellBook.IsLearned(Spell.MagicArrow), Is.True);
        }

        [TestCase("roe_36_crag_scroll_town_portal_meteor_shower", 2, new[] { "Meteor Shower", "Town Portal" })]
        [TestCase("roe_37_crag_scroll_mirth", 3, new[] { "Meteor Shower", "Mirth", "Town Portal" })]
        public void SpellScrollsMakeTheirSpellsAvailableWithoutASpellBook(string save, int scrolls, string[] spells)
        {
            var crag = Hero(save, "Crag Hack");

            Assert.That(crag.EquippedArtifacts.GetEquippedArtifacts().Count(a => a.Item2 == Artifact.SpellScroll), Is.EqualTo(scrolls));
            Assert.That(crag.SpellBook.HasSpellBook, Is.False);
            Assert.That(Spells(crag), Is.EqualTo(spells));
        }

        [Test]
        public void TheSpellOfEachWornScrollIsReadAndNamedAsTheSource()
        {
            var crag = Hero("roe_37_crag_scroll_mirth", "Crag Hack");

            var spells = crag.EquippedArtifacts.GetEquippedArtifacts()
                .Where(a => a.Item2 == Artifact.SpellScroll)
                .Select(a => crag.EquippedArtifacts.GetScrollSpell(a.Item1)?.Name)
                .OrderBy(n => n);

            Assert.That(spells, Is.EqualTo(new[] { "Meteor Shower", "Mirth", "Town Portal" }));
            Assert.That(crag.SpellBook.Source(Spell.Mirth), Is.EqualTo("Spell Scroll"));
        }

        [Test]
        public void DamaconWearsAngelWingsAsACape()
        {
            Assert.That(Hero("roe_38_damacon_angel_wings", "Damacon").EquippedArtifacts.GetArtifact(ArtifactSlots.Cape), Is.EqualTo(Artifact.AngelWings));
        }

        [TestCase("roe_41_tazar_adventure_spells", null, 115)]
        [TestCase("roe_42_tazar_garrisoned", 115, null)]
        public void TazarVisitsTheDungeonAndThenIsGarrisonedInIt(string save, int? garrisoned, int? visiting)
        {
            var game = Load(save);
            var castles = game.History.Current.Castles();
            var dungeon = TestServices.CastleDeserializer.Deserialize(castles[castles.FindByName("6_Dung")]);

            Assert.That(Hero(save, "Tazar").HeroId, Is.EqualTo(115));
            Assert.That(dungeon.GarrisonedHeroId, Is.EqualTo(garrisoned));
            Assert.That(dungeon.VisitingHeroId, Is.EqualTo(visiting));
        }

        [Test]
        public void CastingFlyActivatesOnlyFlyAndSpendsTwentySpellPoints()
        {
            Assert.That(Hero("roe_49_tazar_fly", "Tazar").ActiveSpells, Is.EqualTo(ActiveAdventureSpells.Fly));
            Assert.That(Hero("roe_49_tazar_fly", "Tazar").SpellPoints, Is.EqualTo((ushort)20));
            Assert.That(Hero("roe_48b_rampart_capitol", "Tazar").SpellPoints, Is.EqualTo((ushort)40));
        }
        [Test]
        public void NymusAndDamaconAreAsleepInSaveFiftyNineAndDamaconWakesWhenHeMoves()
        {
            string[] red = ["Lord Haart", "Ufretin", "Torosar ", "Nymus", "Damacon", "Crag Hack", "Tazar"];

            Assert.That(red.Where(n => Hero("roe_58_haart_dug", n).IsAsleep), Is.Empty);
            Assert.That(red.Where(n => Hero("roe_59_nymus_damacon_sleep", n).IsAsleep), Is.EqualTo(new[] { "Nymus", "Damacon" }));
            Assert.That(Hero("roe_65_damacon_before_surrender", "Damacon").IsAsleep, Is.False);
        }

        [Test]
        public void TheAsleepFlagIsEditedPendingAndRevertedLikeAnyOtherField()
        {
            var game = Load("roe_59_nymus_damacon_sleep");
            var original = game.History.Current.ToBytes();
            var nymus = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Nymus"));

            nymus.Update(h => h with { IsAsleep = false });
            nymus.Update(h => h with { IsAsleep = false });

            var change = game.History.GetPendingChanges().Single();
            Assert.That((change.OwnerName, change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Nymus", "Asleep", "yes", "no")));
            var saved = TestServices.GameService.LoadGameFromBytes(game.History.Current.ToBytes(), "edited.GM1");
            Assert.That(saved.History.CreateHeroSession(saved.History.Current.Heroes().FindByName("Nymus")).Current.IsAsleep, Is.False);
            Assert.That(saved.History.CreateHeroSession(saved.History.Current.Heroes().FindByName("Damacon")).Current.IsAsleep, Is.True);

            change.Revert(game.History);
            Assert.That(game.History.Current.ToBytes(), Is.EqualTo(original));

            var tazar = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Tazar"));
            tazar.Update(h => h with { IsAsleep = true });
            var again = TestServices.GameService.LoadGameFromBytes(game.History.Current.ToBytes(), "edited.GM1");
            Assert.That(again.History.CreateHeroSession(again.History.Current.Heroes().FindByName("Tazar")).Current.IsAsleep, Is.True);
        }

        [Test]
        public void ShadowOfDeathAndHornOfTheAbyssHaveTheAsleepFlag()
        {
            var sod = Load("roe_59_nymus_damacon_sleep");
            var hota = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Arts", "arts_01_arts.GM1"));
            var hero = sod.History.CreateHeroSession(sod.History.Current.Heroes().FindByName("Nymus"));
            using var view = TestServices.CreateHeroViewModel(hero);
            using var other = TestServices.CreateHeroViewModel(hota.History.CreateHeroSession(0));

            Assert.That(view.CanSleep, Is.True);
            Assert.That(view.IsAsleep, Is.True);
            Assert.That(other.CanSleep, Is.True);
            Assert.That(other.IsAsleep, Is.False);

            view.IsAsleep = false;
            Assert.That(hero.Current.IsAsleep, Is.False);
        }
    }
}
