using System.Text.Json.Serialization;

namespace Corellium.Api.Net.Models;

public enum KeyKind
{
    ssh,
    adb
}

public record CorelliumProjectKey(
    string Identifier,
    string Label,
    string Key,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] KeyKind Kind,
    string Fingerprint,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);