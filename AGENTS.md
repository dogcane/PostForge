# AGENTS.md

## Stato attuale

PostForge è in **Fase 0 completata**. Soluzione .NET 10 con Clean Architecture, CQRS, ECO persistence, provider model, Angular SPA, IaC Bicep, CI/CD GitHub Actions. Leggi `postforge-project-brief.md` per il dominio, questo file per le convenzioni tecniche.

## Architettura

- **Clean Architecture** a 4 livelli: Domain → Application → Infrastructure → API/Worker.
- **CQRS con Mediator** (`Mediator.SourceGenerator` di martinothamar — source generator, `ValueTask<T>`, Singleton/Scoped lifetime).
- **Provider model** duale: `ISocialPlatformProvider` per piattaforme social, `IAiTextProvider`/`IAiImageProvider` per AI. Le interfacce e i contract vivono nel layer **Domain** (`PostForge.Domain.Providers`); ogni provider è un progetto separato `PostForge.Providers.<Nome>`. Le piattaforme sono identificate dalla stringa `Identifier` del provider (es. `FACEBOOK`), **non** da un enum: aggiungere una piattaforma non tocca il dominio.
- **DDD tattico**: Entity, ValueObject nel layer Domain. Domain events NON nel dominio: verranno modellati a livello Application.
- **Resilienza con Polly** (retry, circuit breaker, rate limiter) su ogni provider esterno.
- **Persistenza con ECO**: le entità ereditano da `AggregateRoot<Guid>` / `Entity<Guid>` di ECO; repository implementano `EntityFrameworkRepository<T, Guid>`; `IDataContext` come unit of work.

## Stack

| Layer | Scelta |
|---|---|
| Runtime | .NET 10 + C# 14 |
| API | ASP.NET Core Web API versionata |
| CQRS | **Mediator** (martinothamar) — source generator, `ValueTask<T>`, `IMessage` constraint |
| Persistenza | **ECO** (ECO.Core + ECO.Providers.EntityFramework + Npgsql) |
| Scheduling | Worker Service + Quartz.NET |
| Messaging | Azure Service Bus (o Storage Queue) |
| Secrets | Azure Key Vault + Managed Identity |
| Storage media | Azure Blob Storage |
| Frontend | Angular SPA |
| Auth | ASP.NET Core Identity |
| Test | xUnit, FluentAssertions, Testcontainers, WireMock.NET, ECO.Providers.InMemory |
| IaC | Bicep + `azd` |

## Stack (definito nel brief)

| Layer | Scelta |
|---|---|
| Runtime | .NET 10 + C# 14 |
| API | ASP.NET Core Web API versionata |
| Persistenza | **ECO** (ECO.Core + ECO.Providers.EntityFramework + Npgsql) |
| Scheduling | Worker Service + Quartz.NET |
| Messaging | Azure Service Bus (o Storage Queue) |
| Secrets | Azure Key Vault + Managed Identity |
| Storage media | Azure Blob Storage |
| Frontend | Angular SPA |
| Auth | ASP.NET Core Identity |
| Test | xUnit, FluentAssertions, Testcontainers, WireMock.NET, ECO.Providers.InMemory |
| IaC | Bicep + `azd` |

## Struttura soluzione

