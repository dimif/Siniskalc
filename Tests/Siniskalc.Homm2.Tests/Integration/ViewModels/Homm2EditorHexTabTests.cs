namespace Siniskalc.Homm2.Tests.Integration.ViewModels
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// Tests the hex tab of the editor, on a Broken Alliance savegame loaded into the editor.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class Homm2EditorHexTabTests
    {
        private Homm2EditorHarness harness = null!;
        private Homm2EditorViewModel editor = null!;

        [SetUp]
        public void LoadASavegame()
        {
            this.harness = new Homm2EditorHarness();
            this.editor = this.harness.Editor;
        }

        [TearDown]
        public void DeleteFolder() => this.harness.Dispose();

        [Test]
        public void TheHexTabEditsTheRawBytesOfAHeroAPlayerAndACastle()
        {
            this.editor.ShowHero(7);
            var hero = (HeroViewModel)this.editor.CurrentPage!;
            using (var target = hero.CreateByteEditTarget()!)
            {
                Assert.That(target.Title, Is.EqualTo("Maximus"));
                Assert.That(target.Regions.Select(r => r.Name), Does.Contain("Experience").And.Contain("Artifacts"));
                var level = target.Regions.Single(r => r.Name == "Level").Start;
                Assert.That(target.Update(bytes => { bytes[level] = 33; return bytes; }), Is.True);
            }

            Assert.That(this.editor.CurrentGame!.History.Current.Heroes().GetModel(7).HeroLevel.Level, Is.EqualTo(33));
            Assert.That(hero.HasPendingChanges, Is.True);

            this.editor.ShowPlayer(PlayerColor.Blue);
            using (var target = ((PlayerViewModel)this.editor.CurrentPage!).CreateByteEditTarget()!)
            {
                Assert.That(target.Title, Is.EqualTo("Blue player"));
                Assert.That(target.Regions.Select(r => r.Name), Does.Contain("Resources"));
            }

            var castle = this.harness.OpenCastleOf(PlayerColor.Blue);
            using (var target = castle.CreateByteEditTarget()!)
            {
                Assert.That(target.Title, Is.EqualTo(castle.Name));
                Assert.That(target.Regions.Select(r => r.Name), Does.Contain("Dwellings").And.Contain("Town buildings"));
            }
        }
    }
}
