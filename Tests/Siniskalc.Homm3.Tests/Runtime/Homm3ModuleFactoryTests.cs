using System.Text;
using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Core.Runtime;
using Siniskalc.Heroes.Wpf.ViewModels;
using Siniskalc.Homm3.Runtime;

namespace Siniskalc.Homm3.Tests.Runtime
{
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class Homm3ModuleFactoryTests
    {
        private static Homm3ModuleFactory Factory(Func<IGameService, IGameService>? decorate = null) =>
            new(Encoding.Latin1, Mock.Of<IFileBackupServiceProvider>(), Mock.Of<ISavefileBytesPatcher>(), new BanishingTextProvider(), decorate);

        [Test]
        public void WithoutADecoratorTheGameServiceIsTheOneTheFactoryBuilt()
        {
            Assert.That(Factory().GameService, Is.InstanceOf<GameService>());
        }

        [Test]
        public void TheGameServiceIsWhatTheDecoratorReturnsForTheServiceTheFactoryBuilt()
        {
            IGameService? built = null;
            var decorated = Mock.Of<IGameService>();

            var factory = Factory(service =>
            {
                built = service;
                return decorated;
            });

            Assert.That(built, Is.InstanceOf<GameService>());
            Assert.That(factory.GameService, Is.SameAs(decorated));
        }

        [Test]
        public void TheEditorLoadsItsGamesThroughTheDecoratedService()
        {
            var decorated = new Mock<IGameService>();
            var factory = Factory(_ => decorated.Object);
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "castles_00_baseline.GM1"));
            decorated.Setup(s => s.LoadGameFromFile("a.GM1")).Returns(game);

            factory.CreateEditor().LoadGame("a.GM1");

            decorated.Verify(s => s.LoadGameFromFile("a.GM1"), Times.Once);
        }
    }
}
