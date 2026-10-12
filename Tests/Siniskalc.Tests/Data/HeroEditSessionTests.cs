using Siniskalc;
using Siniskalc.Core.Data;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;

namespace Siniskalc.Tests.Data
{
    [TestFixture]
    internal sealed class HeroEditSessionTests
    {
        private static readonly HeroOffsets Offsets = new(GameVersion.HornOfTheAbyss);

        [Test]
        public void UpdateWritesAndUndoRestores()
        {
            var history = CreateHistory(1);
            var session = new HeroEditSession(history, 0);
            var before = session.Current.PrimarySkills;

            session.Update(h => h with { PrimarySkills = new HeroPrimarySkills(1, 2, 3, 4) });
            Assert.That(session.Current.PrimarySkills, Is.EqualTo(new HeroPrimarySkills(1, 2, 3, 4)));

            history.Undo();
            Assert.That(session.Current.PrimarySkills, Is.EqualTo(before));
        }

        [Test]
        public void SessionsShareOneHistory()
        {
            var history = CreateHistory(2);
            var first = new HeroEditSession(history, 0);
            var second = new HeroEditSession(history, 1);
            var secondBefore = second.Chunk;
            var changes = 0;
            history.Changed += (_, _) => changes++;

            first.Update(h => h with { SpellPoints = 1 });
            second.Update(h => h with { SpellPoints = 2 });

            Assert.That(changes, Is.EqualTo(2));
            Assert.That(first.Current.SpellPoints, Is.EqualTo((ushort)1));
            Assert.That(second.Current.SpellPoints, Is.EqualTo((ushort)2));

            // Undo is per savefile: it reverts the latest edit, whichever hero it was made to.
            history.Undo();
            Assert.That(changes, Is.EqualTo(3));
            Assert.That(first.Current.SpellPoints, Is.EqualTo((ushort)1));
            Assert.That(second.Chunk, Is.SameAs(secondBefore));
        }

        [Test]
        public void UpdateThatChangesNothingIsNotRecorded()
        {
            var history = CreateHistory(1);
            var session = new HeroEditSession(history, 0);

            session.Update(h => h with { });

            Assert.That(history.CanUndo, Is.False);
        }

        private static SaveDocumentHistory CreateHistory(int heroCount)
        {
            var hero = File.ReadAllBytes(Path.Combine("TestData", "Heroes", "hero_maximus_hota.bin"));
            var save = new byte[hero.Length * heroCount];
            var chunks = new List<HeroChunk>();

            for (var i = 0; i < heroCount; i++)
            {
                hero.CopyTo(save, i * hero.Length);
                chunks.Add(new HeroChunk($"Hero{i}", hero, i * hero.Length));
            }

            return new SaveDocumentHistory(TestServices.DocumentFactory.Create(save, chunks, Offsets));
        }
    }
}
