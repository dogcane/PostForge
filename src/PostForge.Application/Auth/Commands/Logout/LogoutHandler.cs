using Mediator;
using PostForge.Application.Common.Interfaces;

namespace PostForge.Application.Auth.Commands.Logout;

public class LogoutHandler(IAuthenticationService authenticationService) : IRequestHandler<LogoutCommand, bool>
{
    public async ValueTask<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
        => await authenticationService.LogoutAsync(request.RefreshToken, cancellationToken);
}
