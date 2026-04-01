using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorAzureServices;
using BlazorAzureServices.Services;
using BlazorAzureServices.Components;
using BlazorAzureServices.Configuration;
using Microsoft.Extensions.Options;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Configure Azure settings
var azureConfig = new AzureConfiguration();
builder.Configuration.GetSection(AzureConfiguration.SectionName).Bind(azureConfig);
builder.Services.AddSingleton(azureConfig);
builder.Services.AddSingleton<IOptions<AzureConfiguration>>(sp => 
    Microsoft.Extensions.Options.Options.Create(azureConfig));

builder.Services.AddScoped<ConfigurationService>();

await builder.Build().RunAsync();
