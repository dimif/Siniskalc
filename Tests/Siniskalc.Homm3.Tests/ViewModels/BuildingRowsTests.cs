using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.ViewModels
{
    /// <summary>
    /// Tests the row of one building of a castle and the group that sums up a set of rows.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class BuildingRowsTests
    {
        private static readonly IReadOnlySet<Building> None = new HashSet<Building>();

        [Test]
        public void AGroupSumsUpHowManyOfItsRowsAreBuilt()
        {
            var rows = new[]
            {
                new BuildingRowViewModel(Building.Tavern, true, None),
                new BuildingRowViewModel(Building.Fort, false, None),
                new BuildingRowViewModel(Building.MagesGuild1, true, None),
            };

            var group = new BuildingGroupViewModel("Town", rows);

            Assert.That((group.Title, group.Summary), Is.EqualTo(("Town", "2 of 3 built")));
            Assert.That(group.Rows, Is.EqualTo(rows));
        }

        [Test]
        public void ARowShowsTheNameOfTheBuildingAndWhetherItIsBuilt()
        {
            var row = new BuildingRowViewModel(Building.Tavern, true, None);

            Assert.That((row.Name, row.IsBuilt, row.IsChecked, row.IsDisabled, row.IsReplaced), Is.EqualTo(("Tavern", true, true, false, false)));
        }

        [Test]
        public void ARowCanBeToggledOnlyWithAnActionAndNoBlockingReason()
        {
            Assert.That(new BuildingRowViewModel(Building.Tavern, true, None, setBuilt: _ => { }).CanToggle, Is.True);
            Assert.That(new BuildingRowViewModel(Building.Tavern, true, None).CanToggle, Is.False);
            Assert.That(new BuildingRowViewModel(Building.Tavern, true, None, setBuilt: _ => { }, blockedReason: "No").CanToggle, Is.False);
        }

        [Test]
        public void TickingTheBoxAsksForTheBuildingToBeBuiltOrRemoved()
        {
            var asked = new List<bool>();
            var row = new BuildingRowViewModel(Building.Tavern, false, None, setBuilt: asked.Add);

            row.IsChecked = true;
            row.IsChecked = false;

            Assert.That(asked, Is.EqualTo(new[] { true, false }));
        }

        [Test]
        public void TheToolTipIsTheReasonItIsBlockedOtherwiseWhatTheBoxDoesAndWhatIsMissing()
        {
            var blocked = new BuildingRowViewModel(Building.Tavern, false, None, setBuilt: _ => { }, blockedReason: "Needs a hero");
            var build = new BuildingRowViewModel(Building.Tavern, false, None, setBuilt: _ => { });
            var remove = new BuildingRowViewModel(Building.Tavern, true, None, setBuilt: _ => { });
            var needs = new BuildingRowViewModel(Building.RampartDwarfCottage, false, None, setBuilt: _ => { });

            Assert.That(blocked.ToggleToolTip, Is.EqualTo("Needs a hero"));
            Assert.That(build.ToggleToolTip, Is.EqualTo("Build this building"));
            Assert.That(remove.ToggleToolTip, Is.EqualTo("Remove this building"));
            Assert.That(needs.ToggleToolTip, Does.StartWith("Build this building (not built yet: ").And.Contain(Building.RampartCentaurStables.Name));
        }

        [Test]
        public void AForbiddenBuildingThatIsNotBuiltIsDisabledWithANote()
        {
            var forbidden = new BuildingRowViewModel(Building.Tavern, false, None, isEnabled: false);
            var forbiddenButBuilt = new BuildingRowViewModel(Building.Tavern, true, None, isEnabled: false);

            Assert.That((forbidden.IsDisabled, forbidden.Note), Is.EqualTo((true, "Not allowed by the map")));
            Assert.That((forbiddenButBuilt.IsDisabled, forbiddenButBuilt.Note), Is.EqualTo((false, string.Empty)));
        }

        [Test]
        public void ABuildingBuiltWithoutWhatItRequiresCarriesAWarning()
        {
            var warned = new BuildingRowViewModel(Building.RampartDwarfCottage, true, None);
            var fine = new BuildingRowViewModel(Building.RampartDwarfCottage, true, new HashSet<Building> { Building.RampartCentaurStables });
            var notBuilt = new BuildingRowViewModel(Building.RampartDwarfCottage, false, None);

            Assert.That(warned.Warning, Does.StartWith("Built without ").And.Contain(Building.RampartCentaurStables.Name));
            Assert.That(fine.Warning, Is.Empty);
            Assert.That(notBuilt.Warning, Is.Empty);
            Assert.That(warned.Requirements, Does.StartWith("Requires "));
            Assert.That(new BuildingRowViewModel(Building.VillageHall, true, None).Requirements, Is.Empty);
        }

        [Test]
        public void ADwellingNamesItsCreaturesAndTheCountWaitingIsShownWithThousandsSeparators()
        {
            var row = new BuildingRowViewModel(Building.RampartCentaurStables, true, None, creaturesAvailable: 12345);
            var none = new BuildingRowViewModel(Building.Tavern, true, None);

            Assert.That(row.Creatures, Is.EqualTo("Centaur"));
            Assert.That(row.Available, Is.EqualTo($"{12345:N0} available"));
            Assert.That((none.Creatures, none.Available), Is.EqualTo((string.Empty, string.Empty)));
        }

        [Test]
        public void OnlyABuiltBuildingCanBeShownAsReplacedByItsUpgrade()
        {
            Assert.That(new BuildingRowViewModel(Building.RampartCentaurStables, true, None, isReplaced: true).IsReplaced, Is.True);
            Assert.That(new BuildingRowViewModel(Building.RampartCentaurStables, false, None, isReplaced: true).IsReplaced, Is.False);
        }

        [Test]
        public void TheAllowButtonForbidsOrAllowsAndCanBeUsedOnlyWithAnActionAndNoReason()
        {
            var asked = new List<bool>();
            var allowed = new BuildingRowViewModel(Building.Tavern, true, None, isEnabled: true, setAllowed: asked.Add);
            var forbidden = new BuildingRowViewModel(Building.Tavern, true, None, isEnabled: false, setAllowed: asked.Add);

            allowed.ToggleAllowedCommand.Execute(null);
            forbidden.ToggleAllowedCommand.Execute(null);

            Assert.That(asked, Is.EqualTo(new[] { false, true }));
            Assert.That((allowed.AllowText, forbidden.AllowText), Is.EqualTo(("Forbid", "Allow")));
            Assert.That(allowed.AllowToolTip, Does.StartWith("Forbid"));
            Assert.That(forbidden.AllowToolTip, Does.StartWith("Allow"));
            Assert.That(allowed.CanSetAllowed, Is.True);
            Assert.That(new BuildingRowViewModel(Building.Tavern, true, None, setAllowed: _ => { }, allowBlockedReason: "No").CanSetAllowed, Is.False);
            Assert.That(new BuildingRowViewModel(Building.Tavern, true, None).CanSetAllowed, Is.False);
        }

        [Test]
        public void AChildRowIsMarkedAsOne()
        {
            Assert.That(new BuildingRowViewModel(Building.Tavern, true, None, isChild: true).IsChild, Is.True);
            Assert.That(new BuildingRowViewModel(Building.Tavern, true, None).IsChild, Is.False);
        }
    }
}
