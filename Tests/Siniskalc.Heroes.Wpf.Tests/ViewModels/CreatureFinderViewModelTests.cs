using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.ViewModels;
using Siniskalc.Heroes.Wpf.ViewModels;

namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    /// <summary>
    /// The view-only creature finder of the View menu, which shows the byte code of each creature and searches by it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class CreatureFinderViewModelTests
    {
        private static CreatureFinderViewModel NewCatalog()
        {
            var info = new CreatureInfo(1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, null);
            return new CreatureFinderViewModel(new[] { TestCreature.Peasant, TestCreature.Archer, TestCreature.Marksman }.Select(c => ((ICreature)c, info)));
        }

        [Test]
        public void TheCreatureCatalogShowsTheCodeLeftOfTheNameAndHasNoActionOrMenu()
        {
            var finder = NewCatalog();

            Assert.That(finder.ShowsCodes, Is.True);
            Assert.That(finder.Columns[0].Path, Is.EqualTo(nameof(CreatureChoiceViewModel.Code)));
            Assert.That(finder.Columns[1].Path, Is.EqualTo(nameof(CreatureChoiceViewModel.Name)));
            Assert.That(finder.HasAction, Is.False);
            Assert.That(finder.GetRowMenu(finder.Items.Cast<object>().First()), Is.Empty);
        }

        [Test]
        public void TheCreatureCatalogFindsACreatureByItsCodeInHexOrDecimal()
        {
            var finder = NewCatalog();

            foreach (var word in new[] { "0x02", "02", "2" })
            {
                finder.Filter = word;
                Assert.That(finder.Items.Cast<CreatureChoiceViewModel>().Select(c => c.Creature), Does.Contain(TestCreature.Marksman), word);
            }
        }

        [Test]
        public void TheCreatureCatalogStillSearchesByName()
        {
            var finder = NewCatalog();

            finder.Filter = "archer";

            Assert.That(finder.Items.Cast<CreatureChoiceViewModel>().Select(c => c.Creature), Is.EqualTo(new[] { TestCreature.Archer }));
        }
    }
}
