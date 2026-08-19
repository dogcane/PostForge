using Mediator;
using PostForge.Application.Auth.DTOs;

namespace PostForge.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResultDto?>;