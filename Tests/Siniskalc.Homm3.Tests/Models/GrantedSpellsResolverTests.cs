using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Models
{
    [TestFixture]
    internal sealed class GrantedSpellsResolverTests
    {
        [Test]
        public void OneVersionsResolversAreEqualAndResolveThatVersionsSpells()
        {
            var hota = new GrantedSpellsResolver(GameVersion.HornOfTheAbyss);
            var tome = new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()).WithEquipped(ArtifactSlots.Misc1, Artifact.TomeOfFireMagic);

            Assert.That(hota, Is.EqualTo(new GrantedSpellsResolver(GameVersion.HornOfTheAbyss)));
            Assert.That(hota, Is.Not.EqualTo(new GrantedSpellsResolver(GameVersion.RestorationOfErathia)));
            Assert.That(hota.ResolveGrantedSpells(tome).Keys, Does.Contain(Spell.Fireball));
            Assert.That(hota.ResolveGrantedSpells(tome)[Spell.Fireball], Is.EqualTo("Tome of Fire Magic"));
        }
    }
}
