namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The pages of a player: the resources and what they earn each day, the heroes and the tavern, and the towns.
    /// </summary>
    public sealed class Homm2PlayerViewModel : Homm2PageViewModel
    {
        private static readonly Homm2Resource[] ResourceOrder =
        [
            Homm2Resource.Gold, Homm2Resource.Wood, Homm2Resource.Ore, Homm2Resource.Mercury, Homm2Resource.Sulfur, Homm2Resource.Crystal, Homm2Resource.Gems,
        ];

        private readonly Action<int> openHero;
        private readonly Action<int> openCastle;
        private readonly IReadOnlyList<AmountRowViewModel> resources;
        private readonly Homm2PlayerEditSession session;
        private IReadOnlyList<Homm2LinkViewModel> heroes = [];
        private IReadOnlyList<Homm2LinkViewModel> castles = [];
        private IReadOnlyList<Homm2FactRow> tavern = [];
        private string status = string.Empty;

        public Homm2PlayerViewModel(Homm2Game game, Homm2Color color, Action<int> openHero, Action<int> openCastle)
            : base(game, Homm2ChunkKinds.Player, (int)color)
        {
            this.Color = color;
            this.openHero = openHero;
            this.openCastle = openCastle;
            this.session = game.History.CreatePlayerSession((int)color);
            this.resources = ResourceOrder.Select(this.CreateRow).ToList();
            this.SetAllResourcesCommand = new DelegateCommand(_ => this.Add(this.resources.Where(r => r.Name != nameof(Homm2Resource.Gold)), Constants.Player.AllResourcesIncrease));
            this.SetRichCommand = new DelegateCommand(_ => this.Add(this.resources.Where(r => r.Name == nameof(Homm2Resource.Gold)), Constants.Player.GoldIncrease));
            this.Refresh();
        }

        public Homm2Color Color { get; }

        public string Name => this.Color.ToString();

        public string ColorHex => Homm2Palette.Of(this.Color);

        /// <summary>
        /// Gets what the player is up to: out of the game, counting the days to own a castle, or nothing.
        /// </summary>
        public string Status => this.status;

        /// <summary>
        /// Gets the resources, gold first, each editable.
        /// </summary>
        public IReadOnlyList<AmountRowViewModel> Resources => this.resources;

        /// <summary>
        /// Gets the command that adds 99 of every resource but gold.
        /// </summary>
        public ICommand SetAllResourcesCommand { get; }

        /// <summary>
        /// Gets the command that adds 100,000 gold.
        /// </summary>
        public ICommand SetRichCommand { get; }

        public IReadOnlyList<Homm2LinkViewModel> Heroes => this.heroes;

        public IReadOnlyList<Homm2LinkViewModel> Castles => this.castles;

        public IReadOnlyList<Homm2FactRow> Tavern => this.tavern;

        public bool HasHeroes => this.heroes.Count > 0;

        public bool HasCastles => this.castles.Count > 0;

        protected override void Read()
        {
            var document = this.Game.History.Current;
            var player = document.Players().GetModel((int)this.Color);
            var allHeroes = document.Heroes().GetModels();

            this.status = player.IsOut
                ? "Out of the game"
                : player.DaysLeft is { } days ? $"{days} days left to own a castle" : string.Empty;

            foreach (var row in this.resources)
            {
                row.Refresh();
            }

            this.heroes = allHeroes
                .Where(h => h.Owner == this.Color)
                .OrderBy(h => h.Name)
                .Select(h => new Homm2LinkViewModel(h.Name, $"{h.Race}, level {h.Level}", new DelegateCommand(_ => this.openHero(h.HeroId))))
                .ToList();

            this.castles = document.Castles().GetModels()
                .Where(c => c.IsUsed && c.Owner == this.Color)
                .OrderBy(c => c.Name)
                .Select(c => new Homm2LinkViewModel(c.Name, c.Race?.ToString() ?? string.Empty, new DelegateCommand(_ => this.openCastle(c.Index))))
                .ToList();

            this.tavern =
            [
                new Homm2FactRow("First hero", TavernHero(allHeroes, player.Tavern.First)),
                new Homm2FactRow("Second hero", TavernHero(allHeroes, player.Tavern.Second)),
            ];
        }

        private AmountRowViewModel CreateRow(Homm2Resource resource)
        {
            var gold = resource == Homm2Resource.Gold;
            return new AmountRowViewModel(
                resource.ToString(),
                Homm2Palette.Of(resource),
                Constants.Player.MinResource,
                gold ? Constants.Player.MaxGold : Constants.Player.MaxOther,
                gold ? Constants.Player.GoldStep : Constants.Player.OtherStep,
                () => this.session.Current.Resources[resource],
                () => this.Game.History.Original.Players().GetModel((int)this.Color).Resources[resource],
                amount => this.session.Update(player => player with { Resources = player.Resources.With(resource, amount) }),
                () => this.IncomeNote(resource));
        }

        // The game keeps no income for a player that was put out: what the table holds is a counter that walks over the resources
        // (1 wood, then 1 mercury, then 1 ore, on the days after Green was put out in the Broken Alliance saves 22 to 24).
        private string IncomeNote(Homm2Resource resource)
        {
            var player = this.session.Current;
            return !player.IsOut && player.Income[resource] > 0 ? $"+{player.Income[resource]:N0} a day" : string.Empty;
        }

        private void Add(IEnumerable<AmountRowViewModel> rows, int amount)
        {
            var kinds = rows.Select(r => Enum.Parse<Homm2Resource>(r.Name)).ToList();
            this.session.Update(player => player with
            {
                Resources = kinds.Aggregate(player.Resources, (resources, kind) => resources.With(kind, Math.Min(resources[kind] + amount, kind == Homm2Resource.Gold ? Constants.Player.MaxGold : Constants.Player.MaxOther))),
            });
        }

        private static string TavernHero(IReadOnlyList<Homm2HeroModel> heroes, int? id) =>
            id is { } hero && hero >= 0 && hero < heroes.Count ? heroes[hero].Name : "nobody";
    }
}
