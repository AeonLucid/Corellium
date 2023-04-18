namespace Corellium.Api.Net.Models;

public record CorelliumInstanceServicesVpnProxy(
    int DevicePort,
    int RouterPort,
    string Status,
    int? ExposedPort
);

public record CorelliumInstanceServicesVpn(
    CorelliumInstanceServicesVpnProxy[] Proxy
    // TODO: listeners
);

public record CorelliumInstanceServices(
    CorelliumInstanceServicesVpn? Vpn
);

public record CorelliumInstanceBootOptions(
    string BootArgs,
    string Udid,
    string Ecid,
    bool NoSnapshotMount,
    bool Pac,
    bool Aprr
);

public record CorelliumInstanceAgent(
    string Hash, 
    string Info);

public record CorelliumInstanceInfo(
    string Id,
    string Name,
    string Key,
    string Flavor,
    string Type,
    string Project,
    string State,
    DateTimeOffset? StateChanged,
    string? UserTask,
    string TaskState,
    string? Error,
    CorelliumInstanceBootOptions BootOptions,
    string ServiceIp,
    string WifiIp,
    CorelliumInstanceServices Services,
    bool Panicked,
    DateTimeOffset Created,
    string? Ipsw,
    string Os,
    CorelliumInstanceAgent? Agent,
    int? ExposePort,
    // TODO: fault
    string[] Patches,
    string? Fwpackage
);