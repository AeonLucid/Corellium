using System.Text.Json.Serialization;

namespace Corellium.Api.Net.Models.Agent;

public record AppListEntry(
    [property: JsonPropertyName("applicationType")] string ApplicationType,
    [property: JsonPropertyName("bundleID")] string BundleID,
    [property: JsonPropertyName("date")] int Date,
    [property: JsonPropertyName("diskUsage")] int DiskUsage,
    [property: JsonPropertyName("isLaunchable")] bool IsLaunchable,
    [property: JsonPropertyName("running")] bool Running,
    [property: JsonPropertyName("name")] string Name
);

public record AgentAppList(
    [property: JsonPropertyName("apps")] IReadOnlyList<AppListEntry> Apps,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("success")] bool Success
);