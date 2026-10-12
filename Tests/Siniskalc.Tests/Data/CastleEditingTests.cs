using Siniskalc;
using Siniskalc.Core.Data;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// Edits of the owner, the garrison, the dwellings and the Mage Guild of a castle, on the Bulwark savefile in which
    /// "Still Water" (Rampart) and "Jotunngard" (Bulwark) have every building but the support ones.
    /// </summary>
    [TestFixture]
    internal sealed class CastleEditingTests
    {
        private SaveDocumentHistory history = null!;
        private CastleEditSession still = null!;
        private CastleEditSession jotunngard = null!;
        private byte[] original = null!;

        [SetUp]
        public void Load()
        {
            var path = CastleSamples.Folder("Bulwark").Single(p => p.EndsWith("bulwark_24_nothing__fair.GM1"));
            this.history = CastleSamples.Load(path).History;
            this.original = this.history.Current.ToBytes();
            this.still = this.history.CreateCastleSession(this.history.Current.Castles().FindByName("Still Water"));
            this.jotunngard = this.history.CreateCastleSession(this.history.Current.Castles().FindByName("Jotunngard"));
        }

        private CastleModel Reloaded(string name)
        {
            var game = TestServices.GameService.LoadGameFromBytes(this.history.Current.ToBytes(), "edited.GM1");
            var castles = game.History.Current.Castles();
            return TestServices.CastleDeserializer.Deserialize(castles[castles.FindByName(name)]);
        }

        [Test]
        public void TheOwnerChangeIsPendingRevertsAndSurvivesSaving()
        {
            this.still.Update(c => c with { Owner = PlayerColor.Blue });

            var change = this.history.GetPendingChanges().Single();
            Assert.That((change.Path, change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Still Water › General", "Owner", "Red", "Blue")));
            Assert.That(this.Reloaded("Still Water").Owner, Is.EqualTo(PlayerColor.Blue));

            change.Revert(this.history);
            Assert.That(this.history.GetPendingChanges(), Is.Empty);
            Assert.That(this.history.Current.ToBytes(), Is.EqualTo(this.original));
        }

        [Test]
        public void ACastleCanBecomeNeutral()
        {
            this.still.Update(c => c with { Owner = null });

            Assert.That(this.history.GetPendingChanges().Single().Change.After, Is.EqualTo("Neutral"));
            Assert.That(this.Reloaded("Still Water").Owner, Is.Null);
        }

        [Test]
        public void TheGarrisonSlotsAreEditedPendingAndReverted()
        {
            this.still.Update(c => c with { Garrison = c.Garrison.WithSlot(1, Creature.GoldDragon, 5) });
            this.still.Update(c => c with { Garrison = c.Garrison.WithSlot(4, Creature.Centaur, 120) });

            Assert.That(this.history.GetPendingChanges().Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EqualTo(new[]
            {
                ("Slot 1", "empty", "Gold Dragon × 5"),
                ("Slot 4", "empty", "Centaur × 120"),
            }));
            var reloaded = this.Reloaded("Still Water").Garrison;
            Assert.That(reloaded.GetSlot(1), Is.EqualTo(new ArmySlot(1, Creature.GoldDragon.Code, 5)));
            Assert.That(reloaded.GetSlot(4), Is.EqualTo(new ArmySlot(4, Creature.Centaur.Code, 120)));

            this.history.GetPendingChanges()[0].Revert(this.history);
            Assert.That(this.history.GetPendingChanges().Select(c => c.Change.Description), Is.EqualTo(new[] { "Slot 4" }));

            this.still.Update(c => c with { Garrison = c.Garrison.WithSlot(4, Creature.Centaur, 7) });
            Assert.That(this.history.GetPendingChanges().Single().Change.After, Is.EqualTo("Centaur × 7"));

            this.history.GetPendingChanges().Single().Revert(this.history);
            Assert.That(this.history.Current.ToBytes(), Is.EqualTo(this.original));
        }

        [Test]
        public void ACreatureReplacedInTheGarrisonRevertsToTheOriginalCreature()
        {
            this.still.Update(c => c with { Garrison = c.Garrison.WithSlot(2, Creature.Pikeman, 10) });
            this.history.MarkSaved();
            this.still.Update(c => c with { Garrison = c.Garrison.WithSlot(2, Creature.Angel, 1) });

            var change = this.history.GetPendingChanges().Single();
            Assert.That((change.Change.Before, change.Change.After), Is.EqualTo(("Pikeman × 10", "Angel × 1")));

            change.Revert(this.history);
            Assert.That(this.still.Current.Garrison.GetSlot(2), Is.EqualTo(new ArmySlot(2, Creature.Pikeman.Code, 10)));
        }

        [Test]
        public void TheCreaturesWaitingInADwellingAreEditedOnTheStandingDwelling()
        {
            // The Rampart dwelling of level 1 is still the centaur stables; in Bulwark the colliery is not upgraded yet.
            Assert.That(CastleBuilder.DwellingOf(this.still.Current, 0), Is.EqualTo(Building.RampartCentaurStables));
            this.still.Update(c => CastleBuilder.SetCreaturesWaiting(c, 0, 99));
            this.jotunngard.Update(c => CastleBuilder.SetCreaturesWaiting(c, 6, 1234));

            Assert.That(this.history.GetPendingChanges().Select(c => (c.OwnerName, c.Change.Area, c.Change.Description, c.Change.After)), Is.EqualTo(new[]
            {
                ("Still Water", ChangeAreas.Dwellings, "Centaur Stables creatures", "99"),
                ("Jotunngard", ChangeAreas.Dwellings, "Frosthome creatures", "1,234"),
            }).Or.EqualTo(new[]
            {
                ("Still Water", ChangeAreas.Dwellings, "Centaur Stables creatures", "99"),
                ("Jotunngard", ChangeAreas.Dwellings, "Frosthome creatures", "1.234"),
            }));
            Assert.That(this.Reloaded("Still Water").CreaturesAvailable[0], Is.EqualTo(99));
            Assert.That(this.Reloaded("Jotunngard").CreaturesAvailable[6], Is.EqualTo(1234));

            foreach (var change in this.history.GetPendingChanges())
            {
                change.Revert(this.history);
            }

            Assert.That(this.history.Current.ToBytes(), Is.EqualTo(this.original));
        }

        [Test]
        public void TheUpgradedDwellingHoldsItsOwnCount()
        {
            this.still.Update(c => CastleBuilder.Build(c, Building.RampartUpgCentaurStables));
            var moved = this.still.Current.UpgradedCreaturesAvailable[0];

            this.still.Update(c => CastleBuilder.SetCreaturesWaiting(c, 0, moved + 10));

            Assert.That(this.still.Current.UpgradedCreaturesAvailable[0], Is.EqualTo(moved + 10));
            Assert.That(this.still.Current.CreaturesAvailable[0], Is.EqualTo(0));
            Assert.That(this.history.GetPendingChanges().Select(c => c.Change.Description), Is.EqualTo(new[] { "Upg. Centaur Stables" }));
        }

        [Test]
        public void ADwellingThatIsNotBuiltCannotHoldCreatures()
        {
            var empty = CastleBuilder.Unbuild(this.still.Current, Building.RampartDragonCliffs);

            Assert.That(CastleBuilder.DwellingOf(empty, 6), Is.Null);
            Assert.That(CastleBuilder.CreaturesWaiting(empty, 6), Is.Null);
            Assert.Throws<GameRuleException>(() => CastleBuilder.SetCreaturesWaiting(empty, 6, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => CastleBuilder.SetCreaturesWaiting(this.still.Current, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => CastleBuilder.SetCreaturesWaiting(this.still.Current, 0, (int)Constants.Castle.MaxDwellingCreatures + 1));
        }

        [Test]
        public void TheMageGuildLevelIsEditedThroughItsBuildingsAndSurvivesSaving()
        {
            this.still.Update(c => CastleBuilder.SetMageGuildLevel(c, 2));

            Assert.That(this.history.GetPendingChanges().Select(c => c.Change.Description), Is.EqualTo(new[] { "Mage Guild, level 3", "Mage Guild, level 4" }));
            var reloaded = this.Reloaded("Still Water");
            Assert.That(reloaded.MageGuildLevel, Is.EqualTo(2));
            Assert.That(reloaded.SpellCounts, Is.EqualTo(new[] { 5, 4, 0, 0, 0 }));
            Assert.That(reloaded.CurrentBuildings, Does.Contain(Building.MagesGuild2).And.Not.Contain(Building.MagesGuild3));

            this.history.GetPendingChanges().First().Revert(this.history);
            Assert.That(this.still.Current.MageGuildLevel, Is.EqualTo(3));
            Assert.That(this.history.GetPendingChanges().Select(c => c.Change.Description), Is.EqualTo(new[] { "Mage Guild, level 4" }));

            this.history.GetPendingChanges().Single().Revert(this.history);
            Assert.That(this.history.GetPendingChanges(), Is.Empty);
            Assert.That(this.history.Current.ToBytes(), Is.EqualTo(this.original));
        }

        [Test]
        public void BuiltThisTurnCanBeSetAndRevertedAndNothingElseChanges()
        {
            this.still.Update(c => c with { BuiltThisTurn = true });

            var change = this.history.GetPendingChanges().Single();
            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Built this turn", "no", "yes")));

            change.Revert(this.history);
            Assert.That(this.history.Current.ToBytes(), Is.EqualTo(this.original));
        }

        [Test]
        public void EveryEditUndoesInTheOrderItWasMade()
        {
            this.still.Update(c => c with { Owner = PlayerColor.Green });
            this.still.Update(c => CastleBuilder.Unbuild(c, Building.RampartDragonCliffs));
            this.still.Update(c => c with { Garrison = c.Garrison.WithSlot(7, Creature.Angel, 2) });

            while (this.history.CanUndo)
            {
                this.history.Undo();
            }

            Assert.That(this.history.Current.ToBytes(), Is.EqualTo(this.original));
        }

        [Test]
        public void ChangesOfCastlesHeroesAndPlayersAreListedTogether()
        {
            this.still.Update(c => c with { Owner = PlayerColor.Blue });
            this.history.CreatePlayerSession(0).Update(p => p with { Resources = p.Resources.With(PlayerResources.All[0], 1) });

            var changes = this.history.GetPendingChanges();

            Assert.That(changes.Select(c => c.Kind), Is.EquivalentTo(new[] { Homm3ChunkKinds.Player, Homm3ChunkKinds.Castle }));
        }
    }
}
