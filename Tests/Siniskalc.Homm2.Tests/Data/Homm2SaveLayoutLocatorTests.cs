namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    [TestFixture]
    internal sealed class Homm2SaveLayoutLocatorTests
    {
        private readonly Homm2SaveLayoutLocator locator = new();

        [Test]
        public void EverySavegameOfBrokenAllianceIsOfTheBaseGameWithItsTablesWhereTheyAreExpected()
        {
            var saves = Homm2Saves.AllOfBrokenAlliance().ToList();
            Assert.That(saves, Has.Count.EqualTo(24));

            foreach (var save in saves)
            {
                var layout = this.locator.Locate(Homm2Saves.Read(save));

                Assert.That(layout, Is.Not.Null, save);
                Assert.That(layout!.Version, Is.EqualTo(Homm2GameVersion.BaseGame), save);
                Assert.That((layout.Width, layout.Height), Is.EqualTo((72, 72)), save);
                Assert.That(layout.HeroStride, Is.EqualTo(236), save);
                Assert.That(layout.DateStart, Is.EqualTo(0x2BB), save);
                Assert.That(layout.PlayerTableStart, Is.EqualTo(0x2C0), save);
                Assert.That(layout.HeroTableStart, Is.EqualTo(0x79C), save);
                Assert.That(layout.CastleTableStart, Is.EqualTo(0x399A), save);
            }
        }

        [Test]
        public void ThePriceOfLoyaltySavegameIsFoundFiveBytesFurtherWithBiggerHeroes()
        {
            var layout = this.locator.Locate(Homm2Saves.Read(Homm2Saves.Path(Homm2Saves.ArraxTheJerk, "arrax_the_jerk_01_start")));

            Assert.That(layout, Is.Not.Null);
            Assert.That(layout!.Version, Is.EqualTo(Homm2GameVersion.PriceOfLoyalty));
            Assert.That((layout.Width, layout.Height), Is.EqualTo((36, 36)));
            Assert.That(layout.HeroStride, Is.EqualTo(250));
            Assert.That(layout.DateStart, Is.EqualTo(0x2BB + 5));
            Assert.That(layout.PlayerTableStart, Is.EqualTo(0x2C0 + 5));
            Assert.That(layout.HeroTableStart, Is.EqualTo(0x79C + 5));
            Assert.That(layout.CastleTableStart, Is.EqualTo(0x3C93));
        }

        [Test]
        public void EverySavegameIsRecognisedFromItsFirstBytesAlone()
        {
            foreach (var save in Homm2Saves.All())
            {
                var head = Homm2Saves.Read(save).AsSpan(0, this.locator.HeadLength).ToArray();

                Assert.That(this.locator.Recognizes(head), Is.True, save);
            }
        }

        [Test]
        public void ADifferentFileIsNotASavegame()
        {
            var gzip = new byte[] { 0x1F, 0x8B, 0x08, 0x00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            Assert.That(this.locator.Recognizes(gzip), Is.False);
            Assert.That(this.locator.Recognizes(new byte[3]), Is.False);
            Assert.That(this.locator.Locate(gzip), Is.Null);
            Assert.That(this.locator.Locate(new byte[200_000]), Is.Null);
        }

        [Test]
        public void ASavegameWhoseTablesAreNotWhereTheyAreExpectedIsRefused()
        {
            var bytes = Homm2Saves.Read(Homm2Saves.BrokenAllianceSave(1));
            bytes[0x79C + Homm2HeroLayout.Id] = 99;

            Assert.That(this.locator.Locate(bytes), Is.Null);
        }

        [Test]
        public void ASavegameCutShortIsRefused()
        {
            var bytes = Homm2Saves.Read(Homm2Saves.BrokenAllianceSave(1));

            Assert.That(this.locator.Locate(bytes[..0x3000]), Is.Null);
        }
    }
}