```
PostForge.slnx
src/
  PostForge.Domain/        Entities/, ValueObjects/, Providers/ (interfacce + contracts)
  PostForge.Application/   Posts/, Campaigns/, Scheduling/, Ai/
  PostForge.Infrastructure/ Messaging/ (sole interfacce: IPublishJobSender) — progetto cross tra Domain e Application
  PostForge.Infrastructure.DAL/  Persistence (PostForgeDbContext + repository implementations) + AddDataAccess
  PostForge.Infrastructure.Providers/  registries provider (social + AI) + AddProviderRegistries
  PostForge.Infrastructure.Messaging.ServiceBus/  ServiceBusPublishJobSender + AddServiceBusPublishJobSender
  PostForge.Infrastructure.Resilience/  ResiliencePolicies (Polly)
  PostForge.Api/                 composition root: AddInfrastructure = AddDataAccess + provider + AddProviderRegistries + AddServiceBusPublishJobSender
  PostForge.Worker/              registrazione DI locale (AddWorkerInfrastructure) che compone gli stessi moduli Infrastructure, senza ProjectReference verso PostForge.Api
  PostForge.Providers.<Nome>/    un progetto per ogni provider (Facebook, Instagram, TikTok, YouTube, OpenAI, Anthropic, GoogleGemini, MicrosoftFoundry, DallE, StableDiffusion). PostForge.Providers.Fake = fake social provider completo per test/dev (implementa tutta ISocialPlatformProvider)
  PostForge.Api/
  PostForge.Worker/
web/                       Angular SPA
tests/
  PostForge.Providers.<Nome>.Tests/  un progetto di test per ogni provider con test propri (es. PostForge.Providers.Facebook.Tests)
  PostForge.UnitTests/               Domain + Application + Infrastructure (registry provider, ecc.)
  PostForge.IntegrationTests/
infra/
  main.bicep
  azure.yaml
.github/workflows/
```

## Roadmap

1. **Fase 0** — scheletro soluzione, CI, IaC base, auth
2. **Fase 1** — provider Facebook/Instagram via Meta Graph API
3. **Fase 2** — scheduling avanzato + campagne
4. **Fase 3** — AI assist (provider model AI + chiavi in Key Vault)
5. **Fase 4** — TikTok + YouTube provider
6. **Fase 5** — extra (analytics, multi-tenant opzionale)

## Convenzioni

- Provider model: nuova piattaforma/AI provider = nuovo progetto `PostForge.Providers.<Nome>` che implementa l'interfaccia dal Domain (con il proprio extension method DI), zero modifiche a Domain/Application. Piattaforme identificate da `ISocialPlatformProvider.Identifier`.
- Ogni provider esterno ha rate limit e SLA propri → Polly per retry, circuit breaker, timeout dedicati.
- Token OAuth e chiavi AI mai in chiaro → Key Vault + Managed Identity.
- Provider esterni mockabili nei test (WireMock.NET).

## Persistenza e Migrations (EF Core)

**Regola obbligatoria: ogni modifica al DB richiede una migration. Mai usare `EnsureCreated` per evolvere lo schema in produzione/dev persistente.**

- Due `DbContext` sullo stesso database (`PostForgeDb`): `PostForgeDbContext` (DAL — `Posts`, `Campaigns`, `ScheduleSlots`, `MediaAssets`, `PostTags`, `SocialAccounts`, `ProviderCredentials`, `Tenants`, `TenantMemberships`) e `AppIdentityDbContext` (Identity — `AspNet*`, `RefreshTokens`). Condividono la stessa `__EFMigrationsHistory`; gli `Id` migration non collidono (timestamp diversi).
- Flusso per **qualsiasi** modifica a entità / `OnModelCreating` / `ValueConverter` / `OwnsMany`:
  1. Modifica entity + `PostForgeDbContext.cs:21` / `AppIdentityDbContext.cs:12`.
  2. Genera migration con `dotnet ef` (richiede `Microsoft.EntityFrameworkCore.Design` + `DesignTime*Factory`):
     ```powershell
     # Business — PostForgeDbContext
     dotnet ef migrations add <NomeDescrittivo> --context PostForgeDbContext --project src/PostForge.Infrastructure.DAL --startup-project src/PostForge.Api --output-dir Migrations/PostForgeDb
     # Identity — AppIdentityDbContext (solo se tocchi AspNet*/RefreshTokens)
     dotnet ef migrations add <NomeDescrittivo> --context AppIdentityDbContext --project src/PostForge.Infrastructure.Identity --startup-project src/PostForge.Api --output-dir Migrations/AppIdentityDb
     ```
     Le factory design-time (`PostForge.Infrastructure.DAL/DesignTimePostForgeDbContextFactory.cs:1`, `PostForge.Infrastructure.Identity/DesignTimeAppIdentityDbContextFactory.cs:1`) leggono `ConnectionStrings:PostForgeDb` da `src/PostForge.Api/appsettings*.json` con fallback `Host=localhost/...`.
  3. Verifica `dotnet build` e, se hai un DB locale, `dotnet ef database update --context PostForgeDbContext` / `--context AppIdentityDbContext` con `--connection "Host=...;Database=PostForgeDb_Dev;..."` (dev usa `Host=172.24.224.193` via podman `post-postgres` su `0.0.0.0:5432`).
  4. Committa la cartella `Migrations/` generata.
