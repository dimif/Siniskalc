namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Reads standard JSON files relative to the application, never the working directory.
    /// </summary>
    public sealed class JsonGameDefinitionReader : IGameDefinitionReader
    {
        private static readonly ILogger<JsonGameDefinitionReader> Logger = LogFactory.For<JsonGameDefinitionReader>();

        private readonly string root;
        private readonly JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            Converters = { new DefinitionEnumConverter() },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonGameDefinitionReader"/> class.
        /// </summary>
        /// <param name="root">The game data folder the paths are relative to; the folder of the application when omitted.</param>
        /// <exception cref="ArgumentException"><paramref name="root"/> is empty or not a valid path.</exception>
        public JsonGameDefinitionReader(string? root = null)
        {
            this.root = System.IO.Path.GetFullPath(root ?? AppContext.BaseDirectory);
            Logger.LogTrace("Reading game definitions from {Folder}", this.root);
        }

        /// <summary>
        /// Reads a JSON file inside the game data folder. A property named twice, a property the type does not have and a number where
        /// an enum is expected are all errors.
        /// </summary>
        /// <typeparam name="T">The type the definition is read as.</typeparam>
        /// <param name="relativePath">The path of the file, relative to the game data folder.</param>
        /// <returns>The definition.</returns>
        /// <exception cref="GameDefinitionException">The path is empty, rooted or outside the game data folder, or the file cannot be read or understood.</exception>
        public T Read<T>(string relativePath)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(relativePath) || System.IO.Path.IsPathRooted(relativePath))
            {
                throw new GameDefinitionException(relativePath, "A relative definition file path is required.");
            }

            try
            {
                var path = System.IO.Path.GetFullPath(System.IO.Path.Combine(this.root, relativePath));
                var relative = System.IO.Path.GetRelativePath(this.root, path);
                if (relative == ".." || relative.StartsWith(".." + System.IO.Path.DirectorySeparatorChar, StringComparison.Ordinal))
                {
                    throw new GameDefinitionException(relativePath, "The file must be inside the game data directory.");
                }

                using var stream = File.OpenRead(path);
                using var document = JsonDocument.Parse(stream);
                CheckProperties(document.RootElement, "$");
                var definition = document.RootElement.Deserialize<T>(this.options)
                    ?? throw new JsonException("The definition cannot be null.");
                Logger.LogDebug("Read the definition {File} as {Name}", relative, typeof(T).Name);
                return definition;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or ArgumentException or NotSupportedException)
            {
                throw new GameDefinitionException(relativePath, ex.Message, ex);
            }
        }

        private sealed class DefinitionEnumConverter : JsonConverterFactory
        {
            private readonly JsonStringEnumConverter converter = new(allowIntegerValues: false);

            public override bool CanConvert(Type typeToConvert) =>
                !Attribute.IsDefined(typeToConvert, typeof(JsonConverterAttribute)) && this.converter.CanConvert(typeToConvert);

            public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
                this.converter.CreateConverter(typeToConvert, options);
        }

        private static void CheckProperties(JsonElement element, string location)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                var names = new HashSet<string>(StringComparer.Ordinal);
                foreach (var property in element.EnumerateObject())
                {
                    if (!names.Add(property.Name))
                    {
                        throw new JsonException($"Duplicate property '{property.Name}' at {location}.");
                    }

                    CheckProperties(property.Value, location + "." + property.Name);
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    CheckProperties(item, $"{location}[{index++}]");
                }
            }
        }
    }
}
