namespace Corellium.Api.Net.Models;

public class CorelliumInstanceCreate
{
    /// <summary>
    ///     The project to create the instance in.
    ///     No need to set as this is done by the library.
    /// </summary>
    public string Project { get; set; } = null!;
    
    /// <summary>
    ///     The device flavor, such as `iphone6`
    /// </summary>
    public string Flavor { get; set; } = null!;
    
    /// <summary>
    ///     The device operating system version
    /// </summary>
    public string Os { get; set; } = null!;
    
    /// <summary>
    ///     The ID of a previously uploaded image in the project to use as the firmware
    /// </summary>
    public string Ipsw { get; set; } = null!;
    
    /// <summary>
    ///     The device operating system build
    /// </summary>
    public string Osbuild { get; set; } = null!;
    
    /// <summary>
    ///     The ID of snapshot to clone this device off of
    /// </summary>
    public string? Snapshot { get; set; }
    
    /// <summary>
    ///     The device name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    ///     Instance patches, such as `jailbroken` (default), `nonjailbroken` or `corelliumd` which is non-jailbroken with API agent.
    /// </summary>
    public string? Patches { get; set; }
    
    /// <summary>
    ///     Instance patches, such as `jailbroken` (default), `nonjailbroken` or `corelliumd` which is non-jailbroken with API agent.
    /// </summary>
    public CorelliumInstanceCreateBootOptions? BootOptions { get; set; }
}