using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostForge.Application.ProviderCredentials.Commands.CreateProviderCredential;
using PostForge.Application.ProviderCredentials.Commands.DeleteProviderCredential;
using PostForge.Application.ProviderCredentials.Commands.UpdateProviderCredential;
using PostForge.Application.ProviderCredentials.Commands.ValidateProviderCredential;
using PostForge.Application.ProviderCredentials.DTOs;
using PostForge.Application.ProviderCredentials.Queries.GetAllProviderCredentials;
using PostForge.Application.ProviderCredentials.Queries.GetProviderCredentialById;
using PostForge.Domain.Providers;
using PostForge.Domain.ValueObjects;

namespace PostForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/provider-credentials")]
public class ProviderCredentialsController(
    IMediator mediator,
    ISocialPlatformProviderRegistry socialRegistry,
    IProviderRegistry<IAiTextProvider> aiTextRegistry,
    IProviderRegistry<IAiImageProvider> aiImageRegistry) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProviderCredentialDto>>> GetAll()
    {
        var result = await mediator.Send(new GetAllProviderCredentialsQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProviderCredentialDto>> GetById(Guid id)
    {
        var credential = await mediator.Send(new GetProviderCredentialByIdQuery(id));
        if (credential is null)
            return NotFound();
        return Ok(credential);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateProviderCredentialRequest request)
    {
        var command = new CreateProviderCredentialCommand(
            request.ProviderKey,
            request.Scope,
            request.DisplayName,
            request.Description,
            request.KeyVaultReference,
            request.SecretValue,
            request.SettingsJson,
            request.IsEnabled);
        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateProviderCredentialRequest request)
    {
        var command = new UpdateProviderCredentialCommand(
            id,
            request.DisplayName,
            request.Description,
            request.KeyVaultReference,
            request.SecretValue,
            request.SettingsJson,
            request.IsEnabled);
        await mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await mediator.Send(new DeleteProviderCredentialCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/validate")]
    public async Task<ActionResult> Validate(Guid id)
    {
        await mediator.Send(new ValidateProviderCredentialCommand(id));
        return NoContent();
    }

    private static readonly Dictionary<string, (string Label, string Description, ProviderCredentialScope Scope)> KnownProviders = new(StringComparer.OrdinalIgnoreCase)
    {
        ["FACEBOOK"] = ("Facebook", "Meta Graph API - AppId / AppSecret / RedirectUri / PageId", ProviderCredentialScope.Social),
        ["INSTAGRAM"] = ("Instagram", "Instagram Graph API via Facebook", ProviderCredentialScope.Social),
        ["TIKTOK"] = ("TikTok", "TikTok Content Posting API", ProviderCredentialScope.Social),
        ["YOUTUBE"] = ("YouTube", "YouTube Data API v3", ProviderCredentialScope.Social),
        ["openai"] = ("OpenAI", "OpenAI text generation", ProviderCredentialScope.AiText),
        ["anthropic"] = ("Anthropic", "Anthropic Claude API", ProviderCredentialScope.AiText),
        ["google-gemini"] = ("Google Gemini", "Google Gemini API", ProviderCredentialScope.AiText),
        ["microsoft-foundry"] = ("Microsoft Foundry", "Azure OpenAI / Foundry", ProviderCredentialScope.AiText),
        ["dalle"] = ("DALL·E", "OpenAI DALL-E image generation", ProviderCredentialScope.AiImage),
        ["stable-diffusion"] = ("Stable Diffusion", "Stability AI", ProviderCredentialScope.AiImage),
        ["FAKE"] = ("Fake", "Fake provider for testing", ProviderCredentialScope.Social)
    };

    [HttpGet("supported")]
    public ActionResult<object> GetSupportedProviders()
    {
        var installedKeys = socialRegistry.AvailableProviderKeys
            .Concat(aiTextRegistry.AvailableProviderKeys)
            .Concat(aiImageRegistry.AvailableProviderKeys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Preserve KnownProviders insertion order, filter to installed only
        var knownInstalled = KnownProviders
            .Where(kv => installedKeys.Contains(kv.Key))
            .Select(kv => new { key = kv.Key, scope = kv.Value.Scope, label = kv.Value.Label, description = kv.Value.Description });

        // Fallback for any installed provider not in KnownProviders (e.g., future plugins)
        var unknownInstalled = installedKeys
            .Where(k => !KnownProviders.ContainsKey(k))
            .Select(key =>
            {
                var scope = socialRegistry.AvailableProviderKeys.Contains(key, StringComparer.OrdinalIgnoreCase)
                    ? ProviderCredentialScope.Social
                    : aiTextRegistry.AvailableProviderKeys.Contains(key, StringComparer.OrdinalIgnoreCase)
                        ? ProviderCredentialScope.AiText
                        : ProviderCredentialScope.AiImage;
                return new { key, scope, label = key, description = $"Provider {key}" };
            });

        var providers = knownInstalled.Concat(unknownInstalled).ToArray();

        return Ok(providers);
    }
}
