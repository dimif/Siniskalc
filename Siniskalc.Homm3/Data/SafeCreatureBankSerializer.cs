namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    public sealed class SafeCreatureBankSerializer(EntityCatalog catalog) : ISerializer<CreatureBankChunk, CreatureBankModel>
    {
        private static readonly ILogger<SafeCreatureBankSerializer> Logger = LogFactory.For<SafeCreatureBankSerializer>();

        public CreatureBankChunk Serialize(CreatureBankChunk chunk, CreatureBankModel model)
        {
            using var subject = Logger.BeginSubject("Creature bank", chunk.Name);
            var artifactSlots = chunk.Kind == CreatureBankKind.DragonUtopia ? Constants.CreatureBank.ArtifactSlots : 0;
            RangeGuard.ThrowIfOutOfRange(model.Artifacts.Count, artifactSlots, artifactSlots, nameof(model.Artifacts));
            RangeGuard.ThrowIfOutOfRange(model.Strength, Constants.CreatureBank.MinStrength, Constants.CreatureBank.MaxStrength, nameof(model.Strength));
            if (model.Kind != chunk.Kind)
            {
                throw new ArgumentException("A bank's kind cannot be changed.", nameof(model));
            }

            var bytes = chunk.ToArray();
            var resources = Enumerable.Range(0, Constants.CreatureBank.ResourceCount).Select(i => chunk.GetInt32At(CreatureBankLayout.Resources + (i * 4))).ToArray();
            var storedStrength = CreatureBankPresets.FindStrength(chunk.Kind, resources);
            var previousStrength = storedStrength ?? Constants.CreatureBank.MinStrength;
            var previousCleared = chunk.GetInt32At(0) == -1;
            var previousUpgrade = chunk.Kind != CreatureBankKind.DragonUtopia && chunk.GetInt32At(Constants.CreatureBank.CenterSlot * 4) == CreatureBankPresets.Types(chunk.Kind, true)[Constants.CreatureBank.CenterSlot];
            if (previousCleared != model.IsCleared || previousStrength != model.Strength || previousUpgrade != model.UpgradedCenter)
            {
                var types = model.IsCleared ? Enumerable.Repeat(-1, Constants.CreatureSlotCount).ToArray() : CreatureBankPresets.Types(model.Kind, model.UpgradedCenter);
                var counts = model.IsCleared ? new int[Constants.CreatureSlotCount] : CreatureBankPresets.Counts(model.Kind, model.Strength);
                for (var slot = 0; slot < Constants.CreatureSlotCount; slot++)
                {
                    BitConverter.GetBytes(types[slot]).CopyTo(bytes, slot * 4);
                    BitConverter.GetBytes(counts[slot]).CopyTo(bytes, CreatureBankLayout.Counts + (slot * 4));
                }
            }
            else
            {
                var types = previousCleared ? Enumerable.Repeat(-1, Constants.CreatureSlotCount).ToArray() : CreatureBankPresets.Types(chunk.Kind, previousUpgrade);
                var counts = previousCleared ? new int[Constants.CreatureSlotCount] : CreatureBankPresets.Counts(chunk.Kind, previousStrength);
                if (Enumerable.Range(0, Constants.CreatureSlotCount).Any(slot => chunk.GetInt32At(slot * 4) != types[slot] || chunk.GetInt32At(CreatureBankLayout.Counts + (slot * 4)) != counts[slot]))
                {
                    Logger.LogTrace("Kept non-preset guard bytes; the model's guard settings are unchanged");
                }
            }

            if (previousStrength != model.Strength)
            {
                var rewards = CreatureBankPresets.Resources(model.Kind, model.Strength);
                for (var resource = 0; resource < rewards.Length; resource++)
                {
                    BitConverter.GetBytes(rewards[resource]).CopyTo(bytes, CreatureBankLayout.Resources + (resource * 4));
                }
            }
            else if (storedStrength == null)
            {
                Logger.LogTrace("Kept unknown bank resources; model still has tier {StandIn}", previousStrength);
            }

            for (var slot = 0; slot < model.Artifacts.Count; slot++)
            {
                var at = CreatureBankLayout.Artifacts + (slot * 4);
                var code = BitConverter.ToUInt32(bytes, at);
                var interpreted = code == uint.MaxValue ? null
                    : code <= byte.MaxValue && catalog.Artifacts.Find((byte)code) is { Wearable: true } artifact ? artifact : catalog.FallbackArtifact;
                if (interpreted != model.Artifacts[slot])
                {
                    BitConverter.GetBytes(model.Artifacts[slot] is { } replacement ? (uint)replacement.Code : uint.MaxValue).CopyTo(bytes, at);
                }
                else if (code != uint.MaxValue && (code > byte.MaxValue || catalog.Artifacts.Find((byte)code) is not { Wearable: true }))
                {
                    Logger.LogTrace("Kept unknown artifact {Code:X} in slot {Slot}; model still has {StandIn}", code, slot, catalog.FallbackArtifact.Id);
                }
            }

            return chunk.WithBytes(bytes);
        }
    }
}
