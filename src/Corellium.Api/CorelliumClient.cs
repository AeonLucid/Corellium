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

    private TokenResponseCache? _accessToken;
    private CorelliumSupportedDevice[]? _supportedDevices;
    
    private Dictionary<string, CorelliumTeam> _teams;
    private Dictionary<string, CorelliumUser> _users;

    public CorelliumClient(CorelliumOptions options)
    {
        _options = options;
        _teams = new Dictionary<string, CorelliumTeam>();
        _users = new Dictionary<string, CorelliumUser>();
        
        Http = new FlurlClient
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
    
    internal FlurlClient Http { get; }

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
        // TODO: Lock
        if (ShouldRefreshToken())
        {
            var request = new TokensRequest();

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

            var token = await Http
                .Request("/tokens")
                .PostJsonAsync(request)
                .ReceiveJson<TokensResponse>();

            _accessToken = new TokenResponseCache(token.Token, token.Expiration.Subtract(TimeSpan.FromMinutes(15)));
        }
        
        return _accessToken!.Token;
    }

    public async Task<CorelliumProject?> GetProjectNamedAsync(string name)
    {
        var projects = await GetProjectsAsync();
        return projects.FirstOrDefault(x => x.Name.Equals(name));
    }

    /// <summary>
    ///     Returns an array of <see cref="CorelliumProject"/>s that this client is allowed to access.
    /// </summary>
    /// <returns></returns>
    public async Task<List<CorelliumProject>> GetProjectsAsync()
    {
        var result = new List<CorelliumProject>();
        
        var response = await Http
            .Request("/projects")
            .WithHeader("Authorization", await GetAccessTokenAsync())
            .GetJsonAsync<List<ProjectResponse>>();
        
        foreach (var project in response)
        {
            result.Add(new CorelliumProject(this, project));
        }

        return result;
    }

    internal async Task<ProjectResponse> GetProjectAsync(string id)
    {
        return await Http
            .Request("/projects", id)
            .WithHeader("Authorization", await GetAccessTokenAsync())
            .GetJsonAsync<ProjectResponse>();
    }

    /// <summary>
    ///     Returns an array of <see cref="CorelliumImage"/>s that this client is allowed to access.
    /// </summary>
    public async Task<List<CorelliumImage>> FilesAsync()
    {
        return await Http
            .Request("/images")
            .WithHeader("Authorization", await GetAccessTokenAsync())
            .GetJsonAsync<List<CorelliumImage>>();
    }

    /// <summary>
    ///     Returns teams and users belonging to the domain.
    /// 
    ///     This function is only available to administrators.
    /// </summary>
    public async Task<TeamsAndUsers> GetTeamsAndUsersAsync()
    {
        var teams = _teams = new Dictionary<string, CorelliumTeam>();
        var users = _users = new Dictionary<string, CorelliumUser>();
        
        var res = await Http
            .Request("/teams")
            .WithHeader("Authorization", await GetAccessTokenAsync())
            .GetJsonAsync<List<TeamInfo>>();
        
        foreach (var team in res)
        {
            teams.Add(team.Id, new CorelliumTeam(this, team));

            if (team.Id == "all-users")
            {
                foreach (var user in team.Users)
                {
                    users.Add(user.Id, new CorelliumUser(this, user));
                }
            }
        }
        
        return new TeamsAndUsers(teams, users);
    }

    internal CorelliumUser GetUser(string userId)
    {
        return _users[userId];
    }

    internal CorelliumTeam GetTeam(string teamId)
    {
        return _teams[teamId];
    }

    /// <summary>
    ///     TODO: Untested
    ///     Creates a new user in the domain.
    ///
    ///     This function is only available to domain administrators.
    /// </summary>
    public async Task<CorelliumUser> CreateUser(string login, string name, string email, string password)
    {
        var response = await Http
            .Request("/users")
            .WithHeader("Authorization", await GetAccessTokenAsync())
            .PostJsonAsync(new
            {
                Label = name,
                Name = login,
                Email = email,
                Password = password
            })
            .ReceiveJson();
        
        await GetTeamsAndUsersAsync();
        return GetUser(response.Id);
    }

    /// <summary>
    ///     TODO: Untested
    ///     Destroys a user in the domain.
    ///
    ///     This function is only available to domain administrators.
    /// </summary>
    public async Task DestroyUserAsync(string userId)
    {
        await Http
            .Request("/users", userId)
            .WithHeader("Authorization", await GetAccessTokenAsync())
            .DeleteAsync();
    }

    /// <summary>
    ///     Returns supported device list
    /// </summary>
    public async Task<CorelliumSupportedDevice[]> GetSupportedDevicesAsync()
    {
        return _supportedDevices ??= await Http
            .Request("/supported")
            .WithHeader("Authorization", await GetAccessTokenAsync())
            .GetJsonAsync<CorelliumSupportedDevice[]>();
    }

    /// <summary>
    ///     Returns all keys for the project
    /// </summary>
    /// <param name="projectId">Project Id</param>
    /// <returns></returns>
    public async Task<CorelliumProjectKey[]> GetProjectKeysAsync(string projectId)
    {
        return await Http
            .Request("/projects", projectId, "keys")
            .WithHeader("Authorization", await GetAccessTokenAsync())
            .GetJsonAsync<CorelliumProjectKey[]>();
    }

    /// <summary>
    ///     Adds key to the project
    /// </summary>
    /// <param name="projectId">Project Id</param>
    /// <param name="key">The public key, as formatted in a .pub file</param>
    /// <param name="kind"></param>
    /// <param name="label">Defaults to the public key comment, if present</param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<CorelliumProjectKey> AddProjectKeyAsync(string projectId, string key, KeyKind kind, string? label = null)
    {
        return await Http
            .Request("/projects", projectId, "keys")
            .WithHeader("Authorization", await GetAccessTokenAsync())
            .PostJsonAsync(new ProjectKeyCreate(key, kind, label))
            .ReceiveJson<CorelliumProjectKey>();
    }

    public async Task DeleteProjectKeyAsync(string projectId, string keyId)
    {
        await Http
            .Request("/projects", projectId, "keys", keyId)
            .WithHeader("Authorization", await GetAccessTokenAsync())
            .DeleteAsync();
    }
}