namespace Siniskalc.Homm2.Tests.Models
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    [TestFixture]
    internal sealed class Homm2HeroModelTests
    {
        private static Homm2HeroModel Maximus() => Homm2TestServices.LoadBrokenAlliance(10).History.Current.Heroes().GetModel(7);

        [Test]
        public void AnArtifactThatGivesSkillsAddsThemWhenItIsGivenAndTakesThemOffWhenItIsTakenAway()
        {
            var maximus = Maximus();

            var armed = maximus.WithArtifact(Homm2Artifact.UltimateSwordOfDominion);

            Assert.That(armed.PrimarySkills, Is.EqualTo(maximus.PrimarySkills with { Attack = maximus.PrimarySkills.Attack + 12 }));
            Assert.That(armed.Artifacts.Slots, Has.Member(Homm2Artifact.UltimateSwordOfDominion));
            Assert.That(armed.WithoutArtifact(Homm2Artifact.UltimateSwordOfDominion), Is.EqualTo(maximus));
        }

        [Test]
        public void GivingTheUltimateSwordToAnotherHeroMovesItsTwelveAttackPointsAsTheGameDidInSaveE13ArtSw()
        {
            // In the game Maximus (attack 15 with the sword) gave it to Sir Gallant (attack 2): they ended on 3 and 14.
            var maximus = (Maximus() with { PrimarySkills = new Homm2PrimarySkills(15, 15, 15, 15) }).WithArtifact(Homm2Artifact.UltimateSwordOfDominion);
            var gallant = Homm2TestServices.LoadBrokenAlliance(10).History.Current.Heroes().GetModel(1);

            var maximusAfter = maximus.WithoutArtifact(Homm2Artifact.UltimateSwordOfDominion);
            var gallantAfter = gallant.WithArtifact(Homm2Artifact.UltimateSwordOfDominion);

            Assert.That(gallantAfter.PrimarySkills.Attack, Is.EqualTo(gallant.PrimarySkills.Attack + 12));
            Assert.That(maximusAfter.PrimarySkills.Attack, Is.EqualTo(maximus.PrimarySkills.Attack - 12));
        }

        [Test]
        public void ReplacingAnArtifactSwapsItsBonusForTheNewOne()
        {
            var maximus = Maximus().WithArtifact(Homm2Artifact.UltimateSwordOfDominion);
            var slot = maximus.Artifacts.Slots.ToList().IndexOf(Homm2Artifact.UltimateSwordOfDominion);

            var swapped = maximus.WithArtifact(slot, Homm2Artifact.UltimateCloakOfProtection);

            Assert.That(swapped.PrimarySkills.Attack, Is.EqualTo(maximus.PrimarySkills.Attack - 12));
            Assert.That(swapped.PrimarySkills.Defense, Is.EqualTo(maximus.PrimarySkills.Defense + 12));
        }

        [Test]
        public void ASkillNeverFallsBelowZero()
        {
            var maximus = Maximus().WithArtifact(Homm2Artifact.UltimateSwordOfDominion) with { PrimarySkills = new Homm2PrimarySkills(3, 0, 0, 0) };

            Assert.That(maximus.WithoutArtifact(Homm2Artifact.UltimateSwordOfDominion).PrimarySkills.Attack, Is.EqualTo(0));
        }

        [Test]
        public void ArtifactsWithoutSkillBonusesLeaveTheSkillsAlone()
        {
            var maximus = Maximus();

            Assert.That(maximus.WithArtifact(Homm2Artifact.IceCloak).WithArtifact(Homm2Artifact.AmmoCart).PrimarySkills, Is.EqualTo(maximus.PrimarySkills));
        }

        [Test]
        public void SecondarySkillsCanBeSwappedReplacedAndRemovedBySlot()
        {
            var skills = new Homm2SecondarySkills(
            [
                new Homm2SkillEntry(Homm2SecondarySkill.Archery, Homm2SkillLevel.Basic),
                new Homm2SkillEntry(Homm2SecondarySkill.Wisdom, Homm2SkillLevel.Expert),
                new Homm2SkillEntry(Homm2SecondarySkill.Luck, Homm2SkillLevel.Advanced),
            ]);

            Assert.That(skills.Swap(0, 2).Entries.Select(e => e.Skill), Is.EqualTo(new[] { Homm2SecondarySkill.Luck, Homm2SecondarySkill.Wisdom, Homm2SecondarySkill.Archery }));
            Assert.That(skills.WithoutSlot(1).Entries.Select(e => e.Skill), Is.EqualTo(new[] { Homm2SecondarySkill.Archery, Homm2SecondarySkill.Luck }));
            Assert.That(skills.WithSkillInSlot(1, Homm2SecondarySkill.Mysticism).Entries[1], Is.EqualTo(new Homm2SkillEntry(Homm2SecondarySkill.Mysticism, Homm2SkillLevel.Expert)));
            Assert.That(skills.WithSkillInSlot(1, Homm2SecondarySkill.Luck), Is.EqualTo(skills), "a skill the hero has already is not put twice");
        }
    }
}
