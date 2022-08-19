namespace Corellium.Api.Net.Models;

internal record TeamInfo(
    string Id,
    string Label,
    List<TeamUserInfo> Users
);

internal record TeamUserInfo(
    string Id,
    string Label,
    string Name,
    string Email,
    bool Administrator
);