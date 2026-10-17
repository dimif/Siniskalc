namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The obelisks of Threedom (TestData/Homm2/Threedom): nine on the map, a byte each with a bit for every colour that visited it.
    /// </summary>
    [TestFixture]
    internal sealed class ObeliskTests
    {
        private static ObeliskVisits Visits(int save) => TestServices.LoadThreedom(save).History.Current.Obelisks().GetModel()!;

        [Test]
        public void TheMapHasNineObelisksFoundInTheOrderOfTheMap()
        {
            var visits = Visits(71);

            Assert.That(visits.Count, Is.EqualTo(9));
            Assert.That(
                Enumerable.Range(0, 9).Select(visits.PositionOf),
                Is.EqualTo(new[] { new MapPosition(68, 2), new MapPosition(17, 4), new MapPosition(39, 12), new MapPosition(63, 27), new MapPosition(28, 29), new MapPosition(40, 57), new MapPosition(1, 62), new MapPosition(71, 66), new MapPosition(45, 68) }));
        }

        [Test]
        public void TheBitsOfEachObeliskSayWhichColoursVisitedIt()
        {
            var visits = Visits(77);

            Assert.That(visits.Masks, Is.EqualTo(new byte[] { 1, 1, 1, 2, 5, 1, 2, 4, 0 }));
            Assert.That(visits.VisitedCount(Color.Blue), Is.EqualTo(5));
            Assert.That(visits.IsVisited(3, Color.Green), Is.True);
            Assert.That(visits.IsVisited(4, Color.Red), Is.True);
            Assert.That(visits.IsVisited(8, Color.Blue), Is.False);
        }

        [Test]
        public void TheObeliskABlueHeroStoodOnIsTheOneWhoseByteChanged()
        {
            // Charity stood at 28, 29 in save 75 and Colar Blau at 1, 62 in save 80.
            var (before, after) = (Visits(74), Visits(75));
            Assert.That(Enumerable.Range(0, 9).Where(i => before.IsVisited(i, Color.Blue) != after.IsVisited(i, Color.Blue)), Is.EqualTo(new[] { 4 }));
            Assert.That(after.PositionOf(4), Is.EqualTo(new MapPosition(28, 29)));

            var (before80, after80) = (Visits(79), Visits(80));
            Assert.That(Enumerable.Range(0, 9).Where(i => before80.IsVisited(i, Color.Blue) != after80.IsVisited(i, Color.Blue)), Is.EqualTo(new[] { 6 }));
            Assert.That(after80.PositionOf(6), Is.EqualTo(new MapPosition(1, 62)));
        }

        [Test]
        public void ObeliskVisitsAreEditedThroughThePageOfAPlayerAndWrittenBackWithoutTouchingTheOtherColours()
        {
            var game = TestServices.LoadThreedom(77);
            var page = new PlayerViewModel(game, Color.Blue, _ => { }, _ => { });
            var obelisks = page.Obelisks;

            Assert.That(obelisks.HasObelisks, Is.True);
            Assert.That(obelisks.Summary, Is.EqualTo("5 of 9 visited"));
            Assert.That(obelisks.Obelisks[3].Name, Is.EqualTo("Obelisk 4 at X: 63, Y: 27"));

            obelisks.Obelisks[3].IsChecked = true;
            Assert.That(game.History.Current.Obelisks().GetModel()!.Masks[3], Is.EqualTo((byte)3));

            obelisks.VisitAllCommand.Execute(null);
            Assert.That(game.History.Current.Obelisks().GetModel()!.Masks, Is.EqualTo(new byte[] { 1, 1, 1, 3, 5, 1, 3, 5, 1 }));

            obelisks.ClearAllCommand.Execute(null);
            Assert.That(game.History.Current.Obelisks().GetModel()!.Masks, Is.EqualTo(new byte[] { 0, 0, 0, 2, 4, 0, 2, 4, 0 }));
            Assert.That(game.History.GetPendingChanges(), Is.Not.Empty);
        }

        [Test]
        public void AMapWithoutObelisksHasNoObeliskTab()
        {
            var game = TestServices.LoadBrokenAlliance(1);
            var chunks = game.History.Current.Obelisks();

            Assert.That(chunks.Any, Is.EqualTo(chunks.GetModel() is not null));
        }
    }
}
