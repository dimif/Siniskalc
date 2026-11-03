namespace Siniskalc.Core.Runtime
{
    using System;
    using System.IO;

    /// <summary>
    /// The settings a new user starts with, as the file the application ships (<c>Resources/settings.default.json</c>). It is written as
    /// the user's <c>settings.json</c> the first time the editor starts, so the user can see and edit them.
    /// </summary>
    public static class DefaultSettings
    {
        private const string ResourceName = "Siniskalc.Core.Resources.settings.default.json";

        /// <summary>
        /// Gets the text of the default settings file.
        /// </summary>
        /// <exception cref="InvalidOperationException">The default settings are not part of the application.</exception>
        public static string Json
        {
            get
            {
                using var stream = typeof(DefaultSettings).Assembly.GetManifestResourceStream(ResourceName)
                    ?? throw new InvalidOperationException($"The default settings {ResourceName} are not part of the application.");
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }
    }
}
