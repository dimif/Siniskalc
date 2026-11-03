namespace Siniskalc.Core.Runtime
{
    using System;

    /// <summary>
    /// The settings file of the user exists but cannot be read, understood or written. The message is written for the user and
    /// names the file, so the user can fix it, replace it or reset it.
    /// </summary>
    public sealed class SettingsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsException"/> class.
        /// </summary>
        /// <param name="path">The settings file the failure is about.</param>
        /// <param name="message">What went wrong, for the user.</param>
        /// <param name="innerException">The failure behind it.</param>
        public SettingsException(string path, string message, Exception innerException)
            : base(message, innerException)
        {
            this.Path = path;
        }

        /// <summary>
        /// Gets the settings file the failure is about.
        /// </summary>
        public string Path { get; }
    }
}
