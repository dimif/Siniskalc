using Siniskalc.Heroes.ViewModels;
using Siniskalc.Heroes.Wpf.ViewModels;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels
{
    /// <summary>
    /// The view-only finders of the View menu, which show the byte code of each artifact and creature and search by it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class CatalogFindersTests
    {
        private static ArtifactFinderViewModel NewArtifacts() => ArtifactFinderViewModel.ForCatalog(TestCatalog.Hota, new EffectLineProvider());

        private static CreatureFinderViewModel NewCreatures() =>
            new(TestCatalog.Hota.Creatures.All.Select(c => ((Heroes.Entities.ICreature)c, c.Info)));

        [Test]
        public void TheArtifactCatalogShowsTheCodeLeftOfTheNameAndHasNoAction()
        {
            var finder = NewArtifacts();

            Assert.That(finder.Columns[0].Path, Is.EqualTo(nameof(ArtifactChoiceViewModel.Code)));
            Assert.That(finder.Columns[1].Path, Is.EqualTo(nameof(ArtifactChoiceViewModel.Name)));
            Assert.That(finder.HasAction, Is.False);
        }

        [Test]
        public void TheArtifactCatalogFindsAnArtifactByItsCodeInHexOrDecimal()
        {
            var finder = NewArtifacts();
            var skull = TestCatalog.Hota.Artifacts.ById("SkullHelmet");

            foreach (var word in new[] { skull.Code.ToString("X2"), "0x" + skull.Code.ToString("x2"), skull.Code.ToString() })
            {
                finder.Filter = word;
                Assert.That(finder.Items.Cast<ArtifactChoiceViewModel>().Select(c => c.Artifact), Does.Contain(skull), word);
            }
        }

        [Test]
        public void TheArtifactCatalogStillSearchesByName()
        {
            var finder = NewArtifacts();

            finder.Filter = "skull";

            Assert.That(finder.Items.Cast<ArtifactChoiceViewModel>().Select(c => c.Name), Does.Contain(TestCatalog.Hota.Artifacts.ById("SkullHelmet").Name));
        }

        [Test]
        public void TheCreatureCatalogShowsTheCodeLeftOfTheNameAndHasNoAction()
        {
            var finder = NewCreatures();

            Assert.That(finder.Columns[0].Path, Is.EqualTo(nameof(CreatureChoiceViewModel.Code)));
            Assert.That(finder.Columns[1].Path, Is.EqualTo(nameof(CreatureChoiceViewModel.Name)));
            Assert.That(finder.HasAction, Is.False);
            Assert.That(finder.GetRowMenu(finder.Items.Cast<object>().First()), Is.Empty);
        }

        [Test]
        public void TheCreatureCatalogFindsACreatureByItsCode()
        {
            var finder = NewCreatures();
            var creature = TestCatalog.Hota.Creatures.All.First();

            finder.Filter = "0x" + creature.Code.ToString("X2");

            Assert.That(finder.Items.Cast<CreatureChoiceViewModel>().Select(c => c.Creature), Does.Contain(creature));
        }
    }
}
