namespace Siniskalc.Entities
{
    using System.Collections.Generic;

    public sealed class SecondarySkill : Homm3Enumeration
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
        public static readonly SecondarySkill FireMagic = new("Fire Magic", 14);
        public static readonly SecondarySkill AirMagic = new("Air Magic", 15);
        public static readonly SecondarySkill WaterMagic = new("Water Magic", 16);
        public static readonly SecondarySkill EarthMagic = new("Earth Magic", 17);
        public static readonly SecondarySkill Scholar = new("Scholar", 18);
        public static readonly SecondarySkill Tactics = new("Tactics", 19);
        public static readonly SecondarySkill Artillery = new("Artillery", 20);
        public static readonly SecondarySkill Learning = new("Learning", 21);
        public static readonly SecondarySkill Offense = new("Offense", 22);
        public static readonly SecondarySkill Armorer = new("Armorer", 23);
        public static readonly SecondarySkill Intelligence = new("Intelligence", 24);
        public static readonly SecondarySkill Sorcery = new("Sorcery", 25);
        public static readonly SecondarySkill Resistance = new("Resistance", 26);
        public static readonly SecondarySkill FirstAid = new("First Aid", 27);
        public static readonly SecondarySkill Interference = new("Interference", 28, GameVersion.HornOfTheAbyss);
        public static readonly SecondarySkill Runes = new("Runes", 29, GameVersion.HornOfTheAbyss);

        private SecondarySkill(string name, byte code, GameVersion gameVersion)
            : base(name, code, gameVersion)
        {
        }

        private SecondarySkill(string name, byte code)
            : base(name, code)
        {
        }

        public static IReadOnlyList<SecondarySkill> All => GetAll<SecondarySkill>();
    }
}
