namespace Siniskalc.Homm3.Tests.Runtime
{
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm3.Runtime;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// Heroes of Might and Magic III as a game module.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class Homm3ModuleTests
    {
        [Test]
        public void HeroesOfMightAndMagicIIIAcceptsAnyName()
        {
            IGameModule<Homm3EditorViewModel> module = new Homm3Module(() => TestServices.CreateHomm3Editor());

            Assert.That(module.ValidateSavegameName("a very long name for a savegame.GM1"), Is.Null);
        }
    }
}
