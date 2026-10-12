namespace Siniskalc.ViewModels
{
    using Siniskalc.Entities;

    /// <summary>
    /// The words the UI uses for who plays a colour (<see cref="PlayerType"/>).
    /// </summary>
    public static class PlayerTypeText
    {
        public static string Of(PlayerType? control) => control switch
        {
            PlayerType.Human => "Human",
            PlayerType.Computer => "Computer",
            _ => string.Empty,
        };

        /// <summary>
        /// Returns who plays a colour, followed by "current turn" when it is that colour's turn, or only that when it is not known who plays it.
        /// </summary>
        public static string Describe(PlayerType? control, bool isCurrent) =>
            Of(control) + (isCurrent ? (control == null ? "Current turn" : " · current turn") : string.Empty);
    }
}
