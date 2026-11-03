namespace Siniskalc.Homm2.Tests.MultiUnit.ViewModels
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The read-only guardian summary follows the mine model without becoming an edit field.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class MineRowViewModelTests
    {
        [Test]
        public void RefreshShowsTheCurrentGuardianAndNotifiesItsBindingsWithoutEditingTheMine()
        {
            var current = new MineModel(2, MineKind.GoldMine, new MapPosition(43, 23), PlayerColor.Blue, TestCatalog.Standard);
            var session = new Mock<IEditSession<MineModel>>();
            session.SetupGet(s => s.Index).Returns(current.Index);
            session.SetupGet(s => s.Current).Returns(() => current);
            var row = new MineRowViewModel(session.Object, TestCatalog.Standard, "Owner", new MineGuardianTextProvider());
            var raised = new List<string?>();
            row.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
            Assert.That(row.Summary, Is.EqualTo("Unguarded"));

            current = current with { Guardian = new MineGuardian(64, 20, TestCatalog.Standard.Creatures["FireElemental"]) };
            row.Refresh();

            Assert.That(row.Summary, Is.EqualTo("Fire Elemental × 20"));
            Assert.That(raised, Does.Contain(string.Empty).Or.Contain(null).Or.Contain(nameof(row.Summary)));
            session.Verify(s => s.Update(It.IsAny<Func<MineModel, MineModel>>()), Times.Never);
        }

        [Test]
        public void PickingANewOwnerKeepsTheGuardianModel()
        {
            var guardian = new MineGuardian(62, 40, TestCatalog.Standard.Creatures["EarthElemental"]);
            var current = new MineModel(2, MineKind.GoldMine, new MapPosition(43, 23), PlayerColor.Blue, TestCatalog.Standard) { Guardian = guardian };
            var session = new Mock<IEditSession<MineModel>>();
            session.SetupGet(s => s.Current).Returns(() => current);
            session.Setup(s => s.Update(It.IsAny<Func<MineModel, MineModel>>())).Callback((Func<MineModel, MineModel> edit) => current = edit(current));
            var row = new MineRowViewModel(session.Object, TestCatalog.Standard, "Owner", new MineGuardianTextProvider());

            row.OwnerField.Selected = "Orange";

            Assert.That(current.Owner, Is.EqualTo(PlayerColor.Orange));
            Assert.That(current.Guardian, Is.SameAs(guardian));
            Assert.That(row.Summary, Is.EqualTo("Earth Elemental × 40"));
        }
    }
}
