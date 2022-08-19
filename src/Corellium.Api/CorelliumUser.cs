using Corellium.Api.Net.Models;

namespace Corellium.Api;

public class CorelliumUser
{
    private readonly CorelliumClient _client;
    private readonly TeamUserInfo _info;

    internal CorelliumUser(CorelliumClient client, TeamUserInfo info)
    {
        _client = client;
        _info = info;
    }

    /// <summary>
    ///     Gets the id of the user
    /// </summary>
    public string Id => _info.Id;

    /// <summary>
    ///     Gets the username of the user
    /// </summary>
    public string Login => _info.Name;

    /// <summary>
    ///     Gets the full name of the user
    /// </summary>
    public string Name => _info.Label;
    
    /// <summary>
    ///     Gets the email of the user
    /// </summary>
    public string Email => _info.Email;

    /// <summary>
    ///     Delete this user.
    ///
    ///     This function is only available to domain administrators.
    /// </summary>
    public async Task DestroyAsync()
    {
        await _client.DestroyUserAsync(_info.Id);
    }
}