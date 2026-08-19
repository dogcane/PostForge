using ECO.Data;
using Mediator;
using PostForge.Application.Common.Extensions;
using PostForge.Application.Common.Interfaces;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Tenants.Commands.AddUserToTenant;

public class AddUserToTenantHandler(
    IUserAccountService userAccountService,
    ITenantMembershipRepository tenantMembershipRepository,
    IDataContext dataContext) : IRequestHandler<AddUserToTenantCommand, Guid>
{
    public async ValueTask<Guid> Handle(AddUserToTenantCommand request, CancellationToken cancellationToken)
    {
        if (await userAccountService.UserExistsAsync(request.Email, cancellationToken))
            throw new InvalidOperationException($"A user with email '{request.Email}' already exists.");

        var userId = await userAccountService.CreateUserAsync(request.Email, request.Password, cancellationToken);

        var membership = TenantMembership.Create(request.TenantId, userId).EnsureSuccess();

        tenantMembershipRepository.Add(membership);
        await dataContext.SaveChangesAsync(cancellationToken);

        return userId;
    }
}