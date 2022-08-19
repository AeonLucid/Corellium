using System.Text.Json.Serialization;

namespace Corellium.Api.Net.Models;

public record CorelliumSettings(
    int Version,
    [property: JsonPropertyName("internet-access")] bool InternetAccess
);