namespace Siniskalc.Homm2.Tests
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The savegames of the third set for the write test with the game, made with the view models of the editor from the last save of
    /// Threedom (a Price of Loyalty map): the buildings and the things a hero remembers that the saves of Threedom showed.
    /// </summary>
    internal static class ThreedomEditorSaves
    {
        private const int Base = 71;
        private const int Latest = 82;
        private const int Charity = 49;
        private const int ColarBlau = 50;

        public static IReadOnlyList<EditedSave> Build()
        {
            return
            [
                Make("T01BLDS", "Whiteshield builds the thieves' guild, marketplace, both turrets and the captain's quarters; Hillstone takes down the statue, moat and captain's quarters", ChangeBuildings, game =>
                    $"Whiteshield has {BuildingsOf(game, "Whiteshield")}; Hillstone has {BuildingsOf(game, "Hillstone")}. Look at the castle screens: every building built or gone as listed (the turrets and the captain's quarters show on the castle's picture and in the fight)."),
                Make("T02FORGT", "Colar Blau and Charity forget every place they visited", ForgetEverything, game =>
                    "Colar Blau remembers no fort, standing stones, witch doctor's hut, mercenary camp or gazebo, and neither does Charity. Visit one of them again (a fort or a mercenary camp that Colar Blau had visited): it should give its bonus again."),
                Make("T03BONUS", "Colar Blau has +3 morale and +3 luck for the next battle, Charity -3 and -3", BattleBonus, game =>
                    "Colar Blau shows morale +3 and luck +3 in his hero screen and Charity -3 and -3. Fight a battle with each: the bonus should count in the battle and be gone after it."),
                Make("T04OBELS", "Blue visits every obelisk (from the last save of Threedom, 82)", BlueVisitsAllObelisks, _ =>
                    "Blue has visited all nine obelisks: the puzzle map of the Ultimate Artifact (the Puzzle button of the adventure map) shows every piece. Tell me what it shows, and whether the Obelisks of the other players are the same as before.", Latest),
                Make("T05FORT", "Colar Blau forgets the fort at 29, 18 only (from save 82)", ForgetOneFort, game =>
                    $"The forts of Colar Blau: {FortsOf(game)}. Visit the fort at 29, 18 with him: it should give +1 defense again (he had visited it), and the forts still ticked should give nothing.", Latest),
                Make("T06SRC", "Colar Blau has a temple and a fountain behind him, Charity has not used the arena or the stables (from save 82)", SourcesAndForgetfulness, game =>
                    "Colar Blau shows +2 morale and +1 luck in his hero screen (the temple and the fountain, set together with their bonus: T03BONUS set the numbers only and the screen did not show them). Charity can use the arena and the stables again.", Latest),
                Make("T07MINES", "Blue gets the neutral sawmill at 22, 29, the gold mine at 65, 31 and the alchemist's lab at 23, 2; the crystal mine at 44, 6 goes to Red (from save 82)", ChangeMines, game =>
                    $"On the map the flags of the sawmill and the gold mine are Blue's, the crystal mine's flag is Red's, the alchemist's lab has no flag in the savegame. {MinesOf(game)}. Pass a day: the income of Blue should gain 2 wood and 1,000 gold (and 1 mercury) and lose 1 crystal, Red gain 1 crystal. Tell me what the flags and the incomes show.", Latest),
                Make("T08SHRIN", "Three shrines teach other spells: 45, 7 Fireball, 22, 49 Mass Haste (it was Haste), 19, 16 Mass Bless (from save 82)", ChangeShrines, game =>
                    $"{ShrinesOf(game)}. Visit them with a hero that does not know the spell (Colar Blau, who has only Haste): the spell book gets the new one. Tell me what each teaches.", Latest),
                Make("T09SPHNX", "The sphinx can be answered again, with the answer \"shade\", and gives 5,000 gold and 5 gems (from save 82)", ChangeSphinx, game =>
                    "The sphinx at 13, 31 asks its riddle again: answer \"shade\" (or \"darkness\" or \"shadow\") and Blue gets 5,000 gold and 5 gems. Tell me whether it asks, accepts the answers and gives the reward, and whether the sphinx is used up afterwards.", Latest),
            ];
        }

        private static void ChangeMines(Game game)
        {
            var mines = MinesLists.Create(game).Rows;
            mines.Single(m => m.PositionText == "X: 22, Y: 29").OwnerField.Selected = "Blue";
            mines.Single(m => m.PositionText == "X: 65, Y: 31").OwnerField.Selected = "Blue";
            mines.Single(m => m.PositionText == "X: 23, Y: 2").OwnerField.Selected = "Blue";
            mines.Single(m => m.PositionText == "X: 44, Y: 6").OwnerField.Selected = "Red";
        }

        private static string MinesOf(Game game) => string.Join(", ", MinesLists.Create(game).Rows.Where(m => new[] { "X: 22, Y: 29", "X: 65, Y: 31", "X: 23, Y: 2", "X: 44, Y: 6" }.Contains(m.PositionText)).Select(m => $"{m.Kind} at {m.PositionText}: {m.OwnerField.Selected}"));

        private static void ChangeShrines(Game game)
        {
            var shrines = new MapViewModel(game).Shrines;
            shrines.Single(s => s.PositionText == "X: 45, Y: 7").SpellField.Selected = "Fireball";
            shrines.Single(s => s.PositionText == "X: 22, Y: 49").SpellField.Selected = "Mass Haste";
            shrines.Single(s => s.PositionText == "X: 19, Y: 16").SpellField.Selected = "Mass Bless";
        }

        private static string ShrinesOf(Game game) => string.Join(", ", new MapViewModel(game).Shrines.Where(s => new[] { "X: 45, Y: 7", "X: 22, Y: 49", "X: 19, Y: 16" }.Contains(s.PositionText)).Select(s => $"the shrine at {s.PositionText} teaches {s.SpellField.Selected}"));

        private static void ChangeSphinx(Game game)
        {
            var sphinx = new MapViewModel(game).Sphinxes.Single();
            sphinx.SolvedToggle.IsChecked = false;
            sphinx.Answers[2].Text = "shade";
            sphinx.Reward.Single(r => r.Name == "Gold").Value = 5000;
            sphinx.Reward.Single(r => r.Name == "Gems").Value = 5;
        }

        private static void BlueVisitsAllObelisks(Game game) => new PlayerViewModel(game, PlayerColor.Blue, TestServices.BanishingText, _ => { }, _ => { }).Obelisks.VisitAllCommand.Execute(null);

        private static void ForgetOneFort(Game game)
        {
            var forts = Hero(game, ColarBlau).VisitedPlaces.Single(r => r.Name == "Forts");
            forts.Places[1].IsChecked = false;
        }

        private static string FortsOf(Game game) =>
            string.Join(", ", Hero(game, ColarBlau).VisitedPlaces.Single(r => r.Name == "Forts").Places.Select(p => $"{p.Name}: {(p.IsChecked ? "visited" : "not visited")}"));

        private static void SourcesAndForgetfulness(Game game)
        {
            var colar = Hero(game, ColarBlau);
            colar.BattleBonus[0].Value = 0; // save 82 comes from T03BONUS: its numbers are not from any place
            colar.BattleBonus[1].Value = 0;
            colar.BattleSources.Single(r => r.Name == "Temple").IsChecked = true;
            colar.BattleSources.Single(r => r.Name == "Fountain").IsChecked = true;
            var charity = Hero(game, Charity);
            foreach (var row in charity.UsedOnce.Concat(charity.UsedWeekly))
            {
                row.IsChecked = false;
            }
        }

        private static void ChangeBuildings(Game game)
        {
            var whiteshield = Castle(game, "Whiteshield");
            foreach (var name in new[] { "Thieves' Guild", "Marketplace", "Left turret", "Right turret", "Captain's quarters" })
            {
                whiteshield.Buildings.Single(b => b.Name == name).IsChecked = true;
            }

            var hillstone = Castle(game, "Hillstone");
            foreach (var name in new[] { "Statue", "Moat", "Captain's quarters" })
            {
                hillstone.Buildings.Single(b => b.Name == name).IsChecked = false;
            }
        }

        private static void ForgetEverything(Game game)
        {
            foreach (var id in new[] { Charity, ColarBlau })
            {
                foreach (var row in Hero(game, id).VisitedPlaces)
                {
                    row.ForgetCommand.Execute(null);
                }
            }
        }

        private static void BattleBonus(Game game)
        {
            var colar = Hero(game, ColarBlau).BattleBonus;
            colar[0].Value = 3;
            colar[1].Value = 3;
            var charity = Hero(game, Charity).BattleBonus;
            charity[0].Value = -3;
            charity[1].Value = -3;
        }

        private static EditedSave Make(string name, string edit, Action<Game> apply, Func<Game, string> check, int baseSave = Base)
        {
            var game = TestServices.LoadThreedom(baseSave);
            apply(game);
            return new EditedSave(name, baseSave, edit, check(game), game.GetBytesToSave());
        }

        private static CastleViewModel Castle(Game game, string name) => new(game, game.History.Current.Castles().FindByName(name), _ => { });

        private static HeroViewModel Hero(Game game, int id) => new(game, id, _ => { }, TestServices.SpellPoints);

        private static string BuildingsOf(Game game, string castle) =>
            string.Join(", ", Castle(game, castle).Buildings.Where(b => b.IsChecked).Select(b => b.Name));
    }
}
