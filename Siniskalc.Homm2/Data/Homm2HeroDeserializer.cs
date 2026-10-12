namespace Siniskalc.Homm2.Data
{
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a hero. A value the editor does not know (a creature or an artifact it has no name for, a colour that is none)
    /// does not stop the load: see <see cref="Homm2Bytes"/>. The name is read, never written.
    /// </summary>
    public sealed class Homm2HeroDeserializer : IHomm2HeroDeserializer
    {
        /// <inheritdoc/>
        public Homm2Color? DeserializeOwner(Homm2HeroChunk chunk) => Homm2Bytes.Color(chunk.GetByteAt(Homm2HeroLayout.Owner));

        /// <inheritdoc/>
        public Homm2HeroModel Deserialize(Homm2HeroChunk chunk)
        {
            var bytes = chunk.ToArray();
            var counts = Enumerable.Range(0, Constants.ArmySlotCount).Select(i => (int)chunk.GetUInt16At(Homm2HeroLayout.ArmyCounts + (i * 2))).ToList();
            var types = chunk.GetBytesAt(Homm2HeroLayout.ArmyTypes, Constants.ArmySlotCount);

            return new Homm2HeroModel(
                chunk.GetByteAt(Homm2HeroLayout.Id),
                Homm2Bytes.ReadText(bytes, Homm2HeroLayout.Name, Homm2HeroLayout.NameLength),
                this.DeserializeOwner(chunk),
                new Homm2Position((int)chunk.GetUInt32At(Homm2HeroLayout.X), (int)chunk.GetUInt32At(Homm2HeroLayout.Y)),
                Homm2Bytes.OptionalPosition(chunk.GetUInt32At(Homm2HeroLayout.Destination), chunk.GetUInt32At(Homm2HeroLayout.Destination + 4)),
                chunk.GetUInt16At(Homm2HeroLayout.SpellPoints),
                chunk.GetUInt32At(Homm2HeroLayout.MaxMovement),
                chunk.GetUInt32At(Homm2HeroLayout.MovementLeft),
                chunk.GetUInt32At(Homm2HeroLayout.Experience),
                chunk.GetByteAt(Homm2HeroLayout.Level),
                new Homm2PrimarySkills(
                    chunk.GetByteAt(Homm2HeroLayout.PrimarySkills),
                    chunk.GetByteAt(Homm2HeroLayout.PrimarySkills + 1),
                    chunk.GetByteAt(Homm2HeroLayout.PrimarySkills + 2),
                    chunk.GetByteAt(Homm2HeroLayout.PrimarySkills + 3)),
                Homm2Bytes.SecondarySkills(
                    chunk.GetBytesAt(Homm2HeroLayout.SkillLevels, Constants.Hero.SecondarySkillCount),
                    chunk.GetBytesAt(Homm2HeroLayout.SkillOrder, Constants.Hero.SecondarySkillCount)),
                Homm2Bytes.Army(types, counts),
                new Homm2Artifacts(chunk.GetBytesAt(Homm2HeroLayout.Artifacts, Constants.Hero.ArtifactSlotCount).Select(Homm2Bytes.Artifact)),
                Homm2Bytes.SpellBook(chunk.GetBytesAt(Homm2HeroLayout.Spells, Homm2HeroLayout.SpellsLength)));
        }
    }
}
