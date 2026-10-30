using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Runtime;
using Siniskalc.Homm3.ViewModels;

using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Models
{
    /// <summary>
    /// Tests that the artifacts of a hero update its spells, its skill bonuses and its spell points.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroArtifactBonusesTests
    {
        private static Game Load(string name) => SavedGames.Get(Path.Combine("TestData", "Hota", "Arts", name + ".GM1"));

        private static HeroModel Hero(Game game, string name) => game.History.CreateHeroSession(game.History.Current.Heroes().FindByName(name)).Current;

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
        public void ArtifactsAddToTheSavedSkillsAndGiveTheSpellPointsOfTheHigherKnowledge()
        {
            var game = Load("arts_00_before_arts");
            var hero = Hero(game, "Valeska");
            var armed = hero.WithEquippedArtifacts(hero.EquippedArtifacts.WithEquipped(ArtifactSlots.Weapon, Artifact.TitansThunder));

            Assert.That(armed.PrimarySkills.Attack, Is.EqualTo(hero.PrimarySkills.Attack + 9), "the game saves the skills with the artifact in them");
            Assert.That(armed.PrimarySkills.BaseAttack, Is.EqualTo(hero.PrimarySkills.BaseAttack), "what the hero learned is the same");
            Assert.That(armed.PrimarySkills, Is.Not.EqualTo(hero.PrimarySkills));
            Assert.That(TestServices.SpellPoints(game.GameVersion).GetMaximum(armed.PrimarySkills, armed.SecondarySkills), Is.GreaterThan(TestServices.SpellPoints(game.GameVersion).GetMaximum(hero.PrimarySkills, hero.SecondarySkills)));
        }
    }
}
