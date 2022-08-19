namespace Corellium.Api.Data;

internal record TokenResponseCache(string Token, DateTimeOffset RefreshAt);