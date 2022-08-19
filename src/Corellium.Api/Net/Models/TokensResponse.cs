namespace Corellium.Api.Net.Models;

internal record TokensResponse(
    string Token,
    DateTime Expiration
);
