using System.Text.Json.Serialization;

namespace Corellium.Api.Net.Models;

public record ProjectKeyCreate(
    string Key,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] KeyKind Kind,
    string? Label
);