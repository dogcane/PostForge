using Mediator;
using PostForge.Application.Auth.DTOs;
using PostForge.Application.Common.Interfaces;

namespace PostForge.Application.Auth.Commands.Login;

public class LoginHandler(
    IAuthenticationService authenticationService) : IRequestHandler<LoginCommand, LoginResultDto?>
{
    public async ValueTask<LoginResultDto?> Handle(LoginCommand request, CancellationToken cancellationToken)
        => await authenticationService.LoginAsync(request.Email, request.Password, cancellationToken);
}