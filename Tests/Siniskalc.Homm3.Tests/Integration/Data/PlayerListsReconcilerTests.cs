using Siniskalc.Core.Data;
using Siniskalc.Core.Models;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// Giving a hero to another owner, replayed on the saves of <c>TestData/Hota/HeroOwners</c>, in which the game defeated heroes and hired
    /// others between one save and the next: after the edit of the hero, <see cref="PlayerListsReconciler"/> must leave the lists of heroes of
    /// the players and the table of hero owners as the game left them.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class PlayerListsReconcilerTests
    {
        private const string Baseline = "hero_owners_00_baseline";
        private const string ClancyAndUfretinDefeated = "hero_owners_01_clancy_and_ufretin_defeated";
        private const string Week2Day1 = "hero_owners_02_m4w2d1";
        private const string FourRedHeroesDefeated = "hero_owners_03_red_hires_vokial__four_red_heroes_defeated";
        private const string BlueHasEightHeroes = "hero_owners_04_blue_hires_seven_heroes__pink_eliminated";
        private const string SevenBlueHeroesDefeated = "hero_owners_05_seven_blue_heroes_defeated__blue_hires_seven";
        private const string SevenNewBlueHeroesDefeated = "hero_owners_06_seven_new_blue_heroes_defeated";

        private static SaveDocumentHistory Load(string save) => SavedGames.Get(Path.Combine("TestData", "Hota", "HeroOwners", save + ".GM2")).History;

        private static void Defeat(SaveDocumentHistory history, params int[] heroIds)
        {
            foreach (var id in heroIds)
            {
                GiveTo(history, id, PlayerColor.Neutral);
            }
        }

        private static void GiveTo(SaveDocumentHistory history, int heroId, PlayerColor owner) =>
            history.CreateHeroSession(history.Current.Heroes().IndexOfId(heroId)).Update(h => h.WithOwner(owner));

        private static PlayerLists Player(SaveDocument document, PlayerColor color) => document.Players().GetLists(document.Players().FindByColor(color));

        private static byte[] Head(SaveDocument document, PlayerColor color) => document.Players()[document.Players().FindByColor(color)].ToArray().Take(PlayerLayout.TavernHeroes).ToArray();

        private static byte[] Table(SaveDocument document) => document.HeroOwners()[0].ToArray();

        private static HeroModel Hero(SaveDocument document, int heroId) => document.Heroes().GetModel(document.Heroes().IndexOfId(heroId));

        // A hero nobody owns that no tavern offers and the game does not keep out of play.
        private static int FreeHero(SaveDocument document)
        {
            var table = document.HeroOwners().GetModel(0);
            var offered = Enumerable.Range(0, document.Players().Count).SelectMany(p => document.Players().GetModel(p).Tavern.Ids.OfType<int>()).ToHashSet();
            return Enumerable.Range(0, table.Heroes.Count).First(id => table.Heroes[id] == HeroOwnership.Free && !offered.Contains(id) && document.Heroes().IndexOfId(id) >= 0);
        }

        [Test]
        public void DefeatingClancyAndUfretinWritesThePlayersAndTheTableAsTheGameDid()
        {
            var history = Load(Baseline);
            var expected = Load(ClancyAndUfretinDefeated).Current;

            Defeat(history, 22, 17);

            foreach (var color in PlayerColor.All.Where(c => history.Current.Players().FindByColor(c) >= 0))
            {
                Assert.That(Head(history.Current, color), Is.EqualTo(Head(expected, color)), $"bytes 0 to 9 of {color}");
            }

            Assert.That(Table(history.Current), Is.EqualTo(Table(expected)));
            Assert.That(Hero(history.Current, 22).Owner, Is.EqualTo(PlayerColor.Neutral));
        }

        [Test]
        public void DefeatingFourRedHeroesCompactsRedsListAsTheGameDid()
        {
            var history = Load(Week2Day1);
            var before = Table(history.Current);
            var expected = Load(FourRedHeroesDefeated).Current;

            Defeat(history, 55, 84, 102, 144);

            // The game then hired Vokial (65) into the first free place and drew Leena (160) for the tavern; that is not replayed.
            var red = Player(history.Current, PlayerColor.Red);
            Assert.That(red.HeroIds, Is.EqualTo(Numbers.Of(115, 36)));
            var expectedRed = Player(expected, PlayerColor.Red);
            Assert.That(expectedRed.WithHero(65, false, expectedRed).HeroIds, Is.EqualTo(red.HeroIds));
            Assert.That(red.SelectedHero, Is.EqualTo(Player(expected, PlayerColor.Red).SelectedHero));
            var table = Table(history.Current);
            var hired = new[] { 65, 160 };
            Assert.That(Enumerable.Range(0, table.Length).Where(id => !hired.Contains(id)).Select(id => table[id]), Is.EqualTo(Enumerable.Range(0, table.Length).Where(id => !hired.Contains(id)).Select(id => Table(expected)[id])));
            Assert.That(hired.Select(id => table[id]), Is.EqualTo(hired.Select(id => before[id])));
        }

        [Test]
        public void DefeatingSevenHeroesOfOnePlayerLeavesItsFirstHero()
        {
            var history = Load(BlueHasEightHeroes);
            var before = Table(history.Current);
            var expected = Table(Load(SevenBlueHeroesDefeated).Current);
            var defeated = new[] { 169, 164, 184, 129, 70, 71, 10 };

            Defeat(history, defeated);

            Assert.That(Player(history.Current, PlayerColor.Blue).HeroIds, Is.EqualTo(Numbers.Of(0)));
            var table = Table(history.Current);
            Assert.That(defeated.Select(id => table[id]), Is.EqualTo(defeated.Select(id => expected[id])));
            Assert.That(Enumerable.Range(0, table.Length).Where(id => !defeated.Contains(id)).Select(id => table[id]), Is.EqualTo(Enumerable.Range(0, table.Length).Where(id => !defeated.Contains(id)).Select(id => before[id])));
        }

        [Test]
        public void DefeatingSevenHeroesAtOnceWritesTheListAndTheTableAsTheGameDid()
        {
            var history = Load(SevenBlueHeroesDefeated);
            var expected = Load(SevenNewBlueHeroesDefeated).Current;

            // Fiona (205) is past the 200th byte of the table.
            Defeat(history, 79, 57, 48, 205, 58, 144, 127);

            // The game also selected Orrin, which no defeat explains; the rest of bytes 0 to 9 is the game's.
            var head = Head(history.Current, PlayerColor.Blue);
            var game = Head(expected, PlayerColor.Blue);
            Assert.That(head.Where((_, i) => i != PlayerLayout.SelectedHero), Is.EqualTo(game.Where((_, i) => i != PlayerLayout.SelectedHero)));
            Assert.That(Table(history.Current), Is.EqualTo(Table(expected)));
        }

        [Test]
        public void AHeroGivenToAPlayerGoesToTheEndOfItsListAndTheTable()
        {
            var history = Load(Baseline);
            var original = history.Current;
            var hero = FreeHero(original);
            var tan = Player(original, PlayerColor.Tan);

            GiveTo(history, hero, PlayerColor.Tan);

            Assert.That(Player(history.Current, PlayerColor.Tan).HeroIds, Is.EqualTo(new Numbers(tan.HeroIds.Append(hero))));
            Assert.That(history.Current.HeroOwners().GetModel(0).Heroes[hero], Is.EqualTo(new HeroOwnership(PlayerColor.Tan, false)));
            Assert.That(Hero(history.Current, hero).Owner, Is.EqualTo(PlayerColor.Tan));

            history.Undo();
            Assert.That(history.CanUndo, Is.False, "one step of the history");
            Assert.That(Head(history.Current, PlayerColor.Tan), Is.EqualTo(Head(original, PlayerColor.Tan)));
            Assert.That(Table(history.Current), Is.EqualTo(Table(original)));
        }

        [Test]
        public void AHeroMovedToAnotherPlayerLeavesTheListOfTheOneThatHadIt()
        {
            var history = Load(Baseline);

            GiveTo(history, 22, PlayerColor.Tan);

            Assert.That(Player(history.Current, PlayerColor.Blue).HeroIds, Is.EqualTo(Numbers.Of(0)));
            Assert.That(Player(history.Current, PlayerColor.Tan).HeroIds.Last(), Is.EqualTo(22));
            Assert.That(history.Current.HeroOwners().GetModel(0).Heroes[22].Owner, Is.EqualTo(PlayerColor.Tan));
        }

        [Test]
        public void RevertingAMoveToAnotherPlayerPutsBothPlayersAndTheTableBackInOneStep()
        {
            var history = Load(Baseline);
            var original = history.Current;
            GiveTo(history, 22, PlayerColor.Tan);
            var moved = history.Current;

            history.GetPendingChanges().Single(c => c.Change.Description == "Owner").Revert(history);

            Assert.That(Hero(history.Current, 22).Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That(Head(history.Current, PlayerColor.Blue), Is.EqualTo(Head(original, PlayerColor.Blue)));
            Assert.That(Head(history.Current, PlayerColor.Tan), Is.EqualTo(Head(original, PlayerColor.Tan)));
            Assert.That(Table(history.Current), Is.EqualTo(Table(original)));
            Assert.That(history.GetPendingChanges(), Is.Empty);

            history.Undo();
            Assert.That(history.Current, Is.SameAs(moved), "the revert is one step of the history");
        }

        [Test]
        public void AHeroTheTableGivesToAPlayerButNoListHasIsStillTakenFromIt()
        {
            // The table and the lists are two records: in hero_owners_04 the table gives Melodia (29) to Blue and no list has her.
            var history = Load(Baseline);
            var players = history.Current.Players();
            var blue = players.FindByColor(PlayerColor.Blue);
            var lists = players.GetLists(blue);
            history.ApplyAsIs(history.Current.WithPlayer(blue, players.ListsSerializer.Serialize(players[blue], lists.WithHero(22, false, lists))));

            Defeat(history, 22);

            Assert.That(Hero(history.Current, 22).Owner, Is.EqualTo(PlayerColor.Neutral));
            Assert.That(Player(history.Current, PlayerColor.Blue).HeroIds, Is.EqualTo(Numbers.Of(0)));
            Assert.That(history.Current.HeroOwners().GetModel(0).Heroes[22], Is.EqualTo(HeroOwnership.Free));
        }

        [Test]
        public void ASavefileWithNoTableOfHeroOwnersStillHasTheHeroAndTheListsChanged()
        {
            var loaded = Load(Baseline).Current;
            var heroes = loaded.Heroes();
            var history = new SaveDocumentHistory(TestServices.DocumentFactory.Create(loaded.ToBytes(), heroes, heroes.Offsets, loaded.Players(), loaded.Castles()));
            var original = history.Current;
            Assert.That(original.HeroOwners(), Is.Empty);

            Defeat(history, 22);

            Assert.That(Hero(history.Current, 22).Owner, Is.EqualTo(PlayerColor.Neutral));
            Assert.That(Player(history.Current, PlayerColor.Blue).HeroIds, Is.EqualTo(Numbers.Of(0)));

            history.GetPendingChanges().Single(c => c.Change.Description == "Owner").Revert(history);
            Assert.That(Head(history.Current, PlayerColor.Blue), Is.EqualTo(Head(original, PlayerColor.Blue)));
        }

        [Test]
        public void AHeroATavernOffersIsNotGivenToAPlayer()
        {
            var history = Load(Week2Day1);

            Assert.That(() => GiveTo(history, 65, PlayerColor.Red), Throws.TypeOf<GameRuleException>().With.Message.Contains("tavern"));
            Assert.That(history.CanUndo, Is.False);
        }

        [Test]
        public void AHeroIsNotGivenToAPlayerWithEightHeroes()
        {
            var history = Load(BlueHasEightHeroes);
            var hero = FreeHero(history.Current);

            Assert.That(() => GiveTo(history, hero, PlayerColor.Blue), Throws.TypeOf<GameRuleException>());
            Assert.That(history.CanUndo, Is.False);
        }

        [Test]
        public void AHeroTheGameKeepsOutOfPlayIsNotGivenToAPlayer()
        {
            var history = Load(Baseline);

            // Hero 3 has the unexplained 41 in the table.
            Assert.That(() => GiveTo(history, 3, PlayerColor.Red), Throws.TypeOf<GameRuleException>());
            Assert.That(history.CanUndo, Is.False);
        }

        [Test]
        public void TheOwnerOfAGarrisonedHeroIsNotChanged()
        {
            // Hero 29 is garrisoned in Blue's castle: the table gives it to Blue, but it is in no list.
            var history = Load(BlueHasEightHeroes);

            Assert.That(() => GiveTo(history, 29, PlayerColor.Neutral), Throws.TypeOf<GameRuleException>().With.Message.Contains("garrisoned"));
            Assert.That(history.CanUndo, Is.False);
        }

        [Test]
        public void TheSameOwnerChangesNothing()
        {
            var history = Load(Baseline);

            GiveTo(history, 22, PlayerColor.Blue);

            Assert.That(history.CanUndo, Is.False);
        }

        [Test]
        public void TheOwnerChangeIsListedForTheHeroAndNothingIsListedForThePlayers()
        {
            var history = Load(Baseline);

            Defeat(history, 22);

            var changes = history.GetPendingChanges();
            Assert.That(changes.Where(c => c.Change.Description == "Owner").Select(c => (c.OwnerName, c.Change.Before, c.Change.After)), Is.EqualTo(new[] { ("Clancy", "Blue", "Neutral") }));
            Assert.That(changes.Where(c => c.Kind == Homm3ChunkKinds.Player), Is.Empty);
        }

        [Test]
        public void RevertingTheOwnerChangePutsTheListsAndTheTableBack()
        {
            var history = Load(Baseline);
            var original = history.Current;
            Defeat(history, 22, 17);

            history.GetPendingChanges().First(c => c.Change.Description == "Owner" && c.OwnerName == "Clancy").Revert(history);

            Assert.That(Head(history.Current, PlayerColor.Blue), Is.EqualTo(Head(original, PlayerColor.Blue)));
            Assert.That(Hero(history.Current, 22).Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That(history.Current.HeroOwners().GetModel(0).Heroes[22], Is.EqualTo(original.HeroOwners().GetModel(0).Heroes[22]));

            // Ufretin's defeat stays.
            Assert.That(Hero(history.Current, 17).Owner, Is.EqualTo(PlayerColor.Neutral));
            Assert.That(Player(history.Current, PlayerColor.Red).HeroIds, Does.Not.Contain(17));
            Assert.That(history.Current.HeroOwners().GetModel(0).Heroes[17], Is.EqualTo(HeroOwnership.Free));
        }

        [Test]
        public void RevertingEveryChangeGivesTheSavefileBack()
        {
            var history = Load(Week2Day1);
            var original = history.Current.ToBytes();
            Defeat(history, 55, 84, 102, 144);

            foreach (var change in history.GetPendingChanges().Where(c => c.Change.Description == "Owner").ToList())
            {
                change.Revert(history);
            }

            // Leaving the map is a change of its own (the position), so it is put back with the hero alone.
            foreach (var change in history.GetPendingChanges().ToList())
            {
                change.Revert(history);
            }

            Assert.That(history.GetPendingChanges(), Is.Empty);
            Assert.That(history.Current.ToBytes(), Is.EqualTo(original));
        }

        [Test]
        public void AHeroGivenBackByHandTakesItsOldPlaceTheSelectedHeroAndItsEntryOfTheTable()
        {
            var history = Load(Baseline);
            var original = history.Current;

            GiveTo(history, 22, PlayerColor.Tan);
            GiveTo(history, 22, PlayerColor.Blue);

            Assert.That(Head(history.Current, PlayerColor.Blue), Is.EqualTo(Head(original, PlayerColor.Blue)));
            Assert.That(Head(history.Current, PlayerColor.Tan), Is.EqualTo(Head(original, PlayerColor.Tan)));
            Assert.That(Table(history.Current), Is.EqualTo(Table(original)));
        }

        [Test]
        public void RevertingAnOwnerIntoAListThatBecameFullIsRefusedAndNothingIsWritten()
        {
            var history = Load(Baseline);
            Defeat(history, 22);
            while (Player(history.Current, PlayerColor.Blue).HeroIds.Count < Constants.Player.HeroListSize)
            {
                GiveTo(history, FreeHero(history.Current), PlayerColor.Blue);
            }

            var full = history.Current;
            var revert = history.GetPendingChanges().Single(c => c.Change.Description == "Owner" && c.OwnerName == "Clancy");

            Assert.That(() => revert.Revert(history), Throws.TypeOf<GameRuleException>().With.Message.EqualTo($"Blue has the most heroes a player can have on the map ({Constants.Player.HeroListSize})."));
            Assert.That(history.Current, Is.SameAs(full));
        }

        [Test]
        public void AnOwnerWrittenAsItIsLeavesTheListsAndTheTableAlone()
        {
            // An edit of the raw bytes goes into the history as it is (SaveDocumentHistory.ApplyAsIs), so the reconciler does not run.
            var history = Load(Baseline);
            var original = history.Current;
            var heroes = original.Heroes();
            var index = heroes.IndexOfId(22);

            history.ApplyAsIs(original.WithHero(index, heroes.Serializer.Serialize(heroes[index], heroes.GetModel(index).WithOwner(PlayerColor.Neutral))));

            Assert.That(Hero(history.Current, 22).Owner, Is.EqualTo(PlayerColor.Neutral));
            Assert.That(Head(history.Current, PlayerColor.Blue), Is.EqualTo(Head(original, PlayerColor.Blue)));
            Assert.That(Table(history.Current), Is.EqualTo(Table(original)));
        }

        [Test]
        public void UndoAndRedoOfAnOwnerChangeGiveBackTheDocumentsAsTheyWere()
        {
            var history = Load(Baseline);
            var original = history.Current;
            Defeat(history, 22);
            var defeated = history.Current;

            history.Undo();
            Assert.That(history.Current, Is.SameAs(original));
            history.Redo();
            Assert.That(history.Current, Is.SameAs(defeated));
        }
    }
}
