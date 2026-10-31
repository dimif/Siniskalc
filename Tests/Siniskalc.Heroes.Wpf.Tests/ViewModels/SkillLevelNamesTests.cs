namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the names the pages give to the levels of a skill and how they map to the levels of the game.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SkillLevelNamesTests
    {
        [Test]
        public void TheNamesAreNoneBasicAdvancedAndExpert()
        {
            Assert.That((SkillLevelNames.None, SkillLevelNames.Basic, SkillLevelNames.Advanced, SkillLevelNames.Expert), Is.EqualTo(("None", "Basic", "Advanced", "Expert")));
        }

        [Test]
        public void TheListsAreInOrderOfLevelWithAndWithoutNone()
        {
            Assert.That(SkillLevelNames.WithNone, Is.EqualTo(new[] { "None", "Basic", "Advanced", "Expert" }));
            Assert.That(SkillLevelNames.WithoutNone, Is.EqualTo(new[] { "Basic", "Advanced", "Expert" }));
        }

        [TestCase(0, "None")]
        [TestCase(1, "Basic")]
        [TestCase(2, "Advanced")]
        [TestCase(3, "Expert")]
        public void ALevelOfTheGameHasItsName(int code, string name)
        {
            Assert.That(SkillLevelNames.FromLevel(SkillLevel.Of((byte)code)), Is.EqualTo(name));
        }

        [Test]
        public void ALevelThatIsNoLevelIsNamedNone()
        {
            Assert.That(SkillLevelNames.FromLevel(SkillLevel.Of(200)), Is.EqualTo("None"));
        }

        [TestCase("Basic", 1)]
        [TestCase("Advanced", 2)]
        [TestCase("Expert", 3)]
        [TestCase("None", 0)]
        [TestCase("anything else", 0)]
        [TestCase("", 0)]
        public void ANameMapsBackToItsLevelAndNumber(string name, int code)
        {
            Assert.That(SkillLevelNames.ToLevel(name).Code, Is.EqualTo(code));
            Assert.That(SkillLevelNames.ToNumber(name), Is.EqualTo(code));
        }

        [Test]
        public void TheNamesAreCaseSensitive()
        {
            Assert.That(SkillLevelNames.ToNumber("expert"), Is.Zero);
        }
    }
}
