using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.ViewModels;
using Siniskalc.Tests.Models;

namespace Siniskalc.Tests.ViewModels
{
    [TestFixture]
    internal sealed class ArtifactFinderTests
    {
        private static string[] Names(ArtifactFinderViewModel finder) =>
            finder.Artifacts.Cast<ArtifactChoiceViewModel>().Select(c => c.Name).ToArray();

        private static ArtifactChoiceViewModel Choice(ArtifactFinderViewModel finder, Artifact artifact) =>
            finder.Artifacts.Cast<ArtifactChoiceViewModel>().Single(c => c.Artifact == artifact);

        private static ArtifactFinderViewModel NewFinder(List<Artifact> chosen, Func<Artifact, bool>? canChoose = null) =>
            new(GameVersion.HornOfTheAbyss, new EffectLineProvider(), "Find", "Pick", chosen.Add, canChoose);

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
            var vm = new EquippedArtifactsViewModel(TestServices.EffectLines, new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()));
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
        public void TheBackpackButtonOfTheEquipFinderSendsTheArtifactToTheBackpack()
        {
            var game = HeroEditingTests.LoadPrepared();
            using var hero = TestServices.CreateHeroViewModel(game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian")));
            var finder = hero.ArtifactsViewModel.Finder;
            var backpack = hero.BackpackViewModel;
            var empty = backpack.Slots1To32.Concat(backpack.Slots33To64).First(s => string.IsNullOrEmpty(s.SelectedArtifact));
            var helmet = Choice(finder, Artifact.SkullHelmet);

            Assert.That(finder.HasSecondaryAction, Is.True);
            Assert.That(finder.SecondaryActionLabel, Is.EqualTo("Backpack"));
            Assert.That(finder.ChooseSecondaryCommand.CanExecute(helmet), Is.True);

            finder.ChooseSecondaryCommand.Execute(helmet);

            Assert.That(empty.SelectedArtifact, Is.EqualTo(Artifact.SkullHelmet.Name));
            Assert.That(hero.HeroModel.Backpack.All.Any(s => s?.Art == Artifact.SkullHelmet), Is.True);
            Assert.That(backpack.Finder.HasSecondaryAction, Is.False, "the backpack finder only has Add");
        }

        [Test]
        public void TheEquipFinderPutsAScrollWithItsSpellInAFreeItemSlot()
        {
            var vm = new EquippedArtifactsViewModel(TestServices.EffectLines, new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()));
            var scroll = vm.Finder.Artifacts.Cast<ArtifactChoiceViewModel>().Single(c => c.Spell == Spell.TownPortal);

            vm.Finder.Choose(scroll);

            Assert.That(vm.SelectedItem1, Is.EqualTo(SpellScroll.NameOf(Spell.TownPortal)));
        }

        [Test]
        public void OnlyTheItemSlotsOfferScrolls()
        {
            var vm = new EquippedArtifactsViewModel(TestServices.EffectLines, new EquippedArtifacts(new Dictionary<ArtifactSlots, Artifact>()));

            Assert.That(vm.SlotFor(ArtifactSlots.Misc3).Finder.Artifacts.Cast<ArtifactChoiceViewModel>().Any(c => c.Spell == Spell.Fly), Is.True);
            Assert.That(vm.SlotFor(ArtifactSlots.Helmet).Finder.Artifacts.Cast<ArtifactChoiceViewModel>().Any(c => c.Artifact == Artifact.SpellScroll), Is.False);
        }

        [Test]
        public void AddingFromTheBackpackFinderFillsTheFirstEmptySlot()
        {
            var game = HeroEditingTests.LoadPrepared();
            using var hero = TestServices.CreateHeroViewModel(game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian")));
            var backpack = hero.BackpackViewModel;
            var empty = backpack.Slots1To32.Concat(backpack.Slots33To64).First(s => string.IsNullOrEmpty(s.SelectedArtifact));

            backpack.Finder.Choose(Choice(backpack.Finder, Artifact.SkullHelmet));

            Assert.That(empty.SelectedArtifact, Is.EqualTo(Artifact.SkullHelmet.Name));

            game.History.Undo();

            Assert.That(empty.SelectedArtifact, Is.Null.Or.Empty);
        }

        [Test]
        public void TheBackpackFinderListsAScrollPerSpellAndKeepsTheSpell()
        {
            var game = HeroEditingTests.LoadPrepared();
            using var hero = TestServices.CreateHeroViewModel(game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian")));
            var backpack = hero.BackpackViewModel;
            var empty = backpack.Slots1To32.Concat(backpack.Slots33To64).First(s => string.IsNullOrEmpty(s.SelectedArtifact));
            var scroll = backpack.Finder.Artifacts.Cast<ArtifactChoiceViewModel>().Single(c => c.Spell == Spell.TownPortal);

            Assert.That(backpack.Finder.Artifacts.Cast<ArtifactChoiceViewModel>().Count(c => c.Artifact == Artifact.SpellScroll), Is.GreaterThan(1));
            Assert.That(backpack.Finder.Artifacts.Cast<ArtifactChoiceViewModel>().Any(c => c.Artifact == Artifact.SpellScroll && c.Spell == null), Is.False);

            backpack.Finder.Choose(scroll);

            Assert.That(empty.SelectedArtifact, Is.EqualTo(SpellScroll.NameOf(Spell.TownPortal)));
            var slot = game.History.Current.Heroes().FindByName("Christian");
            var saved = game.History.CreateHeroSession(slot).Current.Backpack.All[empty.SlotNumber - 1];
            Assert.That(saved?.Art, Is.EqualTo(Artifact.SpellScroll));
            Assert.That(saved?.Spell, Is.EqualTo(Spell.TownPortal));
        }
    }
}
