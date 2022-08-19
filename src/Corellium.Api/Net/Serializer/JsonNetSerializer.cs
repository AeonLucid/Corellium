using System.Text.Json;
using Flurl.Http.Configuration;

namespace Corellium.Api.Net;

public class JsonNetSerializer : ISerializer
{
    private readonly JsonSerializerOptions _options;

    public JsonNetSerializer(JsonSerializerOptions? options = null)
    {
        _options = options ?? new JsonSerializerOptions();
    }
    
    public string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, _options);
    }

    public T? Deserialize<T>(string s)
    {
        return JsonSerializer.Deserialize<T>(s, _options);
    }

    public T? Deserialize<T>(Stream stream)
    {
        return JsonSerializer.Deserialize<T>(stream, _options);
    }
}