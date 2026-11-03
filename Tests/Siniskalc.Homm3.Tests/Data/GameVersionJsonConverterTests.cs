namespace Siniskalc.Homm3.Tests.Data
{
    using System.Text.Json;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;

    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class GameVersionJsonConverterTests
    {
        private readonly JsonSerializerOptions options = new() { Converters = { new GameVersionJsonConverter() } };

        [TestCase("roe", GameVersion.RestorationOfErathia)]
        [TestCase("ab", GameVersion.ArmageddonsBlade)]
        [TestCase("sod", GameVersion.ShadowOfDeath)]
        [TestCase("chronicles", GameVersion.HeroesChronicles)]
        [TestCase("hota", GameVersion.HornOfTheAbyss)]
        public void EachShortVersionNameIsReadAndWrittenAsAValueAndAsADictionaryKey(string name, GameVersion version)
        {
            Assert.That(JsonSerializer.Deserialize<GameVersion>($"\"{name}\"", this.options), Is.EqualTo(version));
            Assert.That(JsonSerializer.Serialize(version, this.options), Is.EqualTo($"\"{name}\""));
            var dictionary = JsonSerializer.Deserialize<Dictionary<GameVersion, int>>($"{{\"{name}\":1}}", this.options)!;
            Assert.That(dictionary.Single().Key, Is.EqualTo(version));
            Assert.That(JsonSerializer.Serialize(dictionary, this.options), Is.EqualTo($"{{\"{name}\":1}}"));
        }

        [TestCase("\"restorationOfErathia\"")]
        [TestCase("\"unsupported\"")]
        [TestCase("1")]
        [TestCase("null")]
        public void AnUnsupportedOrNonStringVersionIsRejected(string json) =>
            Assert.That(() => JsonSerializer.Deserialize<GameVersion>(json, this.options), Throws.TypeOf<JsonException>());
    }
}
