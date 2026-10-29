namespace Siniskalc.Homm2.Tests.Entities
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Tests the points of attack, defense, spell power and knowledge each artifact of HoMM2 adds to a hero, which the editor adds up for the
    /// total of the primary skills.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ArtifactBonusesTests
    {
        private static readonly (int Code, int Attack, int Defense, int Power, int Knowledge)[] Expected =
        [
            (0, 0, 0, 0, 12), (1, 12, 0, 0, 0), (2, 0, 12, 0, 0), (3, 0, 0, 12, 0), (4, 6, 6, 0, 0), (5, 0, 0, 6, 6), (6, 4, 4, 4, 4),
            (8, 0, 0, 4, 0), (9, 0, 0, 2, 0), (10, 0, 0, 2, 0), (11, 0, 0, 3, 0),
            (17, 1, 0, 0, 0), (18, 0, 1, 0, 0), (19, 0, 1, 0, 0), (20, 1, 0, 0, 0), (22, 0, 2, 0, 0), (23, 3, 0, 0, 0), (24, 2, 0, 0, 0), (25, 0, 3, 0, 0),
            (26, 0, 0, 0, 2), (27, 0, 0, 0, 3), (28, 0, 0, 0, 4), (29, 0, 0, 0, 5),
            (77, 1, 1, 0, 0), (78, 2, 2, 0, 0), (79, 0, 0, 1, 1), (80, 0, 0, 2, 2),
            (87, 0, 0, 3, 0), (88, 0, 5, 0, 0), (89, 0, 0, -2, 0), (90, 5, 5, 5, 0), (94, 0, 0, 5, 0), (95, 5, 0, 0, 0), (96, 2, 2, 2, 2), (99, 0, 0, 5, 0), (100, 1, 4, 0, 0), (101, 5, 0, 0, 0),
        ];

        [Test]
        public void EveryArtifactThatAddsPointsAddsTheOnesTheGameGivesIt()
        {
            foreach (var (code, attack, defense, power, knowledge) in Expected)
            {
                var artifact = Artifact.FromByte((byte)code);

                Assert.That(artifact, Is.Not.Null, $"artifact {code}");
                Assert.That(ArtifactBonuses.Of(artifact!), Is.EqualTo(new PrimarySkillBonus(attack, defense, power, knowledge)), artifact!.Name);
            }
        }

        [Test]
        public void AnArtifactThatAddsNoPointsAddsNone()
        {
            var plain = Artifact.All.Where(a => Expected.All(e => e.Code != a.Code)).ToList();

            Assert.That(plain, Is.Not.Empty);
            Assert.That(plain.Select(ArtifactBonuses.Of), Is.All.EqualTo(PrimarySkillBonus.None));
        }

        [Test]
        public void TheUltimateArtifactsAreTheOnesThatAddTheMost()
        {
            Assert.That(ArtifactBonuses.Of(Artifact.UltimateBookOfKnowledge).Knowledge, Is.EqualTo(12));
            Assert.That(ArtifactBonuses.Of(Artifact.UltimateSwordOfDominion).Attack, Is.EqualTo(12));
            Assert.That(ArtifactBonuses.Of(Artifact.UltimateCloakOfProtection).Defense, Is.EqualTo(12));
            Assert.That(ArtifactBonuses.Of(Artifact.UltimateWandOfMagic).SpellPower, Is.EqualTo(12));
        }
    }
}
