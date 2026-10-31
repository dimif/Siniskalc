using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Tests.Integration.Models;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.Integration.ViewModels
{
    /// <summary>
    /// The artifact finders of a hero page (the equipment and the backpack), which edit the hero of a savegame.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class ArtifactFinderHeroTests
    {
        private static ArtifactChoiceViewModel Choice(ArtifactFinderViewModel finder, Artifact artifact) =>
            finder.Artifacts.Cast<ArtifactChoiceViewModel>().Single(c => c.Artifact == artifact);

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
