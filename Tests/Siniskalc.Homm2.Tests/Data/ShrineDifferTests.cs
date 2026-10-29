namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// What the differ of the shrines of the map lists, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ShrineDifferTests
    {
        private static ShrineModel Shrine() => new(1, 2, new MapPosition(30, 31), Spell.Fireball.Code);

        [Test]
        public void TwoEqualShrinesHaveNoChanges()
        {
            Assert.That(new ShrineDiffer().Diff(0, Shrine(), Shrine()), Is.Empty);
        }

        [Test]
        public void AnotherSpellInAShrineIsOneChangeNamingBothSpells()
        {
            var change = new ShrineDiffer().Diff(1, Shrine(), Shrine().WithSpell(Spell.Fireblast)).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Spell", "Fireball", "Fireblast")));
            Assert.That((change.OwnerName, change.Kind, change.Index), Is.EqualTo((Shrine().Name, Homm2ChunkKinds.Shrines, 1)));
        }

        [Test]
        public void ASpellCodeThatIsNoSpellIsNamedByItsNumber()
        {
            var change = new ShrineDiffer().Diff(1, Shrine(), Shrine() with { SpellCode = 200 }).Single();

            Assert.That(change.Change.After, Is.EqualTo("Spell 200"));
        }
    }
}
