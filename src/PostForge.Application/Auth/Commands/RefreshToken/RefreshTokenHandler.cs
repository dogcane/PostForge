using Mediator;
using PostForge.Application.Auth.DTOs;
using PostForge.Application.Common.Interfaces;

namespace PostForge.Application.Auth.Commands.RefreshToken;

public class RefreshTokenHandler(IAuthenticationService authenticationService) : IRequestHandler<RefreshTokenCommand, LoginResultDto?>
{
    public async ValueTask<LoginResultDto?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        => await authenticationService.RefreshAsync(request.RefreshToken, cancellationToken);
}
