namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The raw bytes of the records of the map that the hex editor works on (Threedom 71: mines, shrines, a sphinx and obelisks).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class ByteEditTargetFactoryTests
    {
        private readonly ByteEditTargetFactory factory = new();
        private SaveDocumentHistory history = null!;

        [SetUp]
        public void Load() => this.history = TestServices.LoadThreedom(71).History;

        [Test]
        public void EveryKnownFieldOfARecordOfTheMapLiesWithinItsBytes()
        {
            var targets = new List<IByteEditTarget>
            {
                this.factory.ForMine(this.history, 0),
                this.factory.ForShrine(this.history, 0),
                this.factory.ForSphinx(this.history, 0),
                this.factory.ForTurn(this.history),
                this.factory.ForBuiltToday(this.history),
                this.factory.ForObelisks(this.history),
            };

            foreach (var target in targets)
            {
                using (target)
                {
                    var length = target.GetBytes().Length;
                    Assert.That(target.Regions, Is.Not.Empty, target.Title);
                    Assert.That(target.Regions.Where(r => r.Start < 0 || r.End > length).Select(r => r.Name), Is.Empty, target.Title);
                }
            }
        }

        [Test]
        public void TheBytesOfAMineAreItsRecordAndAnEditOfTheOwnerByteChangesTheMine()
        {
            var owner = this.history.Current.Mines().GetModel(0).Owner == PlayerColor.Purple ? PlayerColor.Blue : PlayerColor.Purple;
            using (var target = this.factory.ForMine(this.history, 0))
            {
                Assert.That(target.Title, Is.EqualTo(this.history.Current.Mines().GetName(0)));
                Assert.That(target.GetBytes(), Is.EqualTo(this.history.Current.Mines()[0].ToArray()));
                var at = target.Regions.Single(r => r.Name == "Owner").Start;
                Assert.That(target.Update(bytes => { bytes[at] = (byte)owner.Code; return bytes; }), Is.True);
            }

            Assert.That(this.history.Current.Mines().GetModel(0).Owner, Is.EqualTo(owner));
            Assert.That(this.history.GetPendingChanges(), Is.Not.Empty);
        }

        [Test]
        public void TheBytesOfTheObelisksAreTheTableWithAByteForEachObeliskOfTheMap()
        {
            using var target = this.factory.ForObelisks(this.history);

            Assert.That(target.Title, Is.EqualTo("Obelisks"));
            Assert.That(target.GetBytes(), Has.Length.EqualTo(ObeliskLayout.Capacity));
            Assert.That(target.Regions.Single(r => r.Name == "Obelisks").Elements, Has.Count.EqualTo(this.history.Current.Obelisks().GetModel()!.Count), "Threedom has nine");
        }

        [Test]
        public void TheRiddleOfASphinxTakesWhatIsLeftOfItsBlock()
        {
            using var target = this.factory.ForSphinx(this.history, 0);

            Assert.That(target.Regions.Single(r => r.Name == "Riddle").End, Is.EqualTo(target.GetBytes().Length));
        }
    }
}
