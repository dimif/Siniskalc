namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// Tests the row of a shrine on the map of HoMM2: the circle and position it shows, the spell it holds picked from the spells of the game
    /// and the note when the spell is hidden by a hero or not known.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ShrineRowViewModelTests
    {
        private static readonly string[] SpellNames = ["Cure", "Fireball", "Haste"];

        private static ShrineModel Shrine(int circle = 2, int spellCode = 6) => new(3, circle, new MapPosition(12, 34), spellCode);

        private static Mock<IEditSession<ShrineModel>> Session(ShrineModel shrine)
        {
            var current = shrine;
            var session = new Mock<IEditSession<ShrineModel>>();
            session.SetupGet(s => s.Index).Returns(shrine.Index);
            session.SetupGet(s => s.Current).Returns(() => current);
            session.Setup(s => s.Update(It.IsAny<Func<ShrineModel, ShrineModel>>())).Callback((Func<ShrineModel, ShrineModel> edit) => current = edit(current));
            return session;
        }

        [Test]
        public void TheRowShowsTheCirclePositionAndSpellOfTheShrine()
        {
            var row = new ShrineRowViewModel(Session(Shrine()).Object, SpellNames);

            Assert.That((row.Index, row.CircleNumber, row.Circle, row.SpellName, row.X, row.Y), Is.EqualTo((3, 2, "Second circle", "Cure", 12, 34)));
            Assert.That(row.PositionText, Is.EqualTo("X: 12, Y: 34"));
            Assert.That(row.HasNote, Is.False);
        }

        [TestCase(1, "First circle")]
        [TestCase(2, "Second circle")]
        [TestCase(3, "Third circle")]
        public void EachCircleHasItsName(int circle, string name)
        {
            Assert.That(new ShrineRowViewModel(Session(Shrine(circle)).Object, SpellNames).Circle, Is.EqualTo(name));
        }

        [Test]
        public void ThePickedSpellIsWrittenToTheShrineByItsName()
        {
            var session = Session(Shrine());
            var row = new ShrineRowViewModel(session.Object, SpellNames);

            Assert.That(row.SpellField.Choices, Is.EqualTo(SpellNames));
            Assert.That(row.SpellField.Selected, Is.EqualTo("Cure"));

            row.SpellField.Selected = "Fireball";

            Assert.That(session.Object.Current.SpellCode, Is.EqualTo(Spell.Fireball.Code));
        }

        [Test]
        public void AShrineWithAHeroOnItCannotBeEditedAndSaysSo()
        {
            var row = new ShrineRowViewModel(Session(Shrine(spellCode: -1)).Object, SpellNames);

            Assert.That(row.SpellField.IsEnabled, Is.False);
            Assert.That(row.Note, Does.Contain("hero stands on the shrine"));
            Assert.That((row.HasNote, row.SpellName), Is.EqualTo((true, string.Empty)));
        }

        [Test]
        public void AShrineWithASpellTheEditorDoesNotKnowSaysWhichNumberItHolds()
        {
            var unknown = Enumerable.Range(0, 256).First(c => Spell.FromByte((byte)c) == null);
            var row = new ShrineRowViewModel(Session(Shrine(spellCode: unknown)).Object, SpellNames);

            Assert.That(row.SpellField.IsEnabled, Is.True);
            Assert.That(row.Note, Does.Contain($"spell number {unknown}"));
            Assert.That(row.SpellName, Is.Empty);
        }

        [Test]
        public void RefreshShowsTheSpellTheSessionNowHoldsAndTellsTheBindings()
        {
            var session = Session(Shrine());
            var row = new ShrineRowViewModel(session.Object, SpellNames);
            var raised = new List<string?>();
            row.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
            var fieldRaised = new List<string?>();
            row.SpellField.PropertyChanged += (_, e) => fieldRaised.Add(e.PropertyName);
            session.Object.Update(s => s.WithSpell(Spell.Haste));

            row.Refresh();

            Assert.That(raised, Is.EqualTo(new[] { string.Empty }));
            Assert.That(fieldRaised, Is.Not.Empty);
            Assert.That((row.SpellName, row.SpellField.Selected), Is.EqualTo(("Haste", "Haste")));
        }
    }
}
