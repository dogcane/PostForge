using Mediator;
using PostForge.Application.Ai.DTOs;

namespace PostForge.Application.Ai.Commands.GenerateCaption;

public record GenerateCaptionCommand(
    string Brief,
    string? Platform,
    string? Tone) : IRequest<CaptionResultDto>;
