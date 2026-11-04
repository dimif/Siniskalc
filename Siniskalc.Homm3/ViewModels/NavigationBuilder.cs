namespace Siniskalc.Homm3.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Homm3.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Fills the navigation tree with the game (what the savefile contains), which holds one node per player (and one for what
    /// nobody owns). A player's node is its resources page, and holds a "Heroes" group and a "Towns" group.
    /// </summary>
    public sealed class NavigationBuilder : OwnerNavigationBuilderBase<Game, PlayerColor>
    {
        /// <summary>
        /// The badge of the town that is the capital.
        /// </summary>
        public const string CapitalBadge = "capital";

        /// <summary>
        /// The badge of the town where the Grail is built.
        /// </summary>
        public const string GrailBadge = "grail";

        /// <summary>
        /// The badge of the town that has a hero in its garrison.
        /// </summary>
        public const string GarrisonBadge = "garrison";

        private static readonly IReadOnlyDictionary<NavNodeKind, ChunkKind> Kinds = new Dictionary<NavNodeKind, ChunkKind>
        {
            [NavNodeKind.Player] = Homm3ChunkKinds.Player,
            [NavNodeKind.Hero] = Homm3ChunkKinds.Hero,
            [NavNodeKind.Castle] = Homm3ChunkKinds.Castle,
            [NavNodeKind.Mine] = Homm3ChunkKinds.Mines,
            [NavNodeKind.Dwelling] = Homm3ChunkKinds.Dwellings,
            [NavNodeKind.MapObject] = Homm3ChunkKinds.CreatureBanks,
        };

        private readonly BanishingTextProvider banishingText;
        private readonly CreatureBankTextProvider bankText;

        /// <param name="banishingText">Words the countdown of a player with no town is shown with.</param>
        public NavigationBuilder(BanishingTextProvider banishingText, CreatureBankTextProvider bankText)
            : base(Kinds)
        {
            this.banishingText = banishingText;
            this.bankText = bankText;
        }

        /// <summary>
        /// Shows the crown and the Grail mark of the castles and the days left of the players with no town as they are in
        /// <paramref name="game"/> now, after an edit or an undo.
        /// </summary>
        /// <param name="tree">The navigation tree.</param>
        /// <param name="game">The game.</param>
        public override void RefreshMarks(NavigationTree tree, Game game)
        {
            var castles = game.History.Current.Castles().GetModels();
            foreach (var (index, node) in tree.CastleNodes)
            {
                if (index < castles.Count)
                {
                    this.MarkCastle(node, game, castles[index]);
                }
            }

            foreach (var (index, node) in tree.PlayerNodes)
            {
                node.Notes = NotesOf(this.banishingText.Nav(game.History.Current.Players().GetModel(index)));
            }

            if (tree.MapNode is { } map)
            {
                if (map.Children.SingleOrDefault(n => n.Tab == MapSections.Merchants) is { } merchants)
                {
                    merchants.HasChanges = game.History.GetPendingChanges().Any(c => c.Kind == Homm3ChunkKinds.Merchants);
                }

                map.HasChanges = map.Children.Any(n => n.HasChanges);
                if (map.HasChanges && tree.Overview is { } overview)
                {
                    overview.HasChanges = true;
                }
            }
        }

        /// <inheritdoc/>
        protected override void AddExtraNodes(NavigationTree tree, Game game, NavNodeViewModel overview)
        {
            var merchants = game.History.Current.Merchants().Count;
            var banks = game.History.Current.CreatureBanks().GetModels();
            if (merchants == 0 && banks.Count == 0)
            {
                return;
            }

            var node = new NavNodeViewModel("Map", this.bankText.MapNavSummary(merchants, banks.Count), PlayerPalette.NoPlayer, NavNodeKind.Map, onSelected: tree.OnSelected);
            tree.Register(node);
            overview.Children.Add(node);
            if (merchants > 0)
            {
                node.Children.Add(new NavNodeViewModel("Merchants", merchants.ToString(), PlayerPalette.NoPlayer, NavNodeKind.MapSection, index: 0, tab: MapSections.Merchants, onSelected: tree.OnSelected));
            }

            var creatureBanks = new NavNodeViewModel("Creature banks", banks.Count.ToString(), PlayerPalette.NoPlayer, NavNodeKind.MapSection, tab: MapSections.CreatureBanks, onSelected: tree.OnSelected);
            node.Children.Add(creatureBanks);
            foreach (var group in banks.Select((model, index) => (Model: model, Index: index)).GroupBy(b => b.Model.Kind).OrderBy(g => g.Key))
            {
                var section = MapSections.ForBank(group.Key);
                var bankGroup = new NavNodeViewModel(this.bankText.GroupName(group.Key), group.Count().ToString(), PlayerPalette.NoPlayer, NavNodeKind.MapSection, tab: section, onSelected: tree.OnSelected);
                creatureBanks.Children.Add(bankGroup);
                foreach (var (bank, index) in group)
                {
                    var entrance = bank.Position with { X = bank.Position.X - 1 };
                    bankGroup.Children.Add(new NavNodeViewModel(MapLevelText.At(entrance, hasLevels: true), bank.Name, PlayerPalette.NoPlayer, NavNodeKind.MapObject, index, section, tree.OnSelected));
                }
            }
        }

        /// <inheritdoc/>
        protected override IReadOnlyList<PlayerColor> CastleOwners(Game game) => game.History.Current.Castles().GetOwners();

        /// <inheritdoc/>
        protected override IReadOnlyList<PlayerColor> HeroOwners(Game game) => game.History.Current.Heroes().GetOwners();

        /// <inheritdoc/>
        protected override IReadOnlyList<PlayerColor> MineOwners(Game game) => game.History.Current.Mines().GetModels().Select(m => m.Owner).ToList();

        /// <inheritdoc/>
        protected override IReadOnlyList<PlayerColor> DwellingOwners(Game game) => game.History.Current.Dwellings().GetModels().Select(d => d.Owner).ToList();

        // A player is listed when the savefile has the player's data or one of the player's heroes.
        /// <inheritdoc/>
        protected override IEnumerable<PlayerColor> PlayersInGame(Game game) => game.History.Current.Players().GetColors();

        /// <inheritdoc/>
        protected override NavNodeViewModel CreateGroup(NavigationTree tree, Game game, PlayerColor owner)
        {
            var castles = game.History.Current.Castles().GetModels()
                .Select((model, index) => (Index: index, Model: model))
                .Where(c => c.Model.Owner == owner)
                .OrderBy(c => c.Model.Name)
                .ToList();

            // A player with data in the savefile is a page of its own (the resources); the others only hold their heroes and towns.
            // A colour the savefile has no player data for gets a page like Neutral's, numbered below -1 so the tree can tell them apart.
            var playerIndex = owner.IsNeutral ? -1 : game.History.Current.Players().FindByColor(owner);
            var hasPlayerPage = playerIndex >= 0;
            if (!hasPlayerPage)
            {
                playerIndex = NeutralViewModel.IndexOf(owner);
            }

            var castleModels = castles.ToDictionary(c => c.Index, c => c.Model);
            var node = OwnerNavGroup.Create(
                tree,
                OwnerNames<PlayerColor>.Of(owner),
                PlayerPalette.Of(owner),
                playerIndex,
                !owner.IsNeutral,
                string.Empty,
                HeroEntriesOf(game.History.Current.Heroes(), owner),
                castles.Select(c => new NavEntry(c.Index, c.Model.Name, c.Model.Faction.Name)).ToList(),
                (castleNode, index) => this.MarkCastle(castleNode, game, castleModels[index]),
                MineGroupsOf(game.History.Current.Mines().GetModels(), owner, hasLevels: true),
                DwellingGroupsOf(game.History.Current.Dwellings().GetModels(), owner),
                neutralPage: !hasPlayerPage);
            if (hasPlayerPage)
            {
                node.Notes = NotesOf(this.banishingText.Nav(game.History.Current.Players().GetModel(playerIndex)));
            }

            return node;
        }

        // The dwellings of an owner in groups, one for each faction of the creature they give (neutral last); a creature the editor does
        // not know has a group of its own at the end.
        private static IReadOnlyList<NavGroup> DwellingGroupsOf(IEnumerable<DwellingModel> dwellings, PlayerColor owner)
        {
            var own = dwellings.Where(d => d.Owner == owner).ToList();
            var groups = own
                .Where(d => d.Creature != null)
                .GroupBy(d => d.Creature!.Faction)
                .OrderBy(g => g.Key)
                .Select(g => new NavGroup(g.Key.Name, EntriesOf(g)))
                .ToList();
            groups.Add(new NavGroup("Unknown", EntriesOf(own.Where(d => d.Creature == null))));
            return groups;
        }

        private static IReadOnlyList<NavEntry> EntriesOf(IEnumerable<DwellingModel> dwellings) =>
            dwellings.OrderBy(d => d.Position.Level).ThenBy(d => d.Position.X).ThenBy(d => d.Position.Y).Select(d => new NavEntry(d.Index, MapLevelText.At(d.Position, hasLevels: true), d.Creature?.Name ?? $"Creature {d.CreatureCode}")).ToList();

        private static IReadOnlyList<string> NotesOf(string text) => text.Length > 0 ? [text] : [];

        private static string GarrisonedHeroName(Game game, CastleModel castle) =>
            castle.GarrisonedHeroId is { } id ? game.History.Current.Heroes().FindById(id)?.Name ?? $"Unknown hero {id}" : string.Empty;

        private void MarkCastle(NavNodeViewModel node, Game game, CastleModel castle)
        {
            var badges = new List<NavBadge>();
            if (castle.HasBuilt(BuildingType.Capitol))
            {
                badges.Add(new NavBadge(CapitalBadge, "♛", "Capital", NavBadgeTone.Accent));
            }

            if (castle.Buildings.FirstOrDefault(b => b.IsGrail)?.Name is { } grail)
            {
                badges.Add(new NavBadge(GrailBadge, "✦", $"Grail: {grail}", NavBadgeTone.Highlight));
            }

            var garrisoned = GarrisonedHeroName(game, castle);
            if (garrisoned.Length > 0)
            {
                badges.Add(new NavBadge(GarrisonBadge, "⚔", $"Garrisoned: {garrisoned}", NavBadgeTone.Muted));
            }

            node.Badges = badges;
            node.Notes = NotesOf(garrisoned);
        }
    }
}
