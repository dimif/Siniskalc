namespace Siniskalc.Homm2.Data
{
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a hero. A value the editor does not know (a creature or an artifact it has no name for, a colour that is none)
    /// does not stop the load: see <see cref="Bytes"/>. The name is read, never written.
    /// </summary>
    public sealed class HeroDeserializer : IHeroDeserializer
    {
        /// <inheritdoc/>
        public Color? DeserializeOwner(HeroChunk chunk) => Bytes.ReadColor(chunk.GetByteAt(HeroLayout.Owner));

        /// <inheritdoc/>
        public HeroModel Deserialize(HeroChunk chunk)
        {
            var bytes = chunk.ToArray();
            var counts = Enumerable.Range(0, Constants.ArmySlotCount).Select(i => (int)chunk.GetUInt16At(HeroLayout.ArmyCounts + (i * 2))).ToList();
            var types = chunk.GetBytesAt(HeroLayout.ArmyTypes, Constants.ArmySlotCount);

            return new HeroModel(
                chunk.GetByteAt(HeroLayout.Id),
                Bytes.ReadText(bytes, HeroLayout.Name, HeroLayout.NameLength),
                this.DeserializeOwner(chunk),
                new Position(Bytes.ReadCoordinate(chunk.GetUInt32At(HeroLayout.X)), Bytes.ReadCoordinate(chunk.GetUInt32At(HeroLayout.Y))),
                Bytes.OptionalPosition(chunk.GetUInt32At(HeroLayout.Destination), chunk.GetUInt32At(HeroLayout.Destination + 4)),
                chunk.GetUInt16At(HeroLayout.SpellPoints),
                chunk.GetUInt32At(HeroLayout.MaxMovement),
                chunk.GetUInt32At(HeroLayout.MovementLeft),
                chunk.GetUInt32At(HeroLayout.Experience),
                chunk.GetByteAt(HeroLayout.Level),
                new PrimarySkills(
                    chunk.GetByteAt(HeroLayout.PrimarySkills),
                    chunk.GetByteAt(HeroLayout.PrimarySkills + 1),
                    chunk.GetByteAt(HeroLayout.PrimarySkills + 2),
                    chunk.GetByteAt(HeroLayout.PrimarySkills + 3)),
                Bytes.SecondarySkills(
                    chunk.GetBytesAt(HeroLayout.SkillLevels, Constants.Hero.SecondarySkillCount),
                    chunk.GetBytesAt(HeroLayout.SkillOrder, Constants.Hero.SecondarySkillCount)),
                Bytes.Army(types, counts),
                new Artifacts(chunk.GetBytesAt(HeroLayout.Artifacts, Constants.Hero.ArtifactSlotCount).Select(Bytes.ReadArtifact)),
                Bytes.SpellBook(chunk.GetBytesAt(HeroLayout.Spells, HeroLayout.SpellsLength)));
        }
    }
}
