using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// In castles_02_cloud_temple Josephine (a hero with a biography, female by default) was made male, and Theodorus (named Tumlac in
    /// the map, male by default) was set to male explicitly. The other saves are the same map before that.
    /// </summary>
    [TestFixture]
    internal sealed class GenderTests
    {
        private static Game Load(string name) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", name + ".GM1"));

        private static HeroEditSession Session(Game game, string hero) => game.History.CreateHeroSession(game.History.Current.Heroes().FindByName(hero));

        [TestCase("castles_00_baseline", HeroGender.Female)]
        [TestCase("castles_01_blacksmith_garrison_tumlac_to_blue", HeroGender.Female)]
        [TestCase("castles_02_cloud_temple", HeroGender.Male)]
        public void JosephinesGenderIsRead(string save, HeroGender expected)
        {
            Assert.That(Session(Load(save), "Josephine").Current.Gender, Is.EqualTo(expected));
        }

        [TestCase("castles_00_baseline")]
        [TestCase("castles_02_cloud_temple")]
        public void TheHeroNamedTumlacIsMale(string save)
        {
            Assert.That(Session(Load(save), "Tumlac").Current.Gender, Is.EqualTo(HeroGender.Male));
        }

        [Test]
        public void JosephineHasABiographySoHerHeaderIsInFrontOfTheChunk()
        {
            var chunk = Load("castles_00_baseline").History.Current.Heroes().Single(c => c.Name == "Josephine");

            Assert.That(chunk.Biography, Does.StartWith("Josephine is Tumlac's first lieutenant"));
            Assert.That(chunk.HeaderIndex, Is.LessThan(chunk.StartIndex));
            Assert.That(chunk.GetHeaderByteAt(HeroOffsetsFor(chunk)[HeroOffsetName.Gender]), Is.EqualTo((byte)HeroGender.Female));
        }

        [Test]
        public void MakingJosephineMaleChangesTheSameByteAsTheGameDid()
        {
            var baseline = Load("castles_00_baseline");
            var session = Session(baseline, "Josephine");

            session.Update(h => h with { Gender = HeroGender.Male });

            var changed = ChangedIndexes(baseline.Bytes, baseline.GetBytesToSave());
            Assert.That(changed, Has.Count.EqualTo(1));
            var chunk = baseline.History.Current.Heroes().Single(c => c.Name == "Josephine");
            Assert.That(changed.Single(), Is.EqualTo(chunk.HeaderIndex + HeroOffsetsFor(chunk)[HeroOffsetName.Gender]));

            // The same byte the game wrote when the gender was changed in the map.
            var cloud = Load("castles_02_cloud_temple");
            var cloudChunk = cloud.History.Current.Heroes().Single(c => c.Name == "Josephine");
            Assert.That(cloud.Bytes[cloudChunk.HeaderIndex + HeroOffsetsFor(cloudChunk)[HeroOffsetName.Gender]], Is.EqualTo((byte)HeroGender.Male));
        }

        [Test]
        public void TheGenderOfAHeroWithABiographySurvivesSavingAndLoading()
        {
            var game = Load("castles_00_baseline");
            Session(game, "Josephine").Update(h => h with { Gender = HeroGender.Male });

            var reloaded = TestServices.GameService.LoadGameFromBytes(game.GetBytesToSave());

            var josephine = Session(reloaded, "Josephine").Current;
            Assert.That(josephine.Gender, Is.EqualTo(HeroGender.Male));
            Assert.That(josephine.Biography, Is.EqualTo(Session(game, "Josephine").Current.Biography));
        }

        [Test]
        public void TheGenderOfAHeroWithoutABiographyCanBeChanged()
        {
            var game = Load("castles_00_baseline");
            Session(game, "Tumlac").Update(h => h with { Gender = HeroGender.Female });

            var changed = ChangedIndexes(game.Bytes, game.GetBytesToSave());

            Assert.That(changed, Has.Count.EqualTo(1));
            Assert.That(Session(TestServices.GameService.LoadGameFromBytes(game.GetBytesToSave()), "Tumlac").Current.Gender, Is.EqualTo(HeroGender.Female));
        }

        [Test]
        public void TheGenderChangeIsListedAsPendingAndCanBeReverted()
        {
            var game = Load("castles_00_baseline");
            Session(game, "Josephine").Update(h => h with { Gender = HeroGender.Male });

            var change = game.History.GetPendingChanges().Single();

            Assert.That(change.Change.Description, Is.EqualTo("Gender"));
            Assert.That(change.Change.Before, Is.EqualTo("Female"));
            Assert.That(change.Change.After, Is.EqualTo("Male"));

            change.Revert(game.History);
            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
        }

        [Test]
        public void ChangingAnotherFieldLeavesTheGenderAlone()
        {
            var game = Load("bios_custom");
            var session = Session(game, "Alkin");
            var before = session.Current.Gender;

            session.Update(h => h with { SpellPoints = (ushort)(h.SpellPoints + 1) });

            Assert.That(session.Current.Gender, Is.EqualTo(before));
        }

        [Test]
        public void ABiographyShorterThanTheHeaderStillWritesTheGenderOnce()
        {
            // The chunk starts 26 bytes before the faction, which is a biography length after the header, so a short
            // biography leaves the gender byte inside the chunk. Layout: header at 0 to 25, biography of 5 bytes, chunk from 5.
            var bytes = new byte[100];
            var chunk = new HeroChunk("hero", bytes[5..], 5, "short", new byte[HeroChunk.HeaderLength], 0);

            var edited = chunk.WithHeaderByte(20, 1);

            Assert.That(edited.GetHeaderByteAt(20), Is.EqualTo(1));
            var patched = TestServices.BytesPatcher.Patch(bytes, edited.PatchParts);
            Assert.That(ChangedIndexes(bytes, patched), Is.EqualTo(new[] { 20 }));
        }

        private static HeroOffsets HeroOffsetsFor(HeroChunk chunk) => new(GameVersion.HornOfTheAbyss);

        private static List<int> ChangedIndexes(byte[] before, byte[] after)
        {
            var changed = new List<int>();
            for (var i = 0; i < before.Length; i++)
            {
                if (before[i] != after[i])
                {
                    changed.Add(i);
                }
            }

            return changed;
        }
    }
}