- Startup applica le pending in modo idempotente:
  - `PostForge.Infrastructure.DAL/DependencyInjection.cs:77` `EnsurePostForgeDatabaseAsync()` → `await context.Database.MigrateAsync()` (unico punto di creazione/evoluzione per `PostForgeDbContext`).
  - `PostForge.Infrastructure.Identity/SuperUserSeeder.cs:19` analogo per `AppIdentityDbContext` + seeding superuser.
  - Chiamati in `PostForge.Api/Program.cs:70` e `PostForge.Worker/Program.cs:14` **prima** di qualsiasi query. Nessun check `42P01`/`42P07` su `ScheduleSlots` o altre tabelle — lo schema evolve solo via migration.
- **Vietato:** `EnsureCreated`/`EnsureDeleted` come meccanismo di migrazione, `ExecuteSqlRaw("SELECT 1 FROM \"ScheduleSlots\"")` o catch su `PostgresException` per dedurre schema. `EnsureCreated` è solo per test in-memory/Testcontainers (`PostForge.IntegrationTests/Infrastructure/DbContextTests.cs:33`, `PostForge.IntegrationTests/Api/PostForgeWebApplicationFactory.cs:54`); mai in `Api`/`Worker` su DB persistente. Se un DB locale è stale (creato prima delle migrations), resettarlo con `DROP DATABASE ... WITH (FORCE); CREATE DATABASE ...` e poi `MigrateAsync`, non con logica `if (IsDevelopment) EnsureDeleted`.

## Convenzioni C# (.NET 10 / C# 14)

Target `net10.0` → C# 14 di default (niente `<LangVersion>` esplicito). Usare attivamente le feature moderne, niente codice "legacy" se esiste l'equivalente nuovo:

- **Primary constructors (obbligatorio)** — usare **sempre** il primary constructor C# 12/14 per **ogni** classe con dipendenze iniettate o forwarding al `base(...)` quando il linguaggio lo consente. Vale per **tutti** i progetti/layer (Domain escluso per le entity, vedi sotto): handlers, services, provider, registry, **repository (`CampaignRepository`, `PostRepository`, `TenantRepository`, ecc. + `TenantScopedRepository` dove possibile — vedi eccezione `this` sotto)**, `DbContext`, controller, middleware, `IHostedService`, `BackgroundService`, sender/messaging, ecc. È **vietato** introdurre nuovo codice con il pattern `private readonly + ctor { _field = param; }` o `Ctor(...) : base(...) { }` a corpo vuoto quando esiste l'equivalente primary constructor. Quando il costruttore contiene logica che **non accede a `this`** (es. `new Client(http, options.Value)`), convertirla in inizializzatore di campo. Quando la logica **accede a `this`** (es. `TenantScopedRepository` che imposta `DbContext.CurrentTenantId` dopo `base(dataContext)`), gli inizializzatori di campo non possono referenziare `this` (`CS0236`): mantenere il costruttore tradizionale — è l'unica eccezione ammessa oltre alle entity:

