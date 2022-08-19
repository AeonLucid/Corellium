using Corellium.Api.Net.Models;

namespace Corellium.Api;

public class CorelliumTeam
{
    private readonly CorelliumClient _client;
    private readonly TeamInfo _info;

    internal CorelliumTeam(CorelliumClient client, TeamInfo info)
    {
        _client = client;
        _info = info;
    }

    /// <summary>
    ///     Gets the id of the team
    /// </summary>
    public string Id => _info.Id;
    
    /// <summary>
    ///     Gets the name of the team
    /// </summary>
    public string Label => _info.Label;

    /// <summary>
    ///     Gets the users belonging to the team
    /// </summary>
    public IEnumerable<CorelliumUser> Users => _info.Users.Select(x => _client.GetUser(x.Id));
}