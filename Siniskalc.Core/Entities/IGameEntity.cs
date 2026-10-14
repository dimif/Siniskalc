namespace Siniskalc.Core.Entities
{
    /// <summary>
    /// Something a game names and numbers, such as a creature, a colour or a skill: the name the editor shows and the code the savefile
    /// holds for it.
    /// </summary>
    public interface IGameEntity
    {
        string Name { get; }

        /// <summary>
        /// Gets the number the savefile holds for the entity. It means something only within one game.
        /// </summary>
        byte Code { get; }
    }
}