```csharp
// Repository — forwarding al base
public sealed class CampaignRepository(IDataContext dataContext, ITenantContext tenantContext)
    : TenantScopedRepository<Campaign, Guid>(dataContext, tenantContext), ICampaignRepository;

// DbContext
public class PostForgeDbContext(DbContextOptions<PostForgeDbContext> options) : DbContext(options)

// Controller — usa il parametro direttamente, niente campo _mediator
public class AiController(IMediator mediator) : ControllerBase

// Middleware / HostedService
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IMiddleware
public sealed class QuartzHostedService(IScheduler scheduler, ILogger<QuartzHostedService> logger) : IHostedService

// Registry / messaging
public class AiTextProviderRegistry(IEnumerable<IAiTextProvider> providers) : IProviderRegistry<IAiTextProvider>
{
    private readonly Dictionary<string, IAiTextProvider> _providers = providers.ToDictionary(p => p.ProviderKey, StringComparer.OrdinalIgnoreCase);
}
public class ServiceBusPublishJobSender(ServiceBusSender sender) : IPublishJobSender

// Provider con inizializzazione derivata
public class FacebookProvider(HttpClient httpClient, IOptions<FacebookProviderOptions> options) : ISocialPlatformProvider
{
    private readonly FacebookProviderOptions _options = options.Value;
    private readonly FacebookGraphApiClient _client = new(httpClient, options.Value);
}
```

