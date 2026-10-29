using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Runtime;

namespace Siniskalc.Homm3.Tests.Models
{
    /// <summary>
    /// What the savefile holds for the primary skills of a hero is what counts in the game, with the bonus of the equipped artifacts in it.
    /// The Arts saves in TestData/Hota/Arts are the proof: Edric has 2/2/1/1 without artifacts and is saved with 11/11/9/9 once he
    /// wears Titan's Thunder (+9/+9/+8/+8), and Christian goes from 2/2/1/1 to 5/5/4/7 with Armageddon's Blade (+3/+3/+3/+6).
    /// </summary>
    [TestFixture]
    internal sealed class HeroPrimarySkillsTests
    {
        private static Game Load(string name) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Arts", name + ".GM1"));

        private static HeroModel Hero(Game game, string name) => game.History.CreateHeroSession(game.History.Current.Heroes().FindByName(name)).Current;

        private static int[] Saved(HeroModel hero) => [hero.PrimarySkills.Attack, hero.PrimarySkills.Defense, hero.PrimarySkills.SpellPower, hero.PrimarySkills.Knowledge];

        private static int[] Base(HeroModel hero) => [hero.PrimarySkills.BaseAttack, hero.PrimarySkills.BaseDefense, hero.PrimarySkills.BaseSpellPower, hero.PrimarySkills.BaseKnowledge];

        [TestCase("arts_00_before_arts", "Edric", new[] { 2, 2, 1, 1 })]
        [TestCase("arts_01_arts", "Edric", new[] { 11, 11, 9, 9 })]
        [TestCase("arts_01_arts", "Christian", new[] { 5, 5, 4, 7 })]
        [TestCase("arts_01_arts", "Valeska", new[] { 2, 2, 1, 1 })]
        public void TheSavedSkillsAreTheValuesTheGameHolds(string save, string name, int[] expected)
        {
            Assert.That(Saved(Hero(Load(save), name)), Is.EqualTo(expected));
        }

        [TestCase("arts_01_arts", "Edric")]
        [TestCase("arts_01_arts", "Christian")]
        [TestCase("arts_12_all", "Edric")]
        public void WhatIsLeftWithoutTheArtifactsIsWhatTheHeroHadBeforeWearingThem(string save, string name)
        {
            Assert.That(Base(Hero(Load(save), name)), Is.EqualTo(new[] { 2, 2, 1, 1 }));
        }

        [TestCase("Edric")]
        [TestCase("Christian")]
        public void PuttingTheArtifactsOnGivesTheSkillsTheGameSaved(string name)
        {
            var before = Hero(Load("arts_00_before_arts"), name);
            var after = Hero(Load("arts_01_arts"), name);

            var armed = before.WithEquippedArtifacts(after.EquippedArtifacts);

            Assert.That(Saved(armed), Is.EqualTo(Saved(after)));
            Assert.That(armed.PrimarySkills, Is.EqualTo(after.PrimarySkills));
        }

        [Test]
        public void TakingTheArtifactsOffTakesTheirBonusAway()
        {
            var armed = Hero(Load("arts_01_arts"), "Edric");

            var bare = armed.WithEquippedArtifacts(Hero(Load("arts_00_before_arts"), "Edric").EquippedArtifacts);

            Assert.That(Saved(bare), Is.EqualTo(new[] { 2, 2, 1, 1 }));
        }

        [Test]
        public void ChangingTheBaseKeepsTheBonusOnTop()
        {
            var christian = Hero(Load("arts_01_arts"), "Christian");

            var stronger = christian with { PrimarySkills = christian.PrimarySkills.WithBaseAttack(10).WithBaseKnowledge(20) };

            Assert.That(Saved(stronger), Is.EqualTo(new[] { 13, 5, 4, 26 }));
            Assert.That(Base(stronger), Is.EqualTo(new[] { 10, 2, 1, 20 }));
        }

        [Test]
        public void ASkillStaysWithinWhatTheSavefileCanHold()
        {
            var edric = Hero(Load("arts_01_arts"), "Edric");

            var capped = edric with { PrimarySkills = edric.PrimarySkills.WithBaseAttack(250) };

            Assert.That(capped.PrimarySkills.Attack, Is.EqualTo(255));
        }

        [Test]
        public void TheSkillsOfAnEquippedHeroAreSavedAndReadBackAsTheyAre()
        {
            var game = Load("arts_00_before_arts");
            var armedEdric = Hero(Load("arts_01_arts"), "Edric").EquippedArtifacts;
            game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Edric")).Update(h => h.WithEquippedArtifacts(armedEdric));

            var reloaded = TestServices.GameService.LoadGameFromBytes(game.GetBytesToSave(), "edited");

            Assert.That(Saved(Hero(reloaded, "Edric")), Is.EqualTo(new[] { 11, 11, 9, 9 }));
            Assert.That(Base(Hero(reloaded, "Edric")), Is.EqualTo(new[] { 2, 2, 1, 1 }));
        }

        [Test]
        public void LoadingASavefileLeavesTheSavedSkillsAlone()
        {
            var game = Load("arts_01_arts");

            Assert.That(game.History.Current.ApplyChangesTo(game.Bytes), Is.EqualTo(game.Bytes));
            Assert.That(game.History.GetPendingChanges(), Is.Empty);
        }
    }
}
