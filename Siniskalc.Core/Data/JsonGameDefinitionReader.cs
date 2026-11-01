namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Reads standard JSON files relative to the application, never the working directory.
    /// </summary>
    public sealed class JsonGameDefinitionReader : IGameDefinitionReader
    {
        private readonly string root;
        private readonly JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            Converters = { new DefinitionEnumConverter() },
        };

        public JsonGameDefinitionReader(string? root = null)
        {
            this.root = System.IO.Path.GetFullPath(root ?? AppContext.BaseDirectory);
        }

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
                return document.RootElement.Deserialize<T>(this.options)
                    ?? throw new JsonException("The definition cannot be null.");
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
