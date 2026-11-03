using System.Globalization;
using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.ViewModels
{
    /// <summary>
    /// Tests the row of a dwelling of the map: what it shows, the owner picked from the colours and the stock typed as text.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MapDwellingRowViewModelTests
    {
        private static DwellingModel Dwelling(Creature? creature = null, int code = 14, long stock = 7) =>
            new(4, code, creature, PlayerColor.Blue, new MapPosition(9, 8, MapPosition.Underground), stock);

        private static Mock<IEditSession<DwellingModel>> Session(DwellingModel dwelling)
        {
            var current = dwelling;
            var session = new Mock<IEditSession<DwellingModel>>();
            session.SetupGet(s => s.Index).Returns(dwelling.Index);
            session.SetupGet(s => s.Current).Returns(() => current);
            session.Setup(s => s.Update(It.IsAny<Func<DwellingModel, DwellingModel>>())).Callback((Func<DwellingModel, DwellingModel> edit) => current = edit(current));
            return session;
        }

        [Test]
        public void TheRowShowsTheDwellingOfTheSession()
        {
            var row = new MapDwellingRowViewModel(Session(Dwelling(TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Centaur"))).Object);

            Assert.That((row.Index, row.CreatureName, row.X, row.Y, row.Level, row.Stock), Is.EqualTo((4, "Centaur", 9, 8, MapPosition.Underground, 7L)));
            Assert.That(row.PositionText, Does.Contain("at 9, 8").And.Contain("Underground"));
            Assert.That(row.LevelText, Does.Contain("Underground"));
        }

        [Test]
        public void ACreatureTheEditorDoesNotKnowIsNamedByItsCode()
        {
            var row = new MapDwellingRowViewModel(Session(Dwelling(null, 250)).Object);

            Assert.That(row.CreatureName, Is.EqualTo("Creature 250"));
        }

        [Test]
        public void TheOwnerIsPickedFromTheColoursAndWrittenToTheDwelling()
        {
            var session = Session(Dwelling());
            var row = new MapDwellingRowViewModel(session.Object);

            Assert.That((row.OwnerField.Selected, row.OwnerSort), Is.EqualTo(("Blue", "Blue")));

            row.OwnerField.Selected = "Red";

            Assert.That(session.Object.Current.Owner, Is.EqualTo(PlayerColor.Red));
        }

        [Test]
        public void TypingAStockWritesItClampedToTheLimits()
        {
            var session = Session(Dwelling());
            var row = new MapDwellingRowViewModel(session.Object);

            row.StockText = "40";
            Assert.That(session.Object.Current.Stock, Is.EqualTo(40));

            row.StockText = "-5";
            Assert.That(session.Object.Current.Stock, Is.EqualTo(0));

            row.StockText = (Constants.Army.MaxStackSize + 5).ToString(CultureInfo.CurrentCulture);
            Assert.That(session.Object.Current.Stock, Is.EqualTo(Constants.Army.MaxStackSize));
        }

        [Test]
        public void TextThatIsNoNumberOrTheSameStockWritesNothingButTellsTheBinding()
        {
            var session = Session(Dwelling());
            var row = new MapDwellingRowViewModel(session.Object);
            var raised = new List<string?>();
            row.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            row.StockText = "many";
            row.StockText = "7";

            session.Verify(s => s.Update(It.IsAny<Func<DwellingModel, DwellingModel>>()), Times.Never);
            Assert.That(raised, Is.EqualTo(new[] { "StockText", "StockText" }));
            Assert.That(row.StockText, Is.EqualTo("7"));
        }

        [Test]
        public void RefreshTellsEveryBindingAndTheOwnerField()
        {
            var session = Session(Dwelling());
            var row = new MapDwellingRowViewModel(session.Object);
            var raised = new List<string?>();
            row.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
            session.Object.Update(d => d with { Owner = PlayerColor.Teal });

            row.Refresh();

            var ownerRaised = new List<string?>();
            row.OwnerField.PropertyChanged += (_, e) => ownerRaised.Add(e.PropertyName);
            row.Refresh();

            Assert.That(raised, Is.EqualTo(new[] { string.Empty, string.Empty }));
            Assert.That(ownerRaised, Is.Not.Empty);
            Assert.That(row.OwnerField.Selected, Is.EqualTo("Teal"));
        }
    }
}
