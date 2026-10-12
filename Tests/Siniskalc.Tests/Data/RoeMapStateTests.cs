using System.Text;
using System.Text.RegularExpressions;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// What the Restoration of Erathia saves 56 to 77 show about the state of the map and of the heroes, read from the bytes of the savefile
    /// (offsets are explained in docs/KNOWLEDGE_BASE.md, "Heroes", "Obelisks" and "Date and map objects"). The editor does not model most of this yet, so these tests
    /// pin the findings, not editor behaviour: when one of them starts to fail the finding was wrong or the layout was misunderstood.
    /// </summary>
    [TestFixture]
    internal sealed class RoeMapStateTests
    {
        // A hero record is 1094 bytes: the 1082 bytes of the chunk and 12 bytes after it that belong to the same hero.
        private const int HeroChunkLength = 1082;
        private const int AsleepInTail = HeroChunkLength + 5;
        private const int HiddenFlagsInTail = HeroChunkLength + 6;

        // In a hero chunk: the object the hero stands on (type, 1 if it can be visited, index of the object of that type).
        private const int StandingOnType = 11;
        private const int StandingOnIndex = 16;
        private const int OnMap = 6;

        // Effects that last until the next battle or the next day.
        private const int DayEffects = 135;
        private const int BattleEffects = 136;
        private const int MoraleEffects = 138;

        private const int ObeliskTypeId = 57;
        private const int FaerieRingTypeId = 28;
        private const int IdolOfFortuneTypeId = 38;
        private const int TownTypeId = 98;
        private const int HoleTypeId = 124;

        private sealed record Raw(Game Game, byte[] Bytes);

        private static Raw Load(string save)
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Roe", save + ".GM1"));
            return new Raw(game, game.Bytes);
        }

        private static int HeroStart(Raw raw, string name) => raw.Game.History.Current.Heroes()[raw.Game.History.Current.Heroes().FindByName(name)].StartIndex;

        // The first resource of Red. The struct of a player starts PlayerLayout.HeadLength (97) bytes earlier, with its heroes: the offsets used below
        // were found from here, so the head is at -97 and the obelisk flags, in front of the first struct, at -146.
        private static int RedStart(Raw raw) => raw.Game.History.Current.Players().Single(p => p.Name == "Red").StartIndex + PlayerLayout.ResourcesOffset;

        private static HeroModel Hero(Raw raw, string name) => raw.Game.History.CreateHeroSession(raw.Game.History.Current.Heroes().FindByName(name)).Current;

        private static CastleModel Castle(Raw raw, string name)
        {
            var castles = raw.Game.History.Current.Castles();
            return TestServices.CastleDeserializer.Deserialize(castles[castles.FindByName(name)]);
        }

        // The date is three 16-bit values (day, week, month) behind the marker 7F ?? 02 ?, and the last such place in the file is the one.
        private static int DateOffset(byte[] bytes)
        {
            var text = Encoding.Latin1.GetString(bytes);
            var match = Regex.Matches(text.Substring(300000), "\u007f[\u0000\u0001]\u0002[\u0000\u0001][\u0001-\u0007]\u0000[\u0001-\u0004]\u0000[\u0001-\u0009]\u0000", RegexOptions.Singleline).Last();
            return 300000 + match.Index + 4;
        }

        private static int U32(byte[] bytes, int at) => BitConverter.ToInt32(bytes, at);

        [Test]
        public void MovingAHeroOnlyChangesItsPositionFacingWhatItStandsOnAndItsMovementPoints()
        {
            var before = Load("roe_56_tazar_no_move");
            var after = Load("roe_57_tazar_moved");
            var a = HeroStart(before, "Tazar");
            var b = HeroStart(after, "Tazar");

            var changed = Enumerable.Range(0, HeroChunkLength + 12).Where(i => before.Bytes[a + i] != after.Bytes[b + i]).ToList();

            // y (twice), the object it stood on (a town: type, flag, index), the facing and the current movement points; nothing else, in the chunk or after it.
            Assert.That(changed, Is.EqualTo(new[] { 2, 9, 11, 15, 16, 41, 61, 62 }));
            Assert.That(U32(before.Bytes, a + StandingOnType), Is.EqualTo(TownTypeId));
            Assert.That(before.Bytes[a + StandingOnIndex], Is.EqualTo(1), "the Dungeon is the second castle of the savefile");
            Assert.That(Hero(before, "Tazar").MovementPoints.CurrentMovementPoints, Is.EqualTo(2900));
            Assert.That(Hero(after, "Tazar").MovementPoints.CurrentMovementPoints, Is.EqualTo(2800));
            Assert.That(Hero(after, "Tazar").MovementPoints.MaxMovementPoints, Is.EqualTo(2900));
        }

        [Test]
        public void TheEditorCannotTellOnShadowOfDeathWhetherAHeroHasMoved()
        {
            // HeroOffsets reads the HotA byte 1095, which is beyond the chunk here. When the flag is derived from the movement points this changes.
            Assert.That(Hero(Load("roe_57_tazar_moved"), "Tazar").MovedToday, Is.False);
        }

        [Test]
        public void ADigTakesAllTheMovementPointsAndRecordsTheTileAsDug()
        {
            var before = Load("roe_57_tazar_moved");
            var after = Load("roe_58_haart_dug");

            Assert.That(Hero(before, "Lord Haart").MovementPoints.CurrentMovementPoints, Is.EqualTo(2600));
            Assert.That(Hero(after, "Lord Haart").MovementPoints.CurrentMovementPoints, Is.EqualTo(0));
            Assert.That(Hero(after, "Lord Haart").MovementPoints.MaxMovementPoints, Is.EqualTo(2600));
            Assert.That(U32(after.Bytes, HeroStart(after, "Lord Haart") + StandingOnType), Is.EqualTo(HoleTypeId));
            Assert.That(DugTiles(before), Is.Empty);
            Assert.That(DugTiles(after), Is.EqualTo(new[] { (30, 8, 0) }));
            Assert.That(Hero(after, "Lord Haart").Position.X, Is.EqualTo(30));
        }

        // The tiles that have been dug are 5-byte entries (x, y, level, 16-bit number) in front of the 3-byte count and the text of a message.
        private static List<(int X, int Y, int Z)> DugTiles(Raw raw)
        {
            var text = Encoding.Latin1.GetString(raw.Bytes);
            var hint = text.IndexOf("Uh oh... devil tracks", StringComparison.Ordinal);
            var tiles = new List<(int, int, int)>();
            for (var at = hint - 10; at >= 0 && raw.Bytes[at + 2] == 0 && raw.Bytes[at + 3] != 0x16; at -= 5)
            {
                tiles.Insert(0, (raw.Bytes[at], raw.Bytes[at + 1], raw.Bytes[at + 2]));
            }

            return tiles;
        }

        [Test]
        public void TheGrailDigAddsASecondTileToTheListOfDugTilesAndTheGrailToTheBackpack()
        {
            var before = Load("roe_76_tazar_before_dig");
            var after = Load("roe_77_tazar_grail_found");
            var tazar = HeroStart(after, "Tazar");

            Assert.That(DugTiles(before), Is.EqualTo(new[] { (30, 8, 0) }), "Haart's dig stays in the list for the rest of the game");
            Assert.That(DugTiles(after), Is.EqualTo(new[] { (30, 8, 0), (16, 38, 0) }));
            Assert.That(Hero(after, "Tazar").Position, Is.EqualTo(Hero(before, "Tazar").Position));
            Assert.That(Hero(after, "Tazar").MovementPoints.CurrentMovementPoints, Is.EqualTo(0));
            Assert.That(Encoding.Latin1.GetString(before.Bytes), Does.Not.Contain("AVLholr0.def"));
            Assert.That(Encoding.Latin1.GetString(after.Bytes), Does.Contain("AVLholr0.def"));
            Assert.That(U32(after.Bytes, tazar + 560), Is.EqualTo(Artifact.TheGrail.Code), "the first backpack slot");
            Assert.That(U32(before.Bytes, HeroStart(before, "Tazar") + 560), Is.EqualTo(-1));
        }

        [Test]
        public void TheGrailInTheBackpackIsReadAndKeptButNotOffered()
        {
            var game = Load("roe_77_tazar_grail_found").Game;
            var tazar = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Tazar"));
            var carried = tazar.Current.Backpack.All.OfType<BackpackSlot>().Where(s => s.Art != null).ToList();
            var original = game.History.Current.ToBytes();

            Assert.That(carried.Select(s => (s.Slot, s.Art)), Is.EqualTo(new[] { (0, Artifact.TheGrail) }));
            Assert.That(Artifact.All.Values, Does.Not.Contain(Artifact.TheGrail), "it cannot be worn, so the finders never list it");

            tazar.Update(h => h with { MovedToday = h.MovedToday });
            Assert.That(game.History.Current.ToBytes(), Is.EqualTo(original), "saving an untouched backpack keeps the Grail");

            tazar.Update(h => h with { Backpack = h.Backpack.WithSlot(0, null) });
            Assert.That(U32(game.History.Current.ToBytes(), HeroStart(Load("roe_77_tazar_grail_found"), "Tazar") + 560), Is.EqualTo(-1), "taking it out empties the slot");
        }

        [Test]
        public void TheGrailFlagBesideTheDateIsClearedWhenTheGrailIsFound()
        {
            var before = Load("roe_76_tazar_before_dig");
            var after = Load("roe_77_tazar_grail_found");

            Assert.That(before.Bytes[DateOffset(before.Bytes) - 3], Is.EqualTo(1));
            Assert.That(after.Bytes[DateOffset(after.Bytes) - 3], Is.EqualTo(0));
        }

        [Test]
        public void ASleepingHeroHasTheByteFiveAfterItsChunkSetAndMovingWakesIt()
        {
            string[] red = ["Lord Haart", "Ufretin", "Torosar ", "Nymus", "Damacon", "Crag Hack", "Tazar"];
            var awake = Load("roe_58_haart_dug");
            var asleep = Load("roe_59_nymus_damacon_sleep");
            var moved = Load("roe_65_damacon_before_surrender");

            foreach (var name in red)
            {
                Assert.That(awake.Bytes[HeroStart(awake, name) + AsleepInTail], Is.EqualTo(0), name);
                Assert.That(asleep.Bytes[HeroStart(asleep, name) + AsleepInTail], Is.EqualTo(name is "Nymus" or "Damacon" ? 1 : 0), name);
            }

            Assert.That(moved.Bytes[HeroStart(moved, "Damacon") + AsleepInTail], Is.EqualTo(0));
        }

        [Test]
        public void AHeroInsideACastleIsNotOnTheMapAndStandsOnTheTown()
        {
            var outside = Load("roe_60_crag_hack_visiting_with_horde_arts");
            var inside = Load("roe_61_inferno_garrison_and_visiting");
            var nymus = HeroStart(outside, "Nymus");
            var inferno = HeroStart(inside, "Nymus");

            Assert.That(outside.Bytes[nymus + OnMap], Is.EqualTo(1));
            Assert.That(inside.Bytes[inferno + OnMap], Is.EqualTo(0));
            Assert.That(outside.Bytes[nymus + HiddenFlagsInTail] & 0x40, Is.EqualTo(0));
            Assert.That(inside.Bytes[inferno + HiddenFlagsInTail] & 0x40, Is.EqualTo(0x40));
            Assert.That(U32(inside.Bytes, inferno + StandingOnType), Is.EqualTo(TownTypeId));
            Assert.That(inside.Bytes[inferno + StandingOnIndex], Is.EqualTo(6), "the Inferno castle is the seventh castle of the savefile");

            var tazar = HeroStart(inside, "Tazar");
            Assert.That(inside.Bytes[tazar + OnMap], Is.EqualTo(1), "a visiting hero is on the map, at the gate");
            Assert.That(U32(inside.Bytes, tazar + StandingOnType), Is.EqualTo(TownTypeId));
            Assert.That(inside.Bytes[tazar + StandingOnIndex], Is.EqualTo(6));
        }

        [Test]
        public void TheCastleKnowsItsGarrisonedAndVisitingHero()
        {
            var inferno = Castle(Load("roe_61_inferno_garrison_and_visiting"), "4_Inferno");

            Assert.That(inferno.GarrisonedHeroId, Is.EqualTo(55), "Nymus");
            Assert.That(inferno.VisitingHeroId, Is.EqualTo(115), "Tazar");
        }

        [Test]
        public void TheLegionArtifactsOfAVisitingHeroAreNotRecordedInTheCastle()
        {
            var before = Load("roe_59_nymus_damacon_sleep");
            var after = Load("roe_60_crag_hack_visiting_with_horde_arts");
            var crag = Hero(after, "Crag Hack");

            Assert.That(crag.EquippedArtifacts.GetArtifact(ArtifactSlots.Misc4), Is.EqualTo(Artifact.HeadOfLegion));
            Assert.That(crag.EquippedArtifacts.GetArtifact(ArtifactSlots.Misc5), Is.EqualTo(Artifact.ArmsOfLegion));
            Assert.That(Castle(after, "7_Strong").VisitingHeroId, Is.EqualTo(crag.HeroId));

            var stronghold = Castle(before, "7_Strong");
            var withHero = Castle(after, "7_Strong");
            Assert.That(withHero.UpgradedCreaturesAvailable, Is.EquivalentTo(stronghold.UpgradedCreaturesAvailable));
            Assert.That(withHero.CreaturesAvailable, Is.EquivalentTo(stronghold.CreaturesAvailable));
            Assert.That(withHero.Buildings, Is.EquivalentTo(stronghold.Buildings));
        }

        [Test]
        public void TheGrowthOfAWeekShowsTheLegionArtifactsOfTheHeroAtTheGateOfTheStronghold()
        {
            // Week 3 day 1: the weekly growth of every castle has been added since the creatures were recruited (save 47). All castles have the same
            // Resource Silo and capitol, and the 5th and 6th level creatures of these four factions grow by 3 and 2 a week, so the Stronghold, which
            // Crag Hack visits with Arms of Legion (described as +2 on level 5) and Head of Legion (+1 on level 6), is ahead of them by 3 and 1 on those
            // two levels. The one extra creature on level 5 is not explained. Nothing of this is stored: the growth is worked out when the week begins.
            var week = Load("roe_76_tazar_before_dig");
            var stronghold = Castle(week, "7_Strong").UpgradedCreaturesAvailable;

            Assert.That(Castle(week, "7_Strong").VisitingHeroId, Is.EqualTo(102));
            foreach (var other in new[] { "6_Dung", "8_Fort", "5_necropolis" })
            {
                var growth = Castle(week, other).UpgradedCreaturesAvailable;
                Assert.That(stronghold[4] - growth[4], Is.EqualTo(3), other);
                Assert.That(stronghold[5] - growth[5], Is.EqualTo(1), other);
                Assert.That(stronghold[6] - growth[6], Is.EqualTo(0), other);
            }
        }

        [Test]
        public void CapturingANeutralCastleOnlyChangesItsOwnerAndEmptiesItsGarrison()
        {
            var before = Load("roe_63_tazar_idle_of_fortune");
            var after = Load("roe_64_elfwind_captured");

            Assert.That(Castle(before, "Elfwind").Owner, Is.Null);
            Assert.That(Castle(before, "Elfwind").Garrison.Slots, Is.Not.Empty);
            Assert.That(Castle(after, "Elfwind").Owner, Is.EqualTo(PlayerColor.Red));
            Assert.That(Castle(after, "Elfwind").Garrison.Slots, Is.Empty);
            Assert.That(Castle(after, "Elfwind").Buildings, Is.EquivalentTo(Castle(before, "Elfwind").Buildings));
        }

        [TestCase("roe_64_elfwind_captured", 6, 0x73, 0x41, 0x20, 9)]
        [TestCase("roe_66_damacon_surrender", 5, 0xFF, 0x41, 0x54, 9)]
        [TestCase("roe_67_damacon_defeated", 5, 0xFF, 0x41, 0x2D, 9)]
        [TestCase("roe_68_tazar_obelisk", 5, 0x73, 0x41, 0x2D, 9)]
        [TestCase("roe_76_tazar_before_dig", 5, 0x73, 0x4D, 0x6B, 9)]
        public void ThePlayerRecordHoldsTheHeroCountTheSelectedHeroTheTavernAndTheTownCount(string save, int heroes, int selected, int tavern1, int tavern2, int towns)
        {
            var raw = Load(save);
            var red = RedStart(raw);

            Assert.That(raw.Bytes[red - 97], Is.EqualTo(heroes), "number of heroes");
            Assert.That(raw.Bytes[red - 96], Is.EqualTo(selected), "the hero that is selected, FF for none");
            Assert.That(raw.Bytes[red - 87], Is.EqualTo(tavern1), "first hero of the tavern");
            Assert.That(raw.Bytes[red - 86], Is.EqualTo(tavern2), "second hero of the tavern");
            Assert.That(raw.Bytes[red - 74], Is.EqualTo(towns), "number of castles");
            Assert.That(raw.Bytes[red - 95 + heroes], Is.EqualTo(0xFF), "the hero ids come first and the rest of the eight places is FF");
        }

        [Test]
        public void ADefeatedOrFledHeroLeavesThePlayerAndOccupiesTheSecondTavernPlace()
        {
            var before = Load("roe_65_damacon_before_surrender");
            var fled = Load("roe_66_damacon_surrender");
            var gone = Load("roe_67_damacon_defeated");

            Assert.That(Hero(before, "Damacon").Owner, Is.EqualTo(PlayerColor.Red));
            Assert.That(Hero(fled, "Damacon").Owner, Is.Null);
            Assert.That(Hero(gone, "Damacon").Owner, Is.Null);
            Assert.That(fled.Bytes[RedStart(fled) - 86], Is.EqualTo(Damacon), "back in the tavern after fleeing");
            Assert.That(gone.Bytes[RedStart(gone) - 86], Is.Not.EqualTo(Damacon), "after being hired again and defeated Solmyr took the place");
            Assert.That(gone.Bytes[HeroStart(gone, "Damacon") + HiddenFlagsInTail] & 0x40, Is.EqualTo(0x40));
            Assert.That(before.Bytes[HeroStart(before, "Damacon") + HiddenFlagsInTail] & 0x40, Is.EqualTo(0));
        }

        private const int Damacon = 84;

        [TestCase("roe_67_damacon_defeated", "00000")]
        [TestCase("roe_68_tazar_obelisk", "10000")]
        [TestCase("roe_72_tazar_obelisk_2", "11000")]
        [TestCase("roe_73_tazar_obelisk_3", "11001")]
        [TestCase("roe_74_tazar_obelisk_4", "11011")]
        [TestCase("roe_75_tazar_all_obelisks", "11111")]
        [TestCase("roe_77_tazar_grail_found", "11111")]
        public void TheObelisksRedHasVisitedAreOneByteEachAfterTheCountOfObelisksOfTheMap(string save, string visited)
        {
            var raw = Load(save);
            var flags = RedStart(raw) - 146;

            Assert.That(raw.Bytes[flags - 1], Is.EqualTo(5), "the map has five obelisks");
            Assert.That(string.Concat(Enumerable.Range(0, 5).Select(i => raw.Bytes[flags + i])), Is.EqualTo(visited));
        }

        [TestCase("roe_68_tazar_obelisk", 0)]
        [TestCase("roe_72_tazar_obelisk_2", 1)]
        [TestCase("roe_73_tazar_obelisk_3", 4)]
        [TestCase("roe_74_tazar_obelisk_4", 3)]
        [TestCase("roe_75_tazar_all_obelisks", 2)]
        public void AHeroOnAnObeliskHoldsItsTypeAndItsIndexAmongTheObelisks(string save, int index)
        {
            var raw = Load(save);
            var tazar = HeroStart(raw, "Tazar");

            Assert.That(U32(raw.Bytes, tazar + StandingOnType), Is.EqualTo(ObeliskTypeId));
            Assert.That(raw.Bytes[tazar + StandingOnIndex], Is.EqualTo(index));
            Assert.That(raw.Bytes[RedStart(raw) - 146 + index], Is.EqualTo(1), "and the flag of that obelisk is set");
        }

        [Test]
        public void TheIdolOfFortuneGivesMoraleAndTheFaerieRingLuckUntilTheNextBattle()
        {
            var tazarOf = (string save) => { var raw = Load(save); return raw.Bytes.AsSpan(HeroStart(raw, "Tazar"), HeroChunkLength).ToArray(); };

            var before = tazarOf("roe_62_tazar_combat");
            var idol = tazarOf("roe_63_tazar_idle_of_fortune");
            var battle = tazarOf("roe_64_elfwind_captured");
            var faerie = tazarOf("roe_68b_tazar_faerie");
            var after = tazarOf("roe_70_tazar_post_battle");
            var both = tazarOf("roe_71_tazar_faerie_idol");
            var fought = tazarOf("roe_74_tazar_obelisk_4");

            Assert.That(before[MoraleEffects] & 0x02, Is.EqualTo(0));
            Assert.That(idol[MoraleEffects] & 0x02, Is.EqualTo(0x02), "after the Idol of Fortune");
            Assert.That(battle[MoraleEffects] & 0x02, Is.EqualTo(0), "a battle ends it");
            Assert.That(faerie[BattleEffects] & 0x20, Is.EqualTo(0x20), "after the Faerie Ring");
            Assert.That(after[BattleEffects] & 0x20, Is.EqualTo(0), "a battle ends it");
            Assert.That(both[BattleEffects] & 0x20, Is.EqualTo(0x20), "both again");
            Assert.That(both[MoraleEffects] & 0x02, Is.EqualTo(0x02));
            Assert.That(fought[BattleEffects] & 0x20, Is.EqualTo(0));
            Assert.That(fought[MoraleEffects] & 0x02, Is.EqualTo(0));
        }

        [Test]
        public void VisitingTheIdolOfFortuneAndTheFaerieRingIsRememberedInATableOfObjectTypes()
        {
            // Two bytes per object type from 12 bytes after the date: the byte of the Idol of Fortune (type 38) is at +88 and the one of the Faerie Ring (type 28) at +68.
            // It is still set after later battles and after a new day, and visiting again does not change it, so it is not what makes the bonus come back.
            int Flag(string save, int type) { var raw = Load(save); return raw.Bytes[DateOffset(raw.Bytes) + 12 + (2 * type)]; }

            Assert.That(Flag("roe_62_tazar_combat", IdolOfFortuneTypeId), Is.EqualTo(0));
            Assert.That(Flag("roe_63_tazar_idle_of_fortune", IdolOfFortuneTypeId), Is.EqualTo(1));
            Assert.That(Flag("roe_67_damacon_defeated", FaerieRingTypeId), Is.EqualTo(0));
            Assert.That(Flag("roe_68b_tazar_faerie", FaerieRingTypeId), Is.EqualTo(1));
            Assert.That(Flag("roe_76_tazar_before_dig", FaerieRingTypeId), Is.EqualTo(1));
            Assert.That(Flag("roe_76_tazar_before_dig", IdolOfFortuneTypeId), Is.EqualTo(1));
        }
    }
}
