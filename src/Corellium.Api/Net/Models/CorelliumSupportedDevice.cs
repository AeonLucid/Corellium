using System.Text.Json.Serialization;

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

public record CorelliumSupportedDeviceFirmware(
    string Version,
    string Buildid,
    string? Sha256sum,
    string? Sha1sum,
    string Md5sum,
    ulong Size,
    string? Uniqueid,
    CorelliumSupportedDeviceMetadata? Metadata,
    DateTimeOffset Releaseddate,
    DateTimeOffset? Uploadeddate,
    string Url,
    [property: JsonPropertyName("orig_url")] string OrigUrl,
    string Filename
);

public record CorelliumSupportedDeviceQuotas(
    int Cores,
    int Cpus
);

public record CorelliumSupportedDeviceMetadata(
    List<string> Variants
);