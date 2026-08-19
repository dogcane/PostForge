using Mediator;
using PostForge.Application.Tenants.DTOs;

namespace PostForge.Application.Tenants.Queries.GetTenantUsers;

public record GetTenantUsersQuery(Guid TenantId) : IRequest<List<TenantUserDto>>;