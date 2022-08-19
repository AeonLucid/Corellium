using System.Text.Json.Serialization;

namespace Corellium.Api.Net.Models;

public record CorelliumSupportedDeviceFirmware(
    string Version,
    string Buildid,
    string? Sha256sum,
    string? Sha1sum,
    string Md5sum,
    long Size,
    string? Uniqueid,
    CorelliumSupportedDeviceMetadata? Metadata,
    DateTimeOffset Releaseddate,
    DateTimeOffset? Uploadeddate,
    string Url,
    [property: JsonPropertyName("orig_url")] string OrigUrl,
    string Filename
);