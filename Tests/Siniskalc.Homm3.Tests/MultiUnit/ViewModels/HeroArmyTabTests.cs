using Moq;
using Siniskalc.Core.Entities;
using Siniskalc.Heroes.ViewModels;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Tests.Models;
using Siniskalc.Homm3.Tests.ViewModels;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels
{
    /// <summary>
    /// What the army tab of the hero page shows of the stacks and of the creature finder, over a hero session with no savefile
    /// (Christian, with ten pikemen in the first slot).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class HeroArmyTabTests
    {
        private FakeHeroSession session = null!;

        [SetUp]
        public void StartWithChristian()
        {
            this.session = new FakeHeroSession(HeroModels.Christian());
        }

        private HeroViewModel Page() =>
            new(this.session, new EffectLineProvider(), new SpellPointsCalculatorFactory().Create(this.session.GameVersion), new Mock<IByteEditTargetFactory>().Object, openCastle: _ => { });

        [Test]
        public void AStackShowsItsAiAndFightValueForTheWholeStack()
        {
            using var page = this.Page();
            var slot = page.ArmyViewModel.CreatureSlots[0];
            var info = TestCatalog.For(this.session.GameVersion).Creatures.ById("Pikeman").Info;

            Assert.That(slot.CreatureAmount, Is.EqualTo(10));
            Assert.That(slot.TotalAiValue, Is.EqualTo((info.AiValue * 10).ToString("N0")));
            Assert.That(slot.TotalFightValue, Is.EqualTo((info.FightValue * 10).ToString("N0")));
        }

        [Test]
        public void TheArmySummaryAddsUpTheHealthAndTheCostOfTheStacks()
        {
            this.session = new FakeHeroSession(HeroModels.Christian() with { Army = HeroModels.Christian().Army.WithSlot(1, TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Troll"), 3) });
            using var page = this.Page();
            var version = this.session.GameVersion;
            var pikeman = TestCatalog.For(version).Creatures.ById("Pikeman").Info;
            var troll = TestCatalog.For(version).Creatures.ById("Troll").Info;

            var health = ((long)pikeman.Health * 10) + ((long)troll.Health * 3);
            var gold = ((long)pikeman.Gold * 10) + ((long)troll.Gold * 3);

            Assert.That(page.ArmyViewModel.ArmySummary, Does.StartWith($"HP Total {health:N0}"));
            Assert.That(page.ArmyViewModel.ArmySummary, Does.Contain($"Cost {gold:N0}"));
            Assert.That(page.ArmyViewModel.ArmySummary, Does.Contain("AI ").And.Contain("Fight "));
        }

        [Test]
        public void TheArmySummaryFollowsAnEditOfTheSession()
        {
            using var page = this.Page();
            var before = page.ArmyViewModel.ArmySummary;

            this.session.Update(h => h with { Army = h.Army.WithSlot(1, TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Troll"), 3) });

            Assert.That(page.ArmyViewModel.ArmySummary, Is.Not.EqualTo(before));
        }

        [Test]
        public void TheCreatureFinderShowsCostResourcesAndSpecialButNotGrowthOrGold()
        {
            using var page = this.Page();
            var headers = page.ArmyViewModel.Finder.Columns.Select(c => c.Header).ToList();

            Assert.That(headers, Does.Not.Contain("Growth").And.Not.Contain("Gold"));
            Assert.That(headers, Is.SupersetOf(new[] { "Cost", "Resources", "Special" }));
        }

        [Test]
        public void ACreatureChoiceTellsItsSpecialAndNoResourcesWhenItCostsOnlyGold()
        {
            var troll = new CreatureChoiceViewModel(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Troll"), TestCatalog.For(this.session.GameVersion).Creatures.ById("Troll").Info);

            Assert.That(troll.Special, Does.Contain("Regeneration"));
            Assert.That(troll.Resources, Is.Empty);
        }

        [Test]
        public void ACreatureThatCostsAResourceBesidesGoldListsItAlsoInItsTooltip()
        {
            var version = this.session.GameVersion;

            var pricey = TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.All.Where(c => c.IsAvailableIn(version)).Select(c => new CreatureChoiceViewModel(c, c.Info)).First(c => c.Resources.Length > 0);

            Assert.That(pricey.Details, Does.Contain(pricey.Resources));
        }
    }
}
