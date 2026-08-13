# PostForge

Gestisci, pianifica e pubblica contenuti su più piattaforme social (Facebook, Instagram, TikTok, YouTube) con assistenza AI.

## Architettura

Clean Architecture a 4 livelli con CQRS, provider model e DDD tattico.

```
src/
├── PostForge.Domain/          Entità, Value Object, interfacce provider + contract (Domain.Providers)
├── PostForge.Application/     CQRS handlers, validators, query/command records
├── PostForge.Infrastructure/  Persistenza (ECO/EF Core), registry provider, messaggistica
├── PostForge.Providers.<Nome>/ Un progetto per ogni provider (Facebook, Instagram, TikTok, YouTube, OpenAI, Anthropic, GoogleGemini, MicrosoftFoundry, DallE, StableDiffusion)
├── PostForge.Api/             ASP.NET Core Web API
├── PostForge.Worker/          Worker per scheduling e pubblicazione (Quartz.NET)
└── web/                       Angular SPA
```

## Stack

| Layer | Scelta |
|---|---|
| Runtime | .NET 10 + C# 14 |
| API | ASP.NET Core Web API |
| CQRS | Mediator (martinothamar, source generator) |
| Persistenza | ECO + EF Core + SQL Server |
| Provider social | Meta Graph API, TikTok, YouTube |
| Provider AI | OpenAI, Anthropic, Google Gemini, Microsoft Foundry |
| Scheduling | Quartz.NET |
| Messaging | Azure Service Bus |
| Infrastruttura | Azure (Bicep + azd) |
| Frontend | Angular SPA |
| Auth | ASP.NET Core Identity |

## Prerequisiti

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (per test di integrazione)
- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli) (per deploy)

## Sviluppo

```bash
# build
dotnet build

# test unitari
dotnet test tests/PostForge.UnitTests

# test integrazione (richiede Docker)
dotnet test tests/PostForge.IntegrationTests
```

## Deploy

```bash
azd up
```

## Provider model

Aggiungere una nuova piattaforma social o un provider AI richiede un nuovo progetto `PostForge.Providers.<Nome>` che implementa l'interfaccia dal dominio — zero modifiche a Domain/Application.

- `ISocialPlatformProvider` → nuova piattaforma social (identificata dalla stringa `Identifier`, es. `FACEBOOK`)
- `IAiTextProvider` / `IAiImageProvider` → nuovo provider AI
- `IProviderRegistry<TProvider>` → registry per risolvere a runtime
- Interfacce e contract vivono in `PostForge.Domain.Providers`

## Roadmap

1. **Fase 0** — scheletro soluzione, CI, IaC base, auth (✅ completata)
2. **Fase 1** — provider Facebook/Instagram via Meta Graph API
3. **Fase 2** — scheduling avanzato + campagne
4. **Fase 3** — AI assist
5. **Fase 4** — TikTok + YouTube
6. **Fase 5** — extra (analytics, multi-tenant)

## Licenza

MIT
