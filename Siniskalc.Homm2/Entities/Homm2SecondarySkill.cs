namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;

    /// <summary>
    /// A secondary skill of Heroes of Might and Magic II. The code is its place in the tables of a hero (levels and order learned): 0 for Pathfinding.
    /// The order is checked against the savegames for Leadership, Ballistics, Pathfinding, Archery, Wisdom and Estates.
    /// </summary>
    public sealed class Homm2SecondarySkill : Homm2Enumeration
    {
        public static readonly Homm2SecondarySkill Pathfinding = new("Pathfinding", 0);
        public static readonly Homm2SecondarySkill Archery = new("Archery", 1);
        public static readonly Homm2SecondarySkill Logistics = new("Logistics", 2);
        public static readonly Homm2SecondarySkill Scouting = new("Scouting", 3);
        public static readonly Homm2SecondarySkill Diplomacy = new("Diplomacy", 4);
        public static readonly Homm2SecondarySkill Navigation = new("Navigation", 5);
        public static readonly Homm2SecondarySkill Leadership = new("Leadership", 6);
        public static readonly Homm2SecondarySkill Wisdom = new("Wisdom", 7);
        public static readonly Homm2SecondarySkill Mysticism = new("Mysticism", 8);
        public static readonly Homm2SecondarySkill Luck = new("Luck", 9);
        public static readonly Homm2SecondarySkill Ballistics = new("Ballistics", 10);
        public static readonly Homm2SecondarySkill EagleEye = new("Eagle Eye", 11);
        public static readonly Homm2SecondarySkill Necromancy = new("Necromancy", 12);
        public static readonly Homm2SecondarySkill Estates = new("Estates", 13);

        private Homm2SecondarySkill(string name, byte code)
            : base(name, code)
        {
        }

        public static IReadOnlyList<Homm2SecondarySkill> All => GetAll<Homm2SecondarySkill>();

        public static Homm2SecondarySkill? FromByte(byte code) => FromCode<Homm2SecondarySkill>(code);
    }
}
