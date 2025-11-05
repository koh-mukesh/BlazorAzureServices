using System.Text.Json;

namespace BlazorAzureServices.Services;

public class ConfigurationService
{
    private readonly HttpClient _httpClient;

    public ConfigurationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ServiceTableSection>> LoadServiceSectionsAsync()
    {
        try
        {
            Console.WriteLine("Loading configuration from settings.txt...");
            var configContent = await _httpClient.GetStringAsync("settings.txt");
            Console.WriteLine($"Configuration loaded, content length: {configContent.Length}");
            var sections = ParseConfigurationContent(configContent);
            Console.WriteLine($"Parsed {sections.Count} sections");
            return sections;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            return GetDefaultServiceSections();
        }
    }

    private List<ServiceTableSection> ParseConfigurationContent(string content)
    {
        var sections = new List<ServiceTableSection>();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                          .Select(line => line.Trim())
                          .Where(line => !string.IsNullOrWhiteSpace(line))
                          .ToList();

        Console.WriteLine($"Processing {lines.Count} lines from configuration");

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            
            // Skip dashed separator lines
            if (line.StartsWith("---"))
                continue;

            // Check if this line might be a section header (not containing tabs and not being a header row)
            if (!line.Contains('\t') && 
                i + 1 < lines.Count && 
                lines[i + 1].Contains('\t') && 
                !lines[i + 1].StartsWith("---"))
            {
                var sectionName = line;
                var section = CreateServiceSection(sectionName);
                
                // Find the header row (next non-dashed line with tabs)
                int headerIndex = i + 1;
                while (headerIndex < lines.Count && 
                       (lines[headerIndex].StartsWith("---") || !lines[headerIndex].Contains('\t')))
                {
                    headerIndex++;
                }

                if (headerIndex < lines.Count)
                {
                    var headers = ParseTabDelimitedLine(lines[headerIndex]);
                    section.TableHeaders = headers;

                    // Parse data rows
                    int dataIndex = headerIndex + 1;
                    
                    // Skip dashed separator line
                    if (dataIndex < lines.Count && lines[dataIndex].StartsWith("---"))
                        dataIndex++;

                    while (dataIndex < lines.Count && 
                           lines[dataIndex].Contains('\t'))
                    {
                        var dataRow = ParseTabDelimitedLine(lines[dataIndex]);
                        var resource = CreateTableResourceItem(dataRow, headers, sectionName);
                        if (resource != null)
                        {
                            section.Resources.Add(resource);
                        }
                        dataIndex++;
                    }

                    sections.Add(section);
                    i = dataIndex - 1; // Skip processed lines
                }
            }
        }

