using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Runtime;
using Siniskalc.Homm3.ViewModels;

using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Models
{
    /// <summary>
    /// Tests the spells the artifacts of a hero grant, in the saves that show them.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class ArtifactSpellGrantsTests
    {
        private static Game Load(string name) => SavedGames.Get(Path.Combine("TestData", "Hota", "Arts", name + ".GM1"));

        private static HeroModel Hero(Game game, string name) => game.History.CreateHeroSession(game.History.Current.Heroes().FindByName(name)).Current;

        [Test]
        public void TomesGrantTheirSchoolAndTheSpellsOfNoSchool()
        {
            var fire = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("TomeOfFireMagic").GrantedSpells(GameVersion.HornOfTheAbyss);

            Assert.That(fire, Does.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fireball")));
            Assert.That(fire, Does.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Visions")));
            Assert.That(fire, Does.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MagicArrow")));
            Assert.That(fire, Does.Not.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("IceBolt")));
            Assert.That(fire, Does.Not.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("TitansLightningBolt")));
        }

        [Test]
        public void TheSpellbindersHatGrantsEveryLevelFiveSpellButTheTitansBolt()
        {
            var granted = TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("SpellbindersHat").GrantedSpells(GameVersion.HornOfTheAbyss);

            Assert.That(granted, Is.Not.Empty);
            Assert.That(granted.All(s => s.Level == 5), Is.True);
            Assert.That(granted, Does.Not.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("TitansLightningBolt")));
        }

        [Test]
        public void AngelicAllianceGrantsNoSpellBecauseTheSavefileSaysSo()
        {
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("AngelicAlliance").GrantedSpells(GameVersion.HornOfTheAbyss), Is.Empty);
        }

        [Test]
        public void EquippingTitansThunderGivesASpellBookThatTakingItOffDoesNotTakeBack()
        {
            var game = Load("arts_00_before_arts");
            var hero = Hero(game, "Valeska") with { SpellBook = Hero(game, "Valeska").SpellBook.WithHasSpellBook(false) };
            Assert.That(hero.SpellBook.HasSpellBook, Is.False);

            var equipped = hero.WithEquippedArtifacts(hero.EquippedArtifacts.WithEquipped(ArtifactSlots.Weapon, TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("TitansThunder")));
            Assert.That(equipped.SpellBook.HasSpellBook, Is.True);
            Assert.That(equipped.SpellBook.IsSpellBookGranted, Is.True);
            Assert.That(equipped.SpellBook.IsAvailable(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("TitansLightningBolt")), Is.True);

            var unequipped = equipped.WithEquippedArtifacts(equipped.EquippedArtifacts.WithUnequipped(ArtifactSlots.Weapon));
            Assert.That(unequipped.SpellBook.HasSpellBook, Is.True);
            Assert.That(unequipped.SpellBook.IsSpellBookGranted, Is.False);
            Assert.That(unequipped.SpellBook.IsAvailable(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("TitansLightningBolt")), Is.False);
        }

        [Test]
        public void TheGrantsMatchWhatTheGameStoredForTheHeroesOfTheArtsSaves()
        {
            var game = Load("arts_02_arts");
            var edric = Hero(game, "Edric");

            Assert.That(edric.SpellBook.HasSpellBook, Is.True);
            Assert.That(edric.SpellBook.Source(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("TitansLightningBolt")), Is.EqualTo("Titan's Thunder"));
            Assert.That(edric.SpellBook.Source(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fireball")), Is.EqualTo("Tome of Fire Magic"));
            Assert.That(edric.SpellBook.Source(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Visions")), Does.Contain("Tome of"));

            var valeska = Hero(game, "Valeska");
            Assert.That(valeska.SpellBook.Source(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("SummonBoat")), Is.EqualTo("Sea Captain's Hat"));
            Assert.That(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.All.Count(valeska.SpellBook.IsAvailable), Is.EqualTo(2));
        }

        [Test]
        public void TheEffectLinesNameTheSpellsAndTheSpellBookAnArtifactGives()
        {
            var provider = new EffectLineProvider();

            var thunder = provider.Describe(TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("TitansThunder").EffectInfo);
            var tome = provider.Describe(TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("TomeOfFireMagic").EffectInfo);

            Assert.That(thunder, Does.Contain(new EffectLine("Spell book", "Given")));
            Assert.That(thunder, Does.Contain(new EffectLine("Spells", TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("TitansLightningBolt").Name)));
            Assert.That(tome, Does.Contain(new EffectLine("Spells", "all Fire spells")));
            Assert.That(provider.Describe(default), Is.Empty);
        }
    }
}
