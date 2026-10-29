namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the row of one level of a Mage Guild of HoMM2.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class GuildLevelViewModelTests
    {
        [Test]
        public void AGuildLevelHasAHeadingAndKeepsItsSpellFields()
        {
            var fields = new[] { new ChoiceFieldViewModel("Spell 1", ["a"], () => "a", _ => { }) };

            var level = new GuildLevelViewModel(3, fields);

            Assert.That((level.Level, level.Heading), Is.EqualTo((3, "Level 3")));
            Assert.That(level.Spells, Is.EqualTo(fields));
        }

        [Test]
        public void RefreshingALevelRefreshesEveryField()
        {
            var shown = new[] { "a", "a" };
            var first = new ChoiceFieldViewModel("Spell 1", ["a", "b"], () => shown[0], _ => { });
            var second = new ChoiceFieldViewModel("Spell 2", ["a", "b"], () => shown[1], _ => { });
            var raised = new List<string>();
            first.PropertyChanged += (_, _) => raised.Add("first");
            second.PropertyChanged += (_, _) => raised.Add("second");
            var level = new GuildLevelViewModel(1, [first, second]);

            shown[0] = "b";
            shown[1] = "b";
            level.Refresh();

            Assert.That(raised, Is.EquivalentTo(new[] { "first", "second" }));
            Assert.That((first.Selected, second.Selected), Is.EqualTo(("b", "b")));
        }
    }
}
