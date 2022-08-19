namespace Corellium.Api.Net.Models;

public class CorelliumInstanceCreateBootOptions
{
    /// <summary>
    ///     Change the Kernel slide value for an iOS device.
    ///     When not set, the slide will default to zero. When set to an empty value, the slide will be randomized.
    /// </summary>
    public string? KernelSlide { get; set; }
    
    /// <summary>
    ///     Predefined Unique Device ID (UDID) for iOS device
    /// </summary>
    public string? Udid { get; set; }
    
    /// <summary>
    ///     Change the screen metrics for Ranchu devices `XxY[:DPI]`, e.g. `720x1280:280`
    /// </summary>
    public string? Screen { get; set; }
    
    /// <summary>
    ///     Addition features to utilize for the device, valid options include:
    ///     - kalloc : Enable kalloc/kfree trace access via GDB (Enterprise only)
    ///     - gpu : Enable cloud GPU acceleration (Extra costs incurred, cloud only)
    /// </summary>
    public string[]? AdditionalTags { get; set; }
}