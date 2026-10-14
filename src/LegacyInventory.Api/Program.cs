using LegacyInventory.Core.Data;
using LegacyInventory.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// Configuration and logging are automatically loaded from appsettings.json and environment variables.

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        // Match legacy WebApiConfig behavior: camelCase and JSON-only.
        options.SerializerSettings.ContractResolver =
            new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
    });

// IHttpContextAccessor for controllers/services that need HTTP context.
builder.Services.AddHttpContextAccessor();

// Options classes for strongly-typed configuration.
builder.Services.Configure<InventoryDbOptions>(
    builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<CacheOptions>(
    builder.Configuration.GetSection("Cache"));
builder.Services.Configure<PriceFeedOptions>(
    builder.Configuration.GetSection("PriceFeed"));

// HttpClient for PriceFeedClient via DI.
builder.Services.AddHttpClient<PriceFeedClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<PriceFeedOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// Repository and cache services.
builder.Services.AddScoped<ProductRepository>(sp =>
{
    var dbOptions = sp.GetRequiredService<IOptions<InventoryDbOptions>>().Value;
    return new ProductRepository(dbOptions.InventoryDb);
});

builder.Services.AddScoped<ProductCache>(sp =>
{
    var cacheOptions = sp.GetRequiredService<IOptions<CacheOptions>>().Value;
    // Keep the temp-path cache file but allow overriding in the future.
    var minutes = cacheOptions.Minutes;
    var cacheFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
        "legacy-inventory-products.json");
    return new ProductCache(cacheFile, TimeSpan.FromMinutes(minutes));
});

// InventoryService with DI-friendly constructor.
builder.Services.AddScoped<InventoryService>();

// Audit context using IHttpContextAccessor; InventoryService/AuditLogger can depend on it.
builder.Services.AddScoped<IAuditContext>(sp =>
{
    var accessor = sp.GetRequiredService<IHttpContextAccessor>();
    var userName = accessor.HttpContext?.User?.Identity?.IsAuthenticated == true
        ? accessor.HttpContext.User.Identity.Name
        : "anonymous";
    return new AuditContext(userName);
});

// Configure logging from appsettings.json (already wired by the host).

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Options types and audit context implementation used by DI.

public class InventoryDbOptions
{
    public string InventoryDb { get; set; } = string.Empty;
}

public class CacheOptions
{
    public int Minutes { get; set; } = 10;
}

public class PriceFeedOptions
{
    public string BaseUrl { get; set; } = string.Empty;
}

public interface IAuditContext
{
    string UserName { get; }
}

public class AuditContext : IAuditContext
{
    public AuditContext(string userName)
    {
        UserName = userName;
    }

    public string UserName { get; }
}
