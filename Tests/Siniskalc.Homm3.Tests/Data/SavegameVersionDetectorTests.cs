using System.Text;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Which game wrote a savegame, told from the first bytes alone: the signature, and the major and minor numbers of the format. The
    /// bytes are made by hand, so every edge of every range is stated.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SavegameVersionDetectorTests
    {
        private readonly SavegameVersionDetector detector = new();

        private static byte[] Header(string signature, int major, int minor, int length = 40)
        {
            var bytes = new byte[length];
            Encoding.Latin1.GetBytes(signature).CopyTo(bytes, 0);
            if (length > SaveHeaderLayout.MinorVersion)
            {
                bytes[SaveHeaderLayout.MajorVersion] = (byte)major;
                bytes[SaveHeaderLayout.MinorVersion] = (byte)minor;
            }

            return bytes;
        }

        [TestCase(16, 0)]
        [TestCase(30, 0)]
        [TestCase(41, 0)]
        public void FormatsFromSixteenToFortyOneWithMinorZeroAreRestorationOfErathia(int major, int minor)
        {
            Assert.That(this.detector.Detect(Header("H3SVG", major, minor)), Is.EqualTo(GameVersion.RestorationOfErathia));
        }

        [Test]
        public void FormatFortyTwoPointOneIsArmageddonsBlade()
        {
            Assert.That(this.detector.Detect(Header("H3SVG", 42, 1)), Is.EqualTo(GameVersion.ArmageddonsBlade));
        }

        [TestCase(42, 0)]
        [TestCase(42, 4)]
        [TestCase(43, 0)]
        [TestCase(43, 4)]
        public void FormatsFortyTwoAndFortyThreeWithAMinorUpToFourAreShadowOfDeathExceptFortyTwoPointOne(int major, int minor)
        {
            Assert.That(this.detector.Detect(Header("H3SVG", major, minor)), Is.EqualTo(GameVersion.ShadowOfDeath));
        }

        [TestCase(44, 5)]
        [TestCase(46, 9)]
        [TestCase(60, 5)]
        public void FormatsFromFortyFourWithAMinorOfFiveOrMoreAreHornOfTheAbyss(int major, int minor)
        {
            Assert.That(this.detector.Detect(Header("H3SVG", major, minor)), Is.EqualTo(GameVersion.HornOfTheAbyss));
        }

        [TestCase(15, 0)]
        [TestCase(41, 1)]
        [TestCase(16, 1)]
        [TestCase(43, 5)]
        [TestCase(44, 4)]
        [TestCase(0, 0)]
        public void ANumberPairBetweenTheKnownRangesIsAnUnknownGame(int major, int minor)
        {
            Assert.That(this.detector.Detect(Header("H3SVG", major, minor)), Is.EqualTo(GameVersion.Unknown));
        }

        [Test]
        public void ACampaignSavegameIsToldByItsNumbersLikeAnyOther()
        {
            Assert.That(this.detector.Detect(Header("H3SVC", 43, 3)), Is.EqualTo(GameVersion.ShadowOfDeath));
        }

        [Test]
        public void TheSignatureOfHeroesChroniclesIsEnoughWhateverFollows()
        {
            Assert.That(this.detector.Detect(Header("HCHRONSVG", 0, 0, 12)), Is.EqualTo(GameVersion.HeroesChronicles));
        }

        [TestCase("H3SVX")]
        [TestCase("h3svg")]
        [TestCase("")]
        public void ASignatureOfNoHeroesGameIsAnUnknownGameWhateverTheNumbersAre(string signature)
        {
            Assert.That(this.detector.Detect(Header(signature, 43, 3)), Is.EqualTo(GameVersion.Unknown));
        }

        [Test]
        public void BytesTooShortToHoldTheNumbersAreAnUnknownGameAndNoError()
        {
            Assert.That(this.detector.Detect(Header("H3SVG", 43, 3, SaveHeaderLayout.MinimumLength - 1)), Is.EqualTo(GameVersion.Unknown));
            Assert.That(this.detector.Detect([]), Is.EqualTo(GameVersion.Unknown));
            Assert.That(this.detector.Detect(Encoding.Latin1.GetBytes("H3")), Is.EqualTo(GameVersion.Unknown));
        }

        [Test]
        public void BytesJustLongEnoughToHoldTheNumbersAreDetected()
        {
            Assert.That(this.detector.Detect(Header("H3SVG", 43, 3, SaveHeaderLayout.MinimumLength)), Is.EqualTo(GameVersion.ShadowOfDeath));
        }
    }
}
