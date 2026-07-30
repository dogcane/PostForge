using Mediator;
using PostForge.Application.Ai.DTOs;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Ai.Commands.GenerateCaption;

public record GenerateCaptionCommand(
    string Brief,
    SocialPlatform? Platform,
    string? Tone) : IRequest<CaptionResultDto>;
