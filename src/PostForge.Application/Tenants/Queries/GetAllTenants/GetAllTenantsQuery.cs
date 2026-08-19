using Mediator;
using PostForge.Application.Tenants.DTOs;

namespace PostForge.Application.Tenants.Queries.GetAllTenants;

public record GetAllTenantsQuery() : IRequest<List<TenantDto>>;