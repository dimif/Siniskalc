namespace Siniskalc.Homm2.Data
{
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a castle. The name is read, never written.
    /// </summary>
    public sealed class Homm2CastleDeserializer : IHomm2CastleDeserializer
    {
        /// <inheritdoc/>
        public Homm2Color? DeserializeOwner(Homm2CastleChunk chunk) => Homm2Bytes.Color(chunk.GetByteAt(Homm2CastleLayout.Owner));

        /// <inheritdoc/>
        public Homm2CastleModel Deserialize(Homm2CastleChunk chunk)
        {
            var bytes = chunk.ToArray();
            var boatX = chunk.GetByteAt(Homm2CastleLayout.Boat);
            var counts = Enumerable.Range(0, Constants.ArmySlotCount).Select(i => (int)chunk.GetUInt16At(Homm2CastleLayout.GarrisonCounts + (i * 2))).ToList();

            return new Homm2CastleModel(
                chunk.GetByteAt(Homm2CastleLayout.Index),
                Homm2Bytes.ReadText(bytes, Homm2CastleLayout.Name, Constants.Castle.NameLength),
                this.DeserializeOwner(chunk),
                Homm2Bytes.Race(chunk.GetByteAt(Homm2CastleLayout.Race)),
                new Homm2Position(chunk.GetByteAt(Homm2CastleLayout.X), chunk.GetByteAt(Homm2CastleLayout.Y)),
                boatX == Homm2Bytes.Empty ? null : new Homm2Position(boatX, chunk.GetByteAt(Homm2CastleLayout.Boat + 1)),
                Homm2Bytes.Army(chunk.GetBytesAt(Homm2CastleLayout.GarrisonTypes, Constants.ArmySlotCount), counts),
                Homm2Bytes.Id(chunk.GetByteAt(Homm2CastleLayout.VisitingHero)),
                (Homm2TownBuildings)chunk.GetUInt16At(Homm2CastleLayout.TownBuildings),
                (Homm2Dwellings)chunk.GetUInt32At(Homm2CastleLayout.Dwellings),
                new Homm2Numbers(Enumerable.Range(0, Constants.Castle.DwellingCount).Select(i => (int)chunk.GetUInt16At(Homm2CastleLayout.Creatures + (i * 2)))),
                new Homm2Numbers(chunk.GetBytesAt(Homm2CastleLayout.GuildSpells, Homm2Bytes.GuildSpellSlots).Select(Homm2Bytes.GuildSpellNumber)),
                new Homm2Numbers(chunk.GetBytesAt(Homm2CastleLayout.GuildSpellCounts, Constants.Castle.MageGuildLevels).Select(b => (int)b)));
        }
    }
}
