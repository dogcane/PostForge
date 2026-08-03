# PostForge — Project brief

## 1. Executive summary

Applicativo open source in .NET per gestire, pianificare e pubblicare contenuti su più piattaforme social (Facebook, Instagram, TikTok, YouTube) attraverso un **provider model** per le piattaforme e uno per i provider AI. Include gestione campagne, schedulazione avanzata e assistenza AI alla creazione dei contenuti (copy e immagini), con un modello "bring your own key" per i provider AI. Hosting nativo su Azure.

## 2. Perché questo progetto

- **Palestra tecnica**: è pensato per rimettere mano, in modo strutturato, a un progetto .NET moderno completo — Clean Architecture, DDD tattico, CQRS, resilienza, IaC, CI/CD — non solo un tool utilitario.
- **Uso reale**: risolve un problema concreto di gestione multi-piattaforma dei propri contenuti social.
- **Portfolio**: essendo open source e ben documentato, può funzionare anche da caso di studio da mostrare in ottica di consulenza/freelance.

## 3. Obiettivi

- Un solo posto per creare, pianificare e pubblicare post su Facebook, Instagram, TikTok, YouTube.
- Estendibilità: aggiungere una nuova piattaforma o un nuovo provider AI senza toccare il dominio o l'application layer.
- Gestione campagne: raggruppare post sotto un obiettivo (es. awareness, reputazione, lead generation) e un canale (organico vs paid).
- Schedulazione avanzata: calendario editoriale, post ricorrenti, gestione fusi orari, retry automatici sui fallimenti di pubblicazione.
- Assistenza AI: generazione/miglioramento di copy e generazione immagini, con provider scelto e chiave configurata dall'utente.
- Deploy riproducibile su Azure via infrastructure-as-code.

## 4. Non-goal (per la v1)

