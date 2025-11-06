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
            Console.WriteLine("Loading configuration from settings.csv...");
            var configContent = await _httpClient.GetStringAsync("settings.csv");
            Console.WriteLine($"Configuration loaded, content length: {configContent.Length}");
            var sections = ParseCsvConfigurationContent(configContent);
            Console.WriteLine($"Parsed {sections.Count} sections");
            return sections;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            return GetDefaultServiceSections();
        }
    }

    private List<ServiceTableSection> ParseCsvConfigurationContent(string content)
    {
        var sections = new Dictionary<string, ServiceTableSection>();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                          .Select(line => line.Trim())
                          .Where(line => !string.IsNullOrWhiteSpace(line))
                          .ToList();

        Console.WriteLine($"Processing {lines.Count} lines from CSV configuration");

        if (lines.Count < 2)
        {
            Console.WriteLine("CSV file must have at least a header row and one data row");
            return GetDefaultServiceSections();
        }

        // Parse header row
        var headers = ParseCsvLine(lines[0]);
        
        // Process data rows
        for (int i = 1; i < lines.Count; i++)
        {
            var dataRow = ParseCsvLine(lines[i]);
            
            if (dataRow.Count != headers.Count)
            {
                Console.WriteLine($"Row {i + 1} has {dataRow.Count} columns, expected {headers.Count}. Skipping.");
                continue;
            }

            // Get section name from first column
            var sectionName = dataRow[0];
            
            if (!sections.ContainsKey(sectionName))
            {
                sections[sectionName] = CreateServiceSection(sectionName);
                // Set appropriate headers for this section type
                sections[sectionName].TableHeaders = GetHeadersForSection(sectionName, headers);
            }

            var resource = CreateTableResourceItemFromCsv(dataRow, headers, sectionName);
            if (resource != null)
            {
                sections[sectionName].Resources.Add(resource);
            }
        }

        return sections.Values.ToList();
    }

    private List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = "";
        bool inQuotes = false;
        
        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.Trim());
                current = "";
            }
            else
            {
                current += c;
            }
        }
        
        result.Add(current.Trim());
        return result;
    }

    private List<string> GetHeadersForSection(string sectionName, List<string> allHeaders)
    {
        // Return appropriate headers for each section type
        return sectionName.ToLower() switch
        {
            "api management" => new List<string> { "Service Name", "Type", "Resource Group", "Location", "Tier", "Dev Portal", "Status", "Actions" },
            "logic apps" => new List<string> { "Service Name", "Type", "Resource Group", "Location", "Environment", "Status", "Actions" },
            "azure functions" => new List<string> { "Service Name", "Type", "Resource Group", "Location", "Runtime", "Status", "Actions" },
            _ => new List<string> { "Service Name", "Type", "Resource Group", "Location", "Tier", "Status", "Actions" }
        };
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

    private TableResourceItem? CreateTableResourceItemFromCsv(List<string> dataRow, List<string> headers, string sectionName)
    {
        if (dataRow.Count < 2) return null;

        var resource = new TableResourceItem
        {
            Icon = GetServiceIcon(sectionName),
            StatusClass = "running"
        };

        // Map CSV columns directly (assuming fixed CSV structure)
        if (dataRow.Count >= 9)
        {
            // CSV structure: Section,Service Name,Type,Resource Group,Location,TierOrRuntime,ExtraColumn,Status,Tag
            resource.Name = dataRow[1]; // Service Name
            resource.Type = dataRow[2]; // Type
            resource.ResourceGroup = dataRow[3]; // Resource Group
            resource.Location = dataRow[4]; // Location
            resource.TierOrRuntime = dataRow[5]; // TierOrRuntime
            resource.ExtraColumn = dataRow[6]; // ExtraColumn (for Dev Portal, Environment, etc.)
            resource.Status = dataRow[7]; // Status
            resource.StatusClass = GetStatusClass(dataRow[7]);
            resource.Tag = dataRow[8]; // Tag

            // Set ExtraColumn based on section type for display purposes
            if (sectionName.Equals("API Management", StringComparison.OrdinalIgnoreCase))
            {
                // For API Management, ExtraColumn is Dev Portal info - keep as is
            }
            else if (sectionName.Equals("Logic Apps", StringComparison.OrdinalIgnoreCase))
            {
                // For Logic Apps, move TierOrRuntime to ExtraColumn as Environment and clear TierOrRuntime
                resource.ExtraColumn = resource.TierOrRuntime;
                resource.TierOrRuntime = "";
            }
            else if (sectionName.Equals("Azure Functions", StringComparison.OrdinalIgnoreCase))
            {
                // For Azure Functions, TierOrRuntime is Runtime - keep as is, clear ExtraColumn
                resource.ExtraColumn = "";
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
            "logic apps" => "Microsoft.Web/sites",
            "azure functions" => "Microsoft.Web/sites",
            "key vault" => "Microsoft.KeyVault/vaults",
            "application insights" => "Microsoft.Insights/components",
            "cosmos db" => "Microsoft.DocumentDB/databaseAccounts",
            "app services" => "Microsoft.Web/sites",
            "storage" => "Microsoft.Storage/storageAccounts",
            _ => "Microsoft.Resources/resourceGroups"
        };

        // Basic portal URL pattern - you may need to customize this based on your actual subscription ID
        return $"https://portal.azure.com/#@mykohler.onmicrosoft.com/resource/subscriptions/35166c0d-3c12-4f4c-8638-79d7248ae93f/resourceGroups/{resource.ResourceGroup}/providers/{resourceType}/{resource.Name}/overview";
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