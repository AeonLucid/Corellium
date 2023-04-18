using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Corellium.Api.Net.Models;

public class PatchInstanceOptions
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonObject[]? Proxy { get; set; }
}