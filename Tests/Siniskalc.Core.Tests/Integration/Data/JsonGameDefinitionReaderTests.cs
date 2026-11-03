namespace Siniskalc.Core.Tests.Integration.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;

    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class JsonGameDefinitionReaderTests
    {
        private string folder = string.Empty;

        [SetUp]
        public void CreateFolder()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-definitions-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void ADefinitionIsReadRelativeToTheConfiguredDirectory()
        {
            this.Write("""{"id":"pikeman","code":0}""");

            var definition = new JsonGameDefinitionReader(this.folder).Read<SampleDefinition>("creature.json");

            Assert.That(definition.Id, Is.EqualTo("pikeman"));
            Assert.That(definition.Code, Is.Zero);
        }

        [TestCase("""{"id":"pikeman","code":0,"health":10}""")]
        [TestCase("""{"Id":"pikeman","code":0}""")]
        [TestCase("""{"code":0}""")]
        [TestCase("""{"id":"pikeman","code":256}""")]
        [TestCase("""{"id":"pikeman","code":"0"}""")]
        [TestCase("""{"id":"pikeman","code":0,}""")]
        [TestCase("""{"id":"pikeman","code":0,"code":1}""")]
        [TestCase("null")]
        [TestCase("{")]
        public void AnInvalidDefinitionReportsItsFileAndCause(string json)
        {
            this.Write(json);

            var failure = Assert.Throws<GameDefinitionException>(
                () => new JsonGameDefinitionReader(this.folder).Read<SampleDefinition>("creature.json"));

            Assert.That(failure!.Path, Is.EqualTo("creature.json"));
            Assert.That(failure.Message, Does.Contain("creature.json"));
            Assert.That(failure.InnerException, Is.InstanceOf<System.Text.Json.JsonException>());
        }

        [Test]
        public void ADuplicatePropertyInsideAnArrayIsRejected()
        {
            this.Write("""[{"id":"pikeman","id":"other","code":0}]""");

            var failure = Assert.Throws<GameDefinitionException>(
                () => new JsonGameDefinitionReader(this.folder).Read<SampleDefinition[]>("creature.json"));

            Assert.That(failure!.Message, Does.Contain("$[0]").And.Contain("Duplicate property 'id'"));
        }

        [Test]
        public void AMissingFileIsAnErrorRatherThanAnEmptyCatalog()
        {
            var failure = Assert.Throws<GameDefinitionException>(
                () => new JsonGameDefinitionReader(this.folder).Read<SampleDefinition>("missing.json"));

            Assert.That(failure!.Path, Is.EqualTo("missing.json"));
            Assert.That(failure.InnerException, Is.InstanceOf<IOException>());
        }

        [Test]
        public void ADirectoryInPlaceOfAFileIsReported()
        {
            Directory.CreateDirectory(Path.Combine(this.folder, "creature.json"));

            Assert.That(
                () => new JsonGameDefinitionReader(this.folder).Read<SampleDefinition>("creature.json"),
                Throws.TypeOf<GameDefinitionException>());
        }

        [TestCase(@"..\creature.json")]
        [TestCase("")]
        [TestCase("invalid\0.json")]
        public void APathOutsideTheDefinitionDirectoryIsRejected(string path)
        {
            Assert.That(
                () => new JsonGameDefinitionReader(this.folder).Read<SampleDefinition>(path),
                Throws.TypeOf<GameDefinitionException>());
        }

        [Test]
        public void AnAbsolutePathIsRejectedEvenWhenItPointsInsideTheDefinitionDirectory()
        {
            this.Write("""{"id":"pikeman","code":0}""");

            Assert.That(
                () => new JsonGameDefinitionReader(this.folder).Read<SampleDefinition>(Path.Combine(this.folder, "creature.json")),
                Throws.TypeOf<GameDefinitionException>());
        }

        private void Write(string json) => File.WriteAllText(Path.Combine(this.folder, "creature.json"), json);

        private sealed record SampleDefinition
        {
            public required string Id { get; init; }

            public required byte Code { get; init; }
        }
    }
}
