namespace Siniskalc.Homm2.Tests.Models
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    [TestFixture]
    internal sealed class HeroModelTests
    {
        private static HeroModel Maximus() => TestServices.LoadBrokenAlliance(10).History.Current.Heroes().GetModel(7);

        [Test]
        public void AnArtifactThatGivesSkillsAddsThemWhenItIsGivenAndTakesThemOffWhenItIsTakenAway()
        {
            var maximus = Maximus();

            var armed = maximus.WithArtifact(Artifact.UltimateSwordOfDominion);

            Assert.That(armed.PrimarySkills, Is.EqualTo(maximus.PrimarySkills with { Attack = maximus.PrimarySkills.Attack + 12 }));
            Assert.That(armed.Artifacts.Slots, Has.Member(Artifact.UltimateSwordOfDominion));
            Assert.That(armed.WithoutArtifact(Artifact.UltimateSwordOfDominion), Is.EqualTo(maximus));
        }

        [Test]
        public void GivingTheUltimateSwordToAnotherHeroMovesItsTwelveAttackPointsAsTheGameDidInSaveE13ArtSw()
        {
            // In the game Maximus (attack 15 with the sword) gave it to Sir Gallant (attack 2): they ended on 3 and 14.
            var maximus = (Maximus() with { PrimarySkills = new PrimarySkills(15, 15, 15, 15) }).WithArtifact(Artifact.UltimateSwordOfDominion);
            var gallant = TestServices.LoadBrokenAlliance(10).History.Current.Heroes().GetModel(1);

            var maximusAfter = maximus.WithoutArtifact(Artifact.UltimateSwordOfDominion);
            var gallantAfter = gallant.WithArtifact(Artifact.UltimateSwordOfDominion);

            Assert.That(gallantAfter.PrimarySkills.Attack, Is.EqualTo(gallant.PrimarySkills.Attack + 12));
            Assert.That(maximusAfter.PrimarySkills.Attack, Is.EqualTo(maximus.PrimarySkills.Attack - 12));
        }

        [Test]
        public void ReplacingAnArtifactSwapsItsBonusForTheNewOne()
        {
            var maximus = Maximus().WithArtifact(Artifact.UltimateSwordOfDominion);
            var slot = maximus.Artifacts.Slots.ToList().IndexOf(Artifact.UltimateSwordOfDominion);

            var swapped = maximus.WithArtifact(slot, Artifact.UltimateCloakOfProtection);

            Assert.That(swapped.PrimarySkills.Attack, Is.EqualTo(maximus.PrimarySkills.Attack - 12));
            Assert.That(swapped.PrimarySkills.Defense, Is.EqualTo(maximus.PrimarySkills.Defense + 12));
        }

        [Test]
        public void ASkillNeverFallsBelowZero()
        {
            var maximus = Maximus().WithArtifact(Artifact.UltimateSwordOfDominion) with { PrimarySkills = new PrimarySkills(3, 0, 0, 0) };

            Assert.That(maximus.WithoutArtifact(Artifact.UltimateSwordOfDominion).PrimarySkills.Attack, Is.EqualTo(0));
        }

        [Test]
        public void ArtifactsWithoutSkillBonusesLeaveTheSkillsAlone()
        {
            var maximus = Maximus();

            Assert.That(maximus.WithArtifact(Artifact.IceCloak).WithArtifact(Artifact.AmmoCart).PrimarySkills, Is.EqualTo(maximus.PrimarySkills));
        }

        [Test]
        public void SecondarySkillsCanBeSwappedReplacedAndRemovedBySlot()
        {
            var skills = new SecondarySkills(
            [
                new SkillEntry(SecondarySkill.Archery, SkillLevel.Basic),
                new SkillEntry(SecondarySkill.Wisdom, SkillLevel.Expert),
                new SkillEntry(SecondarySkill.Luck, SkillLevel.Advanced),
            ]);

            Assert.That(skills.Swap(0, 2).Entries.Select(e => e.Skill), Is.EqualTo(new[] { SecondarySkill.Luck, SecondarySkill.Wisdom, SecondarySkill.Archery }));
            Assert.That(skills.WithoutSlot(1).Entries.Select(e => e.Skill), Is.EqualTo(new[] { SecondarySkill.Archery, SecondarySkill.Luck }));
            Assert.That(skills.WithSkillInSlot(1, SecondarySkill.Mysticism).Entries[1], Is.EqualTo(new SkillEntry(SecondarySkill.Mysticism, SkillLevel.Expert)));
            Assert.That(skills.WithSkillInSlot(1, SecondarySkill.Luck), Is.EqualTo(skills), "a skill the hero has already is not put twice");
        }
    }
}
