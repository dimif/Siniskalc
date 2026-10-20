namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Core.Data;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The pages of a player: the resources and what they earn each day, the heroes and the tavern, and the towns.
    /// </summary>
    public sealed class PlayerViewModel : PageViewModel, IPlayerPage
    {
        private static readonly ResourceLimits Limits = new(
            Constants.Player.MinResource,
            Constants.Player.MaxGold,
            Constants.Player.MaxOther,
            Constants.Player.GoldStep,
            Constants.Player.OtherStep,
            Constants.Player.AllResourcesIncrease,
            Constants.Player.GoldIncrease);

        private static readonly Resource[] ResourceOrder =
        [
            Resource.Gold, Resource.Wood, Resource.Ore, Resource.Mercury, Resource.Sulfur, Resource.Crystal, Resource.Gems,
        ];

        private readonly Action<int> openHero;
        private readonly Action<int> openCastle;
        private readonly ResourcesEditorViewModel resources;
        private readonly PlayerEditSession session;
        private IReadOnlyList<LinkViewModel> heroes = [];
        private IReadOnlyList<CastleLinkViewModel> castles = [];
        private string status = string.Empty;

        public PlayerViewModel(Game game, PlayerColor color, Action<int> openHero, Action<int> openCastle, IByteEditTargetFactory? byteEditTargets = null)
            : base(game, Homm2ChunkKinds.Player, color.Code, byteEditTargets)
        {
            this.Color = color;
            this.openHero = openHero;
            this.openCastle = openCastle;
            this.session = game.History.CreatePlayerSession(color.Code);
            this.Obelisks = new ObeliskVisitsViewModel(game.History, color);
            this.resources = new ResourcesEditorViewModel(
                ResourceOrder,
                Limits,
                () => this.session.Current.Resources,
                () => this.Game.History.Original.Players().GetModel(this.Color.Code).Resources,
                edit => this.session.Update(player => player with { Resources = edit(player.Resources) }),
                this.IncomeNote);
            this.TavernPlaces = TavernPlaceViewModel.ForTavern(
                () => this.session.Current.Tavern,
                edit => this.session.Update(player => player with { Tavern = edit(player.Tavern) }),
                () => this.Game.History.Current.Heroes(),
                this.openHero);
            this.Refresh();
        }

        public PlayerColor Color { get; }

        public string Name => this.Color.ToString();

        /// <summary>
        /// Gets who plays the colour ("Human" or "Computer"), followed by "current turn" when it is the turn of the player.
        /// </summary>
        public string ControlText => PlayerTypeText.Describe(this.Game.GetControl(this.Color), this.Game.CurrentPlayer == this.Color);

        public string ColorHex => PlayerPalette.Of(this.Color);

        /// <summary>
        /// Gets what the player is up to: out of the game, counting the days to own a castle, or nothing.
        /// </summary>
        public string Status => this.status;

        /// <summary>
        /// Gets the resources, gold first, each editable.
        /// </summary>
        public IReadOnlyList<AmountRowViewModel> Resources => this.resources.Rows;

        /// <summary>
        /// Gets the command that adds 99 of every resource but gold.
        /// </summary>
        public ICommand SetAllResourcesCommand => this.resources.SetAllCommand;

        /// <summary>
        /// Gets the command that adds 100,000 gold.
        /// </summary>
        public ICommand SetRichCommand => this.resources.SetRichCommand;

        /// <summary>
        /// Gets the obelisks of the map and which of them this player has visited.
        /// </summary>
        public ObeliskVisitsViewModel Obelisks { get; }

        public IReadOnlyList<LinkViewModel> Heroes => this.heroes;

        public IReadOnlyList<CastleLinkViewModel> Castles => this.castles;

        /// <summary>
        /// Gets the two places of the tavern of the player, each with the heroes it can be given to.
        /// </summary>
        public IReadOnlyList<TavernPlaceViewModel> TavernPlaces { get; }

        IReadOnlyList<ILink> IPlayerPage.Heroes => this.Heroes;

        IReadOnlyList<ILink> IPlayerPage.Castles => this.Castles;

        public bool HasHeroes => this.heroes.Count > 0;

        public bool HasCastles => this.castles.Count > 0;

        protected override Siniskalc.Core.Data.IByteEditTarget CreateByteEditTarget(IByteEditTargetFactory factory) => factory.ForPlayer(this.session);

        protected override void Read()
        {
            var document = this.Game.History.Current;
            var player = document.Players().GetModel(this.Color.Code);
            var allHeroes = document.Heroes().GetModels();

            this.status = player.IsOut
                ? "Out of the game"
                : player.DaysLeft is { } days ? $"{days} days left to own a castle" : string.Empty;

            this.resources.Refresh();

            this.heroes = allHeroes
                .Where(h => h.Owner == this.Color)
                .OrderBy(h => h.Name)
                .Select(h => new LinkViewModel(h.Name, h.Summary.Description, new DelegateCommand(_ => this.openHero(h.Id))))
                .ToList();

            this.castles = document.Castles().GetModels()
                .Where(c => c.IsUsed && c.Owner == this.Color)
                .OrderBy(c => c.Name)
                .Select(c => CastleLinks.For(c, allHeroes, new DelegateCommand(_ => this.openCastle(c.Index)), this.openHero))
                .ToList();

            this.Obelisks?.Refresh();

            foreach (var place in this.TavernPlaces)
            {
                place.Refresh();
            }
        }

        // The game keeps no income for a player that was put out: what the table holds is a counter that walks over the resources
        // (1 wood, then 1 mercury, then 1 ore, on the days after Green was put out in the Broken Alliance saves 22 to 24).
        private string IncomeNote(Resource resource)
        {
            var player = this.session.Current;
            return !player.IsOut && player.Income[resource] > 0 ? $"+{player.Income[resource]:N0} a day" : string.Empty;
        }

    }
}
