namespace Siniskalc.Homm3.Tests.Integration.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Homm3.Tests.Data;

    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class DragonUtopiaTests
    {
        private static string Control(string name) => Path.Combine("TestData", "Hota", "DragonUtopias", $"dragon_utopias_{name}.GM2");

        [TestCase(12, 3, 5, 1)]
        [TestCase(13, 2, 8, 2)]
        [TestCase(14, 1, 11, 3)]
        public void ReclearingEachWeakerUtopiaPaysItsGoldAndFourArtifactsAgainAndLeavesItDefeated(int save, int strength, int stepOffSave, int bankIndex)
        {
            var before = SavedGames.Get(Control($"{stepOffSave:00}_tier_{strength}_stepoff"));
            var again = SavedGames.Get(Control($"{save:00}_tier_{strength}_won_again"));
            var firstWin = SavedGames.Get(Control($"{stepOffSave - 1:00}_tier_{strength}_won"));
            var banks = again.History.Current.CreatureBanks();
            var bank = banks.Single(b => b.BankIndex == bankIndex);
            Assert.That(banks.Count, Is.EqualTo(28));
            Assert.That(banks.GetModel(bank.Index).IsCleared, Is.True);
            Assert.That(banks.GetModel(bank.Index).Strength, Is.EqualTo(strength));
            Assert.That(bank.OccupyingHero, Is.Not.Null);
            Assert.That(again.Bytes.AsSpan(0x7EB9B, 0xA76).ToArray(), Is.EqualTo(firstWin.Bytes.AsSpan(0x7EB9B, 0xA76).ToArray()));
            var heroIndex = bank.OccupyingHero!.Value;
            var hero = again.History.Current.Heroes()[heroIndex];
            var firstHero = firstWin.History.Current.Heroes()[heroIndex];
            Assert.That(Enumerable.Range(0, HeroChunk.HeaderLength).Select(hero.GetHeaderByteAt),
                Is.EqualTo(Enumerable.Range(0, HeroChunk.HeaderLength).Select(firstHero.GetHeaderByteAt)));
            var goldBefore = before.History.Current.Players().GetModels().Single(p => p.Color == PlayerColor.Red).Resources.Gold;
            var goldAfter = again.History.Current.Players().GetModels().Single(p => p.Color == PlayerColor.Red).Resources.Gold;
            Assert.That(goldAfter - goldBefore, Is.EqualTo((strength + 1) * 10000));
            var oldHero = before.History.Current.Heroes().GetModel(heroIndex);
            var newHero = again.History.Current.Heroes().GetModel(heroIndex);
            var oldArtifacts = oldHero.Backpack.All.Where(s => s != null).Select(s => s!.Art)
                .Concat(oldHero.EquippedArtifacts.GetEquippedArtifacts().Select(a => a.Item2)).ToList();
            var newArtifacts = newHero.Backpack.All.Where(s => s != null).Select(s => s!.Art)
                .Concat(newHero.EquippedArtifacts.GetEquippedArtifacts().Select(a => a.Item2)).ToList();
            foreach (var reward in banks.GetModel(bank.Index).Artifacts.Distinct())
            {
                Assert.That(newArtifacts.Count(a => a == reward) - oldArtifacts.Count(a => a == reward),
                    Is.EqualTo(banks.GetModel(bank.Index).Artifacts.Count(a => a == reward)), reward!.Name);
            }

            Assert.That(newArtifacts.Count - oldArtifacts.Count, Is.EqualTo(4));
            Assert.That(again.GetBytesToSave(), Is.EqualTo(again.Bytes));
        }

        [TestCase(3, 3, 1, 0x6BD7)]
        [TestCase(6, 2, 2, 0x6C77)]
        [TestCase(9, 1, 3, 0x6D17)]
        public void EachWeakerUtopiaVictoryClearsOnlyItsGuardsAndPaysItsRetainedGold(int firstSave, int strength, int bankIndex, int entrance)
        {
            var before = SavedGames.Get(Control($"{firstSave:00}_tier_{strength}_before"));
            var won = SavedGames.Get(Control($"{firstSave + 1:00}_tier_{strength}_won"));
            var steppedOff = SavedGames.Get(Control($"{firstSave + 2:00}_tier_{strength}_stepoff"));
            var banks = before.History.Current.CreatureBanks();
            var bank = banks.Single(b => b.BankIndex == bankIndex);
            var cleared = won.History.Current.CreatureBanks().Single(b => b.BankIndex == bankIndex);
            var off = steppedOff.History.Current.CreatureBanks().Single(b => b.BankIndex == bankIndex);
            var model = banks.GetModel(bank.Index);
            Assert.That(banks.Count, Is.EqualTo(28));
            Assert.That(model.Strength, Is.EqualTo(strength));
            Assert.That(model.Position, Is.EqualTo(new MapPosition(5 + (6 * bankIndex), 15)));
            Assert.That(model.IsCleared, Is.False);
            Assert.That(bank.Entrance.StartIndex, Is.EqualTo(entrance + 6));
            Assert.That(bank.Entrance.GetUInt32At(0), Is.EqualTo(0xFC00001Fu + ((uint)bankIndex << 13)));
            Assert.That(off.Entrance.GetUInt32At(0), Is.EqualTo(0xFE00003Fu + ((uint)bankIndex << 13)));
            Assert.That(cleared.OccupyingHero, Is.Not.Null);
            Assert.That(won.History.Current.CreatureBanks().GetModel(cleared.Index).IsCleared, Is.True);
            Assert.That(off.OccupyingHero, Is.Null);
            var expected = bank.ToArray();
            Array.Fill(expected, (byte)0xFF, 0, 28);
            Array.Clear(expected, 28, 28);
            Assert.That(cleared.ToArray(), Is.EqualTo(expected));
            Assert.That(off.ToArray(), Is.EqualTo(expected));
            var table = bank.StartIndex - (bankIndex * 107) - 2;
            var expectedTable = before.Bytes.AsSpan(table, 0xA76).ToArray();
            expected.CopyTo(expectedTable, bank.StartIndex - table);
            Assert.That(won.Bytes.AsSpan(table, expectedTable.Length).ToArray(), Is.EqualTo(expectedTable), "all 27 other banks and every reward stay unchanged");
            Assert.That(steppedOff.Bytes.AsSpan(table, expectedTable.Length).ToArray(), Is.EqualTo(expectedTable));
            var goldBefore = before.History.Current.Players().GetModels().Single(p => p.Color == PlayerColor.Red).Resources.Gold;
            var goldAfter = won.History.Current.Players().GetModels().Single(p => p.Color == PlayerColor.Red).Resources.Gold;
            Assert.That(goldAfter - goldBefore, Is.EqualTo((strength + 1) * 10000));
            Assert.That(before.GetBytesToSave(), Is.EqualTo(before.Bytes));
            Assert.That(won.GetBytesToSave(), Is.EqualTo(won.Bytes));
            Assert.That(steppedOff.GetBytesToSave(), Is.EqualTo(steppedOff.Bytes));
        }

        [TestCase(3, 3, 1)]
        [TestCase(6, 2, 2)]
        [TestCase(9, 1, 3)]
        public void RestoringEachWeakerUtopiaChangesExactlyItsOriginalGuardsAndEntranceState(int firstSave, int strength, int bankIndex)
        {
            var before = SavedGames.Get(Control($"{firstSave:00}_tier_{strength}_before"));
            foreach (var (sequence, step) in new[] { (firstSave + 1, "won"), (firstSave + 2, "stepoff") })
            {
                var game = SavedGames.Get(Control($"{sequence:00}_tier_{strength}_{step}"));
                var bank = game.History.Current.CreatureBanks().Single(b => b.BankIndex == bankIndex);
                var original = before.History.Current.CreatureBanks().Single(b => b.BankIndex == bankIndex);
                var expected = game.Bytes.ToArray();
                original.GetBytesAt(0, 56).CopyTo(expected, bank.StartIndex);
                original.Entrance.ToArray().CopyTo(expected, bank.Entrance.StartIndex);
                game.History.CreateCreatureBankSession(bank.Index).Update(m => m with { IsCleared = false });
                Assert.That(game.GetBytesToSave(), Is.EqualTo(expected), step);
                var reloaded = TestServices.GameService.LoadGameFromBytes(expected, $"tier_{strength}_restored.GM2");
                Assert.That(reloaded.History.Current.CreatureBanks().GetModel(bank.Index).IsCleared, Is.False);
                Assert.That(reloaded.History.Current.CreatureBanks().GetModel(bank.Index).Artifacts, Is.EqualTo(before.History.Current.CreatureBanks().GetModel(original.Index).Artifacts));
                game.History.Undo();
                Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
                game.History.Redo();
                Assert.That(game.GetBytesToSave(), Is.EqualTo(expected));
                game.History.GetPendingChanges().Single().Revert(game.History);
                Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
            }
        }

        [TestCase("00_before_first", false)]
        [TestCase("01_first_cleared", true)]
        [TestCase("02_first_cleared_again", true)]
        public void BothVictoriesRetainTheSameRewardsAndAllUneditedBytesRoundTrip(string name, bool cleared)
        {
            var game = SavedGames.Get(Control(name));
            var banks = game.History.Current.CreatureBanks();
            Assert.That(banks.Count, Is.EqualTo(8));
            Assert.That(banks.GetModel(0).IsCleared, Is.EqualTo(cleared));
            Assert.That(banks.GetModel(0).Strength, Is.EqualTo(4));
            Assert.That(banks.GetModel(0).Artifacts.Select(a => a!.Code), Is.EqualTo(new byte[] { 136, 72, 129, 128 }));
            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
        }

        [Test]
        public void TheEightUtopiasHaveTheirFixedAndRolledContentsAtTheCorrectEntrances()
        {
            var game = SavedGames.Get(Control("00_before_first"));
            var banks = game.History.Current.CreatureBanks();
            Assert.That(banks.Count, Is.EqualTo(8));
            Assert.That(banks.GetModels().Take(8).Select(m => m.Position),
                Is.EqualTo(new[] { new MapPosition(5, 15), new MapPosition(11, 15), new MapPosition(17, 15), new MapPosition(23, 15),
                    new MapPosition(5, 21), new MapPosition(11, 21), new MapPosition(17, 21), new MapPosition(23, 21) }));
            Assert.That(banks.GetModels().Take(8).Select(m => m.Strength), Is.EqualTo(new[] { 4, 3, 2, 1, 1, 1, 3, 2 }));
            Assert.That(banks.GetModels().Select(m => m.IsCleared), Has.All.False);
            Assert.That(banks.GetModel(0).Artifacts.Select(a => a!.Id), Is.EqualTo(new[] { "AdmiralsHat", "AngelWings", "AngelicAlliance", "ArmageddonsBlade" }));
            Assert.That(banks.GetModel(3).Artifacts.Select(a => a!.Code), Is.EqualTo(new byte[] { 136, 68, 31, 54 }));
            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void EachGuardPresetWritesExactlyItsDragonsAndGoldWithoutRerollingTheFourArtifacts(int strength)
        {
            var game = SavedGames.Get(Control("00_before_first"));
            var bank = game.History.Current.CreatureBanks()[0];
            var expected = game.Bytes.ToArray();
            var counts = strength switch
            {
                1 => new[] { 8, 5, 2, 1, 0, 0, 0 },
                2 => new[] { 8, 6, 3, 2, 0, 0, 0 },
                3 => new[] { 8, 6, 4, 3, 0, 0, 0 },
                _ => new[] { 8, 7, 6, 5, 0, 0, 0 },
            };
            for (var slot = 0; slot < counts.Length; slot++)
            {
                BitConverter.GetBytes(counts[slot]).CopyTo(expected, bank.StartIndex + 28 + (slot * 4));
            }

            BitConverter.GetBytes((strength + 1) * 10000).CopyTo(expected, bank.StartIndex + 80);
            var session = new EditSession<CreatureBankChunk, CreatureBankModel>(game.History, Homm3ChunkKinds.CreatureBanks, 0);
            session.Update(m => m with { Strength = strength });
            Assert.That(game.GetBytesToSave(), Is.EqualTo(expected));
            var reloaded = TestServices.GameService.LoadGameFromBytes(expected, "utopia_guards.GM2");
            Assert.That(reloaded.History.Current.CreatureBanks().GetModel(0).Strength, Is.EqualTo(strength));
        }

        [Test]
        public void UnknownArtifactsAndEmptySlotsSurviveAnEditOfAnotherReward()
        {
            var game = SavedGames.Get(Control("00_before_first"));
            var at = game.History.Current.CreatureBanks()[0].StartIndex + 91;
            var bytes = game.Bytes.ToArray();
            BitConverter.GetBytes(0x12345678u).CopyTo(bytes, at);
            BitConverter.GetBytes(uint.MaxValue).CopyTo(bytes, at + 4);
            var odd = TestServices.GameService.LoadGameFromBytes(bytes, "unknown_utopia_reward.GM2");
            Assert.That(odd.History.Current.CreatureBanks().Count, Is.EqualTo(8));
            Assert.That(odd.History.Current.CreatureBanks().GetModel(0).Artifacts[0], Is.EqualTo(TestCatalog.Hota.FallbackArtifact));
            Assert.That(odd.GetBytesToSave(), Is.EqualTo(bytes));
            var session = new EditSession<CreatureBankChunk, CreatureBankModel>(odd.History, Homm3ChunkKinds.CreatureBanks, 0);
            var replacement = TestCatalog.Hota.Artifacts.ById("BootsOfSpeed");
            session.Update(m => m.WithArtifact(3, replacement));
            var expected = bytes.ToArray();
            BitConverter.GetBytes((uint)replacement.Code).CopyTo(expected, at + 12);
            Assert.That(odd.GetBytesToSave(), Is.EqualTo(expected));
            odd.History.GetPendingChanges().Single().Revert(odd.History);
            Assert.That(odd.GetBytesToSave(), Is.EqualTo(bytes));
        }

        [Test]
        public void AnEntirelyClearedTableIsLocatedWithoutLookingForLiveDragonGuards()
        {
            var game = SavedGames.Get(Control("00_before_first"));
            var bytes = game.Bytes.ToArray();
            foreach (var bank in game.History.Current.CreatureBanks())
            {
                Array.Fill(bytes, (byte)0xFF, bank.StartIndex, 28);
                Array.Clear(bytes, bank.StartIndex + 28, 28);
                BitConverter.GetBytes(bank.Entrance.GetUInt32At(0) | 0x02000020u).CopyTo(bytes, bank.Entrance.StartIndex);
            }

            var allCleared = TestServices.GameService.LoadGameFromBytes(bytes, "all_utopias_cleared.GM2");
            Assert.That(allCleared.History.Current.CreatureBanks().Count, Is.EqualTo(8));
            Assert.That(allCleared.History.Current.CreatureBanks().GetModels().Select(m => m.IsCleared), Has.All.True);
            var bankToRestore = allCleared.History.Current.CreatureBanks()[5];
            Assert.That(bankToRestore.OccupyingHero, Is.Null);
            var session = new EditSession<CreatureBankChunk, CreatureBankModel>(allCleared.History, Homm3ChunkKinds.CreatureBanks, 5);
            session.Update(m => m with { IsCleared = false });
            var restored = allCleared.GetBytesToSave();
            var expected = bytes.ToArray();
            game.History.Current.CreatureBanks()[5].ToArray().CopyTo(expected, bankToRestore.StartIndex);
            BitConverter.GetBytes(game.History.Current.CreatureBanks()[5].Entrance.GetUInt32At(0)).CopyTo(expected, bankToRestore.Entrance.StartIndex);
            Assert.That(restored, Is.EqualTo(expected));
            Assert.That(TestServices.GameService.LoadGameFromBytes(restored, "restored_grid_utopia.GM2").History.Current.CreatureBanks().GetModel(5).IsCleared, Is.False);
            allCleared.History.GetPendingChanges().Single().Revert(allCleared.History);
            Assert.That(allCleared.GetBytesToSave(), Is.EqualTo(bytes));
        }

        [Test]
        public void TheBankTableCanMoveButACompetingCopyOrTruncatedRecordIsNotExposed()
        {
            var game = SavedGames.Get(Control("00_before_first"));
            var bank = game.History.Current.CreatureBanks()[0];
            var last = game.History.Current.CreatureBanks().Last();
            var table = game.Bytes.AsSpan(bank.StartIndex - 2, last.StartIndex + last.Length - bank.StartIndex + 2).ToArray();
            var duplicate = game.Bytes.Concat(table).ToArray();
            Assert.That(TestServices.GameService.LoadGameFromBytes(duplicate, "ambiguous_banks.GM2").History.Current.CreatureBanks(), Is.Empty);
            Array.Clear(duplicate, bank.StartIndex - 2, 2);
            var moved = TestServices.GameService.LoadGameFromBytes(duplicate, "moved_banks.GM2");
            Assert.That(moved.History.Current.CreatureBanks().Count, Is.EqualTo(8));
            Assert.That(moved.History.Current.CreatureBanks()[0].StartIndex, Is.EqualTo(game.Bytes.Length + 2));
            Assert.That(moved.GetBytesToSave(), Is.EqualTo(duplicate));
            var truncated = game.Bytes[..(last.StartIndex + last.Length - 1)];
            Assert.That(TestServices.GameService.LoadGameFromBytes(truncated, "truncated_banks.GM2").History.Current.CreatureBanks(), Is.Empty);
        }

        [Test]
        public void UtopiasKeepTheirBankIndexesInAMixedTableContainingCreatureRewardsAndDifferentArtifactCounts()
        {
            var game = SavedGames.Get(Path.Combine("TestData", "Hota", "VisitedPlaces", "visited_places_27_tazar_artifact_merchant.GM2"));
            var banks = game.History.Current.CreatureBanks();
            var utopias = banks.Where(b => b.Kind == CreatureBankKind.DragonUtopia).ToList();
            Assert.That(utopias.Select(b => b.BankIndex), Is.EqualTo(new[] { 2, 3 }));
            Assert.That(utopias.Select(b => banks.GetModel(b.Index).Position), Is.EqualTo(new[] { new MapPosition(39, 16), new MapPosition(46, 16) }));
            Assert.That(utopias.Select(b => banks.GetModel(b.Index).IsCleared), Is.EqualTo(new[] { true, false }));
            Assert.That(banks.GetModel(utopias[1].Index).Artifacts.Select(a => a!.Code), Is.EqualTo(new byte[] { 35, 133, 34, 139 }));
            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
            game.History.CreateCreatureBankSession(utopias[1].Index).Update(m => m with { Strength = 2 });
            var expected = game.Bytes.ToArray();
            var counts = new[] { 8, 6, 3, 2 };
            for (var slot = 0; slot < counts.Length; slot++)
            {
                BitConverter.GetBytes(counts[slot]).CopyTo(expected, utopias[1].StartIndex + 28 + (slot * 4));
            }

            BitConverter.GetBytes(30000).CopyTo(expected, utopias[1].StartIndex + 80);
            Assert.That(game.GetBytesToSave(), Is.EqualTo(expected), "other banks and their creature rewards are not rewritten");
        }

        [TestCase(80, 50001)]
        [TestCase(28, 999)]
        [TestCase(89, 5)]
        [TestCase(84, 256)]
        public void UnsupportedBankFieldsAreLeftUneditableWithoutChangingTheSave(int offset, int value)
        {
            var game = SavedGames.Get(Control("00_before_first"));
            var bytes = game.Bytes.ToArray();
            BitConverter.GetBytes(value).CopyTo(bytes, game.History.Current.CreatureBanks()[0].StartIndex + offset);
            var unsupported = TestServices.GameService.LoadGameFromBytes(bytes, "unsupported_utopia.GM2");
            Assert.That(unsupported.History.Current.CreatureBanks(), Is.Empty);
            Assert.That(unsupported.GetBytesToSave(), Is.EqualTo(bytes));
        }

        [Test]
        public void RevertingARestoredUtopiaKeepsAnIndependentEditOfTheOccupyingHero()
        {
            var game = SavedGames.Get(Control("01_first_cleared"));
            var heroIndex = game.History.Current.CreatureBanks()[0].OccupyingHero!.Value;
            game.History.CreateHeroSession(heroIndex).Update(h => h.WithSpellPoints(42));
            var heroEdit = game.GetBytesToSave();
            Assert.That(heroEdit, Is.Not.EqualTo(game.Bytes));
            game.History.CreateCreatureBankSession(0).Update(m => m with { IsCleared = false });
            Assert.That(game.GetBytesToSave(), Is.Not.EqualTo(heroEdit));
            game.History.GetPendingChanges().Single(c => c.Change.Description == "Cleared").Revert(game.History);
            Assert.That(game.GetBytesToSave(), Is.EqualTo(heroEdit));
            Assert.That(game.History.Current.Heroes().GetModel(heroIndex).SpellPoints, Is.EqualTo(42));
        }

        [Test]
        public void EveryLocatedUtopiaInTheSaveCorpusRoundTripsWithoutChangingAnyBytes()
        {
            var banksChecked = 0;
            var mixedTables = 0;
            foreach (var path in Directory.EnumerateFiles("TestData", "*.GM?", SearchOption.AllDirectories))
            {
                var game = SavedGames.Get(path);
                var banks = game.History.Current.CreatureBanks();
                if (game.History.Current.GameVersion() != GameVersion.HornOfTheAbyss)
                {
                    Assert.That(banks, Is.Empty, path);
                }

                foreach (var bank in banks)
                {
                    Assert.That(banks.Serializer.Serialize(bank, banks.Deserializer.Deserialize(bank)).HasSameBytes(bank), Is.True, path);
                    banksChecked++;
                }

                if (banks.Count > 0)
                {
                    Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes), path);
                    if (!path.Contains("MerchantStock") && !path.Contains("DragonUtopias"))
                    {
                        mixedTables++;
                    }
                }
            }

            TestContext.Out.WriteLine($"Checked {banksChecked} Utopia records, including {mixedTables} saves outside the eight-Utopia control map");
            Assert.That(banksChecked, Is.GreaterThanOrEqualTo(8 * 14));
            Assert.That(mixedTables, Is.GreaterThan(0));
        }

        [TestCase("A")]
        [TestCase("A biography long enough to place the complete hero header outside the main chunk.")]
        public void RestoringAnOccupiedUtopiaWithABiographyChangesOnlyItsGuardsAndActualHeroHeader(string biography)
        {
            var game = SavedGames.Get(Control("01_first_cleared"));
            var bank = game.History.Current.CreatureBanks()[0];
            var hero = game.History.Current.Heroes()[bank.OccupyingHero!.Value];
            var at = hero.HeaderIndex + HeroChunk.HeaderLength;
            var text = System.Text.Encoding.ASCII.GetBytes(biography);
            var bytes = game.Bytes[..at].Concat(text).Concat(game.Bytes[at..]).ToArray();
            BitConverter.GetBytes(text.Length).CopyTo(bytes, hero.HeaderIndex + HeroChunk.BiographyLengthOffset);
            var withBiography = TestServices.GameService.LoadGameFromBytes(bytes, "utopia_with_biography.GM2");
            var banks = withBiography.History.Current.CreatureBanks();
            Assert.That(banks.Count, Is.EqualTo(8));
            Assert.That(banks[0].Entrance.StartIndex, Is.EqualTo(hero.HeaderIndex + 16));
            var shiftedHero = withBiography.History.Current.Heroes()[banks[0].OccupyingHero!.Value];
            Assert.That(shiftedHero.Biography, Is.EqualTo(biography));
            Assert.That(shiftedHero.StartIndex, Is.EqualTo(hero.StartIndex + text.Length));
            var expected = bytes.ToArray();
            var counts = new[] { 8, 7, 6, 5, 0, 0, 0 };
            var types = new[] { 26, 82, 27, 83, -1, -1, -1 };
            for (var slot = 0; slot < counts.Length; slot++)
            {
                BitConverter.GetBytes(types[slot]).CopyTo(expected, banks[0].StartIndex + (slot * 4));
                BitConverter.GetBytes(counts[slot]).CopyTo(expected, banks[0].StartIndex + 28 + (slot * 4));
            }

            BitConverter.GetBytes(0xFC00001Fu).CopyTo(expected, hero.HeaderIndex + 16);
            var session = withBiography.History.CreateCreatureBankSession(0);
            session.Update(m => m with { IsCleared = false });
            Assert.That(withBiography.GetBytesToSave(), Is.EqualTo(expected));
            var reloaded = TestServices.GameService.LoadGameFromBytes(expected, "restored_utopia_biography.GM2");
            Assert.That(reloaded.History.Current.CreatureBanks().GetModel(0).IsCleared, Is.False);
            Assert.That(reloaded.History.Current.Heroes()[banks[0].OccupyingHero!.Value].Biography, Is.EqualTo(biography));
            withBiography.History.GetPendingChanges().Single().Revert(withBiography.History);
            Assert.That(withBiography.GetBytesToSave(), Is.EqualTo(bytes));
        }

        [Test]
        public void RestoringTheOccupiedUtopiaWritesExactlyTheSuccessfulInGameResetAndCanBeUndoneAndReverted()
        {
            var before = SavedGames.Get(Control("00_before_first"));
            var game = SavedGames.Get(Control("01_first_cleared"));
            var banks = game.History.Current.CreatureBanks();
            Assert.That(banks.Count, Is.EqualTo(8));
            var bank = banks[0];
            Assert.That(bank.OccupyingHero, Is.Not.Null);
            Assert.That(banks.GetModel(0).IsCleared, Is.True);
            var expected = game.Bytes.ToArray();
            before.History.Current.CreatureBanks()[0].GetBytesAt(0, 56).CopyTo(expected, bank.StartIndex);
            BitConverter.GetBytes(0xFC00001Fu).CopyTo(expected, bank.Entrance.StartIndex);
            var session = new EditSession<CreatureBankChunk, CreatureBankModel>(game.History, Homm3ChunkKinds.CreatureBanks, 0);
            session.Update(m => m with { IsCleared = false });
            Assert.That(game.GetBytesToSave(), Is.EqualTo(expected));
            Assert.That(game.History.GetPendingChanges().Select(c => c.Change.Description), Is.EqualTo(new[] { "Cleared" }));
            var reloaded = TestServices.GameService.LoadGameFromBytes(expected, "utopia_restored.GM2");
            Assert.That(reloaded.History.Current.CreatureBanks().Count, Is.EqualTo(8));
            Assert.That(reloaded.History.Current.CreatureBanks().GetModel(0).IsCleared, Is.False);
            game.History.Undo();
            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
            game.History.Redo();
            Assert.That(game.GetBytesToSave(), Is.EqualTo(expected));
            game.History.GetPendingChanges().Single().Revert(game.History);
            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
        }
    }
}
