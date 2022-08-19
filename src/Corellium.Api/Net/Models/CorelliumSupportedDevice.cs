namespace Corellium.Api.Net.Models;

public record CorelliumSupportedDevice(
    string Type,
    string Name,
    string Flavor,
    string Description,
    string Model,
    string? BoardConfig,
    string? Platform,
    int? Cpid,
    int? Bdid,
    List<CorelliumSupportedDeviceFirmware> Firmwares,
    bool Peripherals,
    CorelliumSupportedDeviceQuotas Quotas
);