namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The pages of a player: the resources and what they earn each day, the heroes and the tavern, and the towns.
    /// </summary>
    public sealed class PlayerViewModel : PageViewModel
    {
        private static readonly Resource[] ResourceOrder =
        [
            Resource.Gold, Resource.Wood, Resource.Ore, Resource.Mercury, Resource.Sulfur, Resource.Crystal, Resource.Gems,
        ];

        private readonly Action<int> openHero;
        private readonly Action<int> openCastle;
        private readonly IReadOnlyList<AmountRowViewModel> resources;
        private readonly PlayerEditSession session;
        private IReadOnlyList<LinkViewModel> heroes = [];
        private IReadOnlyList<LinkViewModel> castles = [];
        private IReadOnlyList<FactRow> tavern = [];
        private string status = string.Empty;

        public PlayerViewModel(Game game, Color color, Action<int> openHero, Action<int> openCastle, IByteEditTargetFactory? byteEditTargets = null)
            : base(game, Homm2ChunkKinds.Player, (int)color, byteEditTargets)
        {
            this.Color = color;
            this.openHero = openHero;
            this.openCastle = openCastle;
            this.session = game.History.CreatePlayerSession((int)color);
            this.resources = ResourceOrder.Select(this.CreateRow).ToList();
            this.SetAllResourcesCommand = new DelegateCommand(_ => this.Add(this.resources.Where(r => r.Name != nameof(Resource.Gold)), Constants.Player.AllResourcesIncrease));
            this.SetRichCommand = new DelegateCommand(_ => this.Add(this.resources.Where(r => r.Name == nameof(Resource.Gold)), Constants.Player.GoldIncrease));
            this.Refresh();
        }

        public Color Color { get; }

        public string Name => this.Color.ToString();

        public string ColorHex => Palette.Of(this.Color);

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

        public IReadOnlyList<LinkViewModel> Heroes => this.heroes;

        public IReadOnlyList<LinkViewModel> Castles => this.castles;

        public IReadOnlyList<FactRow> Tavern => this.tavern;

        public bool HasHeroes => this.heroes.Count > 0;

        public bool HasCastles => this.castles.Count > 0;

        protected override Siniskalc.Core.Data.IByteEditTarget CreateByteEditTarget(IByteEditTargetFactory factory) => factory.ForPlayer(this.session);

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
                .Select(h => new LinkViewModel(h.Name, $"{h.Faction}, level {h.Level}", new DelegateCommand(_ => this.openHero(h.HeroId))))
                .ToList();

            this.castles = document.Castles().GetModels()
                .Where(c => c.IsUsed && c.Owner == this.Color)
                .OrderBy(c => c.Name)
                .Select(c => new LinkViewModel(c.Name, c.Faction?.ToString() ?? string.Empty, new DelegateCommand(_ => this.openCastle(c.Index))))
                .ToList();

            this.tavern =
            [
                new FactRow("First hero", TavernHero(allHeroes, player.Tavern.First)),
                new FactRow("Second hero", TavernHero(allHeroes, player.Tavern.Second)),
            ];
        }

        private AmountRowViewModel CreateRow(Resource resource)
        {
            var gold = resource == Resource.Gold;
            return new AmountRowViewModel(
                resource.ToString(),
                Palette.Of(resource),
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
        private string IncomeNote(Resource resource)
        {
            var player = this.session.Current;
            return !player.IsOut && player.Income[resource] > 0 ? $"+{player.Income[resource]:N0} a day" : string.Empty;
        }

        private void Add(IEnumerable<AmountRowViewModel> rows, int amount)
        {
            var kinds = rows.Select(r => Enum.Parse<Resource>(r.Name)).ToList();
            this.session.Update(player => player with
            {
                Resources = kinds.Aggregate(player.Resources, (resources, kind) => resources.With(kind, Math.Min(resources[kind] + amount, kind == Resource.Gold ? Constants.Player.MaxGold : Constants.Player.MaxOther))),
            });
        }

        private static string TavernHero(IReadOnlyList<HeroModel> heroes, int? id) =>
            id is { } hero && hero >= 0 && hero < heroes.Count ? heroes[hero].Name : "nobody";
    }
}
