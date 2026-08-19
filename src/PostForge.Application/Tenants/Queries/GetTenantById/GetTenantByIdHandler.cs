using Mediator;
using PostForge.Application.Common.Mappings;
using PostForge.Application.Tenants.DTOs;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Tenants.Queries.GetTenantById;

public class GetTenantByIdHandler(
    ITenantRepository tenantRepository) : IRequestHandler<GetTenantByIdQuery, TenantDto?>
{
    public async ValueTask<TenantDto?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.LoadAsync(request.Id);
        return tenant is null ? null : tenant.ToDto();
    }
}