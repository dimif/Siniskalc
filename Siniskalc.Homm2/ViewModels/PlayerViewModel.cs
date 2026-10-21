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
        private IReadOnlyList<HeroLinkViewModel> heroes = [];
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

        /// <inheritdoc/>
        public string Name => this.Color.ToString();

        /// <summary>
        /// Gets who plays the colour ("Human" or "Computer"), followed by "current turn" when it is the turn of the player.
        /// </summary>
        public string ControlText => PlayerTypeText.Describe(this.Game.GetControl(this.Color), this.Game.CurrentPlayer == this.Color);

        /// <inheritdoc/>
        public string ColorHex => PlayerPalette.Of(this.Color);

        /// <summary>
        /// Gets what the player is up to: out of the game, counting the days to own a castle, or nothing.
        /// </summary>
        public string Status => this.status;

        /// <inheritdoc/>
        public IReadOnlyList<AmountRowViewModel> Resources => this.resources.Rows;

        /// <inheritdoc/>
        public ICommand SetAllResourcesCommand => this.resources.SetAllCommand;

        /// <inheritdoc/>
        public ICommand SetRichCommand => this.resources.SetRichCommand;

        /// <summary>
        /// Gets the obelisks of the map and which of them this player has visited.
        /// </summary>
        public ObeliskVisitsViewModel Obelisks { get; }

        /// <inheritdoc/>
        public IReadOnlyList<HeroLinkViewModel> Heroes => this.heroes;

        /// <inheritdoc/>
        public IReadOnlyList<CastleLinkViewModel> Castles => this.castles;

        /// <summary>
        /// Gets the two places of the tavern of the player, each with the heroes it can be given to.
        /// </summary>
        public IReadOnlyList<TavernPlaceViewModel> TavernPlaces { get; }

        /// <inheritdoc/>
        IReadOnlyList<ILink> IPlayerPage.Heroes => this.Heroes;

        /// <inheritdoc/>
        IReadOnlyList<ILink> IPlayerPage.Castles => this.Castles;

        /// <inheritdoc/>
        public bool HasHeroes => this.heroes.Count > 0;

        /// <inheritdoc/>
        public bool HasCastles => this.castles.Count > 0;

        protected override Siniskalc.Core.Data.IByteEditTarget CreateByteEditTarget(IByteEditTargetFactory factory) => factory.ForPlayer(this.session);

        protected override void Read()
        {
            var document = this.Game.History.Current;
            var player = document.Players().GetModel(this.Color.Code);
            this.status = player.IsOut
                ? "Out of the game"
                : player.DaysLeft is { } days ? $"{days} days left to own a castle" : string.Empty;

            this.resources.Refresh();

            this.heroes = OwnedLinks.Heroes(document.Heroes(), this.Color, this.openHero);
            this.castles = CastleLinks.OwnedBy(document, this.Color, this.openCastle, this.openHero);

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
