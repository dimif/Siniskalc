namespace Siniskalc.Entities
{
    public record Position(short X, short Y, short Z)
    {
        /// <summary>
        /// The Z of the surface of the map.
        /// </summary>
        public const short Above = 0;

        /// <summary>
        /// The Z of the underground level, if present on the map.
        /// </summary>
        public const short Underground = 1;

        /// <summary>
        /// The position of something that is not on the map, such as a hero that has no marker.
        /// </summary>
        public static readonly Position None = new((short)Constants.Map.NoCoordinate, (short)Constants.Map.NoCoordinate, (short)Constants.Map.NoCoordinate);
    }
}
