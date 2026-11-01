namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Windows.Input;

    /// <summary>
    /// One player in the order of play: a button with the colour and the name of the player, lit when it is the player's turn.
    /// Pressing it gives the turn to the player, when the game lets the editor do that (<see cref="Command"/> is then not null).
    /// </summary>
    public sealed class OrderOfPlayItemViewModel
    {
        /// <param name="name">The name of the player, such as "Blue".</param>
        /// <param name="colorHex">The colour of the player.</param>
        /// <param name="isCurrent">Whether it is the turn of the player.</param>
        /// <param name="command">Gives the turn to the player, or <c>null</c> when it cannot be given.</param>
        /// <param name="toolTip">What the button does or why it does nothing.</param>
        /// <param name="isHuman">Whether the player is a human; the button of a computer player is disabled.</param>
        public OrderOfPlayItemViewModel(string name, string colorHex, bool isCurrent, ICommand? command, string toolTip, bool isHuman = true)
        {
            this.Name = name;
            this.ColorHex = colorHex;
            this.IsCurrent = isCurrent;
            this.Command = command;
            this.ToolTip = toolTip;
            this.IsHuman = isHuman;
        }

        public string Name { get; }

        public string ColorHex { get; }

        public bool IsCurrent { get; }

        public ICommand? Command { get; }

        public string ToolTip { get; }

        public bool IsHuman { get; }
    }
}
