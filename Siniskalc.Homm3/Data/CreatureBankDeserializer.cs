namespace Siniskalc.Homm3.Data
{
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    public sealed class CreatureBankDeserializer(EntityCatalog catalog) : IDeserializer<CreatureBankChunk, CreatureBankModel>
    {
        private static readonly ILogger<CreatureBankDeserializer> Logger = LogFactory.For<CreatureBankDeserializer>();

        public CreatureBankModel Deserialize(CreatureBankChunk chunk)
        {
            using var subject = Logger.BeginSubject("Creature bank", chunk.Name);
            var resources = Enumerable.Range(0, Constants.CreatureBank.ResourceCount).Select(i => chunk.GetInt32At(CreatureBankLayout.Resources + (i * 4))).ToArray();
            var strength = CreatureBankPresets.FindStrength(chunk.Kind, resources);
            if (strength == null)
            {
                Logger.LogTrace("Bank resource preset {Value} is unknown; read as tier {StandIn}", string.Join(',', resources), Constants.CreatureBank.MinStrength);
            }

            var artifacts = Enumerable.Range(0, chunk.Kind == CreatureBankKind.DragonUtopia ? Constants.CreatureBank.ArtifactSlots : 0).Select(slot =>
            {
                var code = chunk.GetUInt32At(CreatureBankLayout.Artifacts + (slot * 4));
                if (code == uint.MaxValue)
                {
                    return null;
                }

                if (code <= byte.MaxValue && catalog.Artifacts.Find((byte)code) is { Wearable: true } artifact)
                {
                    return artifact;
                }

                Logger.LogTrace("Unknown artifact {Code:X} in slot {Slot}; read as {StandIn}", code, slot, catalog.FallbackArtifact.Id);
                return catalog.FallbackArtifact;
            }).ToArray();
            return new CreatureBankModel(
                chunk.Name,
                chunk.Position,
                chunk.Kind,
                strength ?? Constants.CreatureBank.MinStrength,
                chunk.GetInt32At(0) == -1,
                chunk.Kind != CreatureBankKind.DragonUtopia && chunk.GetInt32At(Constants.CreatureBank.CenterSlot * 4) == CreatureBankPresets.Types(chunk.Kind, true)[Constants.CreatureBank.CenterSlot],
                artifacts);
        }
    }
}
