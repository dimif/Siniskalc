using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.ViewModels
{
    /// <summary>
    /// Tests the spell slot of a Mage Guild and the row of a level that holds its slots.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MageGuildRowsTests
    {
        [Test]
        public void ASlotShowsTheSpellItIsAskedFor()
        {
            var spell = Spell.Haste;
            var slot = new MageGuildSpellSlotViewModel(() => spell, [Spell.Haste, Spell.Bless], _ => { });

            Assert.That(slot.Spell, Is.SameAs(Spell.Haste));
            Assert.That(slot.Choices, Is.EqualTo(new[] { Spell.Haste, Spell.Bless }));

            spell = Spell.Bless;
            Assert.That(slot.Spell, Is.SameAs(Spell.Bless));
        }

        [Test]
        public void PickingAnotherSpellReportsItAndPickingTheSameOrNothingDoesNot()
        {
            var picked = new List<Spell>();
            var slot = new MageGuildSpellSlotViewModel(() => Spell.Haste, [Spell.Haste, Spell.Bless], picked.Add);

            slot.Spell = Spell.Bless;
            slot.Spell = Spell.Haste;
            slot.Spell = null!;

            Assert.That(picked, Is.EqualTo(new[] { Spell.Bless }));
        }

        [Test]
        public void RefreshTellsEveryBindingOfTheSlot()
        {
            var slot = new MageGuildSpellSlotViewModel(() => Spell.Haste, [], _ => { });
            var raised = new List<string?>();
            slot.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            slot.Refresh();

            Assert.That(raised, Is.EqualTo(new[] { string.Empty }));
        }

        [Test]
        public void ABuiltLevelCountsItsSpellsAndAnUnbuiltOneSaysSo()
        {
            var built = new MageGuildLevelRowViewModel(2, true, 4, []);
            var notBuilt = new MageGuildLevelRowViewModel(3, false, 4, []);

            Assert.That((built.Level, built.IsBuilt, built.SpellsText), Is.EqualTo((2, true, "4 spells")));
            Assert.That((notBuilt.IsBuilt, notBuilt.SpellsText), Is.EqualTo((false, "Not built")));
        }

        [Test]
        public void UpdatingARowChangesItsStateRefreshesItsSlotsAndTellsTheBindings()
        {
            var slotRaised = 0;
            var slot = new MageGuildSpellSlotViewModel(() => Spell.Haste, [], _ => { });
            slot.PropertyChanged += (_, _) => slotRaised++;
            var row = new MageGuildLevelRowViewModel(1, false, 0, [slot]);
            var raised = new List<string?>();
            row.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
            slotRaised = 0;

            row.Update(true, 5);

            Assert.That((row.IsBuilt, row.SpellsText), Is.EqualTo((true, "5 spells")));
            Assert.That(slotRaised, Is.EqualTo(1));
            Assert.That(raised, Is.Not.Empty);
            Assert.That(row.Slots, Has.Count.EqualTo(1));
        }
    }
}
