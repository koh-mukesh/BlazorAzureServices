# Azure Services Dashboard - Configuration Guide

## Overview

The Azure Services Dashboard now supports configurable service definitions through the `settings.csv` file. This allows you to update service details without modifying the source code.

## Configuration File Format

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

The system generates Azure Portal URLs using a standard pattern:
```
https://portal.azure.com/#resource/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/{resourceType}/{serviceName}/overview
```

Note: You may need to update the subscription ID in the `ConfigurationService.cs` file for correct portal links.

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