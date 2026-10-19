namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The mines, the shrines and the sphinx of Threedom (and the map of Arrax the Jerk): read from the saves, edited, and written back.
    /// </summary>
    [TestFixture]
    internal sealed class MapObjectsTests
    {
        private static MineModel Mine(int save, int x, int y) => TestServices.LoadThreedom(save).History.Current.Mines().GetModels().Single(m => m.Position == new MapPosition(x, y));

        private static MapObjectChunk MineChunk(int save, int x, int y) => TestServices.LoadThreedom(save).History.Current.Mines().Single(c => c.Position == new MapPosition(x, y));

        [Test]
        public void TheMapOfThreedomHasFortyOneMinesWithTheirKindAndPositionAndOwner()
        {
            var mines = TestServices.LoadThreedom(71).History.Current.Mines().GetModels();

            Assert.That(mines, Has.Count.EqualTo(41));
            Assert.That(mines[0], Is.EqualTo(new MineModel(0, MineKind.SulfurMine, new MapPosition(67, 1), Color.Blue)));
            Assert.That(Mine(71, 44, 56), Is.EqualTo(new MineModel(32, MineKind.Sawmill, new MapPosition(44, 56), Color.Blue)).Using<MineModel>((a, b) => a.Kind == b.Kind && a.Position == b.Position && a.Owner == b.Owner));
            Assert.That(Mine(71, 63, 43).Owner, Is.EqualTo(Color.Red));
            Assert.That(Mine(71, 55, 29).Owner, Is.Null);
        }

        [Test]
        public void ACapturedMineChangesOwnerInTheSaveOfItsCapture()
        {
            Assert.That(Mine(6, 34, 53).Owner, Is.Null);
            Assert.That(Mine(7, 34, 53).Owner, Is.EqualTo(Color.Blue), "Colar Blau captured the ore mine under him in save 7");
            Assert.That(Mine(12, 44, 56).Owner, Is.Null);
            Assert.That(Mine(13, 44, 56).Owner, Is.EqualTo(Color.Blue), "and the sawmill in save 13");
        }

        [Test]
        public void TheFlagOfAMineIsItsFirstPicturePlusTheColourOfItsOwnerAndTheMinesWithoutOwnerHaveNone()
        {
            foreach (var chunk in TestServices.LoadThreedom(71).History.Current.Mines())
            {
                var mine = TestServices.Mines.Deserialize(chunk);
                foreach (var part in chunk.Parts)
                {
                    var expected = mine.Owner is { } owner ? new[] { MapObjectLayout.FlagClass, (byte)(MineKinds.FlagPicture(mine.Kind) + owner.Code) } : [MapObjectLayout.NoClass, MapObjectLayout.NoPicture];
                    Assert.That(part.Bytes, Is.EqualTo(expected), mine.Name);
                }
            }

            Assert.That(MineChunk(71, 34, 53).Parts, Has.Count.EqualTo(1), "the flag of the ore mine is in an addon, as the sprite of the mine fills the tile's second object");
            Assert.That(MineChunk(71, 64, 11).Parts, Is.Empty, "an alchemist's lab has no flag to move");
        }

        [Test]
        public void GivingAMineToAnotherPlayerWritesTheOwnerAndTheFlagAndNothingElse()
        {
            var game = TestServices.LoadThreedom(71);
            var chunk = game.History.Current.Mines().Single(c => c.Position == new MapPosition(34, 53));
            var index = game.History.Current.Mines().ToList().IndexOf(chunk);

            game.History.CreateMineSession(index).Update(m => m with { Owner = Color.Green });

            var saved = game.GetBytesToSave();
            var changed = Enumerable.Range(0, saved.Length).Where(i => saved[i] != game.Bytes[i]).ToList();
            Assert.That(changed, Has.Count.EqualTo(2));
            Assert.That(saved[chunk.StartIndex], Is.EqualTo(Color.Green.Code));
            Assert.That(saved[chunk.Parts[0].StartIndex + 1], Is.EqualTo((byte)(14 + Color.Green.Code)));

            var again = TestServices.Parser.Parse(saved, null).History.Current.Mines().GetModels().Single(m => m.Position == new MapPosition(34, 53));
            Assert.That(again.Owner, Is.EqualTo(Color.Green));

            game.History.CreateMineSession(index).Update(m => m with { Owner = null });
            var none = game.GetBytesToSave();
            Assert.That(none[chunk.StartIndex], Is.EqualTo((byte)0xFF));
            Assert.That(none[chunk.Parts[0].StartIndex], Is.EqualTo(MapObjectLayout.NoClass));
            Assert.That(game.History.GetPendingChanges().Single().Change.After, Is.EqualTo("Neutral"));
        }

        [Test]
        public void EveryObjectOfEverySaveIsWrittenBackAsTheSameBytes()
        {
            foreach (var path in Saves.All())
            {
                var game = TestServices.Load(path);
                var document = game.History.Current;
                foreach (var chunk in document.Mines())
                {
                    Assert.That(TestServices.Mines.Serialize(chunk, TestServices.Mines.Deserialize(chunk)).HasSameBytes(chunk), Is.True, $"{Path.GetFileName(path)}: {chunk.Name}");
                }

                foreach (var chunk in document.Shrines())
                {
                    Assert.That(TestServices.Shrines.Serialize(chunk, TestServices.Shrines.Deserialize(chunk)).HasSameBytes(chunk), Is.True, $"{Path.GetFileName(path)}: {chunk.Name}");
                }

                foreach (var chunk in document.Sphinxes())
                {
                    Assert.That(TestServices.Sphinxes.Serialize(chunk, TestServices.Sphinxes.Deserialize(chunk)).HasSameBytes(chunk), Is.True, $"{Path.GetFileName(path)}: {chunk.Name}");
                }
            }
        }

        [Test]
        public void TheShrinesOfThreedomTeachTheSpellsCharityLearned()
        {
            var shrines = TestServices.LoadThreedom(77).History.Current.Shrines().GetModels();

            Assert.That(shrines, Has.Count.EqualTo(13));
            Assert.That(shrines.Count(s => s.Circle == 1), Is.EqualTo(4));
            Assert.That(shrines.Count(s => s.Circle == 2), Is.EqualTo(3));
            Assert.That(shrines.Count(s => s.Circle == 3), Is.EqualTo(6));
            var first = shrines.Single(s => s.Position == new MapPosition(45, 7));
            Assert.That(first.Spell, Is.EqualTo(Spell.Shield), "Charity learned Shield here in save 20");
            Assert.That(shrines.Single(s => s.Position == new MapPosition(29, 42)).Spell, Is.EqualTo(Spell.MassHaste), "save 78");
            Assert.That(shrines.Single(s => s.Position == new MapPosition(31, 47)).Spell, Is.EqualTo(Spell.MassBless), "save 79");
        }

        [Test]
        public void AShrineAHeroStandsOnHasNoSpellToReadAndIsLeftAlone()
        {
            var game = TestServices.LoadThreedom(78);
            var shrines = game.History.Current.Shrines();
            var index = shrines.ToList().FindIndex(c => c.Position == new MapPosition(29, 42));

            Assert.That(shrines.GetModel(index).SpellCode, Is.EqualTo(-1));
            game.History.CreateShrineSession(index).Update(s => s with { SpellCode = Spell.Fireball.Code });
            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
        }

        [Test]
        public void ChangingTheSpellOfAShrineWritesTheSpellShiftedByThreeAndKeepsTheLowBits()
        {
            var game = TestServices.LoadThreedom(77);
            var shrines = game.History.Current.Shrines();
            var index = shrines.ToList().FindIndex(c => c.Position == new MapPosition(45, 7));
            var chunk = shrines[index];
            var before = chunk.GetUInt16At(0);

            game.History.CreateShrineSession(index).Update(s => s.WithSpell(Spell.Fireball));

            var saved = game.GetBytesToSave();
            var after = BitConverter.ToUInt16(saved, chunk.StartIndex);
            Assert.That(after >> 3, Is.EqualTo(Spell.Fireball.Code));
            Assert.That(after & 7, Is.EqualTo(before & 7));
            Assert.That(TestServices.Parser.Parse(saved, null).History.Current.Shrines().GetModel(index).Spell, Is.EqualTo(Spell.Fireball));
        }

        [Test]
        public void TheSphinxOfThreedomAsksItsRiddleAndAcceptsTwoAnswers()
        {
            var sphinx = TestServices.LoadThreedom(73).History.Current.Sphinxes().GetModel(0);

            Assert.That(sphinx.Position, Is.EqualTo(new MapPosition(13, 31)));
            Assert.That(sphinx.IsActive, Is.True);
            Assert.That(sphinx.Reward, Is.EqualTo(new[] { 0, 0, 0, 0, 0, 0, 8000 }));
            Assert.That(sphinx.ArtifactCode, Is.EqualTo(SphinxModel.NoArtifact));
            Assert.That(sphinx.Answers, Is.EqualTo(new[] { "darkness", "shadow" }));
            Assert.That(sphinx.Riddle, Does.StartWith("I Only Creep.").And.EndWith("What Am I?"));
            Assert.That(TestServices.LoadThreedom(74).History.Current.Sphinxes().GetModel(0).IsActive, Is.False, "Colar Blau answered it in save 74");
        }

        [Test]
        public void TheSphinxOfArrax_TheJerkHasSixAnswersAndGivesMercury()
        {
            var game = TestServices.Load(Saves.Path(Saves.ArraxTheJerk, "arrax_the_jerk_01_start"));

            var sphinx = game.History.Current.Sphinxes().GetModels().Single();

            Assert.That(sphinx.Reward, Is.EqualTo(new[] { 0, 15, 0, 0, 0, 0, 0 }));
            Assert.That(sphinx.Answers, Is.EqualTo(new[] { "1", "one", "1 bushel", "one bushel", "1 bushel.", "1." }));
            Assert.That(sphinx.Riddle, Does.StartWith("I have 10 Bushels of Grain."));
        }

        [Test]
        public void ASphinxIsMadeActiveAgainAndGivenNewAnswersAndAShorterRiddleWithinItsRoom()
        {
            var game = TestServices.LoadThreedom(74);
            var session = game.History.CreateSphinxSession(0);

            session.Update(s => s with { IsActive = true, Answers = ["darkness", "shadow", "shade"], Riddle = "What is always there?", Reward = s.Reward.Select((r, i) => i == 6 ? 1234 : r).ToList() });

            var again = TestServices.Parser.Parse(game.GetBytesToSave(), null).History.Current.Sphinxes().GetModel(0);
            Assert.That(again.IsActive, Is.True);
            Assert.That(again.Answers, Is.EqualTo(new[] { "darkness", "shadow", "shade" }));
            Assert.That(again.Riddle, Is.EqualTo("What is always there?"));
            Assert.That(again.Reward[6], Is.EqualTo(1234));
            Assert.That(game.History.GetPendingChanges().Select(c => c.Change.Description), Is.SupersetOf(new[] { "Riddle", "Answers", "Riddle text", "Reward: Gold" }));
            Assert.That(game.GetBytesToSave(), Has.Length.EqualTo(game.Bytes.Length));
        }

        [Test]
        public void ASphinxRefusesWhatDoesNotFitItsBlock()
        {
            var game = TestServices.LoadThreedom(74);
            var session = game.History.CreateSphinxSession(0);

            Assert.That(() => session.Update(s => s with { Riddle = new string('x', s.RiddleCapacity + 1) }), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => session.Update(s => s with { Answers = ["this answer is too long"] }), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => session.Update(s => s with { Answers = Enumerable.Range(0, 9).Select(i => "a" + i).ToList() }), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }
    }
}
