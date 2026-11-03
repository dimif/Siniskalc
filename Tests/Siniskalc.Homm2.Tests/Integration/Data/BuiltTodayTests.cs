namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The set of the castles that have built today, as the saves of TestData/Homm2/BuiltThisTurn and Threedom show it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class BuiltTodayTests
    {
        private const string Folder = "BuiltThisTurn";
        private const int Corackston = 2;
        private const int Xabran = 3;
        private const int Woodhaven = 7;

        private static BuiltTodayModel Set(string save) => TestServices.LoadNamed(Folder, "built_this_turn_" + save).History.Current.BuiltToday().GetModel(0);

        [Test]
        public void NoCastleHasBuiltBeforeAnythingIsBuilt()
        {
            Assert.That(Set("00_baseline").Castles, Is.Empty);
        }

        [Test]
        public void BuildingMarksTheCastleAndTheNextDayClearsIt()
        {
            Assert.That(Set("01_xabran_well").Has(Xabran), Is.True);
            Assert.That(Set("02_day_2").Castles, Is.Empty);
            Assert.That(Set("03_xabran_dwelling_3").Has(Xabran), Is.True);
        }

        [Test]
        public void TheCastlesOfTwoPlayersAreMarkedAtTheSameTimeAndStayMarkedThroughTheOtherTurn()
        {
            Assert.That(Set("05_hotseat_xabran_well").Castles, Is.EqualTo(new[] { Xabran }));
            Assert.That(Set("06_hotseat_orange_turn_day_1").Castles, Is.EqualTo(new[] { Xabran }));
            Assert.That(Set("07_hotseat_woodhaven_well").Castles, Is.EqualTo(new[] { Xabran, Woodhaven }));
            Assert.That(Set("08_hotseat_blue_turn_day_2").Castles, Is.Empty);
        }

        [Test]
        public void TwoCastlesOfOnePlayerAreMarkedOneByOne()
        {
            Assert.That(Set("12_two_castles_day_4").Castles, Is.Empty);
            Assert.That(Set("13_corackston_well__xabran_well").Castles, Is.EqualTo(new[] { Corackston, Xabran }));
            Assert.That(Set("14_two_castles_day_5").Castles, Is.Empty);
        }

        [Test]
        public void TheSetIsFoundAtTheSameDistanceFromTheTableOfCastlesInAnotherMap()
        {
            // Threedom: Hillstone is castle 0 and Whiteshield castle 3; save 35 is the day Whiteshield built its mansion.
            var set = TestServices.LoadThreedom(35).History.Current.BuiltToday().GetModel(0);

            Assert.That(set.Castles, Is.EqualTo(new[] { 0, 3 }));
        }

        [Test]
        public void MarkingACastleSetsOnlyItsBit()
        {
            var game = TestServices.LoadNamed(Folder, "built_this_turn_00_baseline");
            var session = game.History.CreateBuiltTodaySession();
            var castles = game.History.Current.Castles();
            var before = Enumerable.Range(0, castles.Count).Select(castles.GetModel).ToList();

            session.Update(set => set.With(Woodhaven, true));

            Assert.That(session.Current.Castles, Is.EqualTo(new[] { Woodhaven }));
            Assert.That(game.History.GetPendingChanges().Count(c => c.Kind == Homm2ChunkKinds.BuiltToday), Is.EqualTo(1));
            var after = game.History.Current.Castles();
            Assert.That(Enumerable.Range(0, after.Count).Select(after.GetModel), Is.EqualTo(before));
        }

        [Test]
        public void ASavedSetWrittenAsItWasReadChangesNoByte()
        {
            var path = Saves.Named(Folder, "built_this_turn_13_corackston_well__xabran_well");
            var game = TestServices.Load(path);
            var session = game.History.CreateBuiltTodaySession();

            session.Update(set => set.With(Xabran, false).With(Xabran, true));

            Assert.That(game.History.GetPendingChanges().Where(c => c.Kind == Homm2ChunkKinds.BuiltToday), Is.Empty);
        }
    }
}
