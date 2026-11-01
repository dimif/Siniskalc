using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Runtime;

using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// Tests that an edit session knows whether it is edited and that its revision changes with every edit.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class SessionRevisionTests
    {
        private static Game Load(params string[] path) => SavedGames.Get(Path.Combine(["TestData", "Hota", .. path]));

        [Test]
        public void ASessionKnowsWhetherItIsEditedAndItsRevisionChangesWithEveryEdit()
        {
            var game = Load("empire_of_the_world_01_m5w4d6.GM1");
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));
            var other = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Maximus"));
            var revision = session.Revision;

            Assert.That(session.IsDirty, Is.False);

            other.Update(h => h with { SpellPoints = (ushort)(h.SpellPoints + 1) });
            Assert.That(session.Revision, Is.SameAs(revision), "another hero's edit leaves the revision alone");

            session.Update(h => h with { SpellPoints = (ushort)(h.SpellPoints + 1) });
            Assert.That(session.IsDirty, Is.True);
            Assert.That(session.Revision, Is.Not.SameAs(revision));

            game.History.Undo();
            Assert.That(session.IsDirty, Is.False);
        }
    }
}
