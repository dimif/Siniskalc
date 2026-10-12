using Siniskalc.Entities;

namespace Siniskalc.Tests.Entities;

public sealed class ArtifactTests
{
    [Test]
    public void GettingComboArtifactsReturnsCombo()
    {
        var combo = Artifact.Combination;

        Assert.That(() => combo.Count, Is.EqualTo(16));
        Assert.That(() => combo.All(a => a.IsCombination));
    }
}