- Analytics avanzate cross-piattaforma (si parte con gli insight nativi esposti da ciascuna piattaforma, se disponibili).
- Multi-tenancy reale/SaaS (l'architettura la deve rendere possibile in futuro, ma la v1 è single-tenant).
- Editor video o generazione video AI.

## 5. Glossario di dominio

| Termine | Significato |
|---|---|
| `Post` | Un contenuto (testo + media) destinato a una o più piattaforme |
| `Campaign` | Un raggruppamento di post con un obiettivo e un intervallo temporale |
| `ScheduleSlot` / `PublishJob` | L'istanza di pubblicazione di un post su una specifica piattaforma a un dato orario |
| `SocialAccount` | Un account collegato (con token OAuth) su una piattaforma |
| `MediaAsset` | Immagine/video allegato a un post, caricato o generato da AI |
| `ProviderCredential` | Chiave/config di un provider AI o social, gestita in modo sicuro |

## 6. Requisiti funzionali

**Account e provider**
- Collegamento account social via OAuth per ciascuna piattaforma.
- Refresh automatico dei token in scadenza.
- Configurazione chiavi AI personali per provider (testo e immagini), validate al salvataggio.

**Contenuti e campagne**
- Creazione post con testo, media, piattaforme target.
- Adattamento automatico dei vincoli per piattaforma (lunghezza testo, formati media, aspect ratio).
- Campagne con obiettivo (awareness / reputazione / lead generation) e canale (organico / paid), con post associati.

**Schedulazione e pubblicazione**
- Calendario editoriale con vista mensile/settimanale.
- Post ricorrenti (es. rubrica settimanale).
- Coda di pubblicazione con stato (bozza → pronto → schedulato → in pubblicazione → pubblicato / fallito), retry con backoff sui fallimenti.

**AI assist**
- Generazione/riscrittura di caption a partire da un brief, con tono e vincoli di piattaforma.
- Generazione immagini da prompt testuale.
- Scelta del provider AI (testo e immagini indipendenti) a runtime.

## 7. Requisiti non funzionali

- **Estendibilità**: nuovo provider social o AI = nuova classe che implementa un'interfaccia, zero modifiche al dominio.
- **Resilienza**: ogni provider esterno ha rate limit e SLA propri → retry, circuit breaker, timeout dedicati (Polly).
- **Sicurezza**: token OAuth e chiavi AI mai in chiaro (Key Vault + Managed Identity).
- **Osservabilità**: tracciare ogni tentativo di pubblicazione end-to-end (utile per debug su API di terze parti spesso poco documentate).
- **Testabilità**: i provider esterni devono essere mockabili nei test senza chiamare le API reali.

## 8. Architettura

Clean Architecture a livelli, con il provider model concentrato nell'Infrastructure layer (vedi diagramma sopra nella conversazione):

- **API & scheduling host** — ASP.NET Core Web API + un worker per la pubblicazione/schedulazione.
- **Application** — casi d'uso in CQRS (MediatR): comandi come `SchedulePostCommand`, `PublishPostCommand`, `GenerateCaptionCommand`.
- **Domain** — entità e regole: `Post`, `Campaign`, `ScheduleSlot`, `MediaAsset`. Eventi di dominio modellati a livello Application (es. `PostPublishedEvent`).
- **Infrastructure** — due famiglie di provider intercambiabili dietro interfacce comuni: provider social e provider AI, più persistenza e messaging.

## 9. Provider model — social

```csharp
public interface ISocialPlatformProvider
{
    SocialPlatform Platform { get; }
    Task<OAuthTokens> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct);
    Task<OAuthTokens> RefreshTokenAsync(OAuthTokens tokens, CancellationToken ct);
    Task<PublishResult> PublishAsync(PostContent content, OAuthTokens tokens, CancellationToken ct);
    Task<PostInsights?> GetInsightsAsync(string externalPostId, OAuthTokens tokens, CancellationToken ct);
}
```

Implementazioni previste: `FacebookProvider` e `InstagramProvider` (Meta Graph API — terreno già noto), `TikTokProvider` (Content Posting API), `YouTubeProvider` (YouTube Data API v3, upload resumable per i video).

## 10. Provider model — AI

```csharp
public interface IAiTextProvider
{
    string ProviderKey { get; }
    Task<string> GenerateCaptionAsync(CaptionRequest request, CancellationToken ct);
}

public interface IAiImageProvider
{
    string ProviderKey { get; }
    Task<GeneratedImage> GenerateImageAsync(ImageRequest request, CancellationToken ct);
}

public interface IProviderRegistry<TProvider>
{
    TProvider Resolve(string providerKey);
    IReadOnlyCollection<string> AvailableProviderKeys { get; }
}
```

Stesso principio già validato in altri contesti (registry/factory + interfaccia comune + selezione a runtime, con le chiavi gestite in modo sicuro per singolo provider): qui si applica a testo e immagini invece che a voce. Provider candidati: Microsoft Foundry (ex Azure OpenAI Service, comodo come opzione "nativa" Azure), OpenAI, Anthropic, Google Gemini, OpenRouter.

## 11. Modello dati (entità principali)

`SocialAccount(Id, Platform, DisplayName, OAuthTokens)`
`Post(Id, Text, MediaAssets[], TargetPlatforms[], CampaignId?, Status)`
`Campaign(Id, Name, Goal[Awareness|Reputation|LeadGen], Channel[Organic|Paid], DateRange)`
`ScheduleSlot(Id, PostId, Platform, ScheduledAtUtc, Status, RetryCount)`
`MediaAsset(Id, BlobUri, Type, GeneratedByAi bool, SourcePrompt?)`
`ProviderCredential(Id, ProviderKey, Scope[Social|AiText|AiImage], KeyVaultReference)`

## 12. Ciclo di vita di una pubblicazione

```
Draft → Ready → Scheduled → Publishing → Published
                              ↘ Failed → (retry) → Publishing
```

## 13. Stack tecnologico proposto

| Livello | Scelta | Perché |
|---|---|---|
| Runtime | .NET 10 (LTS), C# 14 | ultima LTS, supporto fino a nov 2028 |
| API | ASP.NET Core Web API, versionata | |
| CQRS | MediatR + FluentValidation | pattern già nelle tue corde |
| Persistenza | EF Core 10 + Azure SQL Database | |
| Scheduling/publish | Azure Functions (Timer + Durable Functions) oppure Worker Service + Quartz.NET | le Durable Functions sono una buona palestra di orchestrazione stateful |
| Resilienza | Polly (retry, circuit breaker, rate limiter) | ogni piattaforma ha rate limit propri |
| Messaging | Azure Service Bus (o Storage Queue per partire più leggeri) | disaccoppia API da worker |
| Secrets | Azure Key Vault + Managed Identity | |
| Storage media | Azure Blob Storage | |
| AI "nativo" Azure | Microsoft Foundry (ex Azure OpenAI Service) | un provider AI di casa, coerente con l'hosting |
| AI bring-your-own-key | OpenAI, Anthropic, Google Gemini, OpenRouter | copre anche i provider che già usi altrove |
| Frontend | Angular SPA | ecosistema maturo per UI complesse (calendari, form, media) |
| Auth utenti | ASP.NET Core Identity | valutare Entra External ID se in futuro diventa multi-tenant |
| IaC | Bicep + Azure Developer CLI (`azd`) | `azd up` è di fatto la "CLI che crea il progetto" anche lato infra |
| CI/CD | GitHub Actions | |
| Observability | OpenTelemetry + Application Insights | |
| Test | xUnit, FluentAssertions, Testcontainers, WireMock.NET | per mockare le API social/AI senza chiamarle davvero |

## 14. Struttura soluzione

```
PostForge/
├── src/
│   ├── PostForge.Domain/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   └── Events/
│   ├── PostForge.Application/
│   │   ├── Posts/
│   │   ├── Campaigns/
│   │   ├── Scheduling/
│   │   └── Ai/
│   ├── PostForge.Infrastructure/
│   │   ├── Providers.Social/
│   │   ├── Providers.Ai/
│   │   └── Messaging/
│   ├── PostForge.Infrastructure.DAL/
│   │   └── Persistence (PostForgeDbContext + repository implementations)
│   ├── PostForge.Api/
│   ├── PostForge.Worker/
│   └── web/                    # Angular app
├── tests/
│   ├── PostForge.UnitTests/
│   └── PostForge.IntegrationTests/
├── infra/
│   ├── main.bicep
│   └── azure.yaml
├── .github/workflows/
├── README.md
├── CONTRIBUTING.md
├── LICENSE
└── PostForge.sln
```

## 15. OSS setup

- Licenza: MIT (in linea con i tuoi altri repository open source).
- README con quickstart, architettura, come aggiungere un nuovo provider.
- CONTRIBUTING.md, issue templates, CODE_OF_CONDUCT.md.
- GitHub Actions: build + test su ogni PR, deploy su push a `main`.

## 16. Roadmap

1. **Fase 0 — Setup**: skeleton della solution, CI, IaC di base, auth utente singolo.
2. **Fase 1 — MVP Meta**: provider Facebook/Instagram via Graph API, pubblicazione immediata e schedulata semplice (terreno già noto, primo a raggiungere il MVP).
3. **Fase 2 — Scheduling avanzato + campagne**: calendario, ricorrenze, stati, retry, campagne con obiettivo/canale.
4. **Fase 3 — AI assist**: provider model AI, gestione chiavi utente in Key Vault.
5. **Fase 4 — TikTok + YouTube**: nuovi provider social.
6. **Fase 5 — Extra**: analytics aggregate, multi-tenant opzionale, event sourcing come esercizio avanzato su CQRS+ES.

## 17. Naming — candidati

**A tema con il tuo brand (Roma/mitologia)**
- **Mercurius** — dio romano dei messaggi e della comunicazione: consegna contenuti su più "province" (piattaforme).
- **Nuntius** — latino per "messaggero/araldo".
- **Praeco** — il banditore pubblico romano, chi annunciava le notizie in piazza.
- **Scriptorium** — la stanza degli amanuensi dove si scriveva e copiava: buon ponte fra il tema storico e la parte di scrittura assistita da AI.

**Più diretti/pragmatici**
- **PostForge**
- **OmniPost**
- **Crosspost.NET**
- **SocialQuill**

## 18. Domande aperte

- Uso singolo o pensato fin da subito per essere multi-tenant/SaaS?
- Frontend: Angular (deciso) — da valutare Angular Material vs PrimeNG per la component library
- Worker: Azure Functions serverless o servizio sempre attivo (Container Apps/Worker Service)?
- Database: Azure SQL o Postgres Flexible Server?
- L'AI si limita ad assist su testo/immagini o si spinge anche su suggerimenti di orario/performance ottimali?