- **`field` keyword (C# 14)** per semi-auto properties quando serve un backing field con logica nel getter/setter — niente campo privato + property separati se `field` basta.
- **Default interface implementations** per i membri opzionali delle interfacce provider (già in `ISocialPlatformProvider`): un provider implementa solo ciò che supporta davvero; il resto eredita il default `throw new NotSupportedException(...)`. Non rendere astratti membri che ogni provider dovrebbe stub-are.
- **Records / positional records** per contract e DTO (`record OAuthTokens(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc)`); `record struct` per i value type; `init` + `required` per i modelli immutabili.
- **Collection expressions** `[]` ovunque: `List<MediaAsset> = []`, `?? []`, spread `[.. source]`, `params` collections per metodi variadici. Niente `new List<T>()` / `ToArray()` boilerplate.
- **Target-typed `new`** + pattern matching avanzato (switch expression, property pattern, list pattern) e raw string literals `"""` per payload di test.
- **ImplicitUsings + Nullable** abilitati in ogni csproj.
- **Eccezione DDD**: le Entity di dominio restano con costruttori `private` + factory `Create()` che restituisce `OperationResult<T>` (vedi sotto) — primary constructor NON si applica alle entity, perché la costruzione passa dalla validazione Resulz. Si applica a tutto il resto.

## Stile entità e ValueObject (allineato a `Campaign.cs`)

Tutte le entità (`Entities/`) e i ValueObject con validazione usano lo stesso stile di `src/PostForge.Domain/Entities/Campaign.cs:1`:

- **Regions obbligatori**: `#region Fields`, `#region Properties`, `#region ctor`, `#region Methods` in quest'ordine. Anche se una region è vuota va lasciata (es. `Tenant.cs:10`).
- **Metodi che restituiscono `OperationResult` in modo fluent/expressive**:
  - Estrarre `protected static OperationResult Validate(...)` che accumula le regole con `Resulz.Validation` (`Campaign.cs:45`).
  - `Create` come expression body: `=> Validate(...).IfSuccessThenReturn<TEntity>(() => new Entity(...))` (`Campaign.cs:58`).
  - Mutazioni semplici: `=> Validate(...).IfSuccess(_ => { /* assign */ })` (`Campaign.cs:63`).
  - Validazioni inline: `=> OperationResult.MakeSuccess().With(value, "Ctx").Required()... .Result.IfSuccess(_ => { /* mutate */ })` (`Campaign.cs:67`). Usare `.Result` per ottenere `OperationResult` da `ValueChecker<T>` prima di `IfSuccess`.
  - Guard con errore custom + expression body: `=> condition ? MakeFailure(...) : MakeSuccess().IfSuccess(...)` (es. `Tenant.cs:42`, `ScheduleSlot.cs:52`).
- **Expression bodies quando possibile**: preferire `=>` per `Create`, `Validate` quando breve, mutazioni a singola espressione e property `Id => Identity`, `CanRetry => ...`. Mantenere block body solo quando il flusso richiede `foreach`/`if` multipli non esprimibili fluentmente (es. `Post.cs:128`).

## Domain Events

I domain events NON vivono nel layer Domain: le entità non espongono storage di eventi. Gli eventi saranno modellati a livello **Application** (da definire, ad es. tramite `Mediator`).

## Result pattern (Resulz)

Tutte le entità di dominio usano il Result pattern via **Resulz 1.6.0** (`Dogcane.Resulz`):

- **Costruttori**: `private`. Factory method statico `Create(…) → OperationResult<TEntity>`.
- **Mutation methods**: restituiscono `OperationResult` (non `void`), con validazioni interne.
- **API Resulz**:
  - `OperationResult.MakeSuccess()` / `OperationResult.MakeFailure(ErrorMessage)`
  - `OperationResult<T>.MakeSuccess(T value)` / `OperationResult<T>.MakeFailure(ErrorMessage)`
  - `ErrorMessage.Create(context, description)`
  - Controllo: `result.Success`, `result.Errors`, `result.Value` (solo su `OperationResult<T>`)
  - **NON** usare l'operatore `!` (implicit true/female) su `OperationResult<T>` — non esiste. Usare `!result.Success`.
- **Handler code**: dopo `Create(...)` o mutation, verificare `result.Success` e chiamare `Fail(…)` con errori concatenati.

```csharp
var post = Post.Create(text, campaignId);
if (!post.Success)
    return Result.Fail<Guid>(post.Errors.Select(e => e.Description).ToList());
```

### Validation fluent (Resulz.Validation)

Nei factory method `Create()` usare il fluent validation API di `Resulz.Validation` invece di `if` manuali:

```csharp
using Resulz.Validation;

public static OperationResult<Post> Create(string text, Guid? campaignId = null)
{
    var result = OperationResult.MakeSuccess();
    result.With(text, "Text").Required().StringLength(5000);
    if (!result.Success)
        return result;
    return OperationResult<Post>.MakeSuccess(new Post(text, campaignId));
}
```

**API Validators disponibili:**

| Metodo | Descrizione | Default error |
|---|---|---|
| `.Required()` | String: not null/whitespace. Altri: not null | `{CONTEXT}_REQUIRED` |
| `.StringLength(max)` | String max length | `{CONTEXT}_TOO_LONG` |
| `.Condition(predicate)` | Custom predicate (true = valid) | `{CONTEXT}_CONDITION_FAILED` |
| `.Email()` | Valid email format | `{CONTEXT}_NOT_EMAIL` |
| `.GreaterThen(min)` | `IComparable<T>`: value > min | `{CONTEXT}_NOT_GREATER` |
| `.GreaterThenOrEqual(min)` | value >= min | `{CONTEXT}_NOT_GREATER_OR_EQUAL` |
| `.LessThen(max)` | value < max | `{CONTEXT}_NOT_LESS` |
| `.LessThenOrEqual(max)` | value <= max | `{CONTEXT}_NOT_LESS_OR_EQUAL` |
| `.Between(min, max)` | min < value < max | `{CONTEXT}_NOT_BETWEEN` |
| `.Into(array)` | Value in set | `{CONTEXT}_NOT_INTO` |
| `.EqualTo(value)` | Equality check | `{CONTEXT}_NOT_EQUAL` |
| `.StringMatch(regex)` | Regex match | `{CONTEXT}_NOT_MATCHED` |

Per validazioni custom (es. `Enum.IsDefined`, `DateTime.Kind`, date ordering) usare `.Condition()`:

```csharp
result.With(status, "Status").Condition(v => Enum.IsDefined(typeof(PostStatus), v));
result.With(scheduledAtUtc, "ScheduledAt").Condition(v => v.Kind == DateTimeKind.Utc);
```

`ValueChecker<T>` ha conversione implicita a `OperationResult`. Il `result` originale viene mutato ad ogni `.With()`. Usare `if (!result.Success) return result;` per early exit con conversione implicita `OperationResult → OperationResult<T>` (funziona solo su failure).
