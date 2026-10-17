namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// Editing models and saving them: only the bytes that were edited change, and what the editor does not know is left alone.
    /// </summary>
    [TestFixture]
    internal sealed class EditingTests
    {
        private Game game = null!;

        [SetUp]
        public void LoadTheStart() => this.game = TestServices.LoadBrokenAlliance(1);

        private static IReadOnlyList<int> DifferingOffsets(byte[] a, byte[] b) => Enumerable.Range(0, a.Length).Where(i => a[i] != b[i]).ToList();

        [Test]
        public void EditingTheGoldChangesTheFourBytesOfTheGoldAndNothingElse()
        {
            this.game.History.CreatePlayerSession(0).Update(p => p with { Resources = p.Resources.With(Resource.Gold, 99_999) });

            var saved = this.game.GetBytesToSave();

            var goldAt = this.game.Layout.PlayerStart(0) + PlayerLayout.Resources + (6 * 4);
            Assert.That(DifferingOffsets(this.game.Bytes, saved), Is.Not.Empty.And.All.InRange(goldAt, goldAt + 3));
            Assert.That(BitConverter.ToUInt32(saved, goldAt), Is.EqualTo(99_999));
            Assert.That(TestServices.Parser.Parse(saved, null).History.Current.Players().GetModel(0).Resources.Gold, Is.EqualTo(99_999));
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
                PrimarySkills = new PrimarySkills(9, 8, 7, 6),
                SecondarySkills = SecondarySkills.None
                    .With(SecondarySkill.Wisdom, SkillLevel.Expert)
                    .With(SecondarySkill.Estates, SkillLevel.Advanced)
                    .With(SecondarySkill.Pathfinding, SkillLevel.Basic),
                Army = Army.Empty.WithSlot(0, new ArmySlot(Creature.Titan, 3)).WithSlot(4, new ArmySlot(Creature.Peasant, 32_767)),
                Artifacts = Artifacts.None.Add(Artifact.UltimateCrown).Add(Artifact.IceCloak),
                Spells = new SpellBook([Spell.Fireball, Spell.TownPortal]),
                Owner = Color.Red,
                Destination = new Position(3, 4),
            };

            session.Update(_ => edited);

            var reread = TestServices.Parser.Parse(this.game.GetBytesToSave(), null).History.Current.Heroes().GetModel(7);
            Assert.That(reread, Is.EqualTo(edited));
            Assert.That(reread.SecondarySkills.Entries.Select(e => e.Skill), Is.EqualTo(new[] { SecondarySkill.Wisdom, SecondarySkill.Estates, SecondarySkill.Pathfinding }));
        }

        [Test]
        public void AddingTheSpellBookGivesTheMagicBookAndRemovingItTakesItBack()
        {
            var session = this.game.History.CreateHeroSession(7);

            session.Update(h => h.WithSpellBook());
            Assert.That(session.Current.HasSpellBook, Is.True);
            Assert.That(session.Current.Artifacts.Slots[0], Is.EqualTo(Artifact.MagicBook));

            session.Update(h => h.WithoutSpellBook());
            Assert.That(session.Current.HasSpellBook, Is.False);
        }

        [Test]
        public void ACastleCanBeGivenToAPlayerWithAGarrisonAndNewBuildings()
        {
            var session = this.game.History.CreateCastleSession(3);

            session.Update(c => c with
            {
                Owner = Color.Green,
                Garrison = c.Garrison.WithSlot(0, new ArmySlot(Creature.Paladin, 10)),
                Town = c.Town | TownBuildings.Tavern | TownBuildings.Statue,
                Dwellings = c.Dwellings | Dwellings.Dwelling3 | Dwellings.MageGuild2,
                Creatures = c.Creatures.With(2, 4),
            });

            var castle = TestServices.Parser.Parse(this.game.GetBytesToSave(), null).History.Current.Castles().GetModel(3);
            Assert.That(castle.Owner, Is.EqualTo(Color.Green));
            Assert.That(castle.Garrison.Slots[0], Is.EqualTo(new ArmySlot(Creature.Paladin, 10)));
            Assert.That(castle.Town, Is.EqualTo(TownBuildings.Castle | TownBuildings.Tavern | TownBuildings.Statue));
            Assert.That(castle.MageGuildLevel, Is.EqualTo(2));
            Assert.That(castle.HasDwelling(3), Is.True);
            Assert.That(castle.Creatures[2], Is.EqualTo(4));
        }

        [Test]
        public void AValueTheEditorDoesNotKnowIsReadAsAKnownOneAndLeftAloneUntilTheModelChangesIt()
        {
            var heroes = this.game.History.Current.Heroes();
            var bytes = heroes[7].ToArray();
            bytes[HeroLayout.ArmyTypes] = 0xC8;
            bytes[HeroLayout.Artifacts + 5] = 0xC9;
            bytes[HeroLayout.Owner] = 0x77;
            var chunk = heroes[7].WithBytes(bytes);

            var model = TestServices.HeroDeserializer.Deserialize(chunk);
            Assert.That(model.Army.Slots[0]!.Creature, Is.EqualTo(Creature.Peasant));
            Assert.That(model.Artifacts.Slots[5], Is.EqualTo(Artifact.UltimateBookOfKnowledge));
            Assert.That(model.Owner, Is.Null);

            var untouched = TestServices.HeroSerializer.Serialize(chunk, model).ToArray();
            Assert.That(untouched[HeroLayout.ArmyTypes], Is.EqualTo(0xC8));
            Assert.That(untouched[HeroLayout.Artifacts + 5], Is.EqualTo(0xC9));
            Assert.That(untouched[HeroLayout.Owner], Is.EqualTo(0x77));

            var changed = TestServices.HeroSerializer.Serialize(chunk, model with { Army = model.Army.WithSlot(0, new ArmySlot(Creature.Archer, 1)), Owner = Color.Blue }).ToArray();
            Assert.That(changed[HeroLayout.ArmyTypes], Is.EqualTo(Creature.Archer.Code));
            Assert.That(changed[HeroLayout.Owner], Is.EqualTo((byte)Color.Blue));
        }

        [Test]
        public void ThePriceOfLoyaltyBytesAtTheEndOfAHeroAreNeverTouched()
        {
            var game = TestServices.Load(Saves.Path(Saves.ArraxTheJerk, "arrax_the_jerk_01_start"));
            var chunk = game.History.Current.Heroes()[16];
            var model = TestServices.HeroDeserializer.Deserialize(chunk) with { Experience = 5000, Level = 3 };

            var written = TestServices.HeroSerializer.Serialize(chunk, model).ToArray();

            Assert.That(written, Has.Length.EqualTo(250));
            Assert.That(written.AsSpan(HeroLayout.SharedLength).ToArray(), Is.EqualTo(chunk.ToArray().AsSpan(HeroLayout.SharedLength).ToArray()));
        }

        [Test]
        public void ValuesBeyondTheLimitsAreRefused()
        {
            var session = this.game.History.CreatePlayerSession(0);

            Assert.That(() => session.Update(p => p with { Resources = p.Resources.With(Resource.Gold, -1) }), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => this.game.History.CreateHeroSession(7).Update(h => h with { Level = 300 }), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => this.game.History.CreateHeroSession(7).Update(h => h with { Army = h.Army.WithSlot(0, new ArmySlot(Creature.Peasant, 70_000)) }), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(this.game.History.CanUndo, Is.False);
        }

        [Test]
        public void EditsArePendingChangesThatCanBePutBackOneByOneAndUndone()
        {
            this.game.History.CreatePlayerSession(0).Update(p => p with { Resources = p.Resources.With(Resource.Gold, 9_999).With(Resource.Ore, 1) });
            this.game.History.CreateHeroSession(7).Update(h => h with { Army = h.Army.WithSlot(2, new ArmySlot(Creature.Roc, 2)) });

            var changes = this.game.History.GetPendingChanges();

            Assert.That(changes.Select(c => $"{c.Path}: {c.Change.Description} {c.Change.Before} -> {c.Change.After}"), Is.EqualTo(new[]
            {
                "Maximus › Creatures: Slot 3 empty -> Roc × 2",
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
