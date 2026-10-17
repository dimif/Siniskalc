namespace Siniskalc.Heroes
{
    /// <summary>
    /// The limits that hold in every game of the series.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The numbers of the heroes: a hero's number is a byte, and 255 means no hero.
        /// </summary>
        public static class Hero
        {
            public const int MinId = 0;

            public const int MaxId = 254;

            /// <summary>
            /// The lowest a primary skill can be saved as.
            /// </summary>
            public const long MinSavedPrimarySkill = 0;

            /// <summary>
            /// The highest a primary skill can be saved as: the savefile holds it in a byte, with the bonus of the artifacts in it.
            /// </summary>
            public const long MaxSavedPrimarySkill = 255;
        }

        /// <summary>
        /// The countdown of a player who owns no town.
        /// </summary>
        public static class Player
        {
            /// <summary>
            /// The player is out of the game.
            /// </summary>
            public const int MinDaysLeft = 0;

            /// <summary>
            /// The days a player has to own a town again once the last one is lost.
            /// </summary>
            public const int MaxDaysLeft = 7;
        }

        /// <summary>
        /// The tavern of a player.
        /// </summary>
        public static class Tavern
        {
            public const int Places = 2;
        }
    }
}
