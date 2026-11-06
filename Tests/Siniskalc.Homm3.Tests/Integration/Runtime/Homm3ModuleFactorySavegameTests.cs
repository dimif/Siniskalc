using System.Text;
using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Core.Runtime;
using Siniskalc.Heroes.Wpf.ViewModels;
using Siniskalc.Homm3.Runtime;

namespace Siniskalc.Homm3.Tests.Integration.Runtime
{
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class Homm3ModuleFactorySavegameTests
    {
        [Test]
        public void TheEditorLoadsItsGamesThroughTheDecoratedService()
        {
            var decorated = new Mock<ISavegameService<Game>>();
            var factory = new Homm3ModuleFactory(Encoding.Latin1, Mock.Of<IFileBackupServiceProvider>(), Mock.Of<ISavefileBytesPatcher>(), new BanishingTextProvider(), _ => decorated.Object, new JsonGameDefinitionReader());
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "castles_00_baseline.GM1"));
            decorated.Setup(s => s.LoadGameFromFile("a.GM1")).Returns(game);

            factory.CreateEditor().LoadGame("a.GM1");

            decorated.Verify(s => s.LoadGameFromFile("a.GM1"), Times.Once);
        }
    }
}
