using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Models
{
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class GrantedSpellsResolverTests
    {
        [Test]
        public void ResolversOfTheSameGameVersionAreEqualAndTheTomeOfFireMagicGrantsFireball()
        {
            var hota = new GrantedSpellsResolver(GameVersion.HornOfTheAbyss);
            var tome = new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()).WithEquipped(ArtifactSlots.Misc1, TestCatalog.For(GameVersion.HornOfTheAbyss).Artifacts.ById("TomeOfFireMagic"));

            Assert.That(hota, Is.EqualTo(new GrantedSpellsResolver(GameVersion.HornOfTheAbyss)));
            Assert.That(hota, Is.Not.EqualTo(new GrantedSpellsResolver(GameVersion.RestorationOfErathia)));
            Assert.That(hota.ResolveGrantedSpells(tome).Keys, Does.Contain(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fireball")));
            Assert.That(hota.ResolveGrantedSpells(tome)[TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Fireball")], Is.EqualTo("Tome of Fire Magic"));
        }
    }
}
