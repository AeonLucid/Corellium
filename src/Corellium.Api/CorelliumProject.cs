using Corellium.Api.Net.Models;

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
}