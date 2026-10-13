namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// Editing models and saving them: only the bytes that were edited change, and what the editor does not know is left alone.
    /// </summary>
    [TestFixture]
    internal sealed class Homm2EditingTests
    {
        private Homm2Game game = null!;

        [SetUp]
        public void LoadTheStart() => this.game = Homm2TestServices.LoadBrokenAlliance(1);

        private static IReadOnlyList<int> DifferingOffsets(byte[] a, byte[] b) => Enumerable.Range(0, a.Length).Where(i => a[i] != b[i]).ToList();

        [Test]
        public void EditingTheGoldChangesTheFourBytesOfTheGoldAndNothingElse()
        {
            this.game.History.CreatePlayerSession(0).Update(p => p with { Resources = p.Resources.With(Homm2Resource.Gold, 99_999) });

            var saved = this.game.GetBytesToSave();

            var goldAt = this.game.Layout.PlayerStart(0) + Homm2PlayerLayout.Resources + (6 * 4);
            Assert.That(DifferingOffsets(this.game.Bytes, saved), Is.Not.Empty.And.All.InRange(goldAt, goldAt + 3));
            Assert.That(BitConverter.ToUInt32(saved, goldAt), Is.EqualTo(99_999));
            Assert.That(Homm2TestServices.Parser.Parse(saved, null).History.Current.Players().GetModel(0).Resources.Gold, Is.EqualTo(99_999));
        }

        [Test]
        public void EverythingAHeroHoldsCanBeEditedAndReadBack()
        {
            var session = this.game.History.CreateHeroSession(7);
            var edited = session.Current with
            {
                SpellPoints = 77,
                MovementLeft = 500,
                Experience = 12_345,
                Level = 5,
                PrimarySkills = new Homm2PrimarySkills(9, 8, 7, 6),
                SecondarySkills = Homm2SecondarySkills.None
                    .With(Homm2SecondarySkill.Wisdom, Homm2SkillLevel.Expert)
                    .With(Homm2SecondarySkill.Estates, Homm2SkillLevel.Advanced)
                    .With(Homm2SecondarySkill.Pathfinding, Homm2SkillLevel.Basic),
                Army = Homm2Army.Empty.WithSlot(0, new Homm2ArmySlot(Homm2Creature.Titan, 3)).WithSlot(4, new Homm2ArmySlot(Homm2Creature.Peasant, 32_767)),
                Artifacts = Homm2Artifacts.None.Add(Homm2Artifact.UltimateCrown).Add(Homm2Artifact.IceCloak),
                Spells = new Homm2SpellBook([Homm2Spell.Fireball, Homm2Spell.TownPortal]),
                Owner = Homm2Color.Red,
                Destination = new Homm2Position(3, 4),
            };

            session.Update(_ => edited);

            var reread = Homm2TestServices.Parser.Parse(this.game.GetBytesToSave(), null).History.Current.Heroes().GetModel(7);
            Assert.That(reread, Is.EqualTo(edited));
            Assert.That(reread.SecondarySkills.Entries.Select(e => e.Skill), Is.EqualTo(new[] { Homm2SecondarySkill.Wisdom, Homm2SecondarySkill.Estates, Homm2SecondarySkill.Pathfinding }));
        }

        [Test]
        public void AddingTheSpellBookGivesTheMagicBookAndRemovingItTakesItBack()
        {
            var session = this.game.History.CreateHeroSession(7);

            session.Update(h => h.WithSpellBook());
            Assert.That(session.Current.HasSpellBook, Is.True);
            Assert.That(session.Current.Artifacts.Slots[0], Is.EqualTo(Homm2Artifact.MagicBook));

            session.Update(h => h.WithoutSpellBook());
            Assert.That(session.Current.HasSpellBook, Is.False);
        }

        [Test]
        public void ACastleCanBeGivenToAPlayerWithAGarrisonAndNewBuildings()
        {
            var session = this.game.History.CreateCastleSession(3);

            session.Update(c => c with
            {
                Owner = Homm2Color.Green,
                Garrison = c.Garrison.WithSlot(0, new Homm2ArmySlot(Homm2Creature.Paladin, 10)),
                Town = c.Town | Homm2TownBuildings.Tavern | Homm2TownBuildings.Statue,
                Dwellings = c.Dwellings | Homm2Dwellings.Dwelling3 | Homm2Dwellings.MageGuild2,
                Creatures = c.Creatures.With(2, 4),
            });

            var castle = Homm2TestServices.Parser.Parse(this.game.GetBytesToSave(), null).History.Current.Castles().GetModel(3);
            Assert.That(castle.Owner, Is.EqualTo(Homm2Color.Green));
            Assert.That(castle.Garrison.Slots[0], Is.EqualTo(new Homm2ArmySlot(Homm2Creature.Paladin, 10)));
            Assert.That(castle.Town, Is.EqualTo(Homm2TownBuildings.Castle | Homm2TownBuildings.Tavern | Homm2TownBuildings.Statue));
            Assert.That(castle.MageGuildLevel, Is.EqualTo(2));
            Assert.That(castle.HasDwelling(3), Is.True);
            Assert.That(castle.Creatures[2], Is.EqualTo(4));
        }

        [Test]
        public void AValueTheEditorDoesNotKnowIsReadAsAKnownOneAndLeftAloneUntilTheModelChangesIt()
        {
            var heroes = this.game.History.Current.Heroes();
            var bytes = heroes[7].ToArray();
            bytes[Homm2HeroLayout.ArmyTypes] = 0xC8;
            bytes[Homm2HeroLayout.Artifacts + 5] = 0xC9;
            bytes[Homm2HeroLayout.Owner] = 0x77;
            var chunk = heroes[7].WithBytes(bytes);

            var model = Homm2TestServices.HeroDeserializer.Deserialize(chunk);
            Assert.That(model.Army.Slots[0]!.Creature, Is.EqualTo(Homm2Creature.Peasant));
            Assert.That(model.Artifacts.Slots[5], Is.EqualTo(Homm2Artifact.UltimateBookOfKnowledge));
            Assert.That(model.Owner, Is.Null);

            var untouched = Homm2TestServices.HeroSerializer.Serialize(chunk, model).ToArray();
            Assert.That(untouched[Homm2HeroLayout.ArmyTypes], Is.EqualTo(0xC8));
            Assert.That(untouched[Homm2HeroLayout.Artifacts + 5], Is.EqualTo(0xC9));
            Assert.That(untouched[Homm2HeroLayout.Owner], Is.EqualTo(0x77));

            var changed = Homm2TestServices.HeroSerializer.Serialize(chunk, model with { Army = model.Army.WithSlot(0, new Homm2ArmySlot(Homm2Creature.Archer, 1)), Owner = Homm2Color.Blue }).ToArray();
            Assert.That(changed[Homm2HeroLayout.ArmyTypes], Is.EqualTo(Homm2Creature.Archer.Code));
            Assert.That(changed[Homm2HeroLayout.Owner], Is.EqualTo((byte)Homm2Color.Blue));
        }

        [Test]
        public void ThePriceOfLoyaltyBytesAtTheEndOfAHeroAreNeverTouched()
        {
            var game = Homm2TestServices.Load(Homm2Saves.Path(Homm2Saves.ArraxTheJerk, "arrax_the_jerk_01_start"));
            var chunk = game.History.Current.Heroes()[16];
            var model = Homm2TestServices.HeroDeserializer.Deserialize(chunk) with { Experience = 5000, Level = 3 };

            var written = Homm2TestServices.HeroSerializer.Serialize(chunk, model).ToArray();

            Assert.That(written, Has.Length.EqualTo(250));
            Assert.That(written.AsSpan(Homm2HeroLayout.SharedLength).ToArray(), Is.EqualTo(chunk.ToArray().AsSpan(Homm2HeroLayout.SharedLength).ToArray()));
        }

        [Test]
        public void ValuesBeyondTheLimitsAreRefused()
        {
            var session = this.game.History.CreatePlayerSession(0);

            Assert.That(() => session.Update(p => p with { Resources = p.Resources.With(Homm2Resource.Gold, -1) }), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => this.game.History.CreateHeroSession(7).Update(h => h with { Level = 300 }), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => this.game.History.CreateHeroSession(7).Update(h => h with { Army = h.Army.WithSlot(0, new Homm2ArmySlot(Homm2Creature.Peasant, 70_000)) }), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(this.game.History.CanUndo, Is.False);
        }

        [Test]
        public void EditsArePendingChangesThatCanBePutBackOneByOneAndUndone()
        {
            this.game.History.CreatePlayerSession(0).Update(p => p with { Resources = p.Resources.With(Homm2Resource.Gold, 9_999).With(Homm2Resource.Ore, 1) });
            this.game.History.CreateHeroSession(7).Update(h => h with { Army = h.Army.WithSlot(2, new Homm2ArmySlot(Homm2Creature.Roc, 2)) });

            var changes = this.game.History.GetPendingChanges();

            Assert.That(changes.Select(c => $"{c.Path}: {c.Change.Description} {c.Change.Before} -> {c.Change.After}"), Is.EqualTo(new[]
            {
                "Maximus › Army: Slot 3 empty -> Roc × 2",
                "Blue › Resources: Ore 20 -> 1",
                "Blue › Resources: Gold 7,500 -> 9,999",
            }));

            changes.Single(c => c.Change.Description == "Ore").Revert(this.game.History);
            Assert.That(this.game.History.Current.Players().GetModel(0).Resources.Ore, Is.EqualTo(20));
            Assert.That(this.game.History.GetPendingChanges(), Has.Count.EqualTo(2));

            this.game.History.Undo();
            Assert.That(this.game.History.Current.Players().GetModel(0).Resources.Ore, Is.EqualTo(1));
            this.game.History.Apply(this.game.History.Original);
            Assert.That(this.game.History.GetPendingChanges(), Is.Empty);
        }

        [Test]
        public void SavingMarksTheEditsAsTheNewStartingPoint()
        {
            this.game.History.CreateCastleSession(3).Update(c => c with { Creatures = c.Creatures.With(0, 99) });
            Assert.That(this.game.History.GetPendingChanges().Single().Path, Is.EqualTo("Xabran › Dwellings"));

            this.game.History.MarkSaved();

            Assert.That(this.game.History.GetPendingChanges(), Is.Empty);
        }
    }
}
