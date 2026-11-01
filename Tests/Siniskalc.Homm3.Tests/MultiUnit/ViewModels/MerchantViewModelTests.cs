using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Heroes.Entities;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels
{
    /// <summary>
    /// Tests the page of one merchant: the seven slots that show what is in stock and write a pick or a clear to the session, on the real
    /// slot and finder.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class MerchantViewModelTests
    {
        private static MerchantStockModel Stock(params Artifact?[] slots) =>
            new(1, "Black market", slots.Concat(Enumerable.Repeat<Artifact?>(null, 7 - slots.Length)).ToList());

        private static Mock<IEditSession<MerchantStockModel>> Session(MerchantStockModel stock)
        {
            var current = stock;
            var session = new Mock<IEditSession<MerchantStockModel>>();
            session.SetupGet(s => s.Current).Returns(() => current);
            session.Setup(s => s.Update(It.IsAny<Func<MerchantStockModel, MerchantStockModel>>())).Callback((Func<MerchantStockModel, MerchantStockModel> edit) => current = edit(current));
            return session;
        }

        private static MerchantViewModel Page(Mock<IEditSession<MerchantStockModel>> session, string location = "") =>
            new(session.Object, TestCatalog.For(GameVersion.ShadowOfDeath), TestServices.EffectLines, location);

        [Test]
        public void ThePageHasTheNameOfTheMerchantAndSevenSlotsLabelledFromOne()
        {
            var page = Page(Session(Stock()));

            Assert.That(page.Name, Is.EqualTo("Black market"));
            Assert.That(page.Slots.Select(s => s.Label), Is.EqualTo(Enumerable.Range(1, 7).Select(i => $"Slot {i}")));
        }

        [Test]
        public void TheLocationIsShownOnlyWhenThereIsOne()
        {
            Assert.That(Page(Session(Stock())).HasLocation, Is.False);

            var located = Page(Session(Stock()), "X: 4, Y: 5");
            Assert.That((located.HasLocation, located.Location), Is.EqualTo((true, "X: 4, Y: 5")));
        }

        [Test]
        public void EverySlotShowsTheArtifactInItsPlaceOfTheListAndEmptyOnesAreEmpty()
        {
            var page = Page(Session(Stock(TestCatalog.For(GameVersion.ShadowOfDeath).Artifacts.ById("HelmOfTheAlabasterUnicorn"), null, TestCatalog.For(GameVersion.ShadowOfDeath).Artifacts.ById("OrbOfInhibition"))));

            Assert.That(page.Slots[0].ArtifactName, Is.EqualTo(TestCatalog.For(GameVersion.ShadowOfDeath).Artifacts.ById("HelmOfTheAlabasterUnicorn").Name));
            Assert.That(page.Slots[1].HasArtifact, Is.False);
            Assert.That(page.Slots[2].ArtifactName, Is.EqualTo(TestCatalog.For(GameVersion.ShadowOfDeath).Artifacts.ById("OrbOfInhibition").Name));
        }

        [Test]
        public void ClearingASlotWritesAnEmptySlotToTheSession()
        {
            var session = Session(Stock(TestCatalog.For(GameVersion.ShadowOfDeath).Artifacts.ById("HelmOfTheAlabasterUnicorn"), TestCatalog.For(GameVersion.ShadowOfDeath).Artifacts.ById("OrbOfInhibition")));
            var page = Page(session);

            page.Slots[1].ClearCommand.Execute(null);

            Assert.That(session.Object.Current.Slots[1], Is.Null);
            Assert.That(session.Object.Current.Slots[0], Is.SameAs(TestCatalog.For(GameVersion.ShadowOfDeath).Artifacts.ById("HelmOfTheAlabasterUnicorn")));
        }

        [Test]
        public void PickingAnArtifactInTheFinderOfASlotWritesItToTheSession()
        {
            var session = Session(Stock());
            var page = Page(session);
            var finder = page.Slots[3].Finder;
            var choice = finder.Artifacts.Cast<ArtifactChoiceViewModel>().Single(c => c.Artifact == TestCatalog.For(GameVersion.ShadowOfDeath).Artifacts.ById("SkullHelmet"));

            finder.ChooseCommand.Execute(choice);

            Assert.That(session.Object.Current.Slots[3], Is.SameAs(TestCatalog.For(GameVersion.ShadowOfDeath).Artifacts.ById("SkullHelmet")));
            Assert.That(session.Object.Current.Slots.Count(s => s != null), Is.EqualTo(1));
        }

        [Test]
        public void RefreshShowsWhatTheSessionNowHolds()
        {
            var session = Session(Stock(TestCatalog.For(GameVersion.ShadowOfDeath).Artifacts.ById("HelmOfTheAlabasterUnicorn")));
            var page = Page(session);
            session.Object.Update(stock => stock.WithSlot(0, TestCatalog.For(GameVersion.ShadowOfDeath).Artifacts.ById("OrbOfInhibition")).WithSlot(4, TestCatalog.For(GameVersion.ShadowOfDeath).Artifacts.ById("SkullHelmet")));
            var raised = new List<string?>();
            page.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            page.Refresh();

            Assert.That(page.Slots[0].ArtifactName, Is.EqualTo(TestCatalog.For(GameVersion.ShadowOfDeath).Artifacts.ById("OrbOfInhibition").Name));
            Assert.That(page.Slots[4].ArtifactName, Is.EqualTo(TestCatalog.For(GameVersion.ShadowOfDeath).Artifacts.ById("SkullHelmet").Name));
            Assert.That(raised, Is.Not.Empty);
        }
    }
}
