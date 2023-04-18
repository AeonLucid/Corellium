using Corellium.Api.Exceptions;
using Corellium.Api.Net.Models;
using Flurl.Http;

namespace Corellium.Api;

public class CorelliumInstance : IDisposable
{
    private readonly CorelliumClient _client;
    private readonly CorelliumProject _project;
    private readonly CorelliumInstanceInfo _info;

    private CorelliumAgent? _agent;
    
    public CorelliumInstance(CorelliumClient client, CorelliumProject project, CorelliumInstanceInfo info)
    {
        _client = client;
        _project = project;
        _info = info;
    }

    /// <summary>
    ///     The instance name.
    /// </summary>
    public string Name => _info.Name;
    
    /// <summary>
    ///     The instance state. Possible values include:
    ///     
    ///     State Description
    ///     `on` The instance is powered on.
    ///     `off` The instance is powered off.
    ///     `creating` The instance is in the process of creating.
    ///     `deleting` The instance is in the process of deleting.
    ///     `deleted` The instance is deleted, instance will set to undefined.
    ///     `paused` The instance is paused.
    ///     
    ///     A full list of possible values is available in the API documentation.
    /// </summary>
    public string State => _info.State;
    
    /// <summary>
    ///     The timestamp when the state last changed.
    /// </summary>
    public DateTimeOffset? StateChanged => _info.StateChanged;

    /// <summary>
    ///     The instance flavor, such as `iphone6`.
    /// </summary>
    public string Flavor => _info.Flavor;

    /// <summary>
    ///     The instance type, such as `ios`.
    /// </summary>
    public string Type => _info.Type;

    /// <summary>
    ///     The pending task that is being requested by the user and is being executed by the backend.
    ///     This field is null when no tasks are pending. The returned object has two fields: name and options.
    ///     
    ///     Current options for name are start, stop, pause, unpause, snapshot, revert.
    ///     For start and revert, options.bootOptions contains the boot options the instance is to be started with.
    /// </summary>
    public string? UserTask => _info.UserTask;

    /// <summary>
    ///     Return the current task state.
    /// </summary>
    public string TaskState => _info.TaskState;
    
    /// <summary>
    ///     The instance boot options.
    /// </summary>
    public CorelliumInstanceBootOptions BootOptions => _info.BootOptions;

    public async Task<Stream> TakeScreenshotAsync(string format = "png", int scale = 1)
    {
        return await _client.Http
            .Request($"/instances/{_info.Id}/screenshot.{format}")
            .SetQueryParam("scale", scale)
            .WithHeader("Authorization", await _client.GetAccessTokenAsync())
            .GetStreamAsync();
    }

    public async Task<CorelliumAgent> AgentAsync()
    {
        if (_agent != null && !_agent.Connected && !_agent.PendingConnect)
        {
            using (_agent)
            {
                await _agent.DisconnectAsync();
            }
            
            _agent = null;
        }
        
        if (_agent == null)
        {
            _agent = new CorelliumAgent(_client, this);
        }

        if (!await _agent.ReadyAsync())
        {
            throw new CorelliumAgentException("Agent was not ready");
        }

        return _agent;
    }

    public string GetAgentEndpointAsync()
    {
        if (_info.Agent == null)
        {
            throw new CorelliumAgentException("Agent is not available for this instance at this moment.");
        }

        return _client.Http.BaseUrl + "/agent/" + _info.Agent.Info;
    }

    public void Dispose()
    {
        _agent?.Dispose();
    }
}