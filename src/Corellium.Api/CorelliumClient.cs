using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Corellium.Api.Data;
using Corellium.Api.Net.Handler;
using Corellium.Api.Net.Models;
using Corellium.Api.Net.Serializer;
using Flurl.Http;
using Flurl.Http.Configuration;

namespace Corellium.Api;

public class CorelliumClient
{
    private readonly CorelliumOptions _options;
    private readonly FlurlClient _client;

    private TokenResponseCache? _accessToken;
    private string? _supportedDevices;
    private string? _teams;

    public CorelliumClient(CorelliumOptions options)
    {
        _options = options;
        _client = new FlurlClient
        {
            BaseUrl = $"{_options.Endpoint}/api/v1",
            Settings = new ClientFlurlHttpSettings
            {
                HttpClientFactory = new ProxyHttpClientFactory(new WebProxy("http://127.0.0.1:8888")),
                JsonSerializer = new JsonNetSerializer(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                })
            }
        };
    }

    private bool ShouldRefreshToken()
    {
        if (_accessToken == null)
        {
            return true;
        }
        
        if (_accessToken.RefreshAt < DateTimeOffset.UtcNow)
        {
            return true;
        }

        return false;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (ShouldRefreshToken())
        {
            var request = new CorelliumTokensRequest();

            if (!string.IsNullOrEmpty(_options.ApiToken))
            {
                request.ApiToken = _options.ApiToken;
            }
            else
            {
                request.Username = _options.Username;
                request.Password = _options.Password;
                request.TotpToken = _options.TotpToken;
            }

            var token = await _client
                .Request("/tokens")
                .PostJsonAsync(request)
                .ReceiveJson<CorelliumTokensResponse>();

            _accessToken = new TokenResponseCache(token, token.Expiration.Subtract(TimeSpan.FromMinutes(15)));
        }
        
        return _accessToken!.Response.Token;
    }
}