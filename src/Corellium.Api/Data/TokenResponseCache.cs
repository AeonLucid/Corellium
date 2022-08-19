using Corellium.Api.Net.Models;

namespace Corellium.Api.Data;

internal record TokenResponseCache(CorelliumTokensResponse Response, DateTimeOffset RefreshAt);