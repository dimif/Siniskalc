namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Finds the places by the cells of the map: a cell that holds an object has the number of its type (32 bits), its place in the list of
    /// the objects of the map (16 bits) and its number among the objects of its type (8 bits), as the obelisks do (see
    /// <see cref="ObeliskBytesLoader"/>). The cells come before the table of definitions of the objects. The position is the one the list
    /// gives, which is where a hero stands for most kinds and a tile off for some (the school of war was one tile to the right in the one place
    /// that could be compared).
    /// </summary>
    public sealed class MapPlacesLoader : IMapPlacesLoader
    {
        private const int MaxNumber = 32;

        /// <inheritdoc/>
        public MapPlaces Load(byte[] bytes)
        {
            var firstDefinition = FirstDefinition(bytes);
            var objects = firstDefinition < 0 ? [] : MapObjectPositions.Find(bytes, bytes.Length);
            if (objects.Count == 0)
            {
                return MapPlaces.None;
            }

            var places = new Dictionary<VisitedPlaceKind, IReadOnlyList<(int Number, MapPosition Position)>>();
            foreach (var kind in VisitedPlaceKind.All.Where(k => k.ObjectType != null))
            {
                var found = new SortedDictionary<int, MapPosition>();
                var type = (byte)kind.ObjectType!.Value;
                for (var i = 0; i + 10 <= firstDefinition; i++)
                {
                    if (bytes[i] != type || bytes[i + 1] != 0 || bytes[i + 2] != 0 || bytes[i + 3] != 0 || bytes[i + 7] != 0 || bytes[i + 8] != 0 || bytes[i + 9] != 0)
                    {
                        continue;
                    }

                    var place = BitConverter.ToUInt16(bytes, i + 4);
                    var number = bytes[i + 6];
                    if (place > 0 && place < objects.Count && number < MaxNumber)
                    {
                        found.TryAdd(number, objects[place]);
                    }
                }

                places[kind] = found.Select(f => (f.Key, f.Value)).ToList();
            }

            return new MapPlaces(places);
        }

        private static int FirstDefinition(byte[] bytes)
        {
            for (var i = 2; i + 4 <= bytes.Length; i++)
            {
                if (bytes[i] == (byte)'.' && bytes[i + 1] == (byte)'d' && bytes[i + 2] == (byte)'e' && bytes[i + 3] == (byte)'f')
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
