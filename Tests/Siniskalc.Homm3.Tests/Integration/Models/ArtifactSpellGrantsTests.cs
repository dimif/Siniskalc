using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Runtime;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.Integration.Models
{
    /// <summary>
    /// Tests the spells the artifacts of a hero grant, in the saves that show them.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class ArtifactSpellGrantsTests
    {
        private static Game Load(string name) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Arts", name + ".GM1"));

        private static HeroModel Hero(Game game, string name) => game.History.CreateHeroSession(game.History.Current.Heroes().FindByName(name)).Current;

        [Test]
        public void TomesGrantTheirSchoolAndTheSpellsOfNoSchool()
        {
            var fire = Artifact.TomeOfFireMagic.GrantedSpells(GameVersion.HornOfTheAbyss);

            Assert.That(fire, Does.Contain(Spell.Fireball));
            Assert.That(fire, Does.Contain(Spell.Visions));
            Assert.That(fire, Does.Contain(Spell.MagicArrow));
            Assert.That(fire, Does.Not.Contain(Spell.IceBolt));
            Assert.That(fire, Does.Not.Contain(Spell.TitansLightningBolt));
        }

        [Test]
        public void TheSpellbindersHatGrantsEveryLevelFiveSpellButTheTitansBolt()
        {
            var granted = Artifact.SpellbindersHat.GrantedSpells(GameVersion.HornOfTheAbyss);

            Assert.That(granted, Is.Not.Empty);
            Assert.That(granted.All(s => s.Level == 5), Is.True);
            Assert.That(granted, Does.Not.Contain(Spell.TitansLightningBolt));
        }

        [Test]
        public void AngelicAllianceGrantsNoSpellBecauseTheSavefileSaysSo()
        {
            Assert.That(Artifact.AngelicAlliance.GrantedSpells(GameVersion.HornOfTheAbyss), Is.Empty);
        }

        [Test]
        public void EquippingTitansThunderGivesASpellBookThatTakingItOffDoesNotTakeBack()
        {
            var game = Load("arts_00_before_arts");
            var hero = Hero(game, "Valeska") with { SpellBook = Hero(game, "Valeska").SpellBook.WithHasSpellBook(false) };
            Assert.That(hero.SpellBook.HasSpellBook, Is.False);

            var equipped = hero.WithEquippedArtifacts(hero.EquippedArtifacts.WithEquipped(ArtifactSlots.Weapon, Artifact.TitansThunder));
            Assert.That(equipped.SpellBook.HasSpellBook, Is.True);
            Assert.That(equipped.SpellBook.IsSpellBookGranted, Is.True);
            Assert.That(equipped.SpellBook.IsAvailable(Spell.TitansLightningBolt), Is.True);

            var unequipped = equipped.WithEquippedArtifacts(equipped.EquippedArtifacts.WithUnequipped(ArtifactSlots.Weapon));
            Assert.That(unequipped.SpellBook.HasSpellBook, Is.True);
            Assert.That(unequipped.SpellBook.IsSpellBookGranted, Is.False);
            Assert.That(unequipped.SpellBook.IsAvailable(Spell.TitansLightningBolt), Is.False);
        }

        [Test]
        public void TheGrantsMatchWhatTheGameStoredForTheHeroesOfTheArtsSaves()
        {
            var game = Load("arts_02_arts");
            var edric = Hero(game, "Edric");

            Assert.That(edric.SpellBook.HasSpellBook, Is.True);
            Assert.That(edric.SpellBook.Source(Spell.TitansLightningBolt), Is.EqualTo("Titan's Thunder"));
            Assert.That(edric.SpellBook.Source(Spell.Fireball), Is.EqualTo("Tome of Fire Magic"));
            Assert.That(edric.SpellBook.Source(Spell.Visions), Does.Contain("Tome of"));

            var valeska = Hero(game, "Valeska");
            Assert.That(valeska.SpellBook.Source(Spell.SummonBoat), Is.EqualTo("Sea Captain's Hat"));
            Assert.That(Spell.All.Count(valeska.SpellBook.IsAvailable), Is.EqualTo(2));
        }

        [Test]
        public void TheEffectLinesNameTheSpellsAndTheSpellBookAnArtifactGives()
        {
            var provider = new EffectLineProvider();

            var thunder = provider.Describe(Artifact.TitansThunder.EffectInfo);
            var tome = provider.Describe(Artifact.TomeOfFireMagic.EffectInfo);

            Assert.That(thunder, Does.Contain(new EffectLine("Spell book", "Given")));
            Assert.That(thunder, Does.Contain(new EffectLine("Spells", Spell.TitansLightningBolt.Name)));
            Assert.That(tome, Does.Contain(new EffectLine("Spells", "all Fire spells")));
            Assert.That(provider.Describe(default), Is.Empty);
        }
    }
}
