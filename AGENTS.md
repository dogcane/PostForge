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
| Persistenza | **ECO** (ECO.Core + ECO.Providers.EntityFramework.SqlServer) |
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
| Persistenza | **ECO** (ECO.Core + ECO.Providers.EntityFramework.SqlServer) |
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
  PostForge.Infrastructure/ Messaging/, Resilience/, registries provider
  PostForge.Infrastructure.DAL/  Persistence (PostForgeDbContext + repository implementations)
  PostForge.Providers.<Nome>/    un progetto per ogni provider (Facebook, Instagram, TikTok, YouTube, OpenAI, Anthropic, GoogleGemini, MicrosoftFoundry, DallE, StableDiffusion)
  PostForge.Api/
  PostForge.Worker/
web/                       Angular SPA
tests/
  PostForge.UnitTests/
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
