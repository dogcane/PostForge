using Mediator;

namespace PostForge.Application.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<bool>;
