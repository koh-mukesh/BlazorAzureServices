using System.Text.Json;

namespace BlazorAzureServices.Services;

public class ConfigurationService
{
    private readonly HttpClient _httpClient;
    
    // Azure subscription configuration
    // TODO: Move these to appsettings.json or environment variables for better configuration management
    private const string DefaultSubscriptionId = "35166c0d-3c12-4f4c-8638-79d7248ae93f";
    private const string DataFactorySubscriptionId = "e4e37c2d-09d5-4584-9b5b-e4d389d8cd1b"; // TODO: Update this with actual Data Factory subscription ID
    private const string AzureTenantDomain = "mykohler.onmicrosoft.com";

    public ConfigurationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ServiceTableSection>> LoadServiceSectionsAsync()
    {
        try
        {
            Console.WriteLine("=== [DEBUG] LoadServiceSectionsAsync STARTED ===");
            Console.WriteLine("Loading configuration from settings.csv...");
            var configContent = await _httpClient.GetStringAsync("settings.csv");
            Console.WriteLine($"Configuration loaded, content length: {configContent.Length}");
            
            // Show first few lines of CSV content for debugging
            var lines = configContent.Split('\n').Take(5).ToArray();
            Console.WriteLine("First 5 lines of CSV:");
            for (int i = 0; i < lines.Length; i++)
            {
                Console.WriteLine($"  Line {i}: '{lines[i]}'");
            }
            
            var sections = ParseCsvConfigurationContent(configContent);
            Console.WriteLine($"Parsed {sections.Count} sections");
            
            foreach (var section in sections)
            {
                Console.WriteLine($"Section: {section.Name} - {section.Resources.Count} resources");
            }
            
            Console.WriteLine("=== [DEBUG] LoadServiceSectionsAsync COMPLETED ===");
            return sections;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== [ERROR] LoadServiceSectionsAsync FAILED ===");
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
        Console.WriteLine($"[DEBUG] CreateTableResourceItemFromCsv called:");
        Console.WriteLine($"  - Section Name: '{sectionName}'");
        Console.WriteLine($"  - Data Row Count: {dataRow.Count}");
        Console.WriteLine($"  - Headers Count: {headers.Count}");
        
        if (dataRow.Count < 2) 
        {
            Console.WriteLine($"[WARNING] Data row has less than 2 columns, skipping");
            return null;
        }

        var resource = new TableResourceItem
        {
            Icon = GetServiceIcon(sectionName),
            StatusClass = "running"
        };

        // Map CSV columns directly (assuming fixed CSV structure)
        if (dataRow.Count >= 9)
        {
            Console.WriteLine($"[DEBUG] Processing CSV row with {dataRow.Count} columns:");
            for (int i = 0; i < dataRow.Count && i < 9; i++)
            {
                Console.WriteLine($"  - Column {i}: '{dataRow[i]}'");
            }
            
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

            Console.WriteLine($"[DEBUG] Mapped resource:");
            Console.WriteLine($"  - Name: '{resource.Name}'");
            Console.WriteLine($"  - Type: '{resource.Type}'");
            Console.WriteLine($"  - Resource Group: '{resource.ResourceGroup}'");
            Console.WriteLine($"  - Location: '{resource.Location}'");
            Console.WriteLine($"  - Status: '{resource.Status}'");
            Console.WriteLine($"  - Tag: '{resource.Tag}'");

            // Set ExtraColumn based on section type for display purposes
            if (sectionName.Equals("API Management", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[DEBUG] Processing API Management section");
                // For API Management, ExtraColumn is Dev Portal info - keep as is
            }
            else if (sectionName.Equals("Logic Apps", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[DEBUG] Processing Logic Apps section");
                // For Logic Apps, move TierOrRuntime to ExtraColumn as Environment and clear TierOrRuntime
                resource.ExtraColumn = resource.TierOrRuntime;
                resource.TierOrRuntime = "";
            }
            else if (sectionName.Equals("Azure Functions", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[DEBUG] Processing Azure Functions section");
                // For Azure Functions, TierOrRuntime is Runtime - keep as is, clear ExtraColumn
                resource.ExtraColumn = "";
            }
            else
            {
                Console.WriteLine($"[DEBUG] Processing other section: '{sectionName}'");
            }
        }
        else
        {
            Console.WriteLine($"[WARNING] Data row has only {dataRow.Count} columns, expected at least 9");
        }

        // Generate URL for Azure Portal (basic pattern)
        Console.WriteLine($"[DEBUG] Generating Azure Portal URL...");
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
        try
        {
            Console.WriteLine($"[DEBUG] GenerateAzurePortalUrl called:");
            Console.WriteLine($"  - Resource Name: '{resource?.Name ?? "NULL"}'");
            Console.WriteLine($"  - Resource Group: '{resource?.ResourceGroup ?? "NULL"}'");
            Console.WriteLine($"  - Section Name: '{sectionName ?? "NULL"}'");
            
            // Validate inputs
            if (resource == null)
            {
                Console.WriteLine($"[ERROR] Resource is null");
                return "#";
            }
            
            if (string.IsNullOrWhiteSpace(resource.Name))
            {
                Console.WriteLine($"[ERROR] Resource name is null or empty");
                return "#";
            }
            
            if (string.IsNullOrWhiteSpace(resource.ResourceGroup))
            {
                Console.WriteLine($"[ERROR] Resource group is null or empty");
                return "#";
            }
            
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                Console.WriteLine($"[ERROR] Section name is null or empty");
                return "#";
            }

            // Get resource type mapping
            var sectionLower = sectionName.ToLower();
            Console.WriteLine($"  - Section Name (lowercase): '{sectionLower}'");
            
            var resourceType = sectionLower switch
            {
                "api management" => "Microsoft.ApiManagement/service",
                "logic apps" => "Microsoft.Logic/workflows",
                "azure functions" => "Microsoft.Web/sites",
                "function app" => "Microsoft.Web/sites",
                "key vault" => "Microsoft.KeyVault/vaults",
                "app insights" => "Microsoft.Insights/components",
                "application insights" => "Microsoft.Insights/components",
                "cosmos db" => "Microsoft.DocumentDB/databaseAccounts",
                "app service" => "Microsoft.Web/sites",
                "app services" => "Microsoft.Web/sites",
                "storage account" => "Microsoft.Storage/storageAccounts",
                "storage" => "Microsoft.Storage/storageAccounts",
                "resource group" => "Microsoft.Resources/resourceGroups",
                "data factory v2" => "Microsoft.DataFactory/factories",
                "search service" => "Microsoft.Search/searchServices",
                _ => "Microsoft.Resources/resourceGroups"
            };
            
            Console.WriteLine($"  - Mapped Resource Type: '{resourceType}'");
            
            // Clean resource name (remove spaces and special characters that might cause URL issues)
            var cleanResourceName = resource.Name.Trim();
            var cleanResourceGroup = resource.ResourceGroup.Trim();
            
            Console.WriteLine($"  - Clean Resource Name: '{cleanResourceName}'");
            Console.WriteLine($"  - Clean Resource Group: '{cleanResourceGroup}'");

            // Get subscription ID based on service type
            var subscriptionId = GetSubscriptionIdForService(sectionLower, cleanResourceGroup);
            Console.WriteLine($"  - Subscription ID: '{subscriptionId}'");

            // Generate the Azure portal URL
            var portalUrl = $"https://portal.azure.com/#@{AzureTenantDomain}/resource/subscriptions/{subscriptionId}/resourceGroups/{cleanResourceGroup}/providers/{resourceType}/{cleanResourceName}/overview";
            
            Console.WriteLine($"  - Generated URL: '{portalUrl}'");
            Console.WriteLine($"[DEBUG] GenerateAzurePortalUrl completed successfully");
            
            return portalUrl;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Exception in GenerateAzurePortalUrl: {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            return "#";
        }
    }

    private string GetSubscriptionIdForService(string sectionName, string resourceGroup)
    {
        Console.WriteLine($"[DEBUG] GetSubscriptionIdForService called for section: '{sectionName}', resource group: '{resourceGroup}'");
        
        // Check if this is a Data Factory service
        if (sectionName.Contains("data factory"))
        {
            Console.WriteLine($"[DEBUG] Using Data Factory subscription ID: {DataFactorySubscriptionId}");
            return DataFactorySubscriptionId;
        }
        
        // Check by resource group patterns for Data Factory
        if (resourceGroup.ToLower().Contains("hcm") || 
            resourceGroup.ToLower().Contains("datafactory") || 
            resourceGroup.ToLower().Contains("adf"))
        {
            Console.WriteLine($"[DEBUG] Detected Data Factory resource group pattern, using Data Factory subscription ID: {DataFactorySubscriptionId}");
            return DataFactorySubscriptionId;
        }
        
        // For all other services, use default subscription
        Console.WriteLine($"[DEBUG] Using default subscription ID: {DefaultSubscriptionId}");
        return DefaultSubscriptionId;
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