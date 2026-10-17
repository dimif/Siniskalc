namespace Siniskalc.Heroes.Wpf.Tests.Views
{
    using System.Windows.Controls;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Heroes.Wpf.Views;

    /// <summary>
    /// Tests what the cards of a hero show: a card whose template did not apply is empty and raises no binding error.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class HeroCardsTests
    {
        [OneTimeSetUp]
        public void MakeTheApplication() => WpfTestApplication.Ensure();

        [Test]
        public void TheMovementCardShowsBothStepperGroupsAndWhatTheGamePutsUnderTheMovement()
        {
            var card = new MovementAndSpellPointsCard
            {
                DataContext = new MovementAndSpellPointsCardViewModel(() => 700, _ => { }, () => 1500, new NumberRange(0, 99_999, 100), () => 12, _ => { }, new NumberRange(0, 999)),
                ExtraContent = new TextBlock { Text = "Moved today" },
            };

            using var hosted = WpfTestApplication.Show(card);

            var texts = WpfTestApplication.Descendants<TextBlock>(card).Select(t => t.Text).ToList();
            Assert.That(texts, Is.SupersetOf(new[] { "Movement & Spell points", "Movement", "Spell", "Maximum 1500", "Moved today" }));
        }

        [Test]
        public void TheLevelCardHidesTheGenderRowOfAGameThatHasNone()
        {
            var card = new LevelAndClassCard
            {
                DataContext = new LevelAndClassCardViewModel(() => 4, _ => { }, new NumberRange(1, 75), () => 100, _ => { }, new NumberRange(0, 1_000_000), () => "900 XP to level 5", ["Knight"], () => "Knight"),
            };

            using var hosted = WpfTestApplication.Show(card);

            var visible = WpfTestApplication.Descendants<TextBlock>(card).Where(t => t.IsVisible).Select(t => t.Text).ToList();
            Assert.That(visible, Is.SupersetOf(new[] { "Level and class", "Level", "Experience", "Class", "900 XP to level 5" }));
            Assert.That(visible, Does.Not.Contain("Gender"));
        }

        [Test]
        public void TheMapPointCardShowsTheHeadingAndOnlyTheRowsTheGameGaveItWhatItNeeds()
        {
            var facing = FacingDirection.North;
            var card = new MapPointCard
            {
                DataContext = new MapPointCardViewModel("Position", () => 3, _ => { }, () => 4, _ => { }, new NumberRange(0, 143), getFacing: () => facing, setFacing: v => facing = v),
            };

            using var hosted = WpfTestApplication.Show(card);

            var visible = WpfTestApplication.Descendants<TextBlock>(card).Where(t => t.IsVisible).Select(t => t.Text).ToList();
            Assert.That(visible, Is.SupersetOf(new[] { "Position", "X", "Y", "Facing" }));
            Assert.That(visible, Does.Not.Contain("Level (Z)"));
            Assert.That(WpfTestApplication.Descendants<Button>(card).Where(b => b.IsVisible && (string?)b.Content == "Remove"), Is.Empty);
        }
    }
}