        return sections.Any() ? sections : GetDefaultServiceSections();
    }

    private List<string> ParseTabDelimitedLine(string line)
    {
        return line.Split('\t', StringSplitOptions.None)
                  .Select(cell => cell.Trim())
                  .ToList();
    }

    private ServiceTableSection CreateServiceSection(string sectionName)
    {
        var section = new ServiceTableSection
        {
            Name = sectionName,
            Type = sectionName.ToLower().Replace(" ", ""),
            AddButtonText = sectionName,
            IsExpanded = true,
            ShowExtraColumn = false,
            TableHeaders = new List<string>(),
            Resources = new List<TableResourceItem>()
        };

        // Set icon and special properties based on service type
        section.Icon = GetServiceIcon(sectionName);
        
        // Special handling for API Management
        if (sectionName.Equals("API Management", StringComparison.OrdinalIgnoreCase))
        {
            section.ShowExtraColumn = true;
        }

        return section;
    }

    private string GetServiceIcon(string serviceName)
    {
        return serviceName.ToLower() switch
        {
            "api management" => "🔌",
            "logic apps" => "⚡",
            "azure functions" => "⚡",
            "key vault" => "🔐",
            "application insights" => "📊",
            "cosmos db" => "🌍",
            "app services" => "🌐",
            "storage" => "💾",
            "resource groups" => "📁",
            "data factory" => "🏭",
            "cognitive search" => "🔍",
            _ => "⚙️"
        };
    }

    private TableResourceItem? CreateTableResourceItem(List<string> dataRow, List<string> headers, string sectionName)
    {
        if (dataRow.Count < 2) return null;

        var resource = new TableResourceItem
        {
            Icon = GetServiceIcon(sectionName),
            StatusClass = "running"
        };

        // Map data to properties based on headers
        for (int i = 0; i < Math.Min(dataRow.Count, headers.Count); i++)
        {
            var header = headers[i].ToLower();
            var value = dataRow[i];

            switch (header)
            {
                case "service name":
                    resource.Name = ExtractServiceName(value);
                    resource.Tag = ExtractTag(value);
                    break;
                case "type":
                    resource.Type = value;
                    break;
                case "environment":
                    resource.Tag = value.ToLower();
                    break;
                case "resource group":
                    resource.ResourceGroup = value;
                    break;
                case "location":
                    resource.Location = value;
                    break;
                case "tier":
                case "runtime":
                    resource.TierOrRuntime = value;
                    break;
                case "dev portal":
                    if (sectionName.Equals("API Management", StringComparison.OrdinalIgnoreCase))
                    {
                        resource.ExtraColumn = value;
                    }
                    break;
                case "status":
                    resource.Status = value;
                    resource.StatusClass = GetStatusClass(value);
                    break;
                case "actions":
                    // Actions column is handled by the UI
                    break;
            }
        }

        // Generate URL for Azure Portal (basic pattern)
        resource.Url = GenerateAzurePortalUrl(resource, sectionName);

        return resource;
    }

    private string ExtractServiceName(string nameWithTag)
    {
        // Handle names like "api-kohler-dev2 dev" or "APIM-Extension-Logic-App prod"
        var parts = nameWithTag.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 1)
        {
            var lastPart = parts.Last().ToLower();
            if (IsEnvironmentTag(lastPart))
            {
                return string.Join(" ", parts.Take(parts.Length - 1));
            }
        }
        return nameWithTag;
    }

    private string ExtractTag(string nameWithTag)
    {
        var parts = nameWithTag.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 1)
        {
            var lastPart = parts.Last().ToLower();
            if (IsEnvironmentTag(lastPart))
            {
                return lastPart;
            }
        }
        return "";
    }

    private bool IsEnvironmentTag(string tag)
    {
        var environmentTags = new[] { "dev", "test", "prod", "production", "staging", "uat", "qa" };
        return environmentTags.Contains(tag.ToLower());
    }

    private string GetStatusClass(string status)
    {
        return status.ToLower() switch
        {
            "online" or "enabled" or "running" => "running",
            "offline" or "disabled" or "stopped" => "stopped",
            _ => "running"
        };
    }

    private string GenerateAzurePortalUrl(TableResourceItem resource, string sectionName)
    {
        // This is a basic URL pattern - in a real implementation, you'd need proper resource IDs
        var resourceType = sectionName.ToLower() switch
        {
            "api management" => "Microsoft.ApiManagement/service",
            "logic apps" => "Microsoft.Logic/workflows",
            "azure functions" => "Microsoft.Web/sites",
            "key vault" => "Microsoft.KeyVault/vaults",
            "application insights" => "Microsoft.Insights/components",
            "cosmos db" => "Microsoft.DocumentDB/databaseAccounts",
            "app services" => "Microsoft.Web/sites",
            "storage" => "Microsoft.Storage/storageAccounts",
            _ => "Microsoft.Resources/resourceGroups"
        };

        // Basic portal URL pattern - you may need to customize this based on your actual subscription ID
        return $"https://portal.azure.com/#resource/subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/{resource.ResourceGroup}/providers/{resourceType}/{resource.Name}/overview";
    }

    private List<ServiceTableSection> GetDefaultServiceSections()
    {
        // Fallback to hardcoded sections if configuration fails to load
        return new List<ServiceTableSection>
        {
            new ServiceTableSection
            {
                Name = "API Management",
                Icon = "🔌",
                Type = "apimanagement",
                AddButtonText = "API Management",
                IsExpanded = true,
                ShowExtraColumn = true,
                TableHeaders = new List<string> { "Service Name", "Type", "Resource Group", "Location", "Tier", "Dev Portal", "Status", "Actions" },
                Resources = new List<TableResourceItem>()
            }
        };
    }
}

// Data model classes
public class ServiceTableSection
{
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Type { get; set; } = "";
    public string AddButtonText { get; set; } = "";
    public bool IsExpanded { get; set; } = true;
    public bool ShowExtraColumn { get; set; } = false;
    public List<string> TableHeaders { get; set; } = new();
    public List<TableResourceItem> Resources { get; set; } = new();
}

public class TableResourceItem
{
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Type { get; set; } = "";
    public string ResourceGroup { get; set; } = "";
    public string Location { get; set; } = "";
    public string TierOrRuntime { get; set; } = "";
    public string ExtraColumn { get; set; } = "";
    public string Status { get; set; } = "";
    public string StatusClass { get; set; } = "";
    public string Tag { get; set; } = "";
    public string Url { get; set; } = "";
}

public class NewResourceModel
{
    public string Name { get; set; } = "";
    public string ResourceGroup { get; set; } = "";
    public string Location { get; set; } = "";
    public string TierOrRuntime { get; set; } = "";
}