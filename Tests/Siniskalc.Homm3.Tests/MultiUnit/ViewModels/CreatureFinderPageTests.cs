using Siniskalc.Heroes.ViewModels;
using Siniskalc.Heroes.Wpf.ViewModels;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Tests.Models;
using Siniskalc.Homm3.Tests.ViewModels;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels
{
    /// <summary>
    /// The creature finder of the army of a hero and the stacks it adds to, over a hero session with no savefile. The hero has ten
    /// pikemen in the first slot and six free slots.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class CreatureFinderPageTests
    {
        private FakeHeroSession session = null!;
        private HeroViewModel page = null!;

        private ArmyViewModel Creatures => this.page.ArmyViewModel;

        private CreatureFinderViewModel Finder => this.page.ArmyViewModel.Finder;

        [SetUp]
        public void StartWithTenPikemen()
        {
            this.session = new FakeHeroSession(HeroModels.Christian());
            this.page = HeroPages.Create(this.session);
        }

        [TearDown]
        public void ClosePage()
        {
            this.page.Dispose();
        }

        private static string[] Names(CreatureFinderViewModel vm) =>
            vm.Items.Cast<CreatureChoiceViewModel>().Select(c => c.Name).ToArray();

        private CreatureChoiceViewModel Choice(string name) => this.Finder.Items.Cast<CreatureChoiceViewModel>().Single(c => c.Name == name);

        [Test]
        public void TheFinderListsEveryCreatureOfTheVersionUntilNarrowed()
        {
            Assert.That(this.Finder.Items.Count, Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.All.Count(c => c.IsAvailableIn(this.session.GameVersion))));
            Assert.That(this.Finder.Summary, Does.Contain("creatures"));
        }

        [Test]
        public void TheFinderNarrowsByEveryWordOfNameTownLevelAndForm()
        {
            this.Finder.Filter = "dragon";
            Assert.That(Names(this.Finder), Does.Contain("Green Dragon").And.Contain("Azure Dragon").And.Not.Contain("Pikeman"));

            this.Finder.Filter = "dragon rampart";
            Assert.That(Names(this.Finder), Is.EquivalentTo(new[] { "Green Dragon", "Gold Dragon" }));

            this.Finder.Filter = "level 1";
            Assert.That(Names(this.Finder), Does.Contain("Pikeman").And.Contain("Halberdier").And.Not.Contain("Angel"));
            Assert.That(this.Finder.Summary, Does.Contain("showing"));

            this.Finder.Filter = "castle upgraded level 7";
            Assert.That(Names(this.Finder), Is.EqualTo(new[] { "Archangel" }));
        }

        [Test]
        public void TheFinderFilterMatchesTownNamesToo()
        {
            this.Finder.Filter = "necropolis";

            Assert.That(Names(this.Finder), Does.Contain("Lich").And.Not.Contain("Pikeman"));
        }

        [Test]
        public void TheRowsShowTheStatsOfTheCreatureForTheGameVersion()
        {
            var angel = this.Choice("Angel");
            var info = TestCatalog.For(this.session.GameVersion).Creatures.ById("Angel").Info;

            Assert.That(angel.Attack, Is.EqualTo(info.Atk));
            Assert.That(angel.Health, Is.EqualTo(info.Health));
            Assert.That(angel.Cost, Is.EqualTo(info.Gold));
            Assert.That(angel.DamageRange, Is.EqualTo(info.MinDamage == info.MaxDamage ? $"{info.MinDamage}" : $"{info.MinDamage}-{info.MaxDamage}"));
            Assert.That(angel.Details, Does.Contain("Angel").And.Contain("gold"));
            Assert.That(this.Finder.Columns.Select(c => c.Header), Does.Contain("HP").And.Not.Contain("Form"));
        }

        [Test]
        public void ASlotShowsTheTotalsOfTheWholeStackAndTheyFollowTheAmount()
        {
            var slot = this.Creatures.CreatureSlots[0];
            var info = TestCatalog.For(this.session.GameVersion).Creatures.ById("Pikeman").Info;
            Assert.That(slot.SelectedCreature, Is.EqualTo("Pikeman"));

            slot.CreatureAmount = 10;
            Assert.That(slot.TotalHealth, Is.EqualTo((info.Health * 10).ToString("N0")));
            Assert.That(slot.TotalCost, Is.EqualTo((info.Gold * 10).ToString("N0")));
            Assert.That(slot.TotalDamage, Does.Contain((info.MinDamage * 10).ToString("N0")));

            slot.CreatureAmount = 25;
            Assert.That(slot.TotalHealth, Is.EqualTo((info.Health * 25).ToString("N0")));
            Assert.That(slot.TotalOtherCost, Is.Empty, "pikemen cost only gold");
        }

        [Test]
        public void AnEmptySlotShowsNoTotals()
        {
            var empty = this.Creatures.CreatureSlots.First(s => !s.HasCreature);

            Assert.That(empty.TotalHealth, Is.Empty);
            Assert.That(empty.TotalCost, Is.Empty);
        }

        [Test]
        public void AddingACreatureFillsTheFirstEmptySlotAndPuttingTheHeroBackRemovesIt()
        {
            var empty = this.Creatures.CreatureSlots.First(s => !s.HasCreature);
            this.Finder.AddAmount = 3;

            this.Finder.ChooseCommand.Execute(this.Choice("Angel"));

            Assert.That(empty.SelectedCreature, Is.EqualTo("Angel"));
            Assert.That(empty.CreatureAmount, Is.EqualTo(3));
            Assert.That(this.session.Current.Army.Slots[1]?.Creature, Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Angel")));

            this.session.Update(_ => this.session.Original);

            Assert.That(empty.HasCreature, Is.False);
        }

        [Test]
        public void AddingIsRefusedWhenEverySlotIsInUseAndAllowedAgainWhenOneIsCleared()
        {
            var angel = this.Choice("Angel");
            while (this.Creatures.HasFreeSlot)
            {
                this.Finder.ChooseCommand.Execute(angel);
            }

            Assert.That(this.Creatures.CreatureSlots.All(s => s.HasCreature), Is.True);
            Assert.That(this.Finder.ChooseCommand.CanExecute(angel), Is.False);
            Assert.That(this.Finder.Summary, Does.Contain("in use"));

            this.Creatures.CreatureSlots[2].ClearCommand.Execute(null);

            Assert.That(this.Creatures.HasFreeSlot, Is.True);
            Assert.That(this.Finder.ChooseCommand.CanExecute(angel), Is.True);
        }
    }
}
