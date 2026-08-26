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
using PostForge.Domain.ValueObjects;

namespace PostForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/provider-credentials")]
public class ProviderCredentialsController(IMediator mediator) : ControllerBase
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

    [HttpGet("supported")]
    public ActionResult<object> GetSupportedProviders()
    {
        var providers = new[]
        {
            new { key = "FACEBOOK", scope = ProviderCredentialScope.Social, label = "Facebook", description = "Meta Graph API - AppId / AppSecret / RedirectUri / PageId" },
            new { key = "INSTAGRAM", scope = ProviderCredentialScope.Social, label = "Instagram", description = "Instagram Graph API via Facebook" },
            new { key = "TIKTOK", scope = ProviderCredentialScope.Social, label = "TikTok", description = "TikTok Content Posting API" },
            new { key = "YOUTUBE", scope = ProviderCredentialScope.Social, label = "YouTube", description = "YouTube Data API v3" },
            new { key = "openai", scope = ProviderCredentialScope.AiText, label = "OpenAI", description = "OpenAI text generation" },
            new { key = "anthropic", scope = ProviderCredentialScope.AiText, label = "Anthropic", description = "Anthropic Claude API" },
            new { key = "google-gemini", scope = ProviderCredentialScope.AiText, label = "Google Gemini", description = "Google Gemini API" },
            new { key = "microsoft-foundry", scope = ProviderCredentialScope.AiText, label = "Microsoft Foundry", description = "Azure OpenAI / Foundry" },
            new { key = "dalle", scope = ProviderCredentialScope.AiImage, label = "DALL·E", description = "OpenAI DALL-E image generation" },
            new { key = "stable-diffusion", scope = ProviderCredentialScope.AiImage, label = "Stable Diffusion", description = "Stability AI" }
        };
        return Ok(providers);
    }
}
