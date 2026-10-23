namespace Siniskalc.Wpf.ViewModels
{
    /// <summary>
    /// What a node of the navigation tree stands for.
    /// </summary>
    public enum NavNodeKind
    {
        /// <summary>Only holds other nodes, such as "Heroes" or "Towns"; it has no page.</summary>
        Group,

        /// <summary>The game: the root, whose page shows what the savefile contains.</summary>
        Overview,

        /// <summary>A player: its page shows the resources, and the node holds the player's heroes and towns.</summary>
        Player,

        /// <summary>A hero's pages.</summary>
        Hero,

        /// <summary>A castle's page.</summary>
        Castle,

        /// <summary>The page of what the game keeps about the objects of the map (its mines, shrines, sphinxes).</summary>
        Map,

        /// <summary>The "Heroes" or "Towns" group of a player: the player's page, on the tab with that list.</summary>
        PlayerTab,

        /// <summary>A mine of the map, below the "Mines" group of its owner.</summary>
        Mine,

        /// <summary>A dwelling of the map, below the "Dwellings" group of its owner.</summary>
        Dwelling,

        /// <summary>A group inside the "Mines" or "Dwellings" group of an owner (the mines of one resource, the dwellings of one faction): it opens the owner's tab with that list.</summary>
        OwnerSubGroup,
    }
}
