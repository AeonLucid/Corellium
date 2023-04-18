using System.Text.Json.Serialization;

namespace Corellium.Api.Net.Models.Agent;

public record AgentStat(
    [property: JsonPropertyName("atime")] int atime,
    [property: JsonPropertyName("ctime")] int ctime,
    [property: JsonPropertyName("gid")] int gid,
    [property: JsonPropertyName("mode")] int mode,
    [property: JsonPropertyName("mtime")] int mtime,
    [property: JsonPropertyName("name")] string name,
    [property: JsonPropertyName("size")] int size,
    [property: JsonPropertyName("uid")] int uid
);