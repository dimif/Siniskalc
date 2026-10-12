using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The Restoration of Erathia saves sanya_00 to sanya_05 in TestData/Roe: Red's hero Sanya moves, is put to sleep, wakes by moving and
    /// picks up a quest (the object under her is then a Seer's Hut), a new day starts, and she moves again.
    /// </summary>
    [TestFixture]
    internal sealed class RoeSanyaMovementTests
    {
        private static readonly string[] Saves =
        [
            "sanya_00_baseline", "sanya_01_moved", "sanya_02_asleep", "sanya_03_picks_quest", "sanya_04_new_day_not_moved", "sanya_05_moved_again",
        ];

        private static HeroModel Sanya(int save) => Session(save).Current;

        private static HeroChunk Chunk(int save)
        {
            var heroes = Load(save).History.Current.Heroes();
            return heroes[heroes.FindByName("Sanya")];
        }

        private static Game Load(int save) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Roe", Saves[save] + ".GM1"));

        private static HeroEditSession Session(int save)
        {
            var game = Load(save);
            return game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Sanya"));
        }

        [Test]
        public void MovingSpendsMovementPointsAndTurnsTheHero()
        {
            Assert.That(Sanya(0).MovementPoints.CurrentMovementPoints, Is.EqualTo(1560));
            Assert.That(Sanya(1).MovementPoints.CurrentMovementPoints, Is.EqualTo(1510));
            Assert.That(Sanya(1).Position.Y, Is.EqualTo(Sanya(0).Position.Y + 1));
            Assert.That(Sanya(1).Position.FacingDirection, Is.EqualTo(FacingDirection.South));
        }

        [Test]
        public void SleepingChangesTheAsleepFlagWithoutMovingTheHero()
        {
            Assert.That(Sanya(1).IsAsleep, Is.False);
            Assert.That(Sanya(2).IsAsleep, Is.True);
            Assert.That(Sanya(2).MovementPoints, Is.EqualTo(Sanya(1).MovementPoints));
            Assert.That(Sanya(2).Position, Is.EqualTo(Sanya(1).Position));
        }

        [Test]
        public void MovingWakesTheHeroAndAQuestObjectIsUnderHer()
        {
            Assert.That(Sanya(3).IsAsleep, Is.False);
            Assert.That(Sanya(3).MovementPoints.CurrentMovementPoints, Is.EqualTo(1260));
            Assert.That(Chunk(3).GetByteAt(11), Is.EqualTo(83), "a Seer's Hut");
            Assert.That(Chunk(3).GetByteAt(15), Is.EqualTo(1));
        }

        [Test]
        public void ANewDayRestoresMovementPointsAndNothingElseOfTheHero()
        {
            Assert.That(Sanya(4).MovementPoints.CurrentMovementPoints, Is.EqualTo(Sanya(4).MovementPoints.MaxMovementPoints));
            Assert.That(Sanya(4).Position, Is.EqualTo(Sanya(3).Position));
            Assert.That(Sanya(4).IsAsleep, Is.False);
        }

        [Test]
        public void ShadowOfDeathHasNoMovedTodayByteSoAHeroThatDidNotMoveLooksLikeOneThatDid()
        {
            // Save 4 (a new day, no move) and save 5 (moved again) differ in movement points, position, facing and the object under the
            // hero: there is no byte for "moved today", as in the other Shadow of Death saves.
            Assert.That(Sanya(4).MovedToday, Is.False);
            Assert.That(Sanya(5).MovedToday, Is.False);
            Assert.That(Sanya(5).MovementPoints.CurrentMovementPoints, Is.EqualTo(1419));
        }
    }
}
