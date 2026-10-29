using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels
{
    /// <summary>
    /// Tests the part of a page that shows what one artifact does (its skills, its class and cost and the lines that describe it), on a
    /// page of nothing else and the real provider of the lines.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class ArtifactInfoViewModelBaseTests
    {
        private sealed class InfoPage() : ArtifactInfoViewModelBase("Info", TestServices.EffectLines)
        {
            public void Show(Artifact? artifact) => this.ShowArtifactInfo(artifact);
        }

        [Test]
        public void ThePageStartsWithNothingShown()
        {
            var page = new InfoPage();

            Assert.That((page.Title, page.Attack, page.Knowledge, page.ClassAndCost), Is.EqualTo(("Info", (short)0, (short)0, string.Empty)));
            Assert.That(page.Effects, Is.Null.Or.Empty);
            Assert.That(page.SelectedEffectLines, Is.Empty);
            Assert.That(page.HasSelectedInfo, Is.False);
        }

        [Test]
        public void ShowingAnArtifactShowsItsSkillsItsClassAndCostAndItsLines()
        {
            var page = new InfoPage();

            page.Show(Artifact.ThunderHelmet);

            Assert.That((page.Power, page.Knowledge), Is.EqualTo(((short)-2, (short)10)));
            Assert.That(page.ClassAndCost, Is.EqualTo($"{Artifact.ThunderHelmet.Class} · {10000:N0} gold"));
            Assert.That(page.SelectedEffectLines, Is.Not.Empty);
            Assert.That(page.HasSelectedInfo, Is.True);
        }

        [Test]
        public void AnArtifactThatCostsNothingHasNoClassAndCostLine()
        {
            var page = new InfoPage();

            page.Show(Artifact.OrbOfFirmament);

            Assert.That(page.ClassAndCost, Is.Empty);
            Assert.That(page.Effects, Is.EqualTo("All Air Spell Damage +50%"));
            Assert.That(page.HasSelectedInfo, Is.True);
        }

        [Test]
        public void TheCommandsShowAnArtifactByNameAndClearIt()
        {
            var page = new InfoPage();

            page.ShowArtifactInfoCommand.Execute(Artifact.HelmOfTheAlabasterUnicorn.Name);
            Assert.That(page.Knowledge, Is.EqualTo((short)1));

            page.ClearArtifactInfoCommand.Execute(null);
            Assert.That((page.Knowledge, page.HasSelectedInfo), Is.EqualTo(((short)0, false)));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void ANameThatIsEmptyClearsTheInfo(string? name)
        {
            var page = new InfoPage();
            page.Show(Artifact.HelmOfTheAlabasterUnicorn);

            page.ShowArtifactInfoByName(name);

            Assert.That(page.Knowledge, Is.EqualTo((short)0));
        }

        [Test]
        public void ANameThatIsNoArtifactClearsTheInfo()
        {
            var page = new InfoPage();
            page.Show(Artifact.HelmOfTheAlabasterUnicorn);

            page.ShowArtifactInfoByName("No such artifact");

            Assert.That(page.Knowledge, Is.EqualTo((short)0));
        }

        [Test]
        public void ShowingAnArtifactTellsEveryBinding()
        {
            var page = new InfoPage();
            var raised = new List<string?>();
            page.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            page.Show(Artifact.ThunderHelmet);

            Assert.That(raised, Is.EquivalentTo(new[] { "Attack", "Defense", "Power", "Knowledge", "Morale", "Luck", "Effects", "SelectedEffectLines", "ClassAndCost", "HasSelectedInfo" }));
        }
    }
}
