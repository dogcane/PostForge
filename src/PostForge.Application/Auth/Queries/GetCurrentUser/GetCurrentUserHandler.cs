using Mediator;
using Microsoft.EntityFrameworkCore;
using PostForge.Application.Auth.DTOs;
using PostForge.Application.Common.Interfaces;
using PostForge.Application.Common.Mappings;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Auth.Queries.GetCurrentUser;

public class GetCurrentUserHandler(
    ITenantContext tenantContext,
    ITenantMembershipRepository tenantMembershipRepository,
    ITenantRepository tenantRepository,
    IUserAccountService userAccountService) : IRequestHandler<GetCurrentUserQuery, CurrentUserDto?>
{
    public async ValueTask<CurrentUserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (tenantContext.UserId is not Guid userId)
            return null;

        var email = await userAccountService.GetUserEmailAsync(userId, cancellationToken);
        if (email is null)
            return null;

        var tenantIds = await ((IQueryable<PostForge.Domain.Entities.TenantMembership>)tenantMembershipRepository)
            .Where(m => m.UserId == userId)
            .Select(m => m.TenantId)
            .ToListAsync(cancellationToken);

        var tenants = await ((IQueryable<PostForge.Domain.Entities.Tenant>)tenantRepository)
            .Where(t => tenantIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        return new CurrentUserDto
        {
            UserId = userId,
            Email = email,
            IsSuperUser = await userAccountService.IsSuperUserAsync(userId, cancellationToken),
            Tenants = tenants.Select(t => t.ToDto()).ToList()
        };
    }
}