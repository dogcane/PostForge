using Mediator;
using PostForge.Application.Common.Mappings;
using PostForge.Application.Tenants.DTOs;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Tenants.Queries.GetAllTenants;

public class GetAllTenantsHandler(
    ITenantRepository tenantRepository) : IRequestHandler<GetAllTenantsQuery, List<TenantDto>>
{
    public ValueTask<List<TenantDto>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = tenantRepository.ToList();
        return ValueTask.FromResult(tenants.Select(t => t.ToDto()).ToList());
    }
}