# Legacy Inventory (.NET Framework 4.8): migration test fixture

A small, intentionally dated .NET Framework 4.8 solution. It is the test input for
the AAVA **Legacy .NET Migration** agentic process. It is not a production
application: the database, price feed and credentials are fake.

> The repository name says "Java", but the content is .NET on purpose. The AAVA
> process under test migrates .NET Framework to modern .NET.

## Layout

```
LegacyInventory.sln
src/LegacyInventory.Core/    class library: models, ADO.NET repository, services
src/LegacyInventory.Api/     ASP.NET Web API 2 app (Global.asax, Web.config)
tests/LegacyInventory.Tests/ MSTest unit tests
```

All three projects use the classic (non-SDK) csproj format and `packages.config`.

## Legacy patterns included on purpose

A migration to modern .NET (for example `net10.0`) should find and handle each of these:

| Area | Where | Expected migration action |
|---|---|---|
| Classic csproj + `packages.config` | all projects | Convert to SDK-style, `PackageReference` |
| ASP.NET Web API 2 on System.Web | `LegacyInventory.Api` | ASP.NET Core controllers, `Microsoft.NET.Sdk.Web` |
| `Global.asax` / `Application_Start` | `Global.asax.cs` | Minimal hosting `Program.cs` |
| `Web.config` connection strings and appSettings | `Web.config` | `appsettings.json` + `IConfiguration`; password moved to a secret |
| `ConfigurationManager` | `ProductRepository`, `PriceFeedClient`, `InventoryService` | Options pattern / `IConfiguration` |
| `System.Data.SqlClient` | `ProductRepository` | `Microsoft.Data.SqlClient` |
| `WebClient` (obsolete) | `PriceFeedClient` | `HttpClient` via `IHttpClientFactory` |
| `BinaryFormatter` (removed / unsafe) | `ProductCache` | `System.Text.Json` (or another safe serializer) |
| `HttpContext.Current` in a class library | `AuditLogger`, `ProductsController` | `IHttpContextAccessor` via DI |
| `new` up dependencies inside controllers | `ProductsController` | Constructor injection |
| log4net 2.0.8 configured from Web.config | `Global.asax.cs`, `Web.config` | Upgrade log4net or move to `Microsoft.Extensions.Logging` |
| Newtonsoft.Json 6.0.4 | Core, Api | Upgrade to 13.x or switch to `System.Text.Json` |
| MSTest 1.3.2 | tests | Current MSTest packages |
| `AssemblyInfo.cs` | all projects | Remove or set `GenerateAssemblyInfo=false` |

Known advisories on pinned packages (for the dependency analysis to flag):
Newtonsoft.Json below 13.0.1 (GHSA-5crp-9r3c-p9vr) and log4net below 2.0.10
(CVE-2018-1285).

## Using it with the AAVA process

Start inputs:

| Input | Value |
|---|---|
| repo | `https://github.com/jkloss-ascendion/Legacy-Java-Test` |
| source_branch | `main` |
| target_framework | `net10.0` |
| migration_branch | `migration/net10-inventory` |
| run_id | any unique id, e.g. `inventory-001` |
| scope_paths | leave blank |

The process commits only to the migration branch. `main` stays as the legacy baseline.
