namespace Siniskalc.Core.Entities
{
    /// <summary>
    /// Something a game names and numbers, such as a creature, a colour or a skill.
    /// The name the editor shows and the code the savefile holds for it.
    /// </summary>
    public interface IGameEntity
    {
        /// <summary>
        /// Gets the name of the entity.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the byte code associated with the entity, as stored in the game savefile.
        /// </summary>
        byte Code { get; }
    }
}
