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
        public void TheArtifactCatalogListsEveryArtifactIncludingTheSpellBookAndTheGrail()
        {
            var finder = NewArtifacts();
            var codes = finder.Items.Cast<ArtifactChoiceViewModel>().Select(c => c.CodeValue).ToList();

            Assert.That(codes, Does.Contain((byte)0));
            Assert.That(codes, Does.Contain((byte)2));
            Assert.That(codes, Is.EquivalentTo(TestCatalog.Hota.Artifacts.All.Select(a => a.Code)));
        }

        [Test]
        public void TheArtifactCatalogStillSearchesByName()
        {
            var finder = NewArtifacts();

            finder.Filter = "skull";

            Assert.That(finder.Items.Cast<ArtifactChoiceViewModel>().Select(c => c.Name), Does.Contain(TestCatalog.Hota.Artifacts.ById("SkullHelmet").Name));
        }

        [Test]
        public void TheCreatureCatalogListsEveryCreatureOfTheGameWithItsCode()
        {
            var finder = NewCreatures();

            Assert.That(
                finder.Items.Cast<CreatureChoiceViewModel>().Select(c => c.CodeValue),
                Is.EquivalentTo(TestCatalog.Hota.Creatures.All.Select(c => c.Code)));
        }
    }
}
