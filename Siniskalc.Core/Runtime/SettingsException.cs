namespace Siniskalc.Core.Runtime
{
    using System;

    /// <summary>
    /// The settings file of the user exists but cannot be read, understood or written. The message is written for the user and
    /// names the file, so the user can fix it, replace it or reset it.
    /// </summary>
    public sealed class SettingsException : Exception
    {
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
