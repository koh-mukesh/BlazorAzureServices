namespace BlazorAzureServices.Configuration;

/// <summary>
/// Configuration model for Azure subscription settings
/// </summary>
public class AzureConfiguration
{
    public const string SectionName = "Azure";
    
    public string TenantDomain { get; set; } = string.Empty;
    public Dictionary<string, SubscriptionConfiguration> Subscriptions { get; set; } = new();
    public Dictionary<string, List<string>> ServiceMapping { get; set; } = new();
    public Dictionary<string, List<string>> ResourceGroupMapping { get; set; } = new();
    public Dictionary<string, string> SpecificResourceOverrides { get; set; } = new();
}

/// <summary>
/// Configuration model for individual subscription details
/// </summary>
public class SubscriptionConfiguration
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}