using Siniskalc.Heroes.ViewModels;
using Siniskalc.Heroes.Wpf.ViewModels;
using Siniskalc.Homm2.Entities;
using Siniskalc.Homm2.ViewModels;

namespace Siniskalc.Homm2.Tests.MultiUnit.ViewModels
{
    /// <summary>
    /// The view-only finders of the View menu, which show the byte code of each artifact and creature and search by it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class CatalogFindersTests
    {
        private static ArtifactFinderViewModel NewArtifacts() => ArtifactFinderViewModel.ForCatalog(TestCatalog.Standard, GameVersion.BaseGame);

        private static CreatureFinderViewModel NewCreatures() =>
            new(TestCatalog.Standard.Creatures.Where(c => c.IsAvailableIn(GameVersion.BaseGame)).Select(c => ((Heroes.Entities.ICreature)c, c.Info)));

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
            var artifact = TestCatalog.Standard.Artifacts.First(a => a.IsAvailableIn(GameVersion.BaseGame));

            foreach (var word in new[] { artifact.Code.ToString("X2"), "0x" + artifact.Code.ToString("x2"), artifact.Code.ToString() })
            {
                finder.Filter = word;
                Assert.That(finder.Items.Cast<ArtifactChoiceViewModel>().Select(c => c.Artifact), Does.Contain(artifact), word);
            }
        }

        [Test]
        public void TheCreatureCatalogShowsTheCodeLeftOfTheNameAndFindsACreatureByIt()
        {
            var finder = NewCreatures();
            var creature = TestCatalog.Standard.Creatures.First(c => c.IsAvailableIn(GameVersion.BaseGame));

            finder.Filter = "0x" + creature.Code.ToString("X2");

            Assert.That(finder.Columns[0].Path, Is.EqualTo(nameof(CreatureChoiceViewModel.Code)));
            Assert.That(finder.HasAction, Is.False);
            Assert.That(finder.Items.Cast<CreatureChoiceViewModel>().Select(c => c.Creature), Does.Contain(creature));
        }
    }
}
