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
