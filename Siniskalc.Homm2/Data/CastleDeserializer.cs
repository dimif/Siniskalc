namespace Siniskalc.Homm2.Data
{
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Models;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a castle. The name is read, never written.
    /// </summary>
    public sealed class CastleDeserializer : ICastleDeserializer
    {
        private static readonly ILogger<CastleDeserializer> Logger = LogFactory.For<CastleDeserializer>();

        /// <inheritdoc/>
        public PlayerColor DeserializeOwner(CastleChunk chunk)
        {
            using var subject = Logger.BeginSubject("Castle", chunk.Name);
            return Bytes.ReadColor(chunk.GetByteAt(CastleLayout.Owner));
        }

        /// <inheritdoc/>
        public CastleModel Deserialize(CastleChunk chunk)
        {
            using var subject = Logger.BeginSubject("Castle", chunk.Name);
            var bytes = chunk.ToArray();
            var boatX = chunk.GetByteAt(CastleLayout.Boat);
            var dwellings = (Dwellings)chunk.GetUInt32At(CastleLayout.Dwellings);

            return new CastleModel(
                Bytes.ReadText(bytes, CastleLayout.Name, Constants.Castle.NameLength),
                Bytes.ReadFaction(chunk.GetByteAt(CastleLayout.Faction)),
                this.DeserializeOwner(chunk),
                new MapPosition(chunk.GetByteAt(CastleLayout.X), chunk.GetByteAt(CastleLayout.Y)),
                boatX == Bytes.Empty ? null : new MapPosition(boatX, chunk.GetByteAt(CastleLayout.Boat + 1)),
                ArmyReader.Read(chunk, CastleLayout.Garrison, CreatureCodes.Instance),
                Bytes.Id(chunk.GetByteAt(CastleLayout.VisitingHero)),
                new Numbers(Enumerable.Range(1, Constants.Castle.DwellingCount).Select(d => (int)chunk.GetUInt16At(CastleLayout.CreaturesOffset(d, CastleModel.DwellingLevel(dwellings, d))))),
                chunk.GetByteAt(CastleLayout.Index),
                (TownBuildings)chunk.GetUInt16At(CastleLayout.TownBuildings),
                dwellings,
                new Numbers(chunk.GetBytesAt(CastleLayout.GuildSpellCounts, Constants.Castle.MageGuildLevels).Select(b => (int)b)),
                new MageGuildSlots(chunk.GetBytesAt(CastleLayout.GuildSpells, Bytes.GuildSpellSlots).Select(stored => Spell.FromByte((byte)Bytes.GuildSpellNumber(stored)))));
        }
    }
}
