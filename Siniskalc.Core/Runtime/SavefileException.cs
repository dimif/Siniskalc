namespace Siniskalc.Core.Runtime
{
    using System;

    /// <summary>
    /// A savefile that cannot be read or understood. Its message is written for the user, who is told it as it is.
    /// </summary>
    public class SavefileException : Exception
    {
        public SavefileException(string message)
            : base(message)
        {
        }

        public SavefileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
