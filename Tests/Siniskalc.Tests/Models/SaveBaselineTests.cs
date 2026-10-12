using Siniskalc.Data;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Models
{
    /// <summary>
    /// After saving, what was saved is the baseline: nothing is pending, and undo shows what it undid.
    /// </summary>
    [TestFixture]
    internal sealed class SaveBaselineTests
    {
        private string path;

        [SetUp]
        public void SetUp()
        {
            this.path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.GM1");
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(this.path);
        }

        [Test]
        public void SavingLeavesNothingPending()
        {
            var game = HeroEditingTests.LoadPrepared();
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));
            session.Update(h => h with { SpellPoints = 123 });
            Assert.That(game.History.GetPendingChanges(), Is.Not.Empty);

            TestServices.GameService.SaveGame(game, this.path);

            Assert.That(game.History.GetPendingChanges(), Is.Empty);
        }

        [Test]
        public void EditsAfterSavingAreMeasuredFromTheSavedState()
        {
            var game = HeroEditingTests.LoadPrepared();
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));
            session.Update(h => h with { SpellPoints = 123 });
            TestServices.GameService.SaveGame(game, this.path);

            session.Update(h => h with { SpellPoints = 7 });

            var change = game.History.GetPendingChanges().Single();
            Assert.That(change.Change.Before, Is.EqualTo("123"));
            Assert.That(change.Change.After, Is.EqualTo("7"));
        }

        [Test]
        public void UndoingPastASaveShowsTheUndoneEditAsPending()
        {
            var game = HeroEditingTests.LoadPrepared();
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));
            session.Update(h => h with { SpellPoints = 123 });
            TestServices.GameService.SaveGame(game, this.path);

            game.History.Undo();

            var change = game.History.GetPendingChanges().Single();
            Assert.That(change.Change.Before, Is.EqualTo("123"));
        }

        [Test]
        public void SavedFileHoldsTheEditsAndSavingAgainStillWorks()
        {
            var game = HeroEditingTests.LoadPrepared();
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));
            session.Update(h => h with { SpellPoints = 123 });
            TestServices.GameService.SaveGame(game, this.path);
            session.Update(h => h with { SpellPoints = 7 });
            TestServices.GameService.SaveGame(game, this.path);

            var reloaded = TestServices.GameService.LoadGameFromFile(this.path);
            Assert.That(reloaded.History.CreateHeroSession(reloaded.History.Current.Heroes().FindByName("Christian")).Current.SpellPoints, Is.EqualTo((ushort)7));
        }

        [Test]
        public void SavingOverALargerFileDoesNotLeaveTheOldEndBehind()
        {
            var game = HeroEditingTests.LoadPrepared();
            File.WriteAllBytes(this.path, new byte[game.GetBytesToSave().Length * 4]);

            TestServices.GameService.SaveGame(game, this.path);

            Assert.That(TestServices.SavefileCodec.Read(this.path), Is.EqualTo(game.GetBytesToSave()));
        }

        [Test]
        public void HeroIsNoLongerEditedAfterSaving()
        {
            var game = HeroEditingTests.LoadPrepared();
            using var hero = TestServices.CreateHeroViewModel(game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian")));
            hero.GeneralInfoViewModel.Attack = 42;
            Assert.That(hero.HasPendingChanges, Is.True);

            TestServices.GameService.SaveGame(game, this.path);
            Assert.That(hero.HasPendingChanges, Is.False);

            game.History.Undo();
            Assert.That(hero.HasPendingChanges, Is.True);
        }

        [Test]
        public void HexTargetReportsTheMovedBaseline()
        {
            var game = HeroEditingTests.LoadPrepared();
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));
            using var target = new HeroByteEditTarget(session, TestServices.HeroRegions);
            var raised = 0;
            target.Changed += (_, _) => raised++;
            session.Update(h => h with { SpellPoints = 123 });
            var afterEdit = raised;

            TestServices.GameService.SaveGame(game, this.path);

            Assert.That(raised, Is.GreaterThan(afterEdit));
            Assert.That(target.GetOriginalBytes(), Is.EqualTo(target.GetBytes()));
        }
    }
}
