using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Entities;

[Category(TestKinds.Unit)]
public sealed class ArtifactTests
{
    [Test]
    public void GettingComboArtifactsReturnsCombo()
    {
        var combo = TestCatalog.For(GameVersion.HornOfTheAbyss).WearableArtifacts.Where(a => a.IsCombination).ToList();

        Assert.That(() => combo.Count, Is.EqualTo(16));
        Assert.That(() => combo.All(a => a.IsCombination));
    }
}
