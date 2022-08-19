using Corellium.Api.Net.Models;
using Flurl.Http;

namespace Corellium.Api;

public class CorelliumProject
{
    private readonly CorelliumClient _client;
    
    private ProjectResponse _info;

    internal CorelliumProject(CorelliumClient client, ProjectResponse info)
    {
        _client = client;
        _info = info;
    }

    /// <summary>
    ///     The project id.
    /// </summary>
    public string Id => _info.Id;
    
    /// <summary>
    ///     The project name.
    /// </summary>
    public string Name => _info.Name;
    
    /// <summary>
    ///     The project quotas.
    /// </summary>
    public CorelliumQuotas Quotas => _info.Quotas;
    
    /// <summary>
    ///     How much of the project's quotas are currently used.
    ///     To ensure this information is up to date, call <see cref="RefreshAsync"/> first.
    /// </summary>
    public CorelliumQuotas QuotasUsed => _info.QuotasUsed;
    
    public async Task RefreshAsync()
    {
        _info = await _client.GetProjectAsync(_info.Id);
    }

    public async Task<List<CorelliumInstance>> GetInstancesAsync()
    {
        var info = await _client.Http
            .Request("/projects", _info.Id, "instances")
            .WithHeader("Authorization", await _client.GetAccessTokenAsync())
            .GetJsonAsync<List<CorelliumInstanceInfo>>();

        return info.Select(x => new CorelliumInstance(this, x)).ToList();
    }

    public async Task<CorelliumInstance> GetInstanceAsync(string instanceId)
    {
        var info = await _client.Http
                .Request("/instances", instanceId)
                .WithHeader("Authorization", await _client.GetAccessTokenAsync())
                .GetJsonAsync<CorelliumInstanceInfo>();

        return new CorelliumInstance(this, info);
    }

    public async Task<CorelliumInstance> CreateInstanceAsync(CorelliumInstanceCreate options)
    {
        options.Project = _info.Id;
        
        var res = await _client.Http
            .Request("/instances")
            .WithHeader("Authorization", await _client.GetAccessTokenAsync())
            .PostJsonAsync(options)
            .ReceiveJson<CorelliumInstanceCreateResponse>();

        return await GetInstanceAsync(res.Id);
    }

    public async Task GetVpnConfigAsync(string type = "ovpn", string? clientUuid = null)
    {
        if (clientUuid == null)
        {
            clientUuid = Guid.NewGuid().ToString();
        }
        
        await _client.Http
            .Request("/projects", _info.Id, "vpn-configs", $"{clientUuid}.{type}")
            .WithHeader("Authorization", await _client.GetAccessTokenAsync())
            .GetBytesAsync();
    }

    /// <summary>
    ///     Returns a list of authorized keys associated with the project. When a new
    ///     instance is created in this project, its authorized_keys (iOS) or adbkeys
    ///     (Android) will be populated with these keys by default. Adding or
    ///     removing keys from the project will have no effect on existing instances.
    /// </summary>
    public async Task<CorelliumProjectKey[]> GetKeysAsync()
    {
        return await _client.GetProjectKeysAsync(_info.Id);
    }

    /// <summary>
    ///     Add a public key to project.
    /// </summary>
    /// <param name="key">The public key, as formatted in a .pub file</param>
    /// <param name="kind"></param>
    /// <param name="label">Defaults to the public key comment, if present</param>
    public async Task<CorelliumProjectKey> AddKeyAsync(string key, KeyKind kind, string? label = null)
    {
        return await _client.AddProjectKeyAsync(_info.Id, key, kind, label);
    }

    public async Task DeleteKeyAsync(string keyId)
    {
        await _client.DeleteProjectKeyAsync(_info.Id, keyId);
    }
}