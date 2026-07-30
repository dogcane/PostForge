using Mediator;
using PostForge.Application.Ai.DTOs;

namespace PostForge.Application.Ai.Commands.GenerateImage;

public record GenerateImageCommand(
    string Prompt,
    string? Style) : IRequest<ImageResultDto>;
