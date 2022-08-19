namespace Corellium.Api.Net.Models;

/// <param name="Status">Example "active"</param>
/// <param name="Id">uuid needed to pass to a createInstance call if this is a kernel</param>
/// <param name="Name">Example "Image"</param>
/// <param name="Type">Example "kernel"</param>
/// <param name="Self">Uri</param>
/// <param name="File">File uri</param>
/// <param name="Size"></param>
/// <param name="Checksum"></param>
/// <param name="Encoding">Example "encrypted"</param>
/// <param name="Project">Project uuid</param>
/// <param name="CreatedAt"></param>
/// <param name="UpdatedAt"></param>
public record CorelliumImage(
    string Status,
    string Id,
    string Name,
    string Type,
    string Self,
    string File,
    long Size,
    string Checksum,
    string Encoding,
    string Project,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);