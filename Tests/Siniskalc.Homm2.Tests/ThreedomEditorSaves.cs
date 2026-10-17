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
            ];
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

        private static EditedSave Make(string name, string edit, Action<Game> apply, Func<Game, string> check)
        {
            var game = TestServices.LoadThreedom(Base);
            apply(game);
            return new EditedSave(name, Base, edit, check(game), game.GetBytesToSave());
        }

        private static CastleViewModel Castle(Game game, string name) => new(game, game.History.Current.Castles().FindByName(name), _ => { });

        private static HeroViewModel Hero(Game game, int id) => new(game, id, _ => { });

        private static string BuildingsOf(Game game, string castle) =>
            string.Join(", ", Castle(game, castle).Buildings.Where(b => b.IsChecked).Select(b => b.Name));
    }
}
