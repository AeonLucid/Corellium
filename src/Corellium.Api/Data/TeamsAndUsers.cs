namespace Corellium.Api.Data;

public record TeamsAndUsers(
    IReadOnlyDictionary<string, CorelliumTeam> Teams,
    IReadOnlyDictionary<string, CorelliumUser> Users
);