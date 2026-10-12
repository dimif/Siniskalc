namespace Siniskalc.Models
{
    using System;

    /// <summary>
    /// An edit the rules of the game do not allow, such as building what needs a building that is not there. It is an expected
    /// outcome of what the user asked for, not a fault: its message says why, and the edit is not made.
    /// </summary>
    public sealed class GameRuleException : InvalidOperationException
    {
        public GameRuleException(string message)
            : base(message)
        {
        }
    }
}
