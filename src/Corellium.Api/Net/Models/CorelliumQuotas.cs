namespace Corellium.Api.Net.Models;

public record CorelliumQuotas(
    int Cores,
    int Instances,
    int Ram,
    int Cpus
);