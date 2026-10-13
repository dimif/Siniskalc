namespace Siniskalc.Homm2.Tests
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The second set of savegames for the write test with the game: unlike <see cref="Homm2EditedSaves"/>, which edits the models, these
    /// are made with the view models of the editor, the way a user edits (a castle given to a player through its owner box, a hero
    /// filled with artifacts through the finder, and so on). Each starts from a save of Broken Alliance, and what to look at in the game is
    /// worked out from the savegame that was written.
    /// </summary>
    internal static class Homm2EditorSaves
    {
        private const int Base = 24;

        public static IReadOnlyList<Homm2EditedSave> Build()
        {
            return
            [
                Make("E14GIVE", "Corackston given to Orange through the owner box", GiveCorackston, game =>
                    $"Corackston belongs to Orange (it was Blue's). Blue lists {Towns(game, Homm2Color.Blue)}, Orange lists {Towns(game, Homm2Color.Orange)}."),
                Make("E15LAST", "Red's only town (Bloodreign) given to Blue", GiveBloodreign, game =>
                    $"Blue owns Bloodreign too. Red owns no town and has {game.History.Current.Players().GetModel((int)Homm2Color.Red).DaysLeft} days to get one (his heroes are still there)."),
                Make("E16BLDG", "Xabran: the statue and the well taken down, the shipyard built", ChangeXabranBuildings, game =>
                    $"Xabran has: {BuildingsOf(game, "Xabran")}."),
                Make("E17DWEL", "Whiteshield: dwelling 4 taken back from Minotaur King to Minotaur, dragons upgraded to Red, creatures set in the dwellings", ChangeWhiteshieldDwellings, game =>
                    $"Whiteshield has the dwellings {DwellingsOf(game, "Whiteshield")}. (Before, dwelling 4 was upgraded: a Minotaur King; the first dragon dwelling is now the Red Dragon.)"),
                Make("E18GUILD", "Xabran: the Mage Guild at level 2 with other spells", ChangeXabranGuild, game =>
                    $"Xabran's Mage Guild is level 2. Level 1 teaches {GuildOf(game, "Xabran", 1, 3)}; level 2 teaches {GuildOf(game, "Xabran", 2, 3)} (three spells each: the game shows no more at these levels)."),
                Make("E19ARTS", "Maximus filled with artifacts through the finder", FillMaximusWithArtifacts, game =>
                    $"Maximus carries {ArtifactsOf(game, 7)}. His skills are {SkillsOf(game, 7)} (what the artifacts add is in them)."),
                Make("E20TAKE", "Maximus given the Ultimate Cloak and the Ultimate Sword, then the sword taken off", EquipAndTakeOff, game =>
                    $"Maximus carries {ArtifactsOf(game, 7)}. His skills are {SkillsOf(game, 7)}: the cloak added 12 defense (it was {SkillsOf(Homm2TestServices.LoadBrokenAlliance(Base), 7)}) and the sword left no attack behind."),
                Make("E21SKILL", "Maximus learns eight skills, all at expert", MaximusEightSkills, game =>
                    $"Maximus has eight secondary skills at expert: {SecondaryOf(game, 7)}."),
                Make("E22BOOK", "Sir Gallant buys a spell book and learns the spells without 'fire' in the name", GallantBook, game =>
                    $"Sir Gallant has a spell book with {game.History.Current.Heroes().GetModel(1).Spells.Count} spells: all but {string.Join(", ", Homm2Spell.All.Where(x => !game.History.Current.Heroes().GetModel(1).Spells.Contains(x)).Select(x => x.Name))}."),
                Make("E23LIMIT", "Blue's resources at the limits of the editor", BlueAtTheLimits, game =>
                    $"Blue has {game.History.Current.Players().GetModel(0).Resources.Gold:N0} gold and 999 of every other resource. Look at how the screens show them."),
                Make("E24STACK", "Sir Gallant's army: a stack of 65,535, a neutral creature and a dragon", GallantArmy, game =>
                    $"Sir Gallant has {ArmyOf(game, 1)}. Look at how the 65,535 is shown."),
                Make("E25STATS", "Sir Gallant: skills 20, level 20, experience 50,000, spell points 99, all his movement", GallantStats, game =>
                    $"Sir Gallant is level 20 with 50,000 experience, skills {SkillsOf(game, 1)}, 99 spell points and {game.History.Current.Heroes().GetModel(1).MovementLeft:N0} movement points."),
                Make("E27DWUPG", "Xabran: dwellings upgraded (Ranger, Master Swordsman, Crusader) and built; Whiteshield: the dragons upgraded twice", UpgradeDwellings, game =>
                    $"Xabran has the dwellings {DwellingsOf(game, "Xabran")}; Whiteshield's sixth is the Black Dragon. Look at the creatures each dwelling offers and whether they can be hired."),
                Make("E28GLD5", "Xabran: the Mage Guild at level 5", GuildLevelFive, game =>
                    $"Xabran has a level 5 Mage Guild: it shows {string.Join(", ", game.History.Current.Castles().GetModel(game.History.Current.Castles().FindByName("Xabran")).GuildSpellCounts)} spells at its levels (3, 3, 2, 2, 1 as in the castles of the saves)."),
                Make("E29LIBRY", "Corackston (a Wizard castle): the Library built and the Mage Guild at level 3", WizardLibrary, game =>
                    $"Corackston has the Library and a level 3 guild that shows {string.Join(", ", game.History.Current.Castles().GetModel(game.History.Current.Castles().FindByName("Corackston")).GuildSpellCounts)} spells (4, 4, 3 with the Library)."),
                Make("E30XP50K", "Sir Gallant: level 1 but 50,000 experience", ExperienceOnly, _ =>
                    "Sir Gallant has the level byte at 1 and 50,000 experience. Tell me the level the game shows (it showed 14 for level 20 and 50,000 in E25STATS)."),
                Make("E31LV20", "Sir Gallant: level 20 but 0 experience", LevelOnly, _ =>
                    "Sir Gallant has the level byte at 20 and no experience. Tell me the level the game shows."),
                Make("E32BIGXP", "Sir Gallant: level 1 but 200,000 experience", BigExperience, _ =>
                    "Sir Gallant has the level byte at 1 and 200,000 experience. Tell me the level the game shows."),
                Make("E33STACK", "Sir Gallant's army: 32,767 peasants, 9,999 rogues and a dragon", BigStacks, game =>
                    $"Sir Gallant has {ArmyOf(game, 1)}. 65,535 overflowed in E24STACK, so tell me how these are shown on the map and in the hero screen, and whether splitting a stack works."),
                Make("E34DWUP2", "Xabran and Whiteshield: the upgraded dwellings with their creatures in the list the game keeps them in", UpgradeDwellingsWithCreatures, game =>
                    $"Xabran has the dwellings {DwellingsOf(game, "Xabran")} with 10 waiting in each (E27 showed only the plain ones); Whiteshield's fourth (Minotaur King) and sixth (Black Dragon) dwellings have 5 and 3 waiting. Tell me the creatures that wait in each."),
                Make("E35BLKDRG", "Whiteshield: the dragons upgraded twice with 3 Black Dragons waiting", BlackDragonsWaiting, game =>
                    "Whiteshield's sixth dwelling is the Black Dragon with 3 waiting (in E34DWUP2 it showed 0: the count of a dragon upgraded twice is in the sixth place of the list of upgraded dwellings)."),
                Make("E26ALL", "All of the edits above that do not exclude each other", EverythingTogether, _ =>
                    "Everything of E14, E15, E16, E17, E18, E19, E21, E22, E23, E24 and E25 together: the towns changed hands, Xabran and Whiteshield are built as there, Maximus has the artifacts and the eight skills, Sir Gallant the book, the army and the stats, and Blue the resources at the limits."),
            ];
        }

        private static void GiveCorackston(Homm2Game game) => Castle(game, "Corackston").OwnerField.Selected = "Orange";

        private static void GiveBloodreign(Homm2Game game) => Castle(game, "Bloodreign").OwnerField.Selected = "Blue";

        private static void ChangeXabranBuildings(Homm2Game game)
        {
            var page = Castle(game, "Xabran");
            page.Buildings.Single(b => b.Name == "Statue").IsChecked = false;
            page.Buildings.Single(b => b.Name == "Well").IsChecked = false;
            page.Buildings.Single(b => b.Name == "Shipyard").IsChecked = true;
        }

        private static void ChangeWhiteshieldDwellings(Homm2Game game)
        {
            // Whiteshield has the Minotaur dwelling upgraded (a Minotaur King) and the first dragon dwelling.
            var page = Castle(game, "Whiteshield");
            page.Dwellings[3].Built.Selected = "Minotaur";
            page.Dwellings[3].Waiting.Value = 30;
            page.Dwellings[5].Built.Selected = "Red Dragon";
            page.Dwellings[2].Waiting.Value = 40;
            page.Dwellings[0].Waiting.Value = 0;
        }

        private static void UpgradeDwellings(Homm2Game game)
        {
            var xabran = Castle(game, "Xabran");
            foreach (var (index, creature) in new[] { (1, "Ranger"), (2, "Pikeman"), (3, "Master Swordsman"), (4, "Cavalry"), (5, "Crusader") })
            {
                xabran.Dwellings[index].Built.Selected = creature;
                xabran.Dwellings[index].Waiting.Value = 10;
            }

            Castle(game, "Whiteshield").Dwellings[5].Built.Selected = "Black Dragon";
        }

        private static void UpgradeDwellingsWithCreatures(Homm2Game game)
        {
            UpgradeDwellings(game);
            var whiteshield = Castle(game, "Whiteshield");
            whiteshield.Dwellings[3].Waiting.Value = 5;
            whiteshield.Dwellings[5].Waiting.Value = 3;
        }

        private static void BlackDragonsWaiting(Homm2Game game)
        {
            var whiteshield = Castle(game, "Whiteshield");
            whiteshield.Dwellings[5].Built.Selected = "Black Dragon";
            whiteshield.Dwellings[5].Waiting.Value = 3;
        }

        private static void GuildLevelFive(Homm2Game game) => Castle(game, "Xabran").GuildLevelField.Selected = "Level 5";

        private static void WizardLibrary(Homm2Game game)
        {
            var page = Castle(game, "Corackston");
            page.Buildings.Single(b => b.Name == "Library").IsChecked = true;
            page.GuildLevelField.Selected = "Level 3";
        }

        private static void ExperienceOnly(Homm2Game game) =>
            game.History.CreateHeroSession(1).Update(h => h with { Level = 1, Experience = 50_000 }); // raw: the page keeps the two in step

        private static void LevelOnly(Homm2Game game) =>
            game.History.CreateHeroSession(1).Update(h => h with { Level = 20, Experience = 0 }); // raw: the page keeps the two in step

        private static void BigExperience(Homm2Game game) =>
            game.History.CreateHeroSession(1).Update(h => h with { Level = 1, Experience = 200_000 }); // raw: the page keeps the two in step

        private static void BigStacks(Homm2Game game)
        {
            var army = Hero(game, 1).ArmyEditor;
            foreach (var slot in army.CreatureSlots.Where(s => s.HasCreature))
            {
                slot.ClearCommand.Execute(null);
            }

            army.SendToSlot(Homm2Creature.Peasant, 1, 32_767);
            army.SendToSlot(Homm2Creature.Rogue, 2, 9_999);
            army.SendToSlot(Homm2Creature.BlackDragon, 3, 1);
        }

        private static void ChangeXabranGuild(Homm2Game game)
        {
            var page = Castle(game, "Xabran");
            page.GuildLevelField.Selected = "Level 2";
            page.GuildLevels[0].Spells[0].Selected = "Fireball";
            page.GuildLevels[0].Spells[1].Selected = "Lightning Bolt";
            page.GuildLevels[1].Spells[0].Selected = "Chain Lightning";
        }

        private static void FillMaximusWithArtifacts(Homm2Game game)
        {
            var artifacts = Hero(game, 7).ArtifactsEditor;
            foreach (var artifact in new[]
            {
                Homm2Artifact.UltimateSwordOfDominion, Homm2Artifact.UltimateCloakOfProtection, Homm2Artifact.UltimateWandOfMagic, Homm2Artifact.UltimateBookOfKnowledge,
                Homm2Artifact.UltimateShield, Homm2Artifact.UltimateStaff, Homm2Artifact.UltimateCrown, Homm2Artifact.ThunderMaceOfDominion,
                Homm2Artifact.ArmoredGauntletsOfProtection, Homm2Artifact.DragonSwordOfDominion, Homm2Artifact.PowerAxeOfDominion, Homm2Artifact.DivineBreastplateOfProtection,
                Homm2Artifact.ArcaneNecklaceOfMagic, Homm2Artifact.MagesRingOfPower,
            })
            {
                if (artifacts.Slots.Any(s => !s.HasArtifact))
                {
                    artifacts.Finder.Choose(artifacts.Finder.Items.Cast<Homm2ArtifactChoiceViewModel>().Single(c => c.Artifact == artifact));
                }
            }
        }

        private static void EquipAndTakeOff(Homm2Game game)
        {
            var artifacts = Hero(game, 7).ArtifactsEditor;
            artifacts.Finder.Choose(artifacts.Finder.Items.Cast<Homm2ArtifactChoiceViewModel>().Single(c => c.Artifact == Homm2Artifact.UltimateCloakOfProtection));
            artifacts.Finder.Choose(artifacts.Finder.Items.Cast<Homm2ArtifactChoiceViewModel>().Single(c => c.Artifact == Homm2Artifact.UltimateSwordOfDominion));
            artifacts.Slots.Single(s => s.ArtifactName == "Ultimate Sword of Dominion").ClearCommand.Execute(null);
        }

        private static void MaximusEightSkills(Homm2Game game)
        {
            var skills = Hero(game, 7).SkillsEditor;
            foreach (var row in skills.AllSkills.OrderByDescending(r => r.HasSkill))
            {
                if (row.HasSkill || skills.Slots.Any(s => !s.IsFilled))
                {
                    row.LevelName = SkillLevelText.Expert;
                }
            }
        }

        private static void GallantBook(Homm2Game game)
        {
            var spells = Hero(game, 1).SpellsEditor;
            spells.HasSpellBook = true;
            spells.LearnAllCommand.Execute(null);
            spells.Finder.Filter = "fire";
            spells.ForgetAllCommand.Execute(null);
            spells.Finder.Filter = string.Empty;
        }

        private static void BlueAtTheLimits(Homm2Game game)
        {
            foreach (var row in Player(game, Homm2Color.Blue).Resources)
            {
                row.Value = row.Maximum;
            }
        }

        private static void GallantArmy(Homm2Game game)
        {
            var army = Hero(game, 1).ArmyEditor;
            foreach (var slot in army.CreatureSlots.Where(s => s.HasCreature))
            {
                slot.ClearCommand.Execute(null);
            }

            army.SendToSlot(Homm2Creature.Peasant, 1, 65_535);
            army.Finder.AddAmount = 500;
            army.Finder.Choose(army.Finder.Items.Cast<Homm2CreatureChoiceViewModel>().Single(c => c.Name == "Rogue"));
            army.SendToSlot(Homm2Creature.BlackDragon, 3, 1);
        }

        private static void GallantStats(Homm2Game game)
        {
            var page = Hero(game, 1);
            foreach (var row in page.PrimarySkills)
            {
                row.Value = 20;
            }

            page.Facts.Single(r => r.Name == "Level").Value = 20;
            page.Facts.Single(r => r.Name == "Experience").Value = 50_000;
            page.Facts.Single(r => r.Name == "Spell points").Value = 99;
            page.RestoreMovementCommand.Execute(null);
        }

        private static void EverythingTogether(Homm2Game game)
        {
            GiveCorackston(game);
            GiveBloodreign(game);
            ChangeXabranBuildings(game);
            ChangeWhiteshieldDwellings(game);
            ChangeXabranGuild(game);
            FillMaximusWithArtifacts(game);
            MaximusEightSkills(game);
            GallantBook(game);
            BlueAtTheLimits(game);
            GallantArmy(game);
            GallantStats(game);
        }

        private static Homm2EditedSave Make(string name, string edit, Action<Homm2Game> apply, Func<Homm2Game, string> check)
        {
            var game = Homm2TestServices.LoadBrokenAlliance(Base);
            apply(game);
            return new Homm2EditedSave(name, Base, edit, check(game), game.GetBytesToSave());
        }

        private static Homm2CastleViewModel Castle(Homm2Game game, string name) => new(game, game.History.Current.Castles().FindByName(name), _ => { });

        private static Homm2HeroViewModel Hero(Homm2Game game, int id) => new(game, id, _ => { });

        private static Homm2PlayerViewModel Player(Homm2Game game, Homm2Color color) => new(game, color, _ => { }, _ => { });

        private static string Towns(Homm2Game game, Homm2Color color)
        {
            var document = game.History.Current;
            var names = document.Players().GetModel((int)color).CastleIds.Select(i => document.Castles().GetModel(i).Name).ToList();
            return $"{names.Count} ({string.Join(", ", names)})";
        }

        private static string BuildingsOf(Homm2Game game, string castle)
        {
            var model = game.History.Current.Castles().GetModel(game.History.Current.Castles().FindByName(castle));
            var page = Castle(game, castle);
            return string.Join(", ", page.Buildings.Where(b => b.IsChecked).Select(b => b.Name)
                .Append(model.MageGuildLevel > 0 ? $"Mage Guild {model.MageGuildLevel}" : "no Mage Guild")
                .Concat(page.Dwellings.Where(d => d.IsBuilt).Select(d => d.Built.Selected)));
        }

        private static string DwellingsOf(Homm2Game game, string castle) =>
            string.Join(", ", Castle(game, castle).Dwellings.Select(d => d.IsBuilt ? $"{d.Built.Label}: {d.Built.Selected} ({d.Waiting.Value} waiting)" : $"{d.Built.Label} not built"));

        private static string GuildOf(Homm2Game game, string castle, int level, int shown) =>
            string.Join(", ", Castle(game, castle).GuildLevels[level - 1].Spells.Take(shown).Select(s => s.Selected));

        private static string ArtifactsOf(Homm2Game game, int hero) =>
            string.Join(", ", game.History.Current.Heroes().GetModel(hero).Artifacts.Slots.Where(a => a != null).Select(a => a!.Name));

        private static string SkillsOf(Homm2Game game, int hero)
        {
            var skills = game.History.Current.Heroes().GetModel(hero).PrimarySkills;
            return $"{skills.Attack}/{skills.Defense}/{skills.SpellPower}/{skills.Knowledge}";
        }

        private static string SecondaryOf(Homm2Game game, int hero) =>
            string.Join(", ", game.History.Current.Heroes().GetModel(hero).SecondarySkills.Entries.Select(e => $"{e.Level} {e.Skill.Name}"));

        private static string ArmyOf(Homm2Game game, int hero) =>
            string.Join(", ", game.History.Current.Heroes().GetModel(hero).Army.Slots.Where(s => s != null).Select(s => $"{s!.Count:N0} {s.Creature.Name}"));
    }
}
