using ECO.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Tenants.Commands.RemoveUserFromTenant;

public class RemoveUserFromTenantHandler(
    ITenantMembershipRepository tenantMembershipRepository,
    IDataContext dataContext) : IRequestHandler<RemoveUserFromTenantCommand, Unit>
{
    public async ValueTask<Unit> Handle(RemoveUserFromTenantCommand request, CancellationToken cancellationToken)
    {
        var membership = await ((IQueryable<PostForge.Domain.Entities.TenantMembership>)tenantMembershipRepository)
            .Where(m => m.TenantId == request.TenantId && m.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(
                $"User {request.UserId} is not a member of tenant {request.TenantId}.");

        tenantMembershipRepository.Remove(membership);
        await dataContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}