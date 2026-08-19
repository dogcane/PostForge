using Mediator;
using Microsoft.EntityFrameworkCore;
using PostForge.Application.Common.Interfaces;
using PostForge.Application.Tenants.DTOs;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Tenants.Queries.GetTenantUsers;

public class GetTenantUsersHandler(
    ITenantMembershipRepository tenantMembershipRepository,
    IUserAccountService userAccountService) : IRequestHandler<GetTenantUsersQuery, List<TenantUserDto>>
{
    public async ValueTask<List<TenantUserDto>> Handle(GetTenantUsersQuery request, CancellationToken cancellationToken)
    {
        var memberships = await ((IQueryable<PostForge.Domain.Entities.TenantMembership>)tenantMembershipRepository)
            .Where(m => m.TenantId == request.TenantId)
            .ToListAsync(cancellationToken);

        var users = new List<TenantUserDto>(memberships.Count);
        foreach (var membership in memberships)
        {
            var email = await userAccountService.GetUserEmailAsync(membership.UserId, cancellationToken);
            if (email is null)
                continue;
            users.Add(new TenantUserDto
            {
                UserId = membership.UserId,
                Email = email,
                JoinedAtUtc = membership.JoinedAtUtc
            });
        }

        return users;
    }
}