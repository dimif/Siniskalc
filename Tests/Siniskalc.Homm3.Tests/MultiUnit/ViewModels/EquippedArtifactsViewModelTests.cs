using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels;

[Category(TestKinds.MultiUnit)]
public sealed class EquippedArtifactsViewModelTests
{
    private static string[] PickableNames(EquippedArtifactsViewModel vm, ArtifactSlots slot) =>
        vm.SlotFor(slot).Finder.Items.Cast<ArtifactChoiceViewModel>().Select(c => c.Name).ToArray();

    [Test]
    public void PickingInASlotFinderEquipsAndClosesThePopup()
    {
        var vm = new EquippedArtifactsViewModel(new EffectLineProvider(), new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()));
        var slot = vm.SlotFor(ArtifactSlots.Helmet);
        slot.IsOpen = true;

        slot.Finder.ChooseRow(slot.Finder.Items.Cast<ArtifactChoiceViewModel>().First());

        Assert.That(slot.IsOpen, Is.False);
        Assert.That(vm.SelectedHelm, Is.Not.Null);
        Assert.That(slot.ArtifactName, Is.EqualTo(vm.SelectedHelm));
    }

    [Test]
    public void SelectingCombinationArtifactMirrorsBlockedSlots()
    {
        var model = new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>());
        var vm = new EquippedArtifactsViewModel(new EffectLineProvider(), model);
        var admiral = Artifact.AdmiralsHat;

        vm.SelectedHelm = admiral.Name;

        Assert.That(vm.SelectedHelm, Is.EqualTo(admiral.Name));
        Assert.That(vm.SelectedNeck, Is.EqualTo(admiral.Name));
        Assert.That(PickableNames(vm, ArtifactSlots.Helmet), Does.Contain(admiral.Name));
        Assert.That(PickableNames(vm, ArtifactSlots.Necklace), Does.Contain(admiral.Name));
        Assert.That(vm.TotalEffects.WaterMovement, Is.EqualTo(admiral.EffectInfo.WaterMovement));
    }

    [Test]
    public void ClearingCombinationArtifactClearsAllMirroredSlots()
    {
        var model = new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>());
        var vm = new EquippedArtifactsViewModel(new EffectLineProvider(), model);
        var admiral = Artifact.AdmiralsHat;

        vm.SelectedHelm = admiral.Name;
        vm.SelectedNeck = null;

        Assert.That(vm.SelectedHelm, Is.Null);
        Assert.That(vm.SelectedNeck, Is.Null);
        Assert.That(vm.TotalEffects.WaterMovement, Is.EqualTo(0));
    }

    [Test]
    public void SwappingCombinationArtifactToAnotherArtifactUpdatesMirroredSlots()
    {
        var model = new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>());
        var vm = new EquippedArtifactsViewModel(new EffectLineProvider(), model);
        var admiral = Artifact.AdmiralsHat;
        var necklace = Artifact.Necklaces.First(a => !a.IsCombination);

        vm.SelectedHelm = admiral.Name;
        vm.SelectedNeck = necklace.Name;

        Assert.That(vm.SelectedHelm, Is.Null);
        Assert.That(vm.SelectedNeck, Is.EqualTo(necklace.Name));
        Assert.That(vm.TotalEffects.WaterMovement, Is.EqualTo(0));
        Assert.That(vm.TotalEffects.Pow, Is.EqualTo(necklace.EffectInfo.Pow));
    }

    [Test]
    public void SelectingMiscArtifactsKeepsTheOtherMiscSlots()
    {
        EquippedArtifacts? changed = null;
        var vm = new EquippedArtifactsViewModel(new EffectLineProvider(), new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()), m => changed = m);

        vm.SelectedItem1 = Artifact.CharmOfSpellPoints.Name;
        vm.SelectedItem2 = Artifact.MysticOrbOfSpellPoints.Name;

        Assert.That(vm.SelectedItem1, Is.EqualTo(Artifact.CharmOfSpellPoints.Name));
        Assert.That(vm.SelectedItem2, Is.EqualTo(Artifact.MysticOrbOfSpellPoints.Name));
        Assert.That(changed!.GetArtifact(ArtifactSlots.Misc1), Is.EqualTo(Artifact.CharmOfSpellPoints));
        Assert.That(changed.GetArtifact(ArtifactSlots.Misc2), Is.EqualTo(Artifact.MysticOrbOfSpellPoints));
    }

    [Test]
    public void RefreshShowsTheModelWithoutReportingAChange()
    {
        var changes = 0;
        var vm = new EquippedArtifactsViewModel(new EffectLineProvider(), new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()), _ => changes++);

        vm.Refresh(new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()).WithEquipped(ArtifactSlots.Helmet, Artifact.AdmiralsHat));

        Assert.That(vm.SelectedHelm, Is.EqualTo(Artifact.AdmiralsHat.Name));
        Assert.That(vm.SelectedNeck, Is.EqualTo(Artifact.AdmiralsHat.Name));
        Assert.That(changes, Is.EqualTo(0));
    }

    [Test]
    public void EffectLinesListOnlyEffectsThatAreNotZero()
    {
        var vm = new EquippedArtifactsViewModel(new EffectLineProvider(), new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()));
        Assert.That(vm.TotalEffectLines, Is.Empty);
        Assert.That(vm.HasNoEffects, Is.True);

        vm.SelectedHelm = Artifact.ThunderHelmet.Name;

        Assert.That(vm.HasNoEffects, Is.False);
        Assert.That(vm.TotalEffectLines, Is.EqualTo(new[] { new EffectLine("Spell power", "-2"), new EffectLine("Knowledge", "+10") }));
    }

    [Test]
    public void ClearSlotCommandTakesTheArtifactOffAndMirroredSlotsGoToo()
    {
        var vm = new EquippedArtifactsViewModel(new EffectLineProvider(), new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()));
        vm.SelectedHelm = Artifact.AdmiralsHat.Name;

        Assert.That(vm.ClearSlotCommand.CanExecute(ArtifactSlots.Necklace), Is.True);
        Assert.That(vm.ClearSlotCommand.CanExecute(ArtifactSlots.Feet), Is.False);

        vm.ClearSlotCommand.Execute(ArtifactSlots.Necklace);

        Assert.That(vm.SelectedHelm, Is.Null);
        Assert.That(vm.SelectedNeck, Is.Null);
    }

    [Test]
    public void HoveredArtifactInfoComesAndGoes()
    {
        var vm = new EquippedArtifactsViewModel(new EffectLineProvider(), new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()));
        Assert.That(vm.HasSelectedInfo, Is.False);

        vm.ShowArtifactInfoByName(Artifact.PendantOfCourage.Name);
        Assert.That(vm.HasSelectedInfo, Is.True);
        Assert.That(vm.SelectedEffectLines, Is.EqualTo(new[] { new EffectLine("Morale", "+3"), new EffectLine("Luck", "+3") }));

        vm.ClearArtifactInfo();
        Assert.That(vm.HasSelectedInfo, Is.False);
    }
}
