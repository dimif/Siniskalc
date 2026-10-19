namespace Siniskalc.Heroes.Data
{
    using System;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;

    /// <summary>
    /// Makes the pending changes of the records of a savefile: a change and the way to put it back. The differs of a game use it, so that
    /// what puts a change back (which kind of chunk, which edit session) is the game's and not the differ's.
    /// </summary>
    /// <typeparam name="THero">The model of a hero of the game.</typeparam>
    /// <typeparam name="TPlayer">The model of a player of the game.</typeparam>
    /// <typeparam name="TCastle">The model of a castle of the game.</typeparam>
    /// <typeparam name="TObelisks">The model of the obelisks of the game.</typeparam>
    public interface IPendingChangesFactory<THero, TPlayer, TCastle, TObelisks>
        where THero : class
        where TPlayer : class
        where TCastle : class
        where TObelisks : class
    {
        /// <summary>
        /// Makes the pending change of a hero.
        /// </summary>
        /// <param name="heroName">The name of the hero.</param>
        /// <param name="heroIndex">The place of the hero among the heroes of the savefile.</param>
        /// <param name="change">What changed.</param>
        /// <param name="revert">Puts the hero back to what it was.</param>
        PendingChange ForHero(string heroName, int heroIndex, Change change, Func<THero, THero> revert);

        /// <summary>
        /// Makes the pending change of a player.
        /// </summary>
        /// <param name="playerName">The name of the player.</param>
        /// <param name="playerIndex">The place of the player among the players of the savefile.</param>
        /// <param name="change">What changed.</param>
        /// <param name="revert">Puts the player back to what it was.</param>
        PendingChange ForPlayer(string playerName, int playerIndex, Change change, Func<TPlayer, TPlayer> revert);

        /// <summary>
        /// Makes the pending change of a castle.
        /// </summary>
        /// <param name="castleName">The name of the castle.</param>
        /// <param name="castleIndex">The place of the castle among the castles of the savefile.</param>
        /// <param name="change">What changed.</param>
        /// <param name="revert">Puts the castle back to what it was.</param>
        PendingChange ForCastle(string castleName, int castleIndex, Change change, Func<TCastle, TCastle> revert);

        /// <summary>
        /// Makes the pending change of the obelisks of the map.
        /// </summary>
        /// <param name="index">The place of the record of the obelisks.</param>
        /// <param name="change">What changed.</param>
        /// <param name="revert">Puts the obelisks back to what they were.</param>
        PendingChange ForObelisks(int index, Change change, Func<TObelisks, TObelisks> revert);

        /// <summary>
        /// Makes the pending change of an object of the map that is a record of its own, such as a mine or a shrine.
        /// </summary>
        /// <typeparam name="TModel">The model of the object.</typeparam>
        /// <param name="kind">The kind of the chunk of the object.</param>
        /// <param name="name">The name shown for the object.</param>
        /// <param name="index">The place of the object among the chunks of its kind.</param>
        /// <param name="change">What changed.</param>
        /// <param name="revert">Puts the object back to what it was.</param>
        /// <exception cref="NotImplementedException">The game has no such objects yet.</exception>
        PendingChange ForMapObject<TModel>(ChunkKind kind, string name, int index, Change change, Func<TModel, TModel> revert)
            where TModel : class;
    }
}
