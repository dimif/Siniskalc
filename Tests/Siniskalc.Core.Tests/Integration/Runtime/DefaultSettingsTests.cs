namespace Siniskalc.Core.Tests.Integration.Runtime
{
    using System.Text.Json;
    using Siniskalc.Core.Runtime;

    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class DefaultSettingsTests
    {
        [Test]
        public void TheDefaultSettingsFileTheApplicationShipsHoldsTheDefaultsOfTheSettingsClass()
        {
            var shipped = JsonSerializer.Deserialize<UserSettings>(DefaultSettings.Json)!;
            var defaults = new UserSettings();

            Assert.That(shipped.RecentFiles, Is.Empty);
            Assert.That(shipped.BackupOnSave, Is.EqualTo(defaults.BackupOnSave));
            Assert.That(shipped.AdvancedMode, Is.EqualTo(defaults.AdvancedMode));
            Assert.That(shipped.LogFolder, Is.EqualTo(defaults.LogFolder));
            Assert.That(shipped.LogLevel, Is.EqualTo(defaults.LogLevel));
        }

        [Test]
        public void TheDefaultSettingsFileNamesEveryPropertyOfTheSettingsClass()
        {
            using var document = JsonDocument.Parse(DefaultSettings.Json);
            var named = document.RootElement.EnumerateObject().Select(property => property.Name);

            Assert.That(named, Is.EquivalentTo(typeof(UserSettings).GetProperties().Select(property => property.Name)));
        }
    }
}
