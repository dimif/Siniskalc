namespace Siniskalc.Data
{
    using System;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// Changes a castle and the affected players' town lists in one immutable document.
    /// </summary>
    public static class CastleOwnership
    {
        public static SaveDocument Apply(SaveDocument document, SaveDocument original, int castleIndex, CastleChunk before, CastleChunk after, bool restoreOriginalOrder)
        {
            var result = document.WithCastle(castleIndex, after);
            var previousOwner = Owner(before);
            var owner = Owner(after);
            if (previousOwner == owner)
            {
                return result;
            }

            RangeGuard.ThrowIfOutOfRange(castleIndex, Constants.Player.MinTownId, Constants.Player.MaxTownId, nameof(castleIndex));
            if (previousOwner is { } previous)
            {
                result = UpdatePlayer(result, original, previous, castleIndex, false, restoreOriginalOrder);
            }

            if (owner is { } next)
            {
                result = UpdatePlayer(result, original, next, castleIndex, true, restoreOriginalOrder);
            }

            return result;
        }

        private static PlayerColor? Owner(CastleChunk chunk) =>
            ByteEnum.TryRead<PlayerColor>(chunk.GetByteAt(CastleLayout.Owner), out var color) ? color : null;

        private static SaveDocument UpdatePlayer(SaveDocument document, SaveDocument original, PlayerColor color, int castleIndex, bool add, bool restoreOriginalOrder)
        {
            var kind = Homm3ChunkKinds.Player;
            var players = document.Players();
            var index = players.FindByColor(color);
            if (index < 0)
            {
                kind = Homm3ChunkKinds.InactivePlayer;
                players = document.InactivePlayers();
                index = players.FindByColor(color);
            }

            if (index < 0)
            {
                throw new GameRuleException($"Cannot change town ownership: the savefile has no player record for {color}.");
            }

            var chunk = players[index];
            var count = chunk.GetByteAt(PlayerLayout.TownCount);
            if (count > Constants.Player.MaxTownCount)
            {
                throw new GameRuleException($"Cannot change town ownership: {color}'s town count is not supported.");
            }

            var ids = chunk.GetBytesAt(PlayerLayout.TownIds, count).ToList();
            var townId = (byte)castleIndex;
            if (ids.Distinct().Count() != ids.Count || ids.Contains(Constants.ByteOff) || ids.Contains(townId) == add)
            {
                throw new GameRuleException($"Cannot change town ownership: {color}'s town list disagrees with the castle ownership.");
            }

            if (add)
            {
                if (ids.Count == Constants.Player.MaxTownCount)
                {
                    throw new GameRuleException($"Cannot give {color} more than {Constants.Player.MaxTownCount} towns.");
                }

                ids.Add(townId);
                if (restoreOriginalOrder)
                {
                    var originalChunk = original.GetChunks<PlayerChunk>(kind)[index];
                    var originalCount = originalChunk.GetByteAt(PlayerLayout.TownCount);
                    if (originalCount > Constants.Player.MaxTownCount)
                    {
                        throw new GameRuleException($"Cannot restore town ownership: {color}'s original town count is not supported.");
                    }

                    var originalIds = originalChunk.GetBytesAt(PlayerLayout.TownIds, originalCount);
                    var originalPlace = Array.IndexOf(originalIds, townId);
                    if (originalPlace < 0)
                    {
                        throw new GameRuleException($"Cannot restore town ownership: the town is missing from {color}'s original town list.");
                    }

                    ids.Remove(townId);
                    var next = originalIds.Skip(originalPlace + 1).FirstOrDefault(ids.Contains, Constants.ByteOff);
                    var previous = originalIds.Take(originalPlace).LastOrDefault(ids.Contains, Constants.ByteOff);
                    var insertAt = next != Constants.ByteOff ? ids.IndexOf(next)
                        : previous != Constants.ByteOff ? ids.IndexOf(previous) + 1 : 0;
                    ids.Insert(insertAt, townId);
                }
            }
            else
            {
                ids.Remove(townId);
            }

            var bytes = chunk.ToArray();
            bytes[PlayerLayout.TownCount] = (byte)ids.Count;

            // Only the occupied prefix and the slot it vacates change; unknown trailing bytes are preserved.
            for (var i = 0; i < Math.Max(count, ids.Count); i++)
            {
                bytes[PlayerLayout.TownIds + i] = i < ids.Count ? ids[i] : Constants.ByteOff;
            }

            return document.WithChunk(kind, index, chunk.WithBytes(bytes));
        }
    }
}
