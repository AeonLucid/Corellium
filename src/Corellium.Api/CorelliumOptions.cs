namespace Corellium.Api;

public class CorelliumOptions
{
    public CorelliumOptions(string endpoint, string apiToken)
    {
        Endpoint = endpoint;
        ApiToken = apiToken;
    }
    
    public CorelliumOptions(string endpoint, string username, string password, string? totpToken = null)
    {
        Endpoint = endpoint;
        Username = username;
        Password = password;
        TotpToken = totpToken;
    }
    
    public string Endpoint { get; }
    
    public string? Username { get; }
    
    public string? Password { get; }
    
    public string? TotpToken { get; }

    public string? ApiToken { get; }
}