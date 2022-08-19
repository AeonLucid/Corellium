namespace Corellium.Api.Net.Models;

public record CorelliumTokensResponse(
    CorelliumUser User,
    CorelliumDomain Domain,
    long Time,
    string Token,
    DateTime Expiration
);

public record CorelliumApiToken(
    string Prefix,
    DateTime LastAuthDate,
    string LastAddress
);

public record CorelliumDomain(
    string Name,
    string Label,
    string TermsPolicy,
    int Cores,
    bool Individual,
    string LicenseType,
    bool Android,
    bool NoIOS
);

public record CorelliumMisc(
    CorelliumApiToken ApiToken
);

public record CorelliumUser(
    string Name,
    string Label,
    string Email,
    CorelliumMisc Misc,
    string Id,
    DateTime AgreedToTerms,
    object FederatedUserId,
    string IntercomHMAC
);
