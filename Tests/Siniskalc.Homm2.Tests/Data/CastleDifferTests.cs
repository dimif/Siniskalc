namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// What the differ of a castle lists for two versions of the record: buildings, dwellings with the creatures that wait in them, the
    /// Mage Guild and the garrison, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class CastleDifferTests
    {
        private PendingChangesFake fake = null!;

        [SetUp]
        public void StartWithNoReverts() => this.fake = new();

        private IReadOnlyList<PendingChange> Diff(CastleModel original, CastleModel current) => new CastleDiffer(this.fake.Factory, TestCatalog.Standard).Diff(3, original, current);

        [Test]
        public void TwoEqualCastlesHaveNoChanges()
        {
            Assert.That(this.Diff(DifferSamples.Castle(), DifferSamples.Castle()), Is.Empty);
        }

        [Test]
        public void BuildingAndUpgradingADwellingIsOneChangeThatIsPutBackAsAWhole()
        {
            var original = DifferSamples.Castle();
            var current = original.WithDwellingLevel(2, 1).WithCreaturesWaiting(2, 5).WithDwellingLevel(2, 2);

            var change = this.Diff(original, current).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Dwelling 2", "not built", "upgraded, 5 waiting")));
            Assert.That(this.fake.CastleReverts["Dwelling 2"](current), Is.EqualTo(original));
        }

        [Test]
        public void ChangingTheCreaturesThatWaitInABuiltDwellingIsAChangeOfThatDwelling()
        {
            var original = DifferSamples.Castle().WithDwellingLevel(1, 1);
            var current = original.WithCreaturesWaiting(1, 99);

            var change = this.Diff(original, current).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Dwelling 1", "built, 0 waiting", "built, 99 waiting")));
        }

        [Test]
        public void BuildingTheMageGuildIsOneChangeThatIsPutBackAsAWhole()
        {
            var original = DifferSamples.Castle();
            var current = original.WithMageGuildLevel(2);

            var change = this.Diff(original, current).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Mage Guild", "none", "level 2, spells shown 3/3/0/0/0")));
            Assert.That(this.fake.CastleReverts["Mage Guild"](current), Is.EqualTo(original));
        }

        [Test]
        public void ADifferentSpellInAPlaceOfTheMageGuildIsAChangeOfThatPlaceAndPutBack()
        {
            var original = DifferSamples.Castle().WithMageGuildLevel(2);
            var current = original.WithMageGuildSpell(2, 1, TestCatalog.Standard.Spells["Fireball"]);

            var change = this.Diff(original, current).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Level 2, spell 2", "none", "Fireball")));
            Assert.That(this.fake.CastleReverts["Level 2, spell 2"](current), Is.EqualTo(original));
        }

        [Test]
        public void ABuildingThatIsNotADwellingIsListedByNameAndPutBackToWhatItWas()
        {
            var original = DifferSamples.Castle();
            var current = original.WithBuilding(TownBuildings.Well, true);

            var change = this.Diff(original, current).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Well", "not built", "built")));
            Assert.That(this.fake.CastleReverts["Well"](current).Town.HasFlag(TownBuildings.Well), Is.False);
        }

        [Test]
        public void AnotherOwnerIsListedFirstAndIsPutBackLikeAnyOtherChange()
        {
            var original = DifferSamples.Castle();
            var current = (original with { Owner = PlayerColor.Green }).WithMageGuildLevel(1);

            var changes = this.Diff(original, current);

            Assert.That(changes.Select(c => c.Change.Description), Is.EqualTo(new[] { "Owner", "Mage Guild" }));
            Assert.That((changes[0].Change.Before, changes[0].Change.After, changes[0].Kind, changes[0].Index), Is.EqualTo(("Blue", "Green", Homm2ChunkKinds.Castle, 3)));
            Assert.That(this.fake.CastleReverts["Owner"](current).Owner, Is.EqualTo(PlayerColor.Blue));
        }

        [Test]
        public void AChangedGarrisonStackIsAChangeOfItsSlot()
        {
            var original = DifferSamples.Castle();
            var current = original with { Garrison = original.Garrison.WithSlot(0, TestCatalog.Standard.Creatures["Peasant"], 30) };

            var change = this.Diff(original, current).Single();

            Assert.That(change.Change.Description, Is.EqualTo("Slot 1"));
            Assert.That(this.fake.CastleReverts["Slot 1"](current).Garrison.Slots[0], Is.Null);
        }
    }
}
