namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Joins supported HotA entrance indexes to the counted, variable-length creature-bank table.
    /// Footprint numbers are object numbers, not bank indexes. Occupied entrance state is kept by the hero.
    /// </summary>
    public sealed class CreatureBankLoader : ICreatureBankLoader
    {
        private static readonly ILogger<CreatureBankLoader> Logger = LogFactory.For<CreatureBankLoader>();

        public IReadOnlyList<CreatureBankChunk> Load(byte[] bytes, GameVersion version, IReadOnlyList<HeroChunk> heroes, int searchFrom)
        {
            if (version != GameVersion.HornOfTheAbyss)
            {
                Logger.LogDebug("Creature-bank editing is not established for {Version}", version);
                return [];
            }

            var entrances = FindEntrances(bytes, heroes);
            if (entrances == null)
            {
                Logger.LogWarning("Creature-bank entrances are ambiguous; no banks are exposed");
                return [];
            }

            if (entrances.Count == 0)
            {
                return [];
            }

            List<(int Start, int Length)>? table = null;
            var highest = entrances.Max(e => e.BankIndex);
            for (var at = Math.Max(0, searchFrom); at + 2 + CreatureBankLayout.Artifacts <= bytes.Length; at++)
            {
                var count = BitConverter.ToUInt16(bytes, at);
                if (count <= highest || count > Constants.CreatureBank.MaxBanks || !IsArmy(bytes, at + 2))
                {
                    continue;
                }

                var candidate = ReadTable(bytes, at + 2, count);
                if (candidate == null || entrances.Any(e => !IsSupportedBank(bytes, candidate[e.BankIndex], e)))
                {
                    continue;
                }

                if (table != null)
                {
                    Logger.LogWarning("Competing creature-bank tables; no banks are exposed");
                    return [];
                }

                table = candidate;
            }

            if (table == null)
            {
                Logger.LogDebug("No validated creature-bank table for {Count} supported entrances", entrances.Count);
                return [];
            }

            var chunks = entrances.OrderBy(e => e.BankIndex).Select((e, index) =>
            {
                var (start, length) = table[e.BankIndex];
                var name = $"{e.Kind} {entrances.Count(other => other.Kind == e.Kind && other.BankIndex <= e.BankIndex)}";
                return new CreatureBankChunk(
                    name,
                    bytes.AsSpan(start, length).ToArray(),
                    start,
                    index,
                    e.BankIndex,
                    e.Position,
                    e.Kind,
                    new RawBytesChunk(name, BitConverter.GetBytes(e.State), e.StateOffset),
                    e.HeroIndex);
            }).ToList();
            Logger.LogDebug("Located {Count} supported creature banks", chunks.Count);
            return chunks;
        }

        private static List<Entrance>? FindEntrances(byte[] bytes, IReadOnlyList<HeroChunk> heroes)
        {
            var objects = MapObjectPositions.Find(bytes, bytes.Length);
            if (objects.Count == 0)
            {
                return [];
            }

            var definition = bytes.AsSpan().IndexOf(".def"u8);
            if (definition < 0)
            {
                return [];
            }

            var footprints = new Dictionary<int, (MapPosition Position, CreatureBankKind Kind)>();
            var occupied = new List<(int ObjectIndex, int Subtype, uint HeroId)>();
            var found = new Dictionary<int, Entrance>();
            var unsupported = new HashSet<int>();
            for (var at = 0; at + 10 <= definition; at++)
            {
                var type = BitConverter.ToUInt16(bytes, at);
                var subtype = BitConverter.ToUInt16(bytes, at + 2);
                var objectIndex = BitConverter.ToUInt16(bytes, at + 4);
                if (objectIndex >= objects.Count)
                {
                    continue;
                }

                var state = BitConverter.ToUInt32(bytes, at + 6);
                if (type == 34)
                {
                    occupied.Add((objectIndex, subtype, state));
                    continue;
                }

                var kind = KindOf(type, subtype);
                if (kind == null)
                {
                    continue;
                }

                var isEntrance = IsEntranceState(state, kind.Value);
                if (!isEntrance && state >= Constants.CreatureBank.MaxBanks)
                {
                    if ((state & 0xFC000000) == 0xFC000000)
                    {
                        unsupported.Add(objectIndex);
                    }

                    continue;
                }

                var position = objects[objectIndex];
                footprints.TryAdd(objectIndex, (position, kind.Value));
                if (!isEntrance)
                {
                    continue;
                }

                var entrance = new Entrance(CreatureBankLayout.BankIndex(state), position, kind.Value, state, at + 6, null);
                if (!found.TryAdd(objectIndex, entrance))
                {
                    return null;
                }
            }

            for (var index = 0; index < heroes.Count; index++)
            {
                var hero = heroes[index];
                var type = CreatureBankLayout.HeroValue(hero, CreatureBankLayout.UnderHeroType);
                if (type is not (CreatureBankLayout.ObjectType or CreatureBankLayout.BankObjectType))
                {
                    continue;
                }

                var state = CreatureBankLayout.HeroValue(hero, CreatureBankLayout.UnderHeroState);
                var matches = occupied.Where(cell =>
                    cell.HeroId == hero.GetByteAt(HeroOffsets.HeroIdOffset)
                    && footprints.TryGetValue(cell.ObjectIndex, out var footprint)
                    && KindOf((int)type, cell.Subtype) == footprint.Kind)
                    .Select(cell => cell.ObjectIndex).Distinct().ToList();
                if (matches.Count == 0 && type == CreatureBankLayout.BankObjectType)
                {
                    Logger.LogTrace("Skipped an occupied creature bank with no supported footprint");
                    continue;
                }

                if (matches.Count == 1 && !IsEntranceState(state, footprints[matches[0]].Kind))
                {
                    unsupported.Add(matches[0]);
                    continue;
                }

                if (matches.Count != 1 || found.ContainsKey(matches[0]))
                {
                    return null;
                }

                var objectIndex = matches[0];
                var match = footprints[objectIndex];
                found.Add(objectIndex, new Entrance(
                    CreatureBankLayout.BankIndex(state),
                    match.Position,
                    match.Kind,
                    state,
                    hero.HeaderIndex + CreatureBankLayout.UnderHeroState,
                    index));
            }

            foreach (var objectIndex in unsupported)
            {
                footprints.Remove(objectIndex);
                found.Remove(objectIndex);
            }

            if (unsupported.Count > 0)
            {
                Logger.LogDebug("Skipped {Count} creature-bank entrances with unsupported state bits", unsupported.Count);
            }

            // Every footprint must have exactly one entrance, on the grid or underneath a hero.
            if (found.Count != footprints.Count || found.Values.Select(e => e.BankIndex).Distinct().Count() != found.Count)
            {
                return null;
            }

            return found.Values.ToList();
        }

        private static CreatureBankKind? KindOf(int type, int subtype) => (type, subtype) switch
        {
            (CreatureBankLayout.ObjectType, 0) => CreatureBankKind.DragonUtopia,
            (CreatureBankLayout.BankObjectType, 0) => CreatureBankKind.CyclopsStockpile,
            (CreatureBankLayout.BankObjectType, 1) => CreatureBankKind.DwarvenTreasury,
            (CreatureBankLayout.BankObjectType, 3) => CreatureBankKind.ImpCache,
            (CreatureBankLayout.BankObjectType, 4) => CreatureBankKind.MedusaStores,
            (CreatureBankLayout.BankObjectType, 5) => CreatureBankKind.NagaBank,
            _ => null,
        };

        private static bool IsEntranceState(uint state, CreatureBankKind kind)
        {
            if (kind != CreatureBankKind.DragonUtopia)
            {
                var index = (uint)CreatureBankLayout.BankIndex(state) << CreatureBankLayout.BankIndexShift;
                return state == (0xFC00001F | index) || state == (0xFE00003F | index);
            }

            return (state & 0xFC000000) == 0xFC000000
                && (state & CreatureBankLayout.ResetMask) is CreatureBankLayout.ReadyState or 0x0200003F;
        }

        private static List<(int Start, int Length)>? ReadTable(byte[] bytes, int start, int count)
        {
            var records = new List<(int, int)>();
            var at = start;
            for (var index = 0; index < count; index++)
            {
                if (at + CreatureBankLayout.Artifacts > bytes.Length || !IsArmy(bytes, at))
                {
                    return null;
                }

                for (var resource = 0; resource < Constants.CreatureBank.ResourceCount; resource++)
                {
                    if (BitConverter.ToInt32(bytes, at + CreatureBankLayout.Resources + (resource * 4)) < 0)
                    {
                        return null;
                    }
                }

                var rewardCreature = BitConverter.ToInt32(bytes, at + CreatureBankLayout.RewardCreature);
                if (rewardCreature is < -1 or > byte.MaxValue
                    || (rewardCreature == -1 && bytes[at + CreatureBankLayout.RewardCreatureCount] != 0))
                {
                    return null;
                }

                var artifacts = BitConverter.ToUInt16(bytes, at + CreatureBankLayout.ArtifactCount);
                var length = CreatureBankLayout.Artifacts + (artifacts * 4);
                if (artifacts > Constants.CreatureBank.MaxBankArtifacts || length > bytes.Length - at)
                {
                    return null;
                }

                records.Add((at, length));
                at += length;
            }

            return records;
        }

        private static bool IsArmy(byte[] bytes, int at)
        {
            if (at + CreatureBankLayout.Resources > bytes.Length)
            {
                return false;
            }

            for (var slot = 0; slot < Constants.CreatureSlotCount; slot++)
            {
                var type = BitConverter.ToInt32(bytes, at + (slot * 4));
                var count = BitConverter.ToInt32(bytes, at + CreatureBankLayout.Counts + (slot * 4));
                if (type is < -1 or > byte.MaxValue || count < 0 || (type == -1 && count != 0))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsSupportedBank(byte[] bytes, (int Start, int Length) record, Entrance entrance)
        {
            var kind = entrance.Kind;
            var artifactSlots = kind == CreatureBankKind.DragonUtopia ? Constants.CreatureBank.ArtifactSlots : 0;
            if (record.Length != CreatureBankLayout.Artifacts + (artifactSlots * 4))
            {
                return false;
            }

            var at = record.Start;
            var resources = Enumerable.Range(0, Constants.CreatureBank.ResourceCount).Select(i => BitConverter.ToInt32(bytes, at + CreatureBankLayout.Resources + (i * 4))).ToArray();
            var strength = CreatureBankPresets.FindStrength(kind, resources);
            if (strength == null || BitConverter.ToInt32(bytes, at + CreatureBankLayout.RewardCreature) != -1 || bytes[at + CreatureBankLayout.RewardCreatureCount] != 0)
            {
                return false;
            }

            var cleared = (entrance.State & CreatureBankLayout.EmptyFlag) != 0;
            var upgraded = kind != CreatureBankKind.DragonUtopia && BitConverter.ToInt32(bytes, at + (Constants.CreatureBank.CenterSlot * 4)) == CreatureBankPresets.Types(kind, true)[Constants.CreatureBank.CenterSlot];
            var types = cleared ? Enumerable.Repeat(-1, Constants.CreatureSlotCount).ToArray() : CreatureBankPresets.Types(kind, upgraded);
            var counts = cleared ? new int[Constants.CreatureSlotCount] : CreatureBankPresets.Counts(kind, strength.Value);
            for (var slot = 0; slot < types.Length; slot++)
            {
                if (BitConverter.ToInt32(bytes, at + (slot * 4)) != types[slot]
                    || BitConverter.ToInt32(bytes, at + CreatureBankLayout.Counts + (slot * 4)) != counts[slot])
                {
                    return false;
                }
            }

            return true;
        }

        private sealed record Entrance(int BankIndex, MapPosition Position, CreatureBankKind Kind, uint State, int StateOffset, int? HeroIndex);
    }
}
