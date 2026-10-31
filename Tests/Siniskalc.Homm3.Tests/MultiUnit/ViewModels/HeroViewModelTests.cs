using Moq;
using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Tests.Models;
using Siniskalc.Homm3.Tests.ViewModels;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels
{
    /// <summary>
    /// The hero page over a hero session that has no savefile behind it: what the page shows of the hero and of the castle around it,
    /// and how an edit of the page reaches the session. The child view models are the real ones.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class HeroViewModelTests
    {
        private FakeHeroSession session = null!;
        private Mock<IByteEditTargetFactory> byteEditTargets = null!;
        private readonly List<int> openedCastles = [];

        [SetUp]
        public void StartWithChristianInNoCastle()
        {
            this.openedCastles.Clear();
            this.session = new FakeHeroSession(HeroModels.Christian());
        }

        private HeroViewModel Page(FakeHeroSession? other = null)
        {
            var used = other ?? this.session;
            this.byteEditTargets = new Mock<IByteEditTargetFactory>();
            return new HeroViewModel(used, new EffectLineProvider(), new SpellPointsCalculatorFactory().Create(used.GameVersion), this.byteEditTargets.Object, openCastle: this.openedCastles.Add);
        }

        [Test]
        public void TheHeaderNamesTheHeroItsClassAndLevelAndWhereItStands()
        {
            using var page = this.Page();

            Assert.That(page.Title, Is.EqualTo("Christian"));
            Assert.That(page.PositionText, Is.EqualTo("at 12, 7, Surface"));
            Assert.That(page.SubtitleText, Is.EqualTo("Knight · Level 5 · at 12, 7, Surface"));
            Assert.That(page.ResetCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void MaxGivesTheHeroTheMostMovementAndSpellPoints()
        {
            using var page = this.Page();
            var points = page.GeneralInfoViewModel.MovementAndSpellPoints;

            points.MaxMovementCommand.Execute(null);
            points.MaxSpellPointsCommand.Execute(null);

            Assert.That(this.session.Current.MovementPoints.Current, Is.EqualTo((uint)Constants.Hero.MaxMovementPoints));
            Assert.That(this.session.Current.SpellPoints, Is.EqualTo(Constants.Hero.MaxSpellPoints));
            Assert.That(points.MaxMovementCommand.CanExecute(null), Is.False);
            Assert.That(points.MaxSpellPointsCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void ResetFillsTheSpellPointsToTheMostTheHeroCanHave()
        {
            using var page = this.Page();
            page.GeneralInfoViewModel.MovementAndSpellPoints.MaxSpellPointsCommand.Execute(null);

            page.GeneralInfoViewModel.MovementAndSpellPoints.ResetSpellPointsCommand.Execute(null);

            var most = new SpellPointsCalculatorFactory().Create(this.session.GameVersion).GetMaximum(this.session.Current.PrimarySkills, this.session.Current.SecondarySkills);
            Assert.That(this.session.Current.SpellPoints, Is.EqualTo(most));
        }

        [Test]
        public void TheGeneralTabEditsTheMovedFlagAndOnlyOneActiveSpellRemainsWhenTheOtherIsUnticked()
        {
            using var page = this.Page();
            var general = page.GeneralInfoViewModel;
            var fly = general.ActiveSpells.Spells.Single(t => t.Name == "Fly");
            var visions = general.ActiveSpells.Spells.Single(t => t.Name == "Visions");

            general.MovedToday = true;
            fly.IsChecked = true;
            visions.IsChecked = true;
            fly.IsChecked = false;

            Assert.That(this.session.Current.MovedToday, Is.True);
            Assert.That(this.session.Current.ActiveSpells, Is.EqualTo(ActiveAdventureSpells.Visions));
            Assert.That((visions.IsChecked, fly.IsChecked), Is.EqualTo((true, false)));
        }

        [Test]
        public void ResetPutsTheWholeHeroBack()
        {
            using var page = this.Page();
            page.GeneralInfoViewModel.MovementAndSpellPoints.MaxMovementCommand.Execute(null);
            page.GeneralInfoViewModel.MovementAndSpellPoints.MaxSpellPointsCommand.Execute(null);
            Assert.That(page.ResetCommand.CanExecute(null), Is.True);

            page.ResetCommand.Execute(null);

            Assert.That(page.HasPendingChanges, Is.False);
            Assert.That(this.session.Current, Is.SameAs(this.session.Original));
        }

        [Test]
        public void ValuesBeyondTheLimitsAreClampedByTheGeneralTab()
        {
            using var page = this.Page();
            var general = page.GeneralInfoViewModel;

            general.PrimarySkills.Attack.Base = byte.MaxValue;
            general.Position.X = short.MaxValue;
            general.Destination.Y = short.MinValue;
            general.MovementAndSpellPoints.CurrentSpellPoints = ushort.MaxValue;
            general.MovementAndSpellPoints.CurrentMovement = uint.MaxValue;

            Assert.That(this.session.Current.PrimarySkills.Attack, Is.EqualTo(Constants.Hero.MaxPrimarySkill));
            Assert.That(this.session.Current.Position.X, Is.EqualTo(Constants.Map.MaxCoordinate));
            Assert.That(this.session.Current.Destination.Y, Is.EqualTo(Constants.Map.MinCoordinate));
            Assert.That(this.session.Current.SpellPoints, Is.EqualTo(Constants.Hero.MaxSpellPoints));
            Assert.That(this.session.Current.MovementPoints.Current, Is.EqualTo((uint)Constants.Hero.MaxMovementPoints));
        }

        [Test]
        public void PickingAnOwnerGivesTheHeroToThePlayerThroughTheSession()
        {
            using var page = this.Page();

            page.SelectedOwner = "Red";

            Assert.That(this.session.Current.Owner, Is.EqualTo(PlayerColor.Red));
            Assert.That(page.HasNotice, Is.False);
        }

        [Test]
        public void PickingTheOwnerTheHeroHasAlreadyDoesNothing()
        {
            using var page = this.Page();
            var asked = false;
            this.session.OnUpdate = _ => asked = true;

            page.SelectedOwner = "Blue";

            Assert.That(asked, Is.False);
        }

        [Test]
        public void AnOwnerTheRulesRefuseIsNotPickedAndTheNoticeSaysWhy()
        {
            using var page = this.Page();
            this.session.OnUpdate = _ => throw new GameRuleException("Only the tavern can hold a hero of no player.");

            page.SelectedOwner = "Red";

            Assert.That(page.SelectedOwner, Is.EqualTo("Blue"));
            Assert.That((page.HasNotice, page.Notice), Is.EqualTo((true, "Only the tavern can hold a hero of no player.")));
        }

        [Test]
        public void AFaultOfTheSessionIsNotMadeToLookLikeARuleOfTheGame()
        {
            using var page = this.Page();
            this.session.OnUpdate = _ => throw new InvalidOperationException("broken");

            Assert.That(() => page.SelectedOwner = "Red", Throws.InvalidOperationException.With.Message.EqualTo("broken"));
            Assert.That(page.HasNotice, Is.False);
        }

        [Test]
        public void AHeroInsideACastleNamesItAndOpensIt()
        {
            this.session.GarrisonCastleIndex = 2;
            this.session.CastleNames[2] = "Gryphon Nest";
            using var page = this.Page();

            Assert.That(page.GarrisonCastleName, Is.EqualTo("Gryphon Nest"));
            Assert.That(page.OpenGarrisonCastleCommand.CanExecute(null), Is.True);

            page.OpenGarrisonCastleCommand.Execute(null);

            Assert.That(this.openedCastles, Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void AHeroInNoCastleHasNoCastleToOpen()
        {
            using var page = this.Page();

            Assert.That(page.GarrisonCastleName, Is.Empty);
            Assert.That(page.OpenGarrisonCastleCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void TheSleepBoxIsOnlyOfferedWhereTheSavefileStoresIt()
        {
            using var asleep = this.Page(new FakeHeroSession(HeroModels.Christian()) { SupportsAsleep = true });
            using var never = this.Page(new FakeHeroSession(HeroModels.Christian()) { SupportsAsleep = false, GameVersion = GameVersion.ShadowOfDeath });

            Assert.That((asleep.CanSleep, never.CanSleep), Is.EqualTo((true, false)));
            Assert.That((asleep.IsHota, never.IsHota), Is.EqualTo((true, false)));
        }

        [Test]
        public void ARevisionOfTheHeroShowsItsNewValuesWhileAChangeElsewhereOnlyRefreshesTheCastle()
        {
            using var page = this.Page();
            var changed = new List<string?>();
            page.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            this.session.RaiseHistoryChangedElsewhere();

            Assert.That(changed, Is.EqualTo(new[] { nameof(HeroViewModel.GarrisonCastleName) }));

            this.session.Update(h => h with { SpellPoints = 77 });

            Assert.That(page.GeneralInfoViewModel.MovementAndSpellPoints.CurrentSpellPoints, Is.EqualTo(77));
        }

        [Test]
        public void ADisposedPageNoLongerListensToTheHistory()
        {
            var page = this.Page();
            var changed = 0;
            page.PropertyChanged += (_, _) => changed++;
            page.Dispose();

            this.session.Update(h => h with { SpellPoints = 77 });

            Assert.That(changed, Is.Zero);
        }

        [Test]
        public void TheHexEditorTargetComesFromTheSessionWithTheFactoryOfThePage()
        {
            using var page = this.Page();

            var target = page.CreateByteEditTarget();

            Assert.That(target, Is.SameAs(this.session.Target));
            Assert.That(this.session.FactoryAsked, Is.SameAs(this.byteEditTargets.Object));
        }
    }
}
