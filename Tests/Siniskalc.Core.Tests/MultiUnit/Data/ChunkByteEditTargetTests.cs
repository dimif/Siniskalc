namespace Siniskalc.Core.Tests.MultiUnit.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Tests.Data;

    /// <summary>
    /// Tests the target the hex editor works on for one chunk of a savefile: its bytes and the bytes the savefile had, the edits it
    /// makes through the history, and when it tells that the chunk changed.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class ChunkByteEditTargetTests
    {
        private SaveDocumentHistory history = null!;
        private EditSession<RawBytesChunk, TestModel> session = null!;
        private List<ByteRegion> regions = null!;
        private List<ByteEditTargetChangedEventArgs> changes = null!;

        [SetUp]
        public void StartWithTwoChunks()
        {
            var collections = new Dictionary<ChunkKind, IChunkCollection>
            {
                [TestChunkKinds.Unit] = new TestModelCollection([new RawBytesChunk("first", [1, 2, 3], 0), new RawBytesChunk("second", [4, 5, 6], 3)]),
            };
            this.history = new SaveDocumentHistory(new SaveDocument(new byte[6], collections, new SavefileBytesPatcher()));
            this.session = new EditSession<RawBytesChunk, TestModel>(this.history, TestChunkKinds.Unit, 0);
            this.regions = [new ByteRegion("Value", 0, 1, ByteRegionKind.Other)];
            this.changes = [];
        }

        private ChunkByteEditTarget<RawBytesChunk, TestModel> Target(Func<string>? title = null)
        {
            var target = new ChunkByteEditTarget<RawBytesChunk, TestModel>(this.session, this.regions, title);
            target.Changed += (_, e) => this.changes.Add(e);
            return target;
        }

        [Test]
        public void TheTitleIsTheNameOfTheChunkUnlessAnotherIsGiven()
        {
            using var plain = this.Target();
            using var named = this.Target(() => "Hero Orrin");

            Assert.That(plain.Title, Is.EqualTo("first"));
            Assert.That(named.Title, Is.EqualTo("Hero Orrin"));
        }

        [Test]
        public void TheTitleIsAskedAgainEachTimeSoItFollowsARename()
        {
            var name = "one";
            using var target = this.Target(() => name);

            name = "two";

            Assert.That(target.Title, Is.EqualTo("two"));
        }

        [Test]
        public void TheRegionsAreTheOnesGiven()
        {
            using var target = this.Target();

            Assert.That(target.Regions, Is.SameAs(this.regions));
        }

        [Test]
        public void TheBytesAreThoseOfTheChunkNowAndTheOriginalBytesThoseTheSavefileHad()
        {
            using var target = this.Target();

            target.Update(bytes =>
            {
                bytes[1] = 9;
                return bytes;
            });

            Assert.That(target.GetBytes(), Is.EqualTo(new byte[] { 1, 9, 3 }));
            Assert.That(target.GetOriginalBytes(), Is.EqualTo(new byte[] { 1, 2, 3 }));
        }

        [Test]
        public void TheBytesGivenAreCopiesThatDoNotChangeTheChunk()
        {
            using var target = this.Target();

            target.GetBytes()[0] = 9;
            target.GetOriginalBytes()[0] = 9;

            Assert.That(target.GetBytes(), Is.EqualTo(new byte[] { 1, 2, 3 }));
            Assert.That(target.GetOriginalBytes(), Is.EqualTo(new byte[] { 1, 2, 3 }));
        }

        [Test]
        public void AnEditThatChangesBytesIsReportedAsDoneAndWentIntoTheHistory()
        {
            using var target = this.Target();

            var changed = target.Update(bytes =>
            {
                bytes[0] = 7;
                return bytes;
            });

            Assert.That(changed, Is.True);
            Assert.That(this.history.CanUndo, Is.True);
            Assert.That(this.history.Current.GetChunks<RawBytesChunk>(TestChunkKinds.Unit)[0].GetByteAt(0), Is.EqualTo(7));
        }

        [Test]
        public void AnEditThatLeavesTheBytesAsTheyWereIsReportedAsNothingDoneAndLeavesNoStepToUndo()
        {
            using var target = this.Target();

            var changed = target.Update(bytes => bytes);

            Assert.That(changed, Is.False);
            Assert.That(this.history.CanUndo, Is.False);
            Assert.That(this.changes, Is.Empty);
        }

        [Test]
        public void AnEditOfTheTargetItselfTellsThatItChangedAndThatItWasLocal()
        {
            using var target = this.Target();

            target.Update(bytes =>
            {
                bytes[0] = 7;
                return bytes;
            });

            Assert.That(this.changes.Select(c => c.IsLocal), Is.EqualTo(new[] { true }));
        }

        [Test]
        public void AnUndoTellsThatTheChunkChangedAndThatItWasNotLocal()
        {
            using var target = this.Target();
            target.Update(bytes =>
            {
                bytes[0] = 7;
                return bytes;
            });
            this.changes.Clear();

            this.history.Undo();

            Assert.That(this.changes.Select(c => c.IsLocal), Is.EqualTo(new[] { false }));
            Assert.That(target.GetBytes(), Is.EqualTo(new byte[] { 1, 2, 3 }));
        }

        [Test]
        public void AnEditOfAnotherChunkOfTheSameHistoryTellsNothing()
        {
            using var target = this.Target();
            var other = new EditSession<RawBytesChunk, TestModel>(this.history, TestChunkKinds.Unit, 1);

            other.Update(m => m with { Value = 99 });

            Assert.That(this.changes, Is.Empty);
        }

        [Test]
        public void SavingMovesTheBaselineSoItTellsThatTheChunkChangedAgain()
        {
            using var target = this.Target();
            target.Update(bytes =>
            {
                bytes[0] = 7;
                return bytes;
            });
            this.changes.Clear();

            this.history.MarkSaved();

            Assert.That(this.changes, Has.Count.EqualTo(1));
            Assert.That(target.GetOriginalBytes(), Is.EqualTo(new byte[] { 7, 2, 3 }));
        }

        [Test]
        public void ADisposedTargetNoLongerListensToTheHistory()
        {
            var target = this.Target();
            target.Update(bytes =>
            {
                bytes[0] = 7;
                return bytes;
            });
            this.changes.Clear();

            target.Dispose();
            this.history.Undo();

            Assert.That(this.changes, Is.Empty);
        }

        [Test]
        public void DisposingTwiceDoesNothingTheSecondTime()
        {
            var target = this.Target();

            target.Dispose();

            Assert.That(() => target.Dispose(), Throws.Nothing);
        }
    }
}
