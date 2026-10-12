using Siniskalc.Core.Data;
using Siniskalc.Data;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    [TestFixture]
    internal sealed class HeroByteEditTargetTests
    {
        private Game game;
        private int heroIndex;
        private HeroEditSession session;
        private HeroByteEditTarget target;
        private List<ByteEditTargetChangedEventArgs> changes;

        [SetUp]
        public void SetUp()
        {
            var path = Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1");
            this.game = TestServices.GameService.LoadGameFromFile(path);
            this.heroIndex = this.game.History.Current.Heroes().FindByName("Christian");
            this.session = this.game.History.CreateHeroSession(this.heroIndex);
            this.target = new HeroByteEditTarget(this.session, TestServices.HeroRegions);
            this.changes = [];
            this.target.Changed += (_, e) => this.changes.Add(e);
        }

        [TearDown]
        public void TearDown()
        {
            this.target.Dispose();
        }

        [Test]
        public void ExposesTitleBytesAndRegions()
        {
            Assert.That(this.target.Title, Is.EqualTo("Christian"));
            Assert.That(this.target.GetBytes(), Is.EqualTo(this.session.Chunk.ToArray()));
            Assert.That(this.target.GetOriginalBytes(), Is.EqualTo(this.target.GetBytes()));
            Assert.That(this.target.Regions, Is.Not.Empty);
        }

        [Test]
        public void SingleByteEditCanBeUndoneAndRedone()
        {
            var original = this.target.GetBytes();
            var offset = this.session.Offsets[HeroOffsetName.AttributeAtk];

            Assert.That(this.target.WriteBytes(offset, [99]), Is.True);
            Assert.That(this.target.GetBytes()[offset], Is.EqualTo((byte)99));
            Assert.That(this.session.Current.PrimarySkills.Atk, Is.EqualTo((byte)99));

            this.game.History.Undo();
            Assert.That(this.target.GetBytes(), Is.EqualTo(original));
            Assert.That(this.game.History.CanUndo, Is.False);

            this.game.History.Redo();
            Assert.That(this.target.GetBytes()[offset], Is.EqualTo((byte)99));
        }

        [Test]
        public void MultiByteEditIsOneUndoStep()
        {
            var original = this.target.GetBytes();
            var start = this.session.Offsets[HeroOffsetName.Name];

            Assert.That(this.target.WriteBytes(start, "ABCDEF"u8.ToArray()), Is.True);
            Assert.That(this.changes, Has.Count.EqualTo(1));

            this.game.History.Undo();

            Assert.That(this.target.GetBytes(), Is.EqualTo(original));
            Assert.That(this.game.History.CanUndo, Is.False);
        }

        [Test]
        public void EditThatChangesNothingIsNotRecorded()
        {
            var offset = this.session.Offsets[HeroOffsetName.AttributeAtk];

            Assert.That(this.target.WriteBytes(offset, [this.target.GetBytes()[offset]]), Is.False);

            Assert.That(this.game.History.CanUndo, Is.False);
            Assert.That(this.changes, Is.Empty);
        }

        [Test]
        public void OriginalStaysAsLoadedAndDifferencesAreFound()
        {
            var offset = this.session.Offsets[HeroOffsetName.AttributeDef];
            var before = this.target.GetBytes()[offset];

            this.target.WriteBytes(offset, [(byte)(before + 1)]);

            Assert.That(this.target.GetOriginalBytes()[offset], Is.EqualTo(before));
            Assert.That(
                ByteEditTargetExtensions.FindDifferences(this.target.GetOriginalBytes(), this.target.GetBytes()),
                Is.EqualTo(new[] { offset }));

            this.game.History.Undo();
            Assert.That(ByteEditTargetExtensions.FindDifferences(this.target.GetOriginalBytes(), this.target.GetBytes()), Is.Empty);
        }

        [Test]
        public void ChangedIsLocalForOwnEditsAndNotForUndoOrOtherPages()
        {
            this.target.WriteBytes(this.session.Offsets[HeroOffsetName.AttributeAtk], [50]);
            Assert.That(this.changes.Select(c => c.IsLocal), Is.EqualTo(new[] { true }));

            this.session.Update(h => h with { SpellPoints = 4 });
            Assert.That(this.changes.Select(c => c.IsLocal), Is.EqualTo(new[] { true, false }));

            this.game.History.Undo();
            this.game.History.Undo();
            this.game.History.Redo();
            Assert.That(this.changes.Select(c => c.IsLocal), Is.EqualTo(new[] { true, false, false, false, false }));
        }

        [Test]
        public void EditsToOtherHeroesDoNotRaiseChanged()
        {
            var other = this.game.History.Current.Heroes().Select((c, i) => i).First(i => i != this.heroIndex);

            this.game.History.CreateHeroSession(other).Update(h => h with { SpellPoints = 9 });

            Assert.That(this.changes, Is.Empty);
        }

        [Test]
        public void DisposedTargetStopsListening()
        {
            this.target.Dispose();

            this.session.Update(h => h with { SpellPoints = 4 });

            Assert.That(this.changes, Is.Empty);
        }

        [Test]
        public void UpdateRejectsAChangeOfLength()
        {
            Assert.That(() => this.target.Update(bytes => bytes[..10]), Throws.ArgumentException);
            Assert.That(this.game.History.CanUndo, Is.False);
        }
    }
}
