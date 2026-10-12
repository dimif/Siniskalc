namespace Siniskalc.ViewModels
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

        /// <summary>The "Heroes" or "Towns" group of a player: the player's page, on the tab with that list.</summary>
        PlayerTab,
    }
}
