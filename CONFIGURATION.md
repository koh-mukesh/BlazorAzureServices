# Azure Services Dashboard - Configuration Guide

## Overview

The Azure Services Dashboard supports configurable service definitions through the `settings.csv` file and Azure subscription configuration through `appsettings.json`. This allows you to update service details and Azure subscriptions without modifying the source code.

## Azure Subscription Configuration

### Setup
Azure subscription settings are configured in `appsettings.json` under the `Azure` section:

```json
{
  "Azure": {
    "TenantDomain": "yourcompany.onmicrosoft.com",
    "Subscriptions": {
      "Default": {
        "SubscriptionId": "35166c0d-3c12-4f4c-8638-79d7248ae93f",
        "Name": "CIT-INF",
        "Description": "Corporate IT Infrastructure - Development and testing"
      },
      "Production": {
        "SubscriptionId": "e884a250-7177-4a12-9397-4032aa0f8070",
        "Name": "DefaultWL-Prod-AMER",
        "Description": "Production workloads in Americas region"
      },
      "NonProduction": {
        "SubscriptionId": "e4e37c2d-09d5-4584-9b5b-e4d389d8cd1b",
        "Name": "DefaultWL-NonProd-AMER",
        "Description": "Non-production workloads in Americas region"
      },
      "Communications": {
        "SubscriptionId": "f730bd52-8a69-47b8-9b73-9a36eafa7293",
        "Name": "Corporate Communications - 1",
        "Description": "Corporate communications and marketing services"
      },
      "IoT": {
        "SubscriptionId": "d8ed5f39-3878-4ac5-887b-643deb70777f",
        "Name": "Global IoT",
        "Description": "Internet of Things and connected device services"
      },
      "Dynamics": {
        "SubscriptionId": "21facf0c-3f51-4adc-ac4d-f37eff234c2a",
        "Name": "KCIC-NonProd",
        "Description": "Kohler Customer Intelligence Center - Non-production"
      }
    },
    "ServiceMapping": {
      "Production": ["data factory", "adf", "prod"],
      "NonProduction": ["test", "dev", "staging", "uat"],
      "Communications": ["communications", "marketing", "web", "cms"],
      "IoT": ["iot", "device", "sensor", "telemetry", "edge"],
      "Dynamics": ["dynamics", "crm", "erp", "kcic", "customer"],
      "Default": ["*"]
    },
    "ResourceGroupMapping": {
      "Production": ["prod", "production", "prd", "hcm", "datafactory", "adf", "amer", "sdmp"],
      "NonProduction": ["dev", "test", "staging", "uat", "nonprod", "sandbox"],
      "Communications": ["comm", "marketing", "web", "cms", "portal"],
      "IoT": ["iot", "device", "sensor", "telemetry", "edge", "connected"],
      "Dynamics": ["dynamics", "crm", "erp", "kcic", "customer", "d365"],
      "Default": ["*"]
    }
  }
}
```

### Subscription Routing
The system automatically routes services to subscriptions based on:
1. **Service Type Patterns**: Matches service names against configured patterns
2. **Resource Group Patterns**: Matches resource group names against configured patterns
3. **Fallback**: Uses the default subscription if no patterns match

### Management Interface
Access the subscription management interface at `/subscriptions` to:
- View all configured subscriptions
- Test subscription routing for specific services
- Understand service and resource group mapping rules

## Service Configuration (CSV)

The `settings.csv` file follows a standard CSV (Comma-Separated Values) format:

### Structure
1. **First line**: Header row with column names (comma-separated)
2. **Subsequent lines**: Service data (comma-separated values)
3. **Section column**: First column indicates the service section/category

### Example Format

```csv
Section,Service Name,Type,Resource Group,Location,Tier,Dev Portal,Status,Actions,Environment,Runtime
API Management,api-kohler-dev2 dev,API Management,GLOBAL-API-Management-DEV-rg,Central US,Developer Tier,Open Portal,Online,View,,
API Management,api-kohler-test test,API Management,GLOBAL-API-Management-TEST-rg,East US,Developer Tier,Open Portal,Online,View,,
Logic Apps,APIM-Extension-Logic-App prod,Logic App,GLOBAL-API-Management-PROD-rg,Central US,,,Enabled,View,Production,
Azure Functions,apim-services-func-dev dev,Function App,global-api-management-dev-rg,Central US,,,Running,View,,.NET 6
```

## Column Mappings

The parser automatically maps common column headers to service properties:

- **Service Name**: Maps to service name and extracts environment tags
- **Type**: Service type (API Management, Logic App, etc.)
- **Environment**: Environment tag (dev, test, prod, etc.)
- **Resource Group**: Azure resource group name
- **Location**: Azure region
- **Tier/Runtime**: Service tier or runtime version
- **Dev Portal**: Special handling for API Management portal links
- **Status**: Service status (Online, Enabled, Running, etc.)
- **Actions**: UI action buttons (handled automatically)

## Environment Tags

The system automatically detects and extracts environment tags from service names:
- Supported tags: `dev`, `test`, `prod`, `production`, `staging`, `uat`, `qa`
- Example: "api-kohler-dev2 dev" → Name: "api-kohler-dev2", Tag: "dev"

## Service Icons

Icons are automatically assigned based on service type:
- 🔌 API Management
- ⚡ Logic Apps
- ⚡ Azure Functions
- 🔐 Key Vault
- 📊 Application Insights
- 🌍 Cosmos DB
- 🌐 App Services
- 💾 Storage
- 📁 Resource Groups
- 🏭 Data Factory
- 🔍 Cognitive Search

## Adding New Services

To add new services:

1. Open `settings.csv` (or `wwwroot/settings.csv`)
2. Add new rows with:
   - Section name in first column
   - Service data in subsequent columns
3. Save the file
4. Refresh the application

## File Location

The configuration file must be placed in:
- **Source**: `settings.csv` (root directory)
- **Runtime**: `wwwroot/settings.csv` (automatically copied during build)

## Error Handling

If the configuration file fails to load:
- The application shows a fallback with default API Management services
- Error messages are displayed in the UI
- Console logs provide debugging information

## Azure Portal URLs

The system automatically generates Azure Portal URLs using the configured subscriptions:
```
https://portal.azure.com/#@{tenantDomain}/resource/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/{resourceType}/{serviceName}/overview
```

The subscription ID is automatically determined based on:
- Service type matching (e.g., Data Factory services → DataFactory subscription)
- Resource group pattern matching (e.g., "hcm-*" → DataFactory subscription)
- Fallback to default subscription

No manual subscription ID updates are needed in the code.

## Best Practices

- **Use commas**: Ensure columns are separated by comma characters
2. **Consistent headers**: Use standard column names for automatic mapping
3. **Environment tags**: Include environment indicators in service names
4. **Test format**: Validate the format by checking the application after changes
5. **Backup**: Keep a backup of your configuration file

## Troubleshooting

- **No services shown**: Check if `wwwroot/settings.csv` exists
- **Parsing errors**: Ensure proper CSV format and comma separation
- **Missing columns**: Verify column headers match expected names
- **Console logs**: Check browser developer tools for error messages

## Future Enhancements

The configuration system can be extended to support:
- JSON format
- Multiple configuration files
- Remote configuration sources
- Dynamic reloading
- Custom column mappings