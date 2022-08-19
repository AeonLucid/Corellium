namespace Corellium.Api.Net.Models;

internal record ProjectResponse(
    string Id,
    string Name,
    CorelliumSettings Settings,
    CorelliumQuotas Quotas,
    CorelliumQuotas QuotasUsed
);