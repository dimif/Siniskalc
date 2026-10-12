using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;
using Siniskalc.ViewModels;

namespace Siniskalc.Tests.Models
{
    /// <summary>
    /// The spells a hero can cast come from the spell book (learned), from equipped artifacts (granted) and from elsewhere (other).
    /// The Arts saves in TestData/Hota/Arts are the proof of what artifacts grant: Edric wears the four tomes and Titan's Thunder,
    /// Valeska the Sea Captain's Hat, Ingham the tome of water magic.
    /// </summary>
    [TestFixture]
    internal sealed class SpellBookAndGrantsTests
    {
        private static Game Load(string name) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Arts", name + ".GM1"));

        private static HeroModel Hero(Game game, string name) => game.History.CreateHeroSession(game.History.Current.Heroes().FindByName(name)).Current;

        private static ISet<Spell> Set(params Spell[] spells) => new HashSet<Spell>(spells);

        [Test]
        public void ASpellIsAvailableWhenLearnedGrantedOrAvailableForAnotherReason()
        {
            var book = new SpellBook(Set(Spell.Haste), Set(Spell.Fly))
                .WithGrants(new Dictionary<Spell, string> { [Spell.Armageddon] = "Armageddon's Blade" }, false);

            Assert.That(book.IsAvailable(Spell.Haste), Is.True);
            Assert.That(book.IsAvailable(Spell.Armageddon), Is.True);
            Assert.That(book.IsAvailable(Spell.Fly), Is.True);
            Assert.That(book.IsAvailable(Spell.Slow), Is.False);
            Assert.That(book.Source(Spell.Armageddon), Is.EqualTo("Armageddon's Blade"));
            Assert.That(book.Source(Spell.Fly), Is.EqualTo("Unknown source"));
            Assert.That(book.Source(Spell.Haste), Is.Empty);
            Assert.That(book.Source(Spell.Slow), Is.Empty);
        }

        [Test]
        public void LearningAndForgettingLeaveTheGrantsAlone()
        {
            var book = new SpellBook(Set(), Set())
                .WithGrants(new Dictionary<Spell, string> { [Spell.Prayer] = "Tome" }, false)
                .WithLearned(Spell.Prayer, true)
                .WithLearned(Spell.Prayer, false);

            Assert.That(book.IsLearned(Spell.Prayer), Is.False);
            Assert.That(book.IsAvailable(Spell.Prayer), Is.True);
        }

        [Test]
        public void WithGrantsReturnsTheSameBookWhenNothingChanges()
        {
            var grants = new Dictionary<Spell, string> { [Spell.Prayer] = "Tome" };
            var book = new SpellBook(Set(), Set()).WithGrants(grants, false);

            Assert.That(book.WithGrants(new Dictionary<Spell, string>(grants), false), Is.SameAs(book));
            Assert.That(book.WithGrants(grants, true), Is.Not.SameAs(book));
            Assert.That(book.WithHasSpellBook(false), Is.SameAs(book));
        }

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
        public void SettingTheArtifactsWithoutTheReducerStillUpdatesSpellsAndSkillBonuses()
        {
            var game = Load("arts_00_before_arts");
            var hero = Hero(game, "Valeska");
            var withHat = hero with { EquippedArtifacts = hero.EquippedArtifacts.WithEquipped(ArtifactSlots.Helmet, Artifact.SeaCaptainsHat) };

            Assert.That(withHat.SpellBook.IsAvailable(Spell.SummonBoat), Is.True);
            Assert.That(withHat.SpellBook.Source(Spell.SummonBoat), Is.EqualTo("Sea Captain's Hat"));
            Assert.That(withHat.ArtifactEffects.WaterMovement, Is.GreaterThan((short)0));
        }

        [Test]
        public void ArtifactsAddToTheSkillsWithoutChangingTheSavedValues()
        {
            var game = Load("arts_00_before_arts");
            var hero = Hero(game, "Valeska");
            var armed = hero.WithEquippedArtifacts(hero.EquippedArtifacts.WithEquipped(ArtifactSlots.Weapon, Artifact.TitansThunder));

            Assert.That(armed.PrimarySkills.Atk, Is.EqualTo(hero.PrimarySkills.Atk));
            Assert.That(armed.PrimarySkills.TotalAtk, Is.EqualTo(hero.PrimarySkills.TotalAtk + 9));
            Assert.That(armed.PrimarySkills, Is.EqualTo(hero.PrimarySkills), "equal skills have equal base values; the bonus is derived");
            Assert.That(TestServices.SpellPoints.GetMaximum(game.GameVersion, armed), Is.GreaterThan(TestServices.SpellPoints.GetMaximum(game.GameVersion, hero)));
        }

        [TestCase("arts_00_before_arts")]
        [TestCase("arts_01_arts")]
        [TestCase("arts_02_arts")]
        [TestCase("arts_03_arts")]
        [TestCase("arts_04_arts")]
        [TestCase("arts_05_arts")]
        [TestCase("arts_06_arts")]
        [TestCase("arts_07_before_adv_spells")]
        [TestCase("arts_12_all")]
        public void EveryHeroOfTheArtsSavesWritesBackTheSameBytes(string save)
        {
            var game = Load(save);
            var offsets = game.History.Current.Heroes().Offsets;
            var entities = TestServices.VersionEntities.For(offsets.GameVersion);
            var deserializer = new HeroDeserializer(offsets, entities);
            var serializer = new SafeHeroSerializer(offsets, entities);

            foreach (var chunk in game.History.Current.Heroes())
            {
                Assert.That(serializer.Serialize(chunk, deserializer.Deserialize(chunk)).HasSameBytes(chunk), Is.True, chunk.Name);
            }
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
