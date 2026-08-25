using Mediator;
using PostForge.Application.Auth.DTOs;

namespace PostForge.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResultDto?>;
