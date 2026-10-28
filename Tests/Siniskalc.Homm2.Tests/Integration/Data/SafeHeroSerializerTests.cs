namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// How a hero is written back: what the editor cannot hold is left as the savegame has it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class SafeHeroSerializerTests
    {
        private const int Hero = 7;

        [Test]
        public void ACoordinateTooBigForTheModelIsKeptWhenAnotherFieldOfTheHeroIsEdited()
        {
            var bytes = (byte[])TestServices.LoadBrokenAlliance(1).Bytes.Clone();
            var layout = TestServices.Locator.Locate(bytes)!;
            var x = layout.HeroStart(Hero) + HeroLayout.X;
            BitConverter.GetBytes(0xFFFF_FFF0u).CopyTo(bytes, x);
            var game = TestServices.Parser.Parse(bytes, null);

            game.History.CreateHeroSession(Hero).Update(h => h with { SpellPoints = h.SpellPoints + 1 });

            var saved = game.GetBytesToSave();
            Assert.That(BitConverter.ToUInt32(saved, x), Is.EqualTo(0xFFFF_FFF0u));
        }

        [Test]
        public void ACoordinateTheModelHoldsIsWrittenAsItIs()
        {
            var game = TestServices.LoadBrokenAlliance(1);

            game.History.CreateHeroSession(Hero).Update(h => h with { Position = h.Position with { X = 12, Y = 34 } });

            var saved = game.GetBytesToSave();
            var start = game.Layout.HeroStart(Hero);
            Assert.That(BitConverter.ToUInt32(saved, start + HeroLayout.X), Is.EqualTo(12));
            Assert.That(BitConverter.ToUInt32(saved, start + HeroLayout.Y), Is.EqualTo(34));
        }
    }
}
