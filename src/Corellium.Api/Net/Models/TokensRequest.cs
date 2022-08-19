namespace Corellium.Api.Net.Models;

internal class TokensRequest
{
    public string? ApiToken { get; set; }
    
    public string? Username { get; set; }
    
    public string? Password { get; set; }
    
    public string? TotpToken { get; set; }
}