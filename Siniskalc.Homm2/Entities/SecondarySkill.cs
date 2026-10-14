namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;

    /// <summary>
    /// A secondary skill of Heroes of Might and Magic II. The code is its place in the tables of a hero (levels and order learned): 0 for Pathfinding.
    /// The order is checked against the savegames for Leadership, Ballistics, Pathfinding, Archery, Wisdom and Estates.
    /// </summary>
    public sealed class SecondarySkill : Homm2Enumeration
    {
        public static readonly SecondarySkill Pathfinding = new("Pathfinding", 0);
        public static readonly SecondarySkill Archery = new("Archery", 1);
        public static readonly SecondarySkill Logistics = new("Logistics", 2);
        public static readonly SecondarySkill Scouting = new("Scouting", 3);
        public static readonly SecondarySkill Diplomacy = new("Diplomacy", 4);
        public static readonly SecondarySkill Navigation = new("Navigation", 5);
        public static readonly SecondarySkill Leadership = new("Leadership", 6);
        public static readonly SecondarySkill Wisdom = new("Wisdom", 7);
        public static readonly SecondarySkill Mysticism = new("Mysticism", 8);
        public static readonly SecondarySkill Luck = new("Luck", 9);
        public static readonly SecondarySkill Ballistics = new("Ballistics", 10);
        public static readonly SecondarySkill EagleEye = new("Eagle Eye", 11);
        public static readonly SecondarySkill Necromancy = new("Necromancy", 12);
        public static readonly SecondarySkill Estates = new("Estates", 13);

        private SecondarySkill(string name, byte code)
            : base(name, code)
        {
        }

        public static IReadOnlyList<SecondarySkill> All => GetAll<SecondarySkill>();

        public static SecondarySkill? FromByte(byte code) => FromCode<SecondarySkill>(code);
    }
}
