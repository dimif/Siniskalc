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
        private static Homm3ModuleFactory Factory(Func<ISavegameService<Game>, ISavegameService<Game>>? decorate = null) =>
            new(Encoding.Latin1, Mock.Of<IFileBackupServiceProvider>(), Mock.Of<ISavefileBytesPatcher>(), new BanishingTextProvider(), decorate);

        [Test]
        public void WithoutADecoratorTheGameServiceIsTheOneTheFactoryBuilt()
        {
            Assert.That(Factory().GameService, Is.InstanceOf<SavegameService<Game>>());
        }

        [Test]
        public void TheGameServiceIsWhatTheDecoratorReturnsForTheServiceTheFactoryBuilt()
        {
            ISavegameService<Game>? built = null;
            var decorated = Mock.Of<ISavegameService<Game>>();

            var factory = Factory(service =>
            {
                built = service;
                return decorated;
            });

            Assert.That(built, Is.InstanceOf<SavegameService<Game>>());
            Assert.That(factory.GameService, Is.SameAs(decorated));
        }

        [Test]
        public void TheEditorLoadsItsGamesThroughTheDecoratedService()
        {
            var decorated = new Mock<ISavegameService<Game>>();
            var factory = Factory(_ => decorated.Object);
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "castles_00_baseline.GM1"));
            decorated.Setup(s => s.LoadGameFromFile("a.GM1")).Returns(game);

            factory.CreateEditor().LoadGame("a.GM1");

            decorated.Verify(s => s.LoadGameFromFile("a.GM1"), Times.Once);
        }
    }
}
