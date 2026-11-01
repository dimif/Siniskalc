namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// What the pending changes of a hero say, and how they are put back.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroDifferSavegameTests
    {
        private const int Maximus = 7;

        private Game game = null!;

        [SetUp]
        public void LoadTheStart() => this.game = TestServices.LoadBrokenAlliance(1);

        [Test]
        public void ANewLevelIsOneChangeOfTheExperienceThatIsPutBackWithItsLevel()
        {
            var original = this.game.History.Current.Heroes().GetModel(Maximus);

            this.game.History.CreateHeroSession(Maximus).Update(h => h.WithLevel(original.HeroLevel.Level + 2));

            var change = this.game.History.GetPendingChanges().Single();
            Assert.That(change.Change.Description, Is.EqualTo("Experience"));
            Assert.That(change.Change.Before, Is.EqualTo($"{original.HeroLevel.Experience:N0} (level {original.HeroLevel.Level})"));
            Assert.That(change.Change.After, Does.EndWith($"(level {original.HeroLevel.Level + 2})"));

            change.Revert(this.game.History);

            Assert.That(this.game.History.Current.Heroes().GetModel(Maximus).HeroLevel, Is.EqualTo(original.HeroLevel));
            Assert.That(this.game.History.GetPendingChanges(), Is.Empty);
        }

        [Test]
        public void TurningAHeroIsAChangeOfItsPositionThatShowsTheTileAndTheFacing()
        {
            var original = this.game.History.Current.Heroes().GetModel(Maximus);
            var turned = original.Position.FacingDirection == FacingDirection.South ? FacingDirection.North : FacingDirection.South;

            this.game.History.CreateHeroSession(Maximus).Update(h => h with { Position = h.Position with { FacingDirection = turned } });

            var change = this.game.History.GetPendingChanges().Single();
            var tile = $"at {original.Position.X}, {original.Position.Y}";
            Assert.That(
                (change.Change.Description, change.Change.Before, change.Change.After),
                Is.EqualTo(("Position", $"{tile}, facing {original.Position.FacingDirection}", $"{tile}, facing {turned}")));
        }

        [Test]
        public void ChangedMovementShowsWhatIsLeftAndWhatAFullDayGives()
        {
            var original = this.game.History.Current.Heroes().GetModel(Maximus);
            var left =original.MovementPoints.Current == 0 ? 100 : 0;

            this.game.History.CreateHeroSession(Maximus).Update(h => h.WithMovementLeft(left));

            var change = this.game.History.GetPendingChanges().Single();
            Assert.That(
                (change.Change.Description, change.Change.After),
                Is.EqualTo(("Movement points", $"{left:N0} / {original.MovementPoints.Max:N0}")));
        }

        [Test]
        public void VisitedPlacesAreListedOneKindAtATimeLikeInHomm3()
        {
            var history = this.game.History;
            var original = history.Current.Heroes().GetModel(Maximus).Visited;

            history.CreateHeroSession(Maximus).Update(h => h with { Visited = h.Visited.With(PlaceKind.Fort, original.Of(PlaceKind.Fort) ^ 0b10) });

            var change = history.GetPendingChanges().Single();
            Assert.That(change.Change.Description, Is.EqualTo("Visited: forts"));
            change.Revert(history);
            Assert.That(history.GetPendingChanges(), Is.Empty);
        }

        [Test]
        public void RevertingTheOwnerOfAHeroPutsTheHeroAndTheListsOfThePlayersBackInOneStep()
        {
            var history = this.game.History;
            var original = history.Current.ToBytes();
            var hero = history.Current.Heroes().GetModel(Maximus);
            var newOwner = hero.Owner == PlayerColor.Blue ? PlayerColor.Green : PlayerColor.Blue;

            history.CreateHeroSession(Maximus).Update(h => h.WithOwner(newOwner));
            Assert.That(history.GetPendingChanges().Select(c => c.Change.Description), Does.Contain("Owner"));
            Assert.That(history.Current.Players().GetLists(newOwner.Code).HeroIds, Does.Contain(Maximus));

            history.GetPendingChanges().Single(c => c.Change.Description == "Owner").Revert(history);

            Assert.That(history.GetPendingChanges(), Is.Empty);
            Assert.That(history.Current.ToBytes(), Is.EqualTo(original));

            history.Undo();

            Assert.That(history.GetPendingChanges().Select(c => c.Change.Description), Does.Contain("Owner"));
            Assert.That(history.Current.Players().GetLists(newOwner.Code).HeroIds, Does.Contain(Maximus));
        }

        [Test]
        public void TheListsOfAPlayerNameAHeroByItsTableIndexAndNotByItsScenarioPortraitByte()
        {
            var history = TestServices.Load(Saves.Path("MapDifficultySize", "map_difficulty_size_07_map_hard_extra_large")).History;
            var heroes = history.Current.Heroes().GetModels();
            var index = Enumerable.Range(0, heroes.Count).First(i => heroes[i].PortraitId != i && !heroes[i].Owner.IsNeutral);
            var newOwner = heroes[index].Owner == PlayerColor.Blue ? PlayerColor.Green : PlayerColor.Blue;

            history.CreateHeroSession(index).Update(h => h.WithOwner(newOwner));

            var lists = history.Current.Players().GetLists(newOwner.Code);
            Assert.That(lists.HeroIds, Does.Contain(index));
            Assert.That(history.Current.Players().GetLists(heroes[index].Owner.Code).HeroIds, Does.Not.Contain(index));
        }
    }
}
