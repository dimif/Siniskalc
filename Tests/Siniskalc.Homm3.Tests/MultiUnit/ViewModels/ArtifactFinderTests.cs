using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels
{
    /// <summary>
    /// The finder of artifacts and the way the equipment picks from it, on equipment made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class ArtifactFinderTests
    {
        private static string[] Names(ArtifactFinderViewModel finder) =>
            finder.Artifacts.Cast<ArtifactChoiceViewModel>().Select(c => c.Name).ToArray();

        private static ArtifactChoiceViewModel Choice(ArtifactFinderViewModel finder, Artifact artifact) =>
            finder.Artifacts.Cast<ArtifactChoiceViewModel>().Single(c => c.Artifact == artifact);

        private static ArtifactFinderViewModel NewFinder(List<Artifact> chosen, Func<Artifact, bool>? canChoose = null) =>
            new(GameVersion.HornOfTheAbyss, new EffectLineProvider(), "Find", "Pick", chosen.Add, canChoose);

        private static EquippedArtifactsViewModel NewEquipment() => new(new EffectLineProvider(), new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()), GameVersion.HornOfTheAbyss);

        [Test]
        public void OneSearchBoxMatchesNameSlotTypeAndEffectTogether()
        {
            var finder = NewFinder([]);
            Assert.That(finder.Summary, Does.Contain("artifacts"));

            finder.Filter = "helm";
            Assert.That(Names(finder), Does.Contain(Artifact.SkullHelmet.Name));

            finder.Filter = "relic helm";
            Assert.That(Names(finder), Does.Contain(Artifact.ThunderHelmet.Name).And.Not.Contain(Artifact.SkullHelmet.Name));
            Assert.That(finder.Summary, Does.Contain("showing"));

            finder.Filter = "ring";
            Assert.That(finder.Artifacts.Cast<ArtifactChoiceViewModel>().Select(c => c.SlotName), Does.Contain("Ring"));

            finder.Filter = "immunity curse";
            Assert.That(Names(finder), Does.Contain(Artifact.PendantOfHoliness.Name).And.Not.Contain(Artifact.PendantOfDeath.Name));

            finder.Filter = ArtifactFinderViewModel.CombinedWord;
            Assert.That(Names(finder), Does.Contain(Artifact.AngelicAlliance.Name).And.Not.Contain(Artifact.SkullHelmet.Name));
        }

        [Test]
        public void PickingHandsTheArtifactToTheOwnerOnlyWhenAllowed()
        {
            var chosen = new List<Artifact>();
            var allowed = true;
            var finder = NewFinder(chosen, _ => allowed);
            var choice = Choice(finder, Artifact.SkullHelmet);

            finder.ChooseCommand.Execute(choice);
            Assert.That(chosen, Is.EqualTo(new[] { Artifact.SkullHelmet }));

            allowed = false;
            Assert.That(finder.ChooseCommand.CanExecute(choice), Is.False);
            finder.Choose(choice);
            Assert.That(chosen, Has.Count.EqualTo(1));
        }

        [Test]
        public void EquippingFromTheFinderUsesTheArtifactsSlotAndFirstFreeRingOrItem()
        {
            var vm = NewEquipment();
            var finder = vm.Finder;

            finder.Choose(Choice(finder, Artifact.SkullHelmet));
            Assert.That(vm.SelectedHelm, Is.EqualTo(Artifact.SkullHelmet.Name));

            finder.Choose(Choice(finder, Artifact.ThunderHelmet));
            Assert.That(vm.SelectedHelm, Is.EqualTo(Artifact.ThunderHelmet.Name), "a worn artifact is replaced");

            var ring = Artifact.All.Values.First(a => a.Slot == ArtifactSlots.Rings);
            var otherRing = Artifact.All.Values.First(a => a.Slot == ArtifactSlots.Rings && a != ring);
            finder.Choose(Choice(finder, ring));
            finder.Choose(Choice(finder, otherRing));
            Assert.That(vm.SelectedLeftRing, Is.EqualTo(ring.Name));
            Assert.That(vm.SelectedRightRing, Is.EqualTo(otherRing.Name));

            var third = Artifact.All.Values.First(a => a.Slot == ArtifactSlots.Rings && a != ring && a != otherRing);
            Assert.That(finder.ChooseCommand.CanExecute(Choice(finder, third)), Is.False, "both ring slots are taken");
        }

        [Test]
        public void TheEquipFinderPutsAScrollWithItsSpellInAFreeItemSlot()
        {
            var vm = NewEquipment();
            var scroll = vm.Finder.Artifacts.Cast<ArtifactChoiceViewModel>().Single(c => c.Spell == Spell.TownPortal);

            vm.Finder.Choose(scroll);

            Assert.That(vm.SelectedItem1, Is.EqualTo(SpellScroll.NameOf(Spell.TownPortal)));
        }

        [Test]
        public void OnlyTheItemSlotsOfferScrolls()
        {
            var vm = NewEquipment();

            Assert.That(vm.SlotFor(ArtifactSlots.Misc3).Finder.Artifacts.Cast<ArtifactChoiceViewModel>().Any(c => c.Spell == Spell.Fly), Is.True);
            Assert.That(vm.SlotFor(ArtifactSlots.Helmet).Finder.Artifacts.Cast<ArtifactChoiceViewModel>().Any(c => c.Artifact == Artifact.SpellScroll), Is.False);
        }
    }
}
