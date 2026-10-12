namespace Siniskalc.Homm2.Tests
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// An edited savegame to load in Heroes of Might and Magic II, to find out whether the game takes what the editor writes.
    /// </summary>
    /// <param name="Name">The name the game lists: at most 8 characters before the extension, which is the limit of the game.</param>
    /// <param name="BaseSave">The number of the save of Broken Alliance the edit starts from.</param>
    /// <param name="Edit">What was changed, in a few words.</param>
    /// <param name="Check">What to look at in the game after loading it.</param>
    /// <param name="Bytes">The savegame.</param>
    internal sealed record Homm2EditedSave(string Name, int BaseSave, string Edit, string Check, byte[] Bytes);

    /// <summary>
    /// The edited savegames of the write test with the game: each changes one kind of thing (the control changes nothing), so a save
    /// the game refuses or shows wrongly says what is wrong. See TestData/Homm2/README.md.
    /// </summary>
    internal static class Homm2EditedSaves
    {
        public const string Extension = ".GM1";

        public static IReadOnlyList<Homm2EditedSave> Build()
        {
            return
            [
                Make("E00COPY", 5, "Nothing: the savegame is read and written back", "It loads exactly like the original 5_ARCHRA.GM1: Blue has 7700 gold and the Archery Range is built (Maximus is outside in this save; he enters Xabran in save 07).", _ => { }),
                Make("E01GOLD", 5, "Blue's gold", "Blue has 99,999 gold (the top bar of the kingdom summary).", game => Player(game, 0, p => p with { Resources = p.Resources.With(Homm2Resource.Gold, 99_999) })),
                Make("E02RESRC", 5, "All of Blue's resources", "Blue has 500 of every resource and 50,000 gold.", game => Player(game, 0, p => p with { Resources = new Homm2Resources(500, 500, 500, 500, 500, 500, 50_000) })),
                Make("E03ARMY", 5, "Maximus's army", "Maximus has 7 Black Dragons, 3 Titans, 5 Phoenixes, 20 Paladins and 100 Peasants.", game => Hero(game, 7, h => h with
                {
                    Army = new Homm2Army(
                    [
                        new Homm2ArmySlot(Homm2Creature.BlackDragon, 7),
                        new Homm2ArmySlot(Homm2Creature.Titan, 3),
                        new Homm2ArmySlot(Homm2Creature.Phoenix, 5),
                        new Homm2ArmySlot(Homm2Creature.Paladin, 20),
                        new Homm2ArmySlot(Homm2Creature.Peasant, 100),
                    ]),
                })),
                Make("E04SKILL", 5, "Maximus's primary skills", "Maximus has attack 15, defense 15, spell power 15 and knowledge 15.", game => Hero(game, 7, h => h with { PrimarySkills = new Homm2PrimarySkills(15, 15, 15, 15) })),
                Make("E05SKL2", 5, "Maximus's secondary skills", "Maximus has eight skills: Expert Wisdom, Expert Mysticism, Expert Logistics, Expert Pathfinding, Advanced Archery, Advanced Leadership, Basic Luck and Basic Estates.", game => Hero(game, 7, h => h with
                {
                    SecondarySkills = Homm2SecondarySkills.None
                        .With(Homm2SecondarySkill.Wisdom, Homm2SkillLevel.Expert)
                        .With(Homm2SecondarySkill.Mysticism, Homm2SkillLevel.Expert)
                        .With(Homm2SecondarySkill.Logistics, Homm2SkillLevel.Expert)
                        .With(Homm2SecondarySkill.Pathfinding, Homm2SkillLevel.Expert)
                        .With(Homm2SecondarySkill.Archery, Homm2SkillLevel.Advanced)
                        .With(Homm2SecondarySkill.Leadership, Homm2SkillLevel.Advanced)
                        .With(Homm2SecondarySkill.Luck, Homm2SkillLevel.Basic)
                        .With(Homm2SecondarySkill.Estates, Homm2SkillLevel.Basic),
                })),
                Make("E06ARTI", 10, "Maximus's artifacts", "Maximus (who has the spell book) also carries the Ultimate Sword of Dominion, the Thunder Mace of Dominion, the Ice Cloak, the Evercold Icicle and the Ammo Cart.", game => Hero(game, 7, h => h with
                {
                    Artifacts = h.Artifacts.Add(Homm2Artifact.UltimateSwordOfDominion).Add(Homm2Artifact.ThunderMaceOfDominion).Add(Homm2Artifact.IceCloak).Add(Homm2Artifact.EvercoldIcicle).Add(Homm2Artifact.AmmoCart),
                })),
                Make("E07BOOK", 1, "A spell book and every spell for Maximus", "Maximus has a spell book with all 65 spells (the save starts without a book).", game => Hero(game, 7, h => h.WithSpellBook() with { Spells = new Homm2SpellBook(Homm2Spell.All) })),
                Make("E08LEVEL", 5, "Maximus's level, experience and movement", "Maximus is level 10 with 14,700 experience, and has all his movement points (1,000) although he has none left in the original.", game => Hero(game, 7, h => h with { Level = 10, Experience = 14_700, MovementLeft = h.MaxMovement })),
                Make("E09CAST", 5, "Xabran's buildings and creatures", "Xabran has the dwellings 3 and 4 built on top of the first two (hut, archery range, blacksmith, armory), a tavern, a statue and a well, and 20 creatures wait in each dwelling that is built.", game => Castle(game, 3, c => c with
                {
                    Town = c.Town | Homm2TownBuildings.Tavern | Homm2TownBuildings.Statue | Homm2TownBuildings.Well,
                    Dwellings = c.Dwellings | Homm2Dwellings.Dwelling3 | Homm2Dwellings.Dwelling4,
                    Creatures = Homm2Numbers.Of(20, 20, 20, 20, 20, 20),
                })),
                Make("E10GARR", 5, "Xabran's garrison", "Xabran has a garrison of 10 Paladins, 20 Cavalry and 5 Champions; open the castle to see it.", game => Castle(game, 3, c => c with
                {
                    Garrison = c.Garrison
                        .WithSlot(0, new Homm2ArmySlot(Homm2Creature.Paladin, 10))
                        .WithSlot(1, new Homm2ArmySlot(Homm2Creature.Cavalry, 20))
                        .WithSlot(2, new Homm2ArmySlot(Homm2Creature.Champion, 5)),
                })),
                Make("E11OWNR", 13, "Corackston given to Blue", "Blue owns Corackston (a town without a castle) as well as Xabran; it is in the list of Blue's castles. Its garrison is the neutral one.", game =>
                {
                    Castle(game, 2, c => c with { Owner = Homm2Color.Blue });
                    Player(game, 0, p => p with { CastleIds = Homm2Numbers.Of(3, 2) });
                }),
                Make("E12ALL", 10, "Everything above that is in a save of the same game", "Gold, resources, army, primary skills, secondary skills, artifacts and the garrison of Xabran changed together (Blue has only Xabran in this save; the second town is in E11OWNR).", game =>
                {
                    Player(game, 0, p => p with { Resources = new Homm2Resources(500, 500, 500, 500, 500, 500, 50_000) });
                    Hero(game, 7, h => h with
                    {
                        PrimarySkills = new Homm2PrimarySkills(15, 15, 15, 15),
                        Army = new Homm2Army([new Homm2ArmySlot(Homm2Creature.BlackDragon, 7), new Homm2ArmySlot(Homm2Creature.Titan, 3), null, null, null]),
                        Artifacts = h.Artifacts.Add(Homm2Artifact.UltimateSwordOfDominion).Add(Homm2Artifact.IceCloak),
                        SecondarySkills = h.SecondarySkills.With(Homm2SecondarySkill.Wisdom, Homm2SkillLevel.Expert),
                    });
                    Castle(game, 3, c => c with { Garrison = c.Garrison.WithSlot(0, new Homm2ArmySlot(Homm2Creature.Paladin, 10)) });
                }),
            ];
        }

        private static Homm2EditedSave Make(string name, int baseSave, string edit, string check, Action<Homm2Game> apply)
        {
            var game = Homm2TestServices.LoadBrokenAlliance(baseSave);
            apply(game);
            return new Homm2EditedSave(name, baseSave, edit, check, game.GetBytesToSave());
        }

        private static void Player(Homm2Game game, int index, Func<Homm2PlayerModel, Homm2PlayerModel> edit) => game.History.CreatePlayerSession(index).Update(edit);

        private static void Hero(Homm2Game game, int index, Func<Homm2HeroModel, Homm2HeroModel> edit) => game.History.CreateHeroSession(index).Update(edit);

        private static void Castle(Homm2Game game, int index, Func<Homm2CastleModel, Homm2CastleModel> edit) => game.History.CreateCastleSession(index).Update(edit);
    }
}
