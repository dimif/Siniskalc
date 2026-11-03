namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Siniskalc.Homm3.Entities;

    public sealed class GameVersionJsonConverter : JsonConverter<GameVersion>
    {
        /// <inheritdoc/>
        public override GameVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType == JsonTokenType.String ? Parse(reader.GetString()) : throw new JsonException("A short game version name is required.");

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, GameVersion value, JsonSerializerOptions options) =>
            writer.WriteStringValue(Name(value));

        /// <inheritdoc/>
        public override GameVersion ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            Parse(reader.GetString());

        /// <inheritdoc/>
        public override void WriteAsPropertyName(Utf8JsonWriter writer, GameVersion value, JsonSerializerOptions options) =>
            writer.WritePropertyName(Name(value));

        private static GameVersion Parse(string? value) => value switch
        {
            "unknown" => GameVersion.Unknown,
            "roe" => GameVersion.RestorationOfErathia,
            "ab" => GameVersion.ArmageddonsBlade,
            "sod" => GameVersion.ShadowOfDeath,
            "chronicles" => GameVersion.HeroesChronicles,
            "hota" => GameVersion.HornOfTheAbyss,
            _ => throw new JsonException($"Unknown game version '{value}'; use roe, ab, sod, chronicles or hota."),
        };

        private static string Name(GameVersion value) => value switch
        {
            GameVersion.Unknown => "unknown",
            GameVersion.RestorationOfErathia => "roe",
            GameVersion.ArmageddonsBlade => "ab",
            GameVersion.ShadowOfDeath => "sod",
            GameVersion.HeroesChronicles => "chronicles",
            GameVersion.HornOfTheAbyss => "hota",
            _ => throw new JsonException($"Unsupported game version '{value}'."),
        };
    }
}
