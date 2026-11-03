namespace Siniskalc.Core.Runtime
{
    using System;

    /// <summary>
    /// A savefile that cannot be read or understood. Its message is written for the user, who is told it as it is.
    /// </summary>
    public class SavefileException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SavefileException"/> class.
        /// </summary>
        /// <param name="message">What is wrong with the file, for the user.</param>
        public SavefileException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SavefileException"/> class with the failure behind it.
        /// </summary>
        /// <param name="message">What is wrong with the file, for the user.</param>
        /// <param name="innerException">The failure behind it.</param>
        public SavefileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
