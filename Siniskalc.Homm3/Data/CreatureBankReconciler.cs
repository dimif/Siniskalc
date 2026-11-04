namespace Siniskalc.Homm3.Data
{
    using System;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;

    /// <summary>Keeps each bank entrance's cleared state in step with restoring or reverting its guards.</summary>
    public sealed class CreatureBankReconciler(IDocumentReconciler playerLists) : IDocumentReconciler
    {
        private static readonly ILogger<CreatureBankReconciler> Logger = LogFactory.For<CreatureBankReconciler>();

        public SaveDocument Reconcile(SaveDocument original, SaveDocument before, SaveDocument after)
        {
            var updated = playerLists.Reconcile(original, before, after);
            var previous = before.CreatureBanks();
            var banks = after.CreatureBanks();
            var changed = 0;
            for (var index = 0; index < banks.Count; index++)
            {
                if (ReferenceEquals(previous[index], banks[index]) || previous.GetModel(index).IsCleared == banks.GetModel(index).IsCleared)
                {
                    continue;
                }

                var chunk = updated.CreatureBanks()[index];
                var state = StateOf(updated, chunk);
                var target = banks.GetModel(index).IsCleared ? state | 0x02000020 : (state & ~CreatureBankLayout.ResetMask) | CreatureBankLayout.ReadyState;
                if (banks.GetModel(index).IsCleared == original.CreatureBanks().GetModel(index).IsCleared)
                {
                    target = (state & ~CreatureBankLayout.ResetMask) | (StateOf(original, original.CreatureBanks()[index]) & CreatureBankLayout.ResetMask);
                }

                if (chunk.OccupyingHero is { } heroIndex)
                {
                    var hero = updated.Heroes()[heroIndex];
                    var stateBytes = BitConverter.GetBytes(target);
                    for (var offset = 0; offset < stateBytes.Length; offset++)
                    {
                        hero = hero.WithHeaderByte(CreatureBankLayout.UnderHeroState + offset, stateBytes[offset]);
                    }

                    updated = updated.WithChunk(Homm3ChunkKinds.Hero, heroIndex, hero);
                }
                else
                {
                    updated = updated.WithChunk(Homm3ChunkKinds.CreatureBanks, index, chunk.WithEntrance(BitConverter.GetBytes(target)));
                }

                changed++;
            }

            if (changed > 0)
            {
                Logger.LogDebug("Reconciled {Count} creature-bank entrances with their guards", changed);
            }

            return updated;
        }

        private static uint StateOf(SaveDocument document, CreatureBankChunk chunk) =>
            chunk.OccupyingHero is { } index
                ? CreatureBankLayout.HeroValue(document.Heroes()[index], CreatureBankLayout.UnderHeroState)
                : chunk.Entrance.GetUInt32At(0);
    }
}
