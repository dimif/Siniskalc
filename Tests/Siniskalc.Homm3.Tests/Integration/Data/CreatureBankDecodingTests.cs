using Siniskalc.Homm3.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// Pins HotA byte evidence, not an editing API. Absolute offsets refer to decompressed fixtures;
    /// the controlled resets do not establish a general reset mask for other instances or kinds.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class CreatureBankDecodingTests
    {
        private static byte[] Load(string save) =>
            TestServices.SavefileCodec.Read(Path.Combine("TestData", "Hota", save + ".GM1"));

        private static byte[] Control(string save, string extension = ".GM2") =>
            TestServices.SavefileCodec.Read(Path.Combine("TestData", "Hota", "CreatureBanks", save + extension));

        private static int[] Integers(byte[] bytes, int offset, int count) =>
            Enumerable.Range(0, count).Select(i => BitConverter.ToInt32(bytes, offset + (i * 4))).ToArray();

        private static int NextRecord(byte[] bytes, int offset) =>
            offset + 91 + (BitConverter.ToUInt16(bytes, offset + 89) * 4);

        private static int RecordAt(byte[] bytes, int table, int index)
        {
            Assert.That(index, Is.LessThan(BitConverter.ToUInt16(bytes, table)));
            var offset = table + 2;
            for (var i = 0; i < index; i++)
            {
                offset = NextRecord(bytes, offset);
            }

            return offset;
        }

        [TestCase("bios_standard", 0x722D6, 37, 0x73047)]
        [TestCase("bios_custom", 0x724BC, 37, 0x7322D)]
        [TestCase("tan_not_parsing", 0xDC650, 33, 0xDD23D)]
        [TestCase("cannot_edit_town_spells", 0xDC332, 33, 0xDCF37)]
        [TestCase("empire_of_the_world_01_m5w4d6", 0x1E9C3F, 164, 0x1ED7B1)]
        public void MixedBankTablesIncludeEveryVariableLengthRecord(string save, int table, int count, int end)
        {
            var bytes = Load(save);
            Assert.That(BitConverter.ToUInt16(bytes, table), Is.EqualTo(count));
            Assert.That(NextRecord(bytes, RecordAt(bytes, table, count - 1)), Is.EqualTo(end));
        }

        [TestCase("AVXbnk10.def", 0)]
        [TestCase("AVXbnk20.def", 1)]
        [TestCase("AVXbnk40.def", 3)]
        [TestCase("AVXbnk50.def", 4)]
        [TestCase("AVXbnk60.def", 5)]
        public void SavedBankDefinitionsCarryTheSameTypeAndSubtypeAsTheirEntrances(string name, int subtype)
        {
            var bytes = Load("bios_standard");
            var nameBytes = System.Text.Encoding.ASCII.GetBytes(name);
            var offset = bytes.AsSpan().IndexOf(nameBytes);
            Assert.That(offset, Is.GreaterThanOrEqualTo(2));
            Assert.That(BitConverter.ToUInt16(bytes, offset - 2), Is.EqualTo(nameBytes.Length));
            var tail = offset + nameBytes.Length;
            Assert.That(BitConverter.ToUInt16(bytes, tail + 26), Is.EqualTo(16));
            Assert.That(BitConverter.ToUInt32(bytes, tail + 28), Is.EqualTo(subtype));
        }

        [TestCase(0x27FC, 5, 1364, 2, 0x72396, 38, 38, 2, 0, 0, 0, 0, 0, 8, 4000)]
        [TestCase(0x337E, 1, 1379, 4, 0x72450, 16, 16, 15, 0, 0, 0, 0, 3, 0, 4000)]
        [TestCase(0x3AD0, 3, 1058, 5, 0x724AB, 42, 43, 60, 0, 6, 0, 0, 0, 0, 3000)]
        [TestCase(0x670A, 1, 1421, 7, 0x72565, 16, 16, 30, 0, 0, 0, 0, 10, 0, 7500)]
        [TestCase(0x8AD6, 4, 1022, 8, 0x725C0, 76, 76, 8, 0, 0, 0, 8, 0, 0, 4000)]
        [TestCase(0xA370, 0, 1370, 11, 0x726D9, 94, 95, 8, 8, 8, 8, 8, 8, 8, 0)]
        [TestCase(0xBC7A, 4, 1346, 12, 0x72734, 76, 76, 4, 0, 0, 0, 5, 0, 0, 2000)]
        [TestCase(0xC458, 5, 1073, 13, 0x7278F, 38, 39, 2, 0, 0, 0, 0, 0, 8, 4000)]
        [TestCase(0x14500, 4, 1326, 20, 0x72A1C, 76, 76, 10, 0, 0, 0, 10, 0, 0, 5000)]
        [TestCase(0x14694, 0, 626, 21, 0x72A77, 94, 95, 8, 8, 8, 8, 8, 8, 8, 0)]
        public void EachNamedBankEntranceInBiosStandardIndexesItsGuardsAndResources(
            int entrance, int subtype, int objectIndex, int index, int record,
            int creature, int center, int amount,
            int wood, int mercury, int ore, int sulfur, int crystal, int gems, int gold)
        {
            var bytes = Load("bios_standard");
            Assert.That(BitConverter.ToUInt16(bytes, entrance), Is.EqualTo(16));
            Assert.That(BitConverter.ToUInt16(bytes, entrance + 2), Is.EqualTo(subtype));
            Assert.That(BitConverter.ToUInt16(bytes, entrance + 4), Is.EqualTo(objectIndex));
            var state = BitConverter.ToUInt32(bytes, entrance + 6);
            Assert.That(state, Is.EqualTo(0xFC00001Fu | ((uint)index << 13)));
            Assert.That(RecordAt(bytes, 0x722D6, (int)((state >> 13) & 0xFFF)), Is.EqualTo(record));
            Assert.That(Integers(bytes, record, 7), Is.EqualTo(new[] { creature, creature, center, creature, creature, -1, -1 }));
            Assert.That(Integers(bytes, record + 28, 7), Is.EqualTo(new[] { amount, amount, amount, amount, amount, 0, 0 }));
            Assert.That(Integers(bytes, record + 56, 7), Is.EqualTo(new[] { wood, mercury, ore, sulfur, crystal, gems, gold }));
            Assert.That(BitConverter.ToInt32(bytes, record + 84), Is.EqualTo(-1));
            Assert.That(bytes[record + 88], Is.Zero);
            Assert.That(BitConverter.ToUInt16(bytes, record + 89), Is.Zero);
        }

        [TestCase("bios_custom", 0x724BC, 0xA386, 0, 11, 94, 94, 10, 0, 10, 0)]
        [TestCase("bios_custom", 0x724BC, 0xC46E, 5, 13, 38, 39, 4, 5, 16, 8000)]
        [TestCase("bios_custom", 0x724BC, 0x3394, 1, 4, 16, 17, 15, 4, 3, 4000)]
        [TestCase("cannot_edit_town_spells", 0xDC332, 0x1408E, 0, 9, 94, 95, 6, 0, 6, 0)]
        [TestCase("cannot_edit_town_spells", 0xDC332, 0x16ACE, 1, 10, 16, 16, 20, 4, 5, 5000)]
        [TestCase("tan_not_parsing", 0xDC650, 0x16C56, 1, 10, 16, 17, 10, 4, 2, 2500)]
        [TestCase("tan_not_parsing", 0xDC650, 0x43ADC, 3, 27, 42, 43, 20, 1, 2, 1000)]
        [TestCase("cannot_edit_town_spells", 0xDC332, 0x4101C, 3, 25, 42, 43, 30, 1, 3, 1500)]
        [TestCase("cannot_edit_town_spells", 0xDC332, 0x854C, 3, 7, 42, 43, 40, 1, 4, 2000)]
        [TestCase("cannot_edit_town_spells", 0xDC332, 0x65A4, 4, 5, 76, 77, 6, 3, 6, 3000)]
        [TestCase("empire_of_the_world_01_m5w4d6", 0x1E9C3F, 0xC10DA, 0, 106, 94, 95, 4, 0, 4, 0)]
        public void OtherSavesEstablishAdditionalGuardTiersAndCenterUpgrades(
            string save, int table, int entrance, int subtype, int index, int creature, int center, int amount,
            int resource, int reward, int gold)
        {
            var bytes = Load(save);
            Assert.That(BitConverter.ToUInt16(bytes, entrance), Is.EqualTo(16));
            Assert.That(BitConverter.ToUInt16(bytes, entrance + 2), Is.EqualTo(subtype));
            Assert.That(BitConverter.ToUInt32(bytes, entrance + 6), Is.EqualTo(0xFC00001Fu | ((uint)index << 13)));
            var at = RecordAt(bytes, table, index);
            Assert.That(Integers(bytes, at, 7), Is.EqualTo(new[] { creature, creature, center, creature, creature, -1, -1 }));
            Assert.That(Integers(bytes, at + 28, 7), Is.EqualTo(new[] { amount, amount, amount, amount, amount, 0, 0 }));
            var resources = subtype == 0 ? new[] { reward, reward, reward, reward, reward, reward, 0 } : new int[7];
            resources[resource] = reward;
            resources[6] = gold;
            Assert.That(Integers(bytes, at + 56, 7), Is.EqualTo(resources));
            Assert.That(BitConverter.ToInt32(bytes, at + 84), Is.EqualTo(-1));
            Assert.That(bytes[at + 88], Is.Zero);
            Assert.That(BitConverter.ToUInt16(bytes, at + 89), Is.Zero);
        }

        [TestCase(0x1F0A, 1, 0, 0xFE00041Fu, 0, 0, 0, 0, 2, 0, 2500)]
        [TestCase(0x4AB8, 0, 2, 0xFE00441Fu, 8, 8, 8, 8, 8, 8, 0)]
        [TestCase(0x878C, 4, 3, 0xFE00641Fu, 0, 0, 0, 10, 0, 0, 5000)]
        [TestCase(0x64D02, 5, 68, 0xFE08805Fu, 0, 0, 0, 0, 0, 8, 4000)]
        [TestCase(0x6836E, 5, 72, 0xFE09005Fu, 0, 0, 0, 0, 0, 12, 6000)]
        [TestCase(0xA9CD6, 3, 98, 0xFE0C441Fu, 0, 6, 0, 0, 0, 0, 3000)]
        [TestCase(0x342DA, 4, 38, 0xFE04C81Fu, 0, 0, 0, 8, 0, 0, 4000)]
        [TestCase(0x15007A, 3, 159, 0xFE13F01Fu, 0, 3, 0, 0, 0, 0, 1500)]
        public void EmptyBanksInEmpireKeepResourcesAndHaveDifferentLowStateBits(
            int entrance, int subtype, int index, uint state,
            int wood, int mercury, int ore, int sulfur, int crystal, int gems, int gold)
        {
            var bytes = Load("empire_of_the_world_01_m5w4d6");
            Assert.That(BitConverter.ToUInt16(bytes, entrance), Is.EqualTo(16));
            Assert.That(BitConverter.ToUInt16(bytes, entrance + 2), Is.EqualTo(subtype));
            Assert.That(BitConverter.ToUInt32(bytes, entrance + 6), Is.EqualTo(state));
            Assert.That((state >> 13) & 0xFFF, Is.EqualTo(index));
            var at = RecordAt(bytes, 0x1E9C3F, index);
            Assert.That(Integers(bytes, at, 7), Is.All.EqualTo(-1));
            Assert.That(Integers(bytes, at + 28, 7), Is.All.Zero);
            Assert.That(Integers(bytes, at + 56, 7), Is.EqualTo(new[] { wood, mercury, ore, sulfur, crystal, gems, gold }));
            Assert.That(BitConverter.ToInt32(bytes, at + 84), Is.EqualTo(-1));
            Assert.That(bytes[at + 88], Is.Zero);
            Assert.That(BitConverter.ToUInt16(bytes, at + 89), Is.Zero);
        }

        [Test]
        public void TheCompanionMapFixesFourCyclopsTiersAtTheSavedObjectAnchors()
        {
            var map = TestServices.SavefileCodec.Read(Path.Combine("TestData", "Hota", "CreatureBanks", "creature_banks_00_baseline.h3m"));
            Assert.That(BitConverter.ToUInt32(map, 0x12482), Is.EqualTo(101));
            Assert.That(BitConverter.ToUInt32(map, 0x1378B), Is.EqualTo(12));
            Assert.That(System.Text.Encoding.ASCII.GetString(map, 0x1378F, 12), Is.EqualTo("AVXbnk10.def"));
            Assert.That(BitConverter.ToUInt32(map, 0x1378F + 12 + 16), Is.EqualTo(16));
            Assert.That(BitConverter.ToUInt32(map, 0x1378F + 12 + 20), Is.Zero);
            var bytes = Control("creature_banks_00_baseline");
            Assert.That(BitConverter.ToUInt16(bytes, 0x3AEB4), Is.EqualTo(224));
            for (var i = 0; i < 4; i++)
            {
                var at = 0x14984 + (i * 21);
                var position = new byte[] { (byte)(2 + (i * 2)), 24, 0 };
                Assert.That(map[at..(at + 3)], Is.EqualTo(position));
                Assert.That(BitConverter.ToUInt32(map, at + 3), Is.EqualTo(84));
                Assert.That(BitConverter.ToUInt32(map, at + 12), Is.EqualTo(3 - i));
                Assert.That(map[at + 16], Is.EqualTo(0xFF));
                var savedPosition = 0x3AEB4 + 2 + ((171 + i) * 5) + 2;
                Assert.That(bytes[savedPosition..(savedPosition + 3)], Is.EqualTo(position));
                var entrance = 0xA5B1 + (i * 0x38);
                Assert.That(BitConverter.ToUInt16(bytes, entrance), Is.EqualTo(16));
                Assert.That(BitConverter.ToUInt16(bytes, entrance + 2), Is.Zero);
                Assert.That(BitConverter.ToUInt16(bytes, entrance + 4), Is.EqualTo(171 + i));
                Assert.That(BitConverter.ToUInt32(bytes, entrance + 6), Is.EqualTo(0xFC01001Fu + ((uint)i << 13)));
                var record = RecordAt(bytes, 0x7EB9B, 8 + i);
                var amount = 10 - (i * 2);
                Assert.That(Integers(bytes, record, 7), Is.EqualTo(new[] { 94, 94, i == 1 ? 95 : 94, 94, 94, -1, -1 }));
                Assert.That(Integers(bytes, record + 28, 7), Is.EqualTo(new[] { amount, amount, amount, amount, amount, 0, 0 }));
                Assert.That(Integers(bytes, record + 56, 7), Is.EqualTo(new[] { amount, amount, amount, amount, amount, amount, 0 }));
            }
        }

        [Test]
        public void WinningTheControlledCyclopsStockpileChangesOnlyItsGuardsInTheWholeBankTable()
        {
            var before = Control("creature_banks_00_baseline");
            var won = Control("creature_banks_01_cyclops_stockpile_1_won");
            var steppedOff = Control("creature_banks_02_cyclops_stockpile_1_stepped_off");
            Assert.That(BitConverter.ToUInt16(before, 0x7EB9B), Is.EqualTo(28));
            Assert.That(RecordAt(before, 0x7EB9B, 8), Is.EqualTo(0x7EEF5));
            Assert.That(NextRecord(before, RecordAt(before, 0x7EB9B, 27)), Is.EqualTo(0x7F611));
            var expected = (byte[])before.Clone();
            for (var i = 0; i < 7; i++)
            {
                BitConverter.GetBytes(-1).CopyTo(expected, 0x7EEF5 + (i * 4));
                BitConverter.GetBytes(0u).CopyTo(expected, 0x7EEF5 + 28 + (i * 4));
            }

            Assert.That(won[0x7EB9B..0x7F611], Is.EqualTo(expected[0x7EB9B..0x7F611]));
            Assert.That(steppedOff[0x7EB9B..0x7F611], Is.EqualTo(won[0x7EB9B..0x7F611]));
            Assert.That(Enumerable.Range(0x7EB9B, 0x7F611 - 0x7EB9B).Count(i => before[i] != won[i]), Is.EqualTo(25));
            Assert.That(Integers(won, 0x7EEF5 + 56, 7), Is.EqualTo(new[] { 10, 10, 10, 10, 10, 10, 0 }));
        }

        [Test]
        public void TheWonCyclopsEntranceIsHiddenInTheVictoriousHeroHeaderUntilItStepsOff()
        {
            var before = Control("creature_banks_00_baseline");
            var won = Control("creature_banks_01_cyclops_stockpile_1_won");
            var steppedOff = Control("creature_banks_02_cyclops_stockpile_1_stepped_off");
            Assert.That(BitConverter.ToUInt32(before, 0xA5B1 + 6), Is.EqualTo(0xFC01001Fu));
            Assert.That(BitConverter.ToUInt32(won, 0xA5B1), Is.EqualTo(34));
            Assert.That(BitConverter.ToUInt32(won, 0xA5B1 + 6), Is.EqualTo(50));
            Assert.That(BitConverter.ToUInt32(steppedOff, 0xA5B1), Is.EqualTo(16));
            Assert.That(BitConverter.ToUInt32(steppedOff, 0xA5B1 + 6), Is.EqualTo(0xFE01003Fu));
            var game = TestServices.GameService.LoadGameFromBytes(won, "creature_banks_01_cyclops_stockpile_1_won.GM2");
            var hero = game.History.Current.Heroes().Single(h => h.HeaderIndex == 0x4AC51);
            Assert.That(new[] { hero.GetHeaderByteAt(0), hero.GetHeaderByteAt(2), hero.GetHeaderByteAt(4) }, Is.EqualTo(new byte[] { 1, 24, 0 }));
            Assert.That(BitConverter.ToUInt32(won, hero.HeaderIndex + 11), Is.EqualTo(16));
            Assert.That(hero.GetHeaderByteAt(15), Is.EqualTo(1));
            Assert.That(BitConverter.ToUInt32(won, hero.HeaderIndex + 16), Is.EqualTo(0xFE01003Fu));
            Assert.That(BitConverter.ToUInt32(steppedOff, hero.HeaderIndex + 11), Is.Zero);
            Assert.That(BitConverter.ToUInt32(steppedOff, hero.HeaderIndex + 16), Is.Zero);
            Assert.That(before[0xA5B1..(0xA5B1 + 6)], Is.EqualTo(steppedOff[0xA5B1..(0xA5B1 + 6)]));
        }

        [Test]
        public void TheControlledCyclopsVictoryGivesTenOfEachNonGoldResourceExactlyOnce()
        {
            var before = Control("creature_banks_00_baseline");
            var won = Control("creature_banks_01_cyclops_stockpile_1_won");
            var steppedOff = Control("creature_banks_02_cyclops_stockpile_1_stepped_off");
            var game = TestServices.GameService.LoadGameFromBytes(won, "creature_banks_01_cyclops_stockpile_1_won.GM2");
            var red = game.History.Current.Players().Single(p => p.Name == "Red");
            Assert.That(red.StartIndex + PlayerLayout.ResourcesOffset, Is.EqualTo(0x3B5C4));
            var start = Integers(before, 0x3B5C4, 7);
            var finish = Integers(won, 0x3B5C4, 7);
            Assert.That(finish.Zip(start, (a, b) => a - b), Is.EqualTo(new[] { 10, 10, 10, 10, 10, 10, 0 }));
            Assert.That(Integers(steppedOff, 0x3B5C4, 7), Is.EqualTo(finish));
        }

        [Test]
        public void TheCyclopsResetProposalRestoresOnlyTheExactObservedGuardsAndEntranceState()
        {
            var before = Control("creature_banks_00_baseline");
            var steppedOff = Control("creature_banks_02_cyclops_stockpile_1_stepped_off");
            var proposal = (byte[])steppedOff.Clone();
            Array.Copy(before, 0x7EEF5, proposal, 0x7EEF5, 56);
            Array.Copy(before, 0xA5B1 + 6, proposal, 0xA5B1 + 6, 4);
            var changed = Enumerable.Range(0, proposal.Length).Where(i => proposal[i] != steppedOff[i]).ToArray();
            var expected = Enumerable.Range(0x7EEF5, 20)
                .Concat(Enumerable.Range(0, 5).Select(i => 0x7EEF5 + 28 + (i * 4)))
                .Concat(new[] { 0xA5B7, 0xA5BA }).Order().ToArray();
            Assert.That(changed, Is.EqualTo(expected));
            Assert.That(changed, Has.Length.EqualTo(27));
            Assert.That(proposal[0x7EEF5..(0x7EEF5 + 91)], Is.EqualTo(before[0x7EEF5..(0x7EEF5 + 91)]));
            Assert.That(Integers(proposal, 0x3B5C4, 7), Is.EqualTo(Integers(steppedOff, 0x3B5C4, 7)));
        }

        [Test]
        public void WinningTheRestoredCyclopsStockpileGivesTheFullRewardAgainAndClearsItAgain()
        {
            var firstWin = Control("creature_banks_01_cyclops_stockpile_1_won");
            var steppedOff = Control("creature_banks_02_cyclops_stockpile_1_stepped_off");
            var secondWin = Control("creature_banks_03_cyclops_stockpile_1_won_again", ".GM1");
            Assert.That(secondWin[0x7EB9B..0x7F611], Is.EqualTo(firstWin[0x7EB9B..0x7F611]));
            Assert.That(Integers(secondWin, 0x7EEF5, 7), Is.All.EqualTo(-1));
            Assert.That(Integers(secondWin, 0x7EEF5 + 28, 7), Is.All.Zero);
            Assert.That(Integers(secondWin, 0x7EEF5 + 56, 7), Is.EqualTo(new[] { 10, 10, 10, 10, 10, 10, 0 }));
            Assert.That(BitConverter.ToUInt32(secondWin, 0xA5B1), Is.EqualTo(34));
            Assert.That(BitConverter.ToUInt32(secondWin, 0xA5B1 + 6), Is.EqualTo(50));
            Assert.That(secondWin[0x4AC51..(0x4AC51 + 26)], Is.EqualTo(firstWin[0x4AC51..(0x4AC51 + 26)]));
            Assert.That(BitConverter.ToUInt32(secondWin, 0x4AC51 + 11), Is.EqualTo(16));
            Assert.That(secondWin[0x4AC51 + 15], Is.EqualTo(1));
            Assert.That(BitConverter.ToUInt32(secondWin, 0x4AC51 + 16), Is.EqualTo(0xFE01003Fu));
            var firstResources = Integers(steppedOff, 0x3B5C4, 7);
            var secondResources = Integers(secondWin, 0x3B5C4, 7);
            Assert.That(secondResources, Is.EqualTo(new[] { 20, 20, 20, 20, 20, 20, 700 }));
            Assert.That(secondResources.Zip(firstResources, (a, b) => a - b), Is.EqualTo(new[] { 10, 10, 10, 10, 10, 10, 0 }));
            var game = TestServices.GameService.LoadGameFromBytes(secondWin, "creature_banks_03_cyclops_stockpile_1_won_again.GM1");
            Assert.That(game.History.Current.ToBytes(), Is.EqualTo(secondWin));
        }

        [Test]
        public void TheCompanionMapFixesFourTreasuryTiersAtTheSavedObjectAnchors()
        {
            var map = TestServices.SavefileCodec.Read(Path.Combine("TestData", "Hota", "CreatureBanks", "creature_banks_00_baseline.h3m"));
            Assert.That(BitConverter.ToUInt32(map, 0x137C5), Is.EqualTo(12));
            Assert.That(System.Text.Encoding.ASCII.GetString(map, 0x137C9, 12), Is.EqualTo("AVXbnk20.def"));
            Assert.That(BitConverter.ToUInt32(map, 0x137C9 + 12 + 16), Is.EqualTo(16));
            Assert.That(BitConverter.ToUInt32(map, 0x137C9 + 12 + 20), Is.EqualTo(1));
            var bytes = Control("creature_banks_04_dwarven_treasury_1_before_victory");
            int[] amounts = [30, 20, 15, 10];
            int[] crystal = [10, 5, 3, 2];
            int[] gold = [7500, 5000, 4000, 2500];
            for (var i = 0; i < 4; i++)
            {
                var at = 0x149D8 + (i * 21);
                var position = new byte[] { (byte)(11 + (i * 3)), 24, 0 };
                Assert.That(map[at..(at + 3)], Is.EqualTo(position));
                Assert.That(BitConverter.ToUInt32(map, at + 3), Is.EqualTo(85));
                Assert.That(BitConverter.ToUInt32(map, at + 12), Is.EqualTo(3 - i));
                Assert.That(map[at + 16], Is.EqualTo(0xFF));
                var savedPosition = 0x3AEB4 + 2 + ((175 + i) * 5) + 2;
                Assert.That(bytes[savedPosition..(savedPosition + 3)], Is.EqualTo(position));
                var entrance = 0xA6A7 + (i * 0x4E);
                Assert.That(BitConverter.ToUInt16(bytes, entrance), Is.EqualTo(16));
                Assert.That(BitConverter.ToUInt16(bytes, entrance + 2), Is.EqualTo(1));
                Assert.That(BitConverter.ToUInt16(bytes, entrance + 4), Is.EqualTo(175 + i));
                Assert.That(BitConverter.ToUInt32(bytes, entrance + 6), Is.EqualTo(0xFC01801Fu + ((uint)i << 13)));
                var record = RecordAt(bytes, 0x7EB9B, 12 + i);
                var amount = amounts[i];
                Assert.That(Integers(bytes, record, 7), Is.EqualTo(new[] { 16, 16, i == 1 ? 17 : 16, 16, 16, -1, -1 }));
                Assert.That(Integers(bytes, record + 28, 7), Is.EqualTo(new[] { amount, amount, amount, amount, amount, 0, 0 }));
                Assert.That(Integers(bytes, record + 56, 7), Is.EqualTo(new[] { 0, 0, 0, 0, crystal[i], 0, gold[i] }));
                Assert.That(BitConverter.ToInt32(bytes, record + 84), Is.EqualTo(-1));
                Assert.That(bytes[record + 88], Is.Zero);
                Assert.That(BitConverter.ToUInt16(bytes, record + 89), Is.Zero);
            }
        }

        [Test]
        public void WinningTheControlledTreasuryChangesOnlyItsGuardsInTheWholeBankTable()
        {
            var before = Control("creature_banks_04_dwarven_treasury_1_before_victory");
            var won = Control("creature_banks_05_dwarven_treasury_1_won");
            var steppedOff = Control("creature_banks_06_dwarven_treasury_1_stepped_off");
            Assert.That(BitConverter.ToUInt16(before, 0x7EB9B), Is.EqualTo(28));
            Assert.That(RecordAt(before, 0x7EB9B, 12), Is.EqualTo(0x7F061));
            Assert.That(NextRecord(before, RecordAt(before, 0x7EB9B, 27)), Is.EqualTo(0x7F611));
            var expected = (byte[])before.Clone();
            for (var i = 0; i < 7; i++)
            {
                BitConverter.GetBytes(-1).CopyTo(expected, 0x7F061 + (i * 4));
                BitConverter.GetBytes(0u).CopyTo(expected, 0x7F061 + 28 + (i * 4));
            }

            Assert.That(won[0x7EB9B..0x7F611], Is.EqualTo(expected[0x7EB9B..0x7F611]));
            Assert.That(steppedOff[0x7EB9B..0x7F611], Is.EqualTo(won[0x7EB9B..0x7F611]));
            Assert.That(Enumerable.Range(0x7EB9B, 0x7F611 - 0x7EB9B).Count(i => before[i] != won[i]), Is.EqualTo(25));
            Assert.That(Integers(won, 0x7F061 + 56, 7), Is.EqualTo(new[] { 0, 0, 0, 0, 10, 0, 7500 }));
        }

        [Test]
        public void AnOccupiedTreasuryKeepsItsSubtypeInTheGridAndItsClearedStateInTheHeroHeader()
        {
            var before = Control("creature_banks_04_dwarven_treasury_1_before_victory");
            var won = Control("creature_banks_05_dwarven_treasury_1_won");
            var steppedOff = Control("creature_banks_06_dwarven_treasury_1_stepped_off");
            Assert.That(BitConverter.ToUInt16(before, 0xA6A7), Is.EqualTo(16));
            Assert.That(BitConverter.ToUInt32(before, 0xA6A7 + 6), Is.EqualTo(0xFC01801Fu));
            Assert.That(BitConverter.ToUInt16(won, 0xA6A7), Is.EqualTo(34));
            Assert.That(BitConverter.ToUInt16(won, 0xA6A7 + 2), Is.EqualTo(1));
            Assert.That(BitConverter.ToUInt16(won, 0xA6A7 + 4), Is.EqualTo(175));
            Assert.That(BitConverter.ToUInt32(won, 0xA6A7 + 6), Is.EqualTo(50));
            Assert.That(BitConverter.ToUInt16(steppedOff, 0xA6A7), Is.EqualTo(16));
            Assert.That(BitConverter.ToUInt16(steppedOff, 0xA6A7 + 2), Is.EqualTo(1));
            Assert.That(BitConverter.ToUInt32(steppedOff, 0xA6A7 + 6), Is.EqualTo(0xFE01803Fu));
            var game = TestServices.GameService.LoadGameFromBytes(won, "creature_banks_05_dwarven_treasury_1_won.GM2");
            var hero = game.History.Current.Heroes().Single(h => h.HeaderIndex == 0x4AC51);
            Assert.That(new[] { hero.GetHeaderByteAt(0), hero.GetHeaderByteAt(2), hero.GetHeaderByteAt(4) }, Is.EqualTo(new byte[] { 10, 24, 0 }));
            Assert.That(BitConverter.ToUInt32(won, hero.HeaderIndex + 11), Is.EqualTo(16));
            Assert.That(hero.GetHeaderByteAt(15), Is.EqualTo(1));
            Assert.That(BitConverter.ToUInt32(won, hero.HeaderIndex + 16), Is.EqualTo(0xFE01803Fu));
            Assert.That(BitConverter.ToUInt16(steppedOff, hero.HeaderIndex), Is.EqualTo(10));
            Assert.That(BitConverter.ToUInt16(steppedOff, hero.HeaderIndex + 2), Is.EqualTo(25));
            Assert.That(BitConverter.ToUInt32(steppedOff, hero.HeaderIndex + 11), Is.Zero);
            Assert.That(BitConverter.ToUInt32(steppedOff, hero.HeaderIndex + 16), Is.Zero);
            foreach (var footprint in new[] { 0xA68D, 0xA6C1 })
            {
                Assert.That(before[footprint..(footprint + 10)], Is.EqualTo(won[footprint..(footprint + 10)]));
                Assert.That(before[footprint..(footprint + 10)], Is.EqualTo(steppedOff[footprint..(footprint + 10)]));
                Assert.That(BitConverter.ToUInt32(before, footprint + 6), Is.EqualTo(12));
            }
        }

        [Test]
        public void TheControlledTreasuryVictoryGivesTenCrystalAndSevenThousandFiveHundredGold()
        {
            var before = Control("creature_banks_04_dwarven_treasury_1_before_victory");
            var won = Control("creature_banks_05_dwarven_treasury_1_won");
            var steppedOff = Control("creature_banks_06_dwarven_treasury_1_stepped_off");
            var start = Integers(before, 0x3B5C4, 7);
            var finish = Integers(won, 0x3B5C4, 7);
            Assert.That(start, Is.EqualTo(new[] { 0, 0, 0, 0, 0, 0, 700 }));
            Assert.That(finish, Is.EqualTo(new[] { 0, 0, 0, 0, 10, 0, 8200 }));
            Assert.That(finish.Zip(start, (a, b) => a - b), Is.EqualTo(new[] { 0, 0, 0, 0, 10, 0, 7500 }));
            Assert.That(Integers(steppedOff, 0x3B5C4, 7), Is.EqualTo(finish));
        }

        [Test]
        public void TheTreasuryResetProposalPreservesItsSubtypeAndChangesOnlyTheObservedGuardAndStateBytes()
        {
            var before = Control("creature_banks_04_dwarven_treasury_1_before_victory");
            var steppedOff = Control("creature_banks_06_dwarven_treasury_1_stepped_off");
            var proposal = (byte[])steppedOff.Clone();
            Array.Copy(before, 0x7F061, proposal, 0x7F061, 56);
            Array.Copy(before, 0xA6A7 + 6, proposal, 0xA6A7 + 6, 4);
            var changed = Enumerable.Range(0, proposal.Length).Where(i => proposal[i] != steppedOff[i]).ToArray();
            var expected = Enumerable.Range(0x7F061, 20)
                .Concat(Enumerable.Range(0, 5).Select(i => 0x7F061 + 28 + (i * 4)))
                .Concat(new[] { 0xA6AD, 0xA6B0 }).Order().ToArray();
            Assert.That(changed, Is.EqualTo(expected));
            Assert.That(changed, Has.Length.EqualTo(27));
            Assert.That(proposal[0xA6A7..(0xA6A7 + 6)], Is.EqualTo(steppedOff[0xA6A7..(0xA6A7 + 6)]));
            Assert.That(BitConverter.ToUInt16(proposal, 0xA6A7 + 2), Is.EqualTo(1));
            Assert.That(proposal[0x7F061..(0x7F061 + 91)], Is.EqualTo(before[0x7F061..(0x7F061 + 91)]));
            Assert.That(Integers(proposal, 0x3B5C4, 7), Is.EqualTo(Integers(steppedOff, 0x3B5C4, 7)));
        }

        [Test]
        public void WinningTheRestoredTreasuryGivesTheFullRewardAgainAndPreservesItsSubtype()
        {
            var firstWin = Control("creature_banks_05_dwarven_treasury_1_won");
            var steppedOff = Control("creature_banks_06_dwarven_treasury_1_stepped_off");
            var secondWin = Control("creature_banks_07_dwarven_treasury_1_won_again");
            Assert.That(secondWin[0x7EB9B..0x7F611], Is.EqualTo(firstWin[0x7EB9B..0x7F611]));
            Assert.That(Integers(secondWin, 0x7F061, 7), Is.All.EqualTo(-1));
            Assert.That(Integers(secondWin, 0x7F061 + 28, 7), Is.All.Zero);
            Assert.That(Integers(secondWin, 0x7F061 + 56, 7), Is.EqualTo(new[] { 0, 0, 0, 0, 10, 0, 7500 }));
            Assert.That(BitConverter.ToUInt16(secondWin, 0xA6A7), Is.EqualTo(34));
            Assert.That(BitConverter.ToUInt16(secondWin, 0xA6A7 + 2), Is.EqualTo(1));
            Assert.That(BitConverter.ToUInt16(secondWin, 0xA6A7 + 4), Is.EqualTo(175));
            Assert.That(BitConverter.ToUInt32(secondWin, 0xA6A7 + 6), Is.EqualTo(50));
            Assert.That(secondWin[0x4AC51..(0x4AC51 + 26)], Is.EqualTo(firstWin[0x4AC51..(0x4AC51 + 26)]));
            Assert.That(BitConverter.ToUInt32(secondWin, 0x4AC51 + 11), Is.EqualTo(16));
            Assert.That(secondWin[0x4AC51 + 15], Is.EqualTo(1));
            Assert.That(BitConverter.ToUInt32(secondWin, 0x4AC51 + 16), Is.EqualTo(0xFE01803Fu));
            var firstResources = Integers(steppedOff, 0x3B5C4, 7);
            var secondResources = Integers(secondWin, 0x3B5C4, 7);
            Assert.That(secondResources, Is.EqualTo(new[] { 0, 0, 0, 0, 20, 0, 15700 }));
            Assert.That(secondResources.Zip(firstResources, (a, b) => a - b), Is.EqualTo(new[] { 0, 0, 0, 0, 10, 0, 7500 }));
            var game = TestServices.GameService.LoadGameFromBytes(secondWin, "creature_banks_07_dwarven_treasury_1_won_again.GM2");
            Assert.That(game.History.Current.ToBytes(), Is.EqualTo(secondWin));
        }

        [Test]
        public void TheCompanionMapFixesFourImpCacheTiersAtTheSavedObjectAnchors()
        {
            var map = TestServices.SavefileCodec.Read(Path.Combine("TestData", "Hota", "CreatureBanks", "creature_banks_00_baseline.h3m"));
            var bytes = Control("creature_banks_08_imp_cache_1_before_victory", ".GM1");
            Assert.That(System.Text.Encoding.ASCII.GetString(map, 0x13803, 12), Is.EqualTo("AVXbnk40.def"));
            Assert.That(BitConverter.ToUInt32(map, 0x13803 + 12 + 16), Is.EqualTo(16));
            Assert.That(BitConverter.ToUInt32(map, 0x13803 + 12 + 20), Is.EqualTo(3));
            var amounts = new[] { 60, 40, 30, 20 };
            var mercury = new[] { 6, 4, 3, 2 };
            var gold = new[] { 3000, 2000, 1500, 1000 };
            for (var i = 0; i < 4; i++)
            {
                var at = 0x14A2C + (i * 21);
                var position = new byte[] { (byte)(2 + (i * 2)), 27, 0 };
                Assert.That(map[at..(at + 3)], Is.EqualTo(position));
                Assert.That(BitConverter.ToUInt32(map, at + 3), Is.EqualTo(86));
                Assert.That(BitConverter.ToUInt32(map, at + 12), Is.EqualTo(3 - i));
                Assert.That(map[at + 16], Is.EqualTo(0xFF));
                var savedPosition = 0x3AEB4 + 2 + ((179 + i) * 5) + 2;
                Assert.That(bytes[savedPosition..(savedPosition + 3)], Is.EqualTo(position));
                var entrance = 0xB99B + (i * 0x38);
                Assert.That(BitConverter.ToUInt16(bytes, entrance), Is.EqualTo(16));
                Assert.That(BitConverter.ToUInt16(bytes, entrance + 2), Is.EqualTo(3));
                Assert.That(BitConverter.ToUInt16(bytes, entrance + 4), Is.EqualTo(179 + i));
                Assert.That(BitConverter.ToUInt32(bytes, entrance + 6), Is.EqualTo(0xFC02001Fu + ((uint)i << 13)));
                var record = RecordAt(bytes, 0x7EB9B, 16 + i);
                var amount = amounts[i];
                Assert.That(Integers(bytes, record, 7), Is.EqualTo(new[] { 42, 42, i < 2 ? 43 : 42, 42, 42, -1, -1 }));
                Assert.That(Integers(bytes, record + 28, 7), Is.EqualTo(new[] { amount, amount, amount, amount, amount, 0, 0 }));
                Assert.That(Integers(bytes, record + 56, 7), Is.EqualTo(new[] { 0, mercury[i], 0, 0, 0, 0, gold[i] }));
                Assert.That(BitConverter.ToInt32(bytes, record + 84), Is.EqualTo(-1));
                Assert.That(bytes[record + 88], Is.Zero);
                Assert.That(BitConverter.ToUInt16(bytes, record + 89), Is.Zero);
            }
        }

        [Test]
        public void WinningTheControlledImpCacheChangesOnlyItsGuardsInTheWholeBankTable()
        {
            var before = Control("creature_banks_08_imp_cache_1_before_victory", ".GM1");
            var won = Control("creature_banks_09_imp_cache_1_won", ".GM1");
            var steppedOff = Control("creature_banks_10_imp_cache_1_stepped_off", ".GM1");
            Assert.That(BitConverter.ToUInt16(before, 0x7EB9B), Is.EqualTo(28));
            Assert.That(RecordAt(before, 0x7EB9B, 16), Is.EqualTo(0x7F1CD));
            Assert.That(NextRecord(before, RecordAt(before, 0x7EB9B, 27)), Is.EqualTo(0x7F611));
            var expected = (byte[])before.Clone();
            for (var i = 0; i < 7; i++)
            {
                BitConverter.GetBytes(-1).CopyTo(expected, 0x7F1CD + (i * 4));
                BitConverter.GetBytes(0u).CopyTo(expected, 0x7F1CD + 28 + (i * 4));
            }

            Assert.That(won[0x7EB9B..0x7F611], Is.EqualTo(expected[0x7EB9B..0x7F611]));
            Assert.That(steppedOff[0x7EB9B..0x7F611], Is.EqualTo(won[0x7EB9B..0x7F611]));
            Assert.That(Enumerable.Range(0x7EB9B, 0x7F611 - 0x7EB9B).Count(i => before[i] != won[i]), Is.EqualTo(25));
            Assert.That(Integers(won, 0x7F1CD + 56, 7), Is.EqualTo(new[] { 0, 6, 0, 0, 0, 0, 3000 }));
        }

        [Test]
        public void TheImpCacheVictoryPreservesItsGridSubtypeAndHidesTheClearedStateInTheHeroHeader()
        {
            var before = Control("creature_banks_08_imp_cache_1_before_victory", ".GM1");
            var won = Control("creature_banks_09_imp_cache_1_won", ".GM1");
            var steppedOff = Control("creature_banks_10_imp_cache_1_stepped_off", ".GM1");
            Assert.That(BitConverter.ToUInt32(before, 0xB99B + 6), Is.EqualTo(0xFC02001Fu));
            Assert.That(BitConverter.ToUInt16(won, 0xB99B), Is.EqualTo(34));
            Assert.That(BitConverter.ToUInt16(won, 0xB99B + 2), Is.EqualTo(3));
            Assert.That(BitConverter.ToUInt16(won, 0xB99B + 4), Is.EqualTo(179));
            Assert.That(BitConverter.ToUInt32(won, 0xB99B + 6), Is.EqualTo(50));
            Assert.That(BitConverter.ToUInt16(steppedOff, 0xB99B), Is.EqualTo(16));
            Assert.That(BitConverter.ToUInt16(steppedOff, 0xB99B + 2), Is.EqualTo(3));
            Assert.That(BitConverter.ToUInt16(steppedOff, 0xB99B + 4), Is.EqualTo(179));
            Assert.That(BitConverter.ToUInt32(steppedOff, 0xB99B + 6), Is.EqualTo(0xFE02003Fu));
            var game = TestServices.GameService.LoadGameFromBytes(won, "creature_banks_09_imp_cache_1_won.GM1");
            var hero = game.History.Current.Heroes().Single(h => h.HeaderIndex == 0x4AC51);
            Assert.That(new[] { hero.GetHeaderByteAt(0), hero.GetHeaderByteAt(2), hero.GetHeaderByteAt(4) }, Is.EqualTo(new byte[] { 2, 27, 0 }));
            Assert.That(BitConverter.ToUInt32(won, hero.HeaderIndex + 11), Is.EqualTo(16));
            Assert.That(hero.GetHeaderByteAt(15), Is.EqualTo(1));
            Assert.That(BitConverter.ToUInt32(won, hero.HeaderIndex + 16), Is.EqualTo(0xFE02003Fu));
            Assert.That(BitConverter.ToUInt16(steppedOff, hero.HeaderIndex), Is.EqualTo(2));
            Assert.That(BitConverter.ToUInt16(steppedOff, hero.HeaderIndex + 2), Is.EqualTo(28));
            Assert.That(BitConverter.ToUInt32(steppedOff, hero.HeaderIndex + 11), Is.Zero);
            Assert.That(BitConverter.ToUInt32(steppedOff, hero.HeaderIndex + 16), Is.Zero);
            Assert.That(before[0xB981..(0xB981 + 10)], Is.EqualTo(won[0xB981..(0xB981 + 10)]));
            Assert.That(before[0xB981..(0xB981 + 10)], Is.EqualTo(steppedOff[0xB981..(0xB981 + 10)]));
            Assert.That(BitConverter.ToUInt32(before, 0xB981 + 6), Is.EqualTo(16));
            Assert.That(game.History.Current.ToBytes(), Is.EqualTo(won));
        }

        [Test]
        public void TheControlledImpCacheVictoryGivesSixMercuryAndThreeThousandGold()
        {
            var before = Control("creature_banks_08_imp_cache_1_before_victory", ".GM1");
            var won = Control("creature_banks_09_imp_cache_1_won", ".GM1");
            var steppedOff = Control("creature_banks_10_imp_cache_1_stepped_off", ".GM1");
            var start = Integers(before, 0x3B5C4, 7);
            var finish = Integers(won, 0x3B5C4, 7);
            Assert.That(start, Is.EqualTo(new[] { 0, 0, 0, 0, 0, 0, 700 }));
            Assert.That(finish, Is.EqualTo(new[] { 0, 6, 0, 0, 0, 0, 3700 }));
            Assert.That(finish.Zip(start, (a, b) => a - b), Is.EqualTo(new[] { 0, 6, 0, 0, 0, 0, 3000 }));
            Assert.That(Integers(steppedOff, 0x3B5C4, 7), Is.EqualTo(finish));
        }

        [Test]
        public void TheImpCacheResetProposalRestoresTheOriginalFamiliarAndOnlyTheObservedGuardAndStateBytes()
        {
            var before = Control("creature_banks_08_imp_cache_1_before_victory", ".GM1");
            var steppedOff = Control("creature_banks_10_imp_cache_1_stepped_off", ".GM1");
            var proposal = (byte[])steppedOff.Clone();
            Array.Copy(before, 0x7F1CD, proposal, 0x7F1CD, 56);
            Array.Copy(before, 0xB99B + 6, proposal, 0xB99B + 6, 4);
            var changed = Enumerable.Range(0, proposal.Length).Where(i => proposal[i] != steppedOff[i]).ToArray();
            var expected = Enumerable.Range(0x7F1CD, 20)
                .Concat(Enumerable.Range(0, 5).Select(i => 0x7F1CD + 28 + (i * 4)))
                .Concat(new[] { 0xB9A1, 0xB9A4 }).Order().ToArray();
            Assert.That(changed, Is.EqualTo(expected));
            Assert.That(changed, Has.Length.EqualTo(27));
            Assert.That(proposal[0xB99B..(0xB99B + 6)], Is.EqualTo(steppedOff[0xB99B..(0xB99B + 6)]));
            Assert.That(BitConverter.ToUInt16(proposal, 0xB99B + 2), Is.EqualTo(3));
            Assert.That(proposal[0x7F1CD..(0x7F1CD + 91)], Is.EqualTo(before[0x7F1CD..(0x7F1CD + 91)]));
            Assert.That(Integers(proposal, 0x7F1CD, 7), Is.EqualTo(new[] { 42, 42, 43, 42, 42, -1, -1 }));
            Assert.That(Integers(proposal, 0x3B5C4, 7), Is.EqualTo(Integers(steppedOff, 0x3B5C4, 7)));
        }

        [Test]
        public void WinningTheRestoredImpCacheGivesTheFullRewardAgainAndPreservesItsSubtype()
        {
            var firstWin = Control("creature_banks_09_imp_cache_1_won", ".GM1");
            var steppedOff = Control("creature_banks_10_imp_cache_1_stepped_off", ".GM1");
            var secondWin = Control("creature_banks_11_imp_cache_1_won_again", ".GM1");
            Assert.That(secondWin[0x7EB9B..0x7F611], Is.EqualTo(firstWin[0x7EB9B..0x7F611]));
            Assert.That(Integers(secondWin, 0x7F1CD, 7), Is.All.EqualTo(-1));
            Assert.That(Integers(secondWin, 0x7F1CD + 28, 7), Is.All.Zero);
            Assert.That(Integers(secondWin, 0x7F1CD + 56, 7), Is.EqualTo(new[] { 0, 6, 0, 0, 0, 0, 3000 }));
            Assert.That(BitConverter.ToUInt16(secondWin, 0xB99B), Is.EqualTo(34));
            Assert.That(BitConverter.ToUInt16(secondWin, 0xB99B + 2), Is.EqualTo(3));
            Assert.That(BitConverter.ToUInt16(secondWin, 0xB99B + 4), Is.EqualTo(179));
            Assert.That(BitConverter.ToUInt32(secondWin, 0xB99B + 6), Is.EqualTo(50));
            Assert.That(secondWin[0x4AC51..(0x4AC51 + 26)], Is.EqualTo(firstWin[0x4AC51..(0x4AC51 + 26)]));
            Assert.That(BitConverter.ToUInt32(secondWin, 0x4AC51 + 11), Is.EqualTo(16));
            Assert.That(secondWin[0x4AC51 + 15], Is.EqualTo(1));
            Assert.That(BitConverter.ToUInt32(secondWin, 0x4AC51 + 16), Is.EqualTo(0xFE02003Fu));
            var firstResources = Integers(steppedOff, 0x3B5C4, 7);
            var secondResources = Integers(secondWin, 0x3B5C4, 7);
            Assert.That(secondResources, Is.EqualTo(new[] { 0, 12, 0, 0, 0, 0, 6700 }));
            Assert.That(secondResources.Zip(firstResources, (a, b) => a - b), Is.EqualTo(new[] { 0, 6, 0, 0, 0, 0, 3000 }));
            var game = TestServices.GameService.LoadGameFromBytes(secondWin, "creature_banks_11_imp_cache_1_won_again.GM1");
            Assert.That(game.History.Current.ToBytes(), Is.EqualTo(secondWin));
        }

        [Test]
        public void TheCompanionMapFixesFourMedusaStoresTiersAtTheSavedObjectAnchors()
        {
            var map = TestServices.SavefileCodec.Read(Path.Combine("TestData", "Hota", "CreatureBanks", "creature_banks_00_baseline.h3m"));
            var bytes = Control("creature_banks_12_medusa_stores_1_before_victory");
            Assert.That(System.Text.Encoding.ASCII.GetString(map, 0x1383D, 12), Is.EqualTo("AVXbnk50.def"));
            Assert.That(BitConverter.ToUInt32(map, 0x1383D + 12 + 16), Is.EqualTo(16));
            Assert.That(BitConverter.ToUInt32(map, 0x1383D + 12 + 20), Is.EqualTo(4));
            var amounts = new[] { 10, 8, 6, 4 };
            var sulfur = new[] { 10, 8, 6, 5 };
            var gold = new[] { 5000, 4000, 3000, 2000 };
            for (var i = 0; i < 4; i++)
            {
                var at = 0x14A80 + (i * 21);
                var position = new byte[] { (byte)(10 + (i * 2)), 27, 0 };
                Assert.That(map[at..(at + 3)], Is.EqualTo(position));
                Assert.That(BitConverter.ToUInt32(map, at + 3), Is.EqualTo(87));
                Assert.That(BitConverter.ToUInt32(map, at + 12), Is.EqualTo(3 - i));
                Assert.That(map[at + 16], Is.EqualTo(0xFF));
                var savedPosition = 0x3AEB4 + 2 + ((183 + i) * 5) + 2;
                Assert.That(bytes[savedPosition..(savedPosition + 3)], Is.EqualTo(position));
                var entrance = 0xBA5D + (i * 0x34);
                Assert.That(BitConverter.ToUInt16(bytes, entrance), Is.EqualTo(16));
                Assert.That(BitConverter.ToUInt16(bytes, entrance + 2), Is.EqualTo(4));
                Assert.That(BitConverter.ToUInt16(bytes, entrance + 4), Is.EqualTo(183 + i));
                Assert.That(BitConverter.ToUInt32(bytes, entrance + 6), Is.EqualTo(0xFC02801Fu + ((uint)i << 13)));
                var record = RecordAt(bytes, 0x7EB9B, 20 + i);
                var amount = amounts[i];
                Assert.That(Integers(bytes, record, 7), Is.EqualTo(new[] { 76, 76, i == 3 ? 77 : 76, 76, 76, -1, -1 }));
                Assert.That(Integers(bytes, record + 28, 7), Is.EqualTo(new[] { amount, amount, amount, amount, amount, 0, 0 }));
                Assert.That(Integers(bytes, record + 56, 7), Is.EqualTo(new[] { 0, 0, 0, sulfur[i], 0, 0, gold[i] }));
                Assert.That(BitConverter.ToInt32(bytes, record + 84), Is.EqualTo(-1));
                Assert.That(bytes[record + 88], Is.Zero);
                Assert.That(BitConverter.ToUInt16(bytes, record + 89), Is.Zero);
            }
        }

        [Test]
        public void WinningTheControlledMedusaStoresChangesOnlyItsGuardsInTheWholeBankTable()
        {
            var before = Control("creature_banks_12_medusa_stores_1_before_victory");
            var won = Control("creature_banks_13_medusa_stores_1_won");
            var steppedOff = Control("creature_banks_14_medusa_stores_1_stepped_off");
            Assert.That(BitConverter.ToUInt16(before, 0x7EB9B), Is.EqualTo(28));
            Assert.That(RecordAt(before, 0x7EB9B, 20), Is.EqualTo(0x7F339));
            Assert.That(NextRecord(before, RecordAt(before, 0x7EB9B, 27)), Is.EqualTo(0x7F611));
            var expected = (byte[])before.Clone();
            for (var i = 0; i < 7; i++)
            {
                BitConverter.GetBytes(-1).CopyTo(expected, 0x7F339 + (i * 4));
                BitConverter.GetBytes(0u).CopyTo(expected, 0x7F339 + 28 + (i * 4));
            }

            Assert.That(won[0x7EB9B..0x7F611], Is.EqualTo(expected[0x7EB9B..0x7F611]));
            Assert.That(steppedOff[0x7EB9B..0x7F611], Is.EqualTo(won[0x7EB9B..0x7F611]));
            Assert.That(Enumerable.Range(0x7EB9B, 0x7F611 - 0x7EB9B).Count(i => before[i] != won[i]), Is.EqualTo(25));
            Assert.That(Integers(won, 0x7F339 + 56, 7), Is.EqualTo(new[] { 0, 0, 0, 10, 0, 0, 5000 }));
        }

        [Test]
        public void TheMedusaStoresVictoryPreservesItsGridSubtypeAndHidesTheClearedStateInTheHeroHeader()
        {
            var before = Control("creature_banks_12_medusa_stores_1_before_victory");
            var won = Control("creature_banks_13_medusa_stores_1_won");
            var steppedOff = Control("creature_banks_14_medusa_stores_1_stepped_off");
            Assert.That(BitConverter.ToUInt32(before, 0xBA5D + 6), Is.EqualTo(0xFC02801Fu));
            Assert.That(BitConverter.ToUInt16(won, 0xBA5D), Is.EqualTo(34));
            Assert.That(BitConverter.ToUInt16(won, 0xBA5D + 2), Is.EqualTo(4));
            Assert.That(BitConverter.ToUInt16(won, 0xBA5D + 4), Is.EqualTo(183));
            Assert.That(BitConverter.ToUInt32(won, 0xBA5D + 6), Is.EqualTo(50));
            Assert.That(BitConverter.ToUInt16(steppedOff, 0xBA5D), Is.EqualTo(16));
            Assert.That(BitConverter.ToUInt16(steppedOff, 0xBA5D + 2), Is.EqualTo(4));
            Assert.That(BitConverter.ToUInt16(steppedOff, 0xBA5D + 4), Is.EqualTo(183));
            Assert.That(BitConverter.ToUInt32(steppedOff, 0xBA5D + 6), Is.EqualTo(0xFE02803Fu));
            var game = TestServices.GameService.LoadGameFromBytes(won, "creature_banks_13_medusa_stores_1_won.GM2");
            var hero = game.History.Current.Heroes().Single(h => h.HeaderIndex == 0x4AC51);
            Assert.That(new[] { hero.GetHeaderByteAt(0), hero.GetHeaderByteAt(2), hero.GetHeaderByteAt(4) }, Is.EqualTo(new byte[] { 9, 27, 0 }));
            Assert.That(BitConverter.ToUInt32(won, hero.HeaderIndex + 11), Is.EqualTo(16));
            Assert.That(hero.GetHeaderByteAt(15), Is.EqualTo(1));
            Assert.That(BitConverter.ToUInt32(won, hero.HeaderIndex + 16), Is.EqualTo(0xFE02803Fu));
            Assert.That(BitConverter.ToUInt16(steppedOff, hero.HeaderIndex), Is.EqualTo(9));
            Assert.That(BitConverter.ToUInt16(steppedOff, hero.HeaderIndex + 2), Is.EqualTo(28));
            Assert.That(BitConverter.ToUInt32(steppedOff, hero.HeaderIndex + 11), Is.Zero);
            Assert.That(BitConverter.ToUInt32(steppedOff, hero.HeaderIndex + 16), Is.Zero);
            Assert.That(before[0xBA77..(0xBA77 + 10)], Is.EqualTo(won[0xBA77..(0xBA77 + 10)]));
            Assert.That(before[0xBA77..(0xBA77 + 10)], Is.EqualTo(steppedOff[0xBA77..(0xBA77 + 10)]));
            Assert.That(BitConverter.ToUInt32(before, 0xBA77 + 6), Is.EqualTo(20));
            Assert.That(game.History.Current.ToBytes(), Is.EqualTo(won));
        }

        [Test]
        public void TheControlledMedusaStoresVictoryGivesTenSulfurAndFiveThousandGold()
        {
            var before = Control("creature_banks_12_medusa_stores_1_before_victory");
            var won = Control("creature_banks_13_medusa_stores_1_won");
            var steppedOff = Control("creature_banks_14_medusa_stores_1_stepped_off");
            var start = Integers(before, 0x3B5C4, 7);
            var finish = Integers(won, 0x3B5C4, 7);
            Assert.That(start, Is.EqualTo(new[] { 0, 0, 0, 0, 0, 0, 700 }));
            Assert.That(finish, Is.EqualTo(new[] { 0, 0, 0, 10, 0, 0, 5700 }));
            Assert.That(finish.Zip(start, (a, b) => a - b), Is.EqualTo(new[] { 0, 0, 0, 10, 0, 0, 5000 }));
            Assert.That(Integers(steppedOff, 0x3B5C4, 7), Is.EqualTo(finish));
        }

        [Test]
        public void TheMedusaStoresResetProposalChangesOnlyTheObservedGuardAndStateBytes()
        {
            var before = Control("creature_banks_12_medusa_stores_1_before_victory");
            var steppedOff = Control("creature_banks_14_medusa_stores_1_stepped_off");
            var proposal = (byte[])steppedOff.Clone();
            Array.Copy(before, 0x7F339, proposal, 0x7F339, 56);
            Array.Copy(before, 0xBA5D + 6, proposal, 0xBA5D + 6, 4);
            var changed = Enumerable.Range(0, proposal.Length).Where(i => proposal[i] != steppedOff[i]).ToArray();
            var expected = Enumerable.Range(0x7F339, 20)
                .Concat(Enumerable.Range(0, 5).Select(i => 0x7F339 + 28 + (i * 4)))
                .Concat(new[] { 0xBA63, 0xBA66 }).Order().ToArray();
            Assert.That(changed, Is.EqualTo(expected));
            Assert.That(changed, Has.Length.EqualTo(27));
            Assert.That(proposal[0xBA5D..(0xBA5D + 6)], Is.EqualTo(steppedOff[0xBA5D..(0xBA5D + 6)]));
            Assert.That(BitConverter.ToUInt16(proposal, 0xBA5D + 2), Is.EqualTo(4));
            Assert.That(proposal[0x7F339..(0x7F339 + 91)], Is.EqualTo(before[0x7F339..(0x7F339 + 91)]));
            Assert.That(Integers(proposal, 0x3B5C4, 7), Is.EqualTo(Integers(steppedOff, 0x3B5C4, 7)));
        }

        [Test]
        public void WinningTheRestoredMedusaStoresGivesTheFullRewardAgainAndPreservesItsSubtype()
        {
            var firstWin = Control("creature_banks_13_medusa_stores_1_won");
            var steppedOff = Control("creature_banks_14_medusa_stores_1_stepped_off");
            var secondWin = Control("creature_banks_15_medusa_stores_1_won_again");
            Assert.That(secondWin[0x7EB9B..0x7F611], Is.EqualTo(firstWin[0x7EB9B..0x7F611]));
            Assert.That(Integers(secondWin, 0x7F339, 7), Is.All.EqualTo(-1));
            Assert.That(Integers(secondWin, 0x7F339 + 28, 7), Is.All.Zero);
            Assert.That(Integers(secondWin, 0x7F339 + 56, 7), Is.EqualTo(new[] { 0, 0, 0, 10, 0, 0, 5000 }));
            Assert.That(BitConverter.ToUInt16(secondWin, 0xBA5D), Is.EqualTo(34));
            Assert.That(BitConverter.ToUInt16(secondWin, 0xBA5D + 2), Is.EqualTo(4));
            Assert.That(BitConverter.ToUInt16(secondWin, 0xBA5D + 4), Is.EqualTo(183));
            Assert.That(BitConverter.ToUInt32(secondWin, 0xBA5D + 6), Is.EqualTo(50));
            Assert.That(secondWin[0x4AC51..(0x4AC51 + 26)], Is.EqualTo(firstWin[0x4AC51..(0x4AC51 + 26)]));
            Assert.That(BitConverter.ToUInt32(secondWin, 0x4AC51 + 11), Is.EqualTo(16));
            Assert.That(secondWin[0x4AC51 + 15], Is.EqualTo(1));
            Assert.That(BitConverter.ToUInt32(secondWin, 0x4AC51 + 16), Is.EqualTo(0xFE02803Fu));
            var firstResources = Integers(steppedOff, 0x3B5C4, 7);
            var secondResources = Integers(secondWin, 0x3B5C4, 7);
            Assert.That(secondResources, Is.EqualTo(new[] { 0, 0, 0, 20, 0, 0, 10700 }));
            Assert.That(secondResources.Zip(firstResources, (a, b) => a - b), Is.EqualTo(new[] { 0, 0, 0, 10, 0, 0, 5000 }));
            var game = TestServices.GameService.LoadGameFromBytes(secondWin, "creature_banks_15_medusa_stores_1_won_again.GM2");
            Assert.That(game.History.Current.ToBytes(), Is.EqualTo(secondWin));
        }

        [Test]
        public void TheCompanionMapFixesFourNagaBankTiersAtTheSavedObjectAnchors()
        {
            var map = TestServices.SavefileCodec.Read(Path.Combine("TestData", "Hota", "CreatureBanks", "creature_banks_00_baseline.h3m"));
            var bytes = Control("creature_banks_16_naga_bank_1_before_victory", ".GM1");
            Assert.That(System.Text.Encoding.ASCII.GetString(map, 0x13877, 12), Is.EqualTo("AVXbnk60.def"));
            Assert.That(BitConverter.ToUInt32(map, 0x13877 + 12 + 16), Is.EqualTo(16));
            Assert.That(BitConverter.ToUInt32(map, 0x13877 + 12 + 20), Is.EqualTo(5));
            var amounts = new[] { 6, 4, 3, 2 };
            var gems = new[] { 24, 16, 12, 8 };
            var gold = new[] { 12000, 8000, 6000, 4000 };
            var centers = new[] { 38, 39, 38, 39 };
            for (var i = 0; i < 4; i++)
            {
                var at = 0x14AD4 + (i * 21);
                var position = new byte[] { (byte)(18 + (i * 2)), 27, 0 };
                Assert.That(map[at..(at + 3)], Is.EqualTo(position));
                Assert.That(BitConverter.ToUInt32(map, at + 3), Is.EqualTo(88));
                Assert.That(BitConverter.ToUInt32(map, at + 12), Is.EqualTo(3 - i));
                Assert.That(map[at + 16], Is.EqualTo(0xFF));
                var savedPosition = 0x3AEB4 + 2 + ((187 + i) * 5) + 2;
                Assert.That(bytes[savedPosition..(savedPosition + 3)], Is.EqualTo(position));
                var entrance = 0xBB31 + (i * 0x38);
                Assert.That(BitConverter.ToUInt16(bytes, entrance), Is.EqualTo(16));
                Assert.That(BitConverter.ToUInt16(bytes, entrance + 2), Is.EqualTo(5));
                Assert.That(BitConverter.ToUInt16(bytes, entrance + 4), Is.EqualTo(187 + i));
                Assert.That(BitConverter.ToUInt32(bytes, entrance + 6), Is.EqualTo(0xFC03001Fu + ((uint)i << 13)));
                var record = RecordAt(bytes, 0x7EB9B, 24 + i);
                var amount = amounts[i];
                Assert.That(Integers(bytes, record, 7), Is.EqualTo(new[] { 38, 38, centers[i], 38, 38, -1, -1 }));
                Assert.That(Integers(bytes, record + 28, 7), Is.EqualTo(new[] { amount, amount, amount, amount, amount, 0, 0 }));
                Assert.That(Integers(bytes, record + 56, 7), Is.EqualTo(new[] { 0, 0, 0, 0, 0, gems[i], gold[i] }));
                Assert.That(BitConverter.ToInt32(bytes, record + 84), Is.EqualTo(-1));
                Assert.That(bytes[record + 88], Is.Zero);
                Assert.That(BitConverter.ToUInt16(bytes, record + 89), Is.Zero);
            }
        }

        [Test]
        public void WinningTheControlledNagaBankChangesOnlyItsGuardsInTheWholeBankTable()
        {
            var before = Control("creature_banks_16_naga_bank_1_before_victory", ".GM1");
            var won = Control("creature_banks_17_naga_bank_1_won", ".GM1");
            var steppedOff = Control("creature_banks_18_naga_bank_1_stepped_off", ".GM1");
            Assert.That(BitConverter.ToUInt16(before, 0x7EB9B), Is.EqualTo(28));
            Assert.That(RecordAt(before, 0x7EB9B, 24), Is.EqualTo(0x7F4A5));
            Assert.That(NextRecord(before, RecordAt(before, 0x7EB9B, 27)), Is.EqualTo(0x7F611));
            var expected = (byte[])before.Clone();
            for (var i = 0; i < 7; i++)
            {
                BitConverter.GetBytes(-1).CopyTo(expected, 0x7F4A5 + (i * 4));
                BitConverter.GetBytes(0u).CopyTo(expected, 0x7F4A5 + 28 + (i * 4));
            }

            Assert.That(won[0x7EB9B..0x7F611], Is.EqualTo(expected[0x7EB9B..0x7F611]));
            Assert.That(steppedOff[0x7EB9B..0x7F611], Is.EqualTo(won[0x7EB9B..0x7F611]));
            Assert.That(Enumerable.Range(0x7EB9B, 0x7F611 - 0x7EB9B).Count(i => before[i] != won[i]), Is.EqualTo(25));
            Assert.That(Integers(won, 0x7F4A5 + 56, 7), Is.EqualTo(new[] { 0, 0, 0, 0, 0, 24, 12000 }));
        }

        [Test]
        public void TheNagaBankVictoryPreservesItsGridSubtypeAndHidesTheClearedStateInTheHeroHeader()
        {
            var before = Control("creature_banks_16_naga_bank_1_before_victory", ".GM1");
            var won = Control("creature_banks_17_naga_bank_1_won", ".GM1");
            var steppedOff = Control("creature_banks_18_naga_bank_1_stepped_off", ".GM1");
            Assert.That(BitConverter.ToUInt32(before, 0xBB31 + 6), Is.EqualTo(0xFC03001Fu));
            Assert.That(BitConverter.ToUInt16(won, 0xBB31), Is.EqualTo(34));
            Assert.That(BitConverter.ToUInt16(won, 0xBB31 + 2), Is.EqualTo(5));
            Assert.That(BitConverter.ToUInt16(won, 0xBB31 + 4), Is.EqualTo(187));
            Assert.That(BitConverter.ToUInt32(won, 0xBB31 + 6), Is.EqualTo(50));
            Assert.That(BitConverter.ToUInt16(steppedOff, 0xBB31), Is.EqualTo(16));
            Assert.That(BitConverter.ToUInt16(steppedOff, 0xBB31 + 2), Is.EqualTo(5));
            Assert.That(BitConverter.ToUInt16(steppedOff, 0xBB31 + 4), Is.EqualTo(187));
            Assert.That(BitConverter.ToUInt32(steppedOff, 0xBB31 + 6), Is.EqualTo(0xFE03003Fu));
            var game = TestServices.GameService.LoadGameFromBytes(won, "creature_banks_17_naga_bank_1_won.GM1");
            var hero = game.History.Current.Heroes().Single(h => h.HeaderIndex == 0x4AC51);
            Assert.That(new[] { hero.GetHeaderByteAt(0), hero.GetHeaderByteAt(2), hero.GetHeaderByteAt(4) }, Is.EqualTo(new byte[] { 17, 27, 0 }));
            Assert.That(BitConverter.ToUInt32(won, hero.HeaderIndex + 11), Is.EqualTo(16));
            Assert.That(hero.GetHeaderByteAt(15), Is.EqualTo(1));
            Assert.That(BitConverter.ToUInt32(won, hero.HeaderIndex + 16), Is.EqualTo(0xFE03003Fu));
            Assert.That(BitConverter.ToUInt16(steppedOff, hero.HeaderIndex), Is.EqualTo(17));
            Assert.That(BitConverter.ToUInt16(steppedOff, hero.HeaderIndex + 2), Is.EqualTo(28));
            Assert.That(BitConverter.ToUInt32(steppedOff, hero.HeaderIndex + 11), Is.Zero);
            Assert.That(BitConverter.ToUInt32(steppedOff, hero.HeaderIndex + 16), Is.Zero);
            Assert.That(before[0xBB4B..(0xBB4B + 10)], Is.EqualTo(won[0xBB4B..(0xBB4B + 10)]));
            Assert.That(before[0xBB4B..(0xBB4B + 10)], Is.EqualTo(steppedOff[0xBB4B..(0xBB4B + 10)]));
            Assert.That(BitConverter.ToUInt32(before, 0xBB4B + 6), Is.EqualTo(24));
            Assert.That(game.History.Current.ToBytes(), Is.EqualTo(won));
        }

        [Test]
        public void TheControlledNagaBankVictoryGivesTwentyFourGemsAndTwelveThousandGold()
        {
            var before = Control("creature_banks_16_naga_bank_1_before_victory", ".GM1");
            var won = Control("creature_banks_17_naga_bank_1_won", ".GM1");
            var steppedOff = Control("creature_banks_18_naga_bank_1_stepped_off", ".GM1");
            var start = Integers(before, 0x3B5C4, 7);
            var finish = Integers(won, 0x3B5C4, 7);
            Assert.That(start, Is.EqualTo(new[] { 0, 0, 0, 0, 0, 0, 700 }));
            Assert.That(finish, Is.EqualTo(new[] { 0, 0, 0, 0, 0, 24, 12700 }));
            Assert.That(finish.Zip(start, (a, b) => a - b), Is.EqualTo(new[] { 0, 0, 0, 0, 0, 24, 12000 }));
            Assert.That(Integers(steppedOff, 0x3B5C4, 7), Is.EqualTo(finish));
        }

        [Test]
        public void TheNagaBankResetProposalChangesOnlyTheObservedGuardAndStateBytes()
        {
            var before = Control("creature_banks_16_naga_bank_1_before_victory", ".GM1");
            var steppedOff = Control("creature_banks_18_naga_bank_1_stepped_off", ".GM1");
            var proposal = (byte[])steppedOff.Clone();
            Array.Copy(before, 0x7F4A5, proposal, 0x7F4A5, 56);
            Array.Copy(before, 0xBB31 + 6, proposal, 0xBB31 + 6, 4);
            var changed = Enumerable.Range(0, proposal.Length).Where(i => proposal[i] != steppedOff[i]).ToArray();
            var expected = Enumerable.Range(0x7F4A5, 20)
                .Concat(Enumerable.Range(0, 5).Select(i => 0x7F4A5 + 28 + (i * 4)))
                .Concat(new[] { 0xBB37, 0xBB3A }).Order().ToArray();
            Assert.That(changed, Is.EqualTo(expected));
            Assert.That(changed, Has.Length.EqualTo(27));
            Assert.That(proposal[0xBB31..(0xBB31 + 6)], Is.EqualTo(steppedOff[0xBB31..(0xBB31 + 6)]));
            Assert.That(BitConverter.ToUInt16(proposal, 0xBB31 + 2), Is.EqualTo(5));
            Assert.That(proposal[0x7F4A5..(0x7F4A5 + 91)], Is.EqualTo(before[0x7F4A5..(0x7F4A5 + 91)]));
            Assert.That(Integers(proposal, 0x3B5C4, 7), Is.EqualTo(Integers(steppedOff, 0x3B5C4, 7)));
        }

        [Test]
        public void WinningTheRestoredNagaBankGivesTheFullRewardAgainAndPreservesItsSubtype()
        {
            var firstWin = Control("creature_banks_17_naga_bank_1_won", ".GM1");
            var steppedOff = Control("creature_banks_18_naga_bank_1_stepped_off", ".GM1");
            var secondWin = Control("creature_banks_19_naga_bank_1_won_again");
            Assert.That(secondWin[0x7EB9B..0x7F611], Is.EqualTo(firstWin[0x7EB9B..0x7F611]));
            Assert.That(Integers(secondWin, 0x7F4A5, 7), Is.All.EqualTo(-1));
            Assert.That(Integers(secondWin, 0x7F4A5 + 28, 7), Is.All.Zero);
            Assert.That(Integers(secondWin, 0x7F4A5 + 56, 7), Is.EqualTo(new[] { 0, 0, 0, 0, 0, 24, 12000 }));
            Assert.That(BitConverter.ToUInt16(secondWin, 0xBB31), Is.EqualTo(34));
            Assert.That(BitConverter.ToUInt16(secondWin, 0xBB31 + 2), Is.EqualTo(5));
            Assert.That(BitConverter.ToUInt16(secondWin, 0xBB31 + 4), Is.EqualTo(187));
            Assert.That(BitConverter.ToUInt32(secondWin, 0xBB31 + 6), Is.EqualTo(50));
            Assert.That(secondWin[0x4AC51..(0x4AC51 + 26)], Is.EqualTo(firstWin[0x4AC51..(0x4AC51 + 26)]));
            Assert.That(BitConverter.ToUInt32(secondWin, 0x4AC51 + 11), Is.EqualTo(16));
            Assert.That(secondWin[0x4AC51 + 15], Is.EqualTo(1));
            Assert.That(BitConverter.ToUInt32(secondWin, 0x4AC51 + 16), Is.EqualTo(0xFE03003Fu));
            var firstResources = Integers(steppedOff, 0x3B5C4, 7);
            var secondResources = Integers(secondWin, 0x3B5C4, 7);
            Assert.That(secondResources, Is.EqualTo(new[] { 0, 0, 0, 0, 0, 48, 24700 }));
            Assert.That(secondResources.Zip(firstResources, (a, b) => a - b), Is.EqualTo(new[] { 0, 0, 0, 0, 0, 24, 12000 }));
            var game = TestServices.GameService.LoadGameFromBytes(secondWin, "creature_banks_19_naga_bank_1_won_again.GM2");
            Assert.That(game.History.Current.ToBytes(), Is.EqualTo(secondWin));
        }

        [Test]
        public void ACyclopsFootprintOrdinalIsNotItsBankTableIndex()
        {
            var bytes = Load("bios_standard");
            Assert.That(BitConverter.ToUInt16(bytes, 0xA392), Is.EqualTo(16));
            Assert.That(BitConverter.ToUInt16(bytes, 0xA392 + 2), Is.Zero);
            Assert.That(BitConverter.ToUInt16(bytes, 0xA392 + 4), Is.EqualTo(1370));
            Assert.That(BitConverter.ToUInt32(bytes, 0xA392 + 6), Is.EqualTo(30));
            Assert.That((BitConverter.ToUInt32(bytes, 0xA370 + 6) >> 13) & 0xFFF, Is.EqualTo(11));
        }
    }
}
