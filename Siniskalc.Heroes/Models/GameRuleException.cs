namespace Siniskalc.Heroes.Models
{
    using System;

    /// <summary>
    /// An edit the rules of the game do not allow, such as building what needs a building that is not there. It is an expected
    /// outcome of what the user asked for, not a fault: its message says why, and the edit is not made.
    /// </summary>
    public sealed class GameRuleException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameRuleException"/> class.
        /// </summary>
        /// <param name="message">Why the rules do not allow the edit, for the user.</param>
        public GameRuleException(string message)
            : base(message)
        {
        }
    }
}
