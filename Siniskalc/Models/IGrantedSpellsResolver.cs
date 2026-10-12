namespace Siniskalc.Models
{
    using System.Collections.Generic;
    using Siniskalc.Entities;

    /// <summary>
    /// Works out which spells equipped artifacts let a hero cast.
    /// </summary>
    public interface IGrantedSpellsResolver
    {
        /// <summary>
        /// Gets, for each spell the artifacts let the hero cast, the names of the artifacts that give it, such as "Sea Captain's Hat".
        /// </summary>
        IReadOnlyDictionary<Spell, string> ResolveGrantedSpells(EquippedArtifacts artifacts);
    }
}
