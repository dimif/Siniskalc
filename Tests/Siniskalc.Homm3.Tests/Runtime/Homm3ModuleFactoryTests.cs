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
        private static Homm3ModuleFactory Factory(Func<ISavegameService<Game>, ISavegameService<Game>> decorate) =>
            new(Encoding.Latin1, Mock.Of<IFileBackupServiceProvider>(), Mock.Of<ISavefileBytesPatcher>(), new BanishingTextProvider(), decorate, new JsonGameDefinitionReader());

        [Test]
        public void ADecoratorThatReturnsTheServiceLeavesTheGameServiceAsTheFactoryBuiltIt()
        {
            Assert.That(Factory(service => service).GameService, Is.InstanceOf<SavegameService<Game>>());
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
    }
}
