namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the row of one resource: the limits and steps it takes from the game (gold apart from the others), its colour and the
    /// edits it writes to the set of resources.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ResourceRowViewModelTests
    {
        private static readonly ResourceLimits Limits = new(0, 1000, 100, 50, 5, 99, 500, 10, 1000);

        private ResourceSet current = null!;
        private ResourceSet original = null!;

        [SetUp]
        public void StartWithFourHundredGoldAndSevenWood()
        {
            this.current = ResourceSet.None.With(Resource.Gold, 400).With(Resource.Wood, 7);
            this.original = ResourceSet.None.With(Resource.Gold, 300).With(Resource.Wood, 7);
        }

        private ResourceRowViewModel Row(Resource resource, Func<Resource, string>? note = null) =>
            new(resource, Limits, () => this.current, () => this.original, edit => this.current = edit(this.current), note);

        [Test]
        public void GoldHasTheGoldLimitAndStepAndTheOthersHaveTheirOwn()
        {
            var gold = this.Row(Resource.Gold);
            var wood = this.Row(Resource.Wood);

            Assert.That((gold.Minimum, gold.Maximum, gold.Step), Is.EqualTo((0L, 1000L, 50L)));
            Assert.That((wood.Minimum, wood.Maximum, wood.Step), Is.EqualTo((0L, 100L, 5L)));
        }

        [Test]
        public void ARowIsNamedAfterItsResourceAndColouredWithItsColour()
        {
            var row = this.Row(Resource.Mercury);

            Assert.That((row.Name, row.Resource, row.ColorHex), Is.EqualTo(("Mercury", Resource.Mercury, ResourcePalette.Of("Mercury"))));
        }

        [Test]
        public void OnlyTheGoldRowIsGold()
        {
            Assert.That((this.Row(Resource.Gold).IsGold, this.Row(Resource.Wood).IsGold), Is.EqualTo((true, false)));
        }

        [Test]
        public void TheValueIsTheAmountOfThatResourceAndTheChangeIsAgainstTheOriginalSet()
        {
            var gold = this.Row(Resource.Gold);

            Assert.That((gold.Value, gold.IsChanged), Is.EqualTo((400L, true)));
            Assert.That(this.Row(Resource.Wood).IsChanged, Is.False);
        }

        [Test]
        public void SettingTheValueChangesOnlyThatResourceOfTheSetAndKeepsItWithinTheLimits()
        {
            var wood = this.Row(Resource.Wood);

            wood.Value = 500;

            Assert.That(this.current[Resource.Wood], Is.EqualTo(100));
            Assert.That(this.current[Resource.Gold], Is.EqualTo(400));
        }

        [Test]
        public void TheNoteIsAskedForTheResourceOfTheRow()
        {
            var row = this.Row(Resource.Ore, resource => $"note for {resource}");

            Assert.That(row.Note, Is.EqualTo("note for Ore"));
            Assert.That(this.Row(Resource.Ore).Note, Is.Empty);
        }
    }
}
