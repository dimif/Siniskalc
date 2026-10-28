namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Homm2.Data;

    /// <summary>
    /// Records of a castle and of a player as the game writes a fresh one: every id, every creature and every spell is the empty mark, and
    /// the rest is zero. A test sets the bytes it is about, so none of them needs a savegame.
    /// </summary>
    internal static class BlankRecords
    {
        public static CastleChunk Castle(Action<byte[]>? set = null)
        {
            var bytes = new byte[CastleLayout.RecordSize];
            bytes[CastleLayout.Owner] = 0xFF;
            bytes[CastleLayout.Boat] = 0xFF;
            bytes[CastleLayout.Boat + 1] = 0xFF;
            bytes[CastleLayout.VisitingHero] = 0xFF;
            Array.Fill(bytes, (byte)0xFF, CastleLayout.GarrisonTypes, 5);
            Array.Fill(bytes, (byte)0xFF, CastleLayout.GuildSpells, 20);
            set?.Invoke(bytes);
            return new CastleChunk("Castle", bytes, 0);
        }

        public static PlayerChunk Player(string color, Action<byte[]>? set = null)
        {
            var bytes = new byte[PlayerLayout.RecordSize];
            Array.Fill(bytes, (byte)0xFF, PlayerLayout.Heroes, 5);
            Array.Fill(bytes, (byte)0xFF, PlayerLayout.Castles, 8);
            bytes[PlayerLayout.SelectedHero] = 0xFF;
            bytes[PlayerLayout.SelectedCastle] = 0xFF;
            bytes[PlayerLayout.DaysLeft] = 0xFF;
            bytes[PlayerLayout.Tavern] = 0xFF;
            bytes[PlayerLayout.Tavern + 1] = 0xFF;
            set?.Invoke(bytes);
            return new PlayerChunk(color, bytes, 0);
        }
    }
}
