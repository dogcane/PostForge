using ECO.Data;
using Mediator;
using PostForge.Application.Common.Extensions;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Tenants.Commands.CreateTenant;

public class CreateTenantHandler(
    ITenantRepository tenantRepository,
    IDataContext dataContext) : IRequestHandler<CreateTenantCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = Tenant.Create(request.Name, request.Slug).EnsureSuccess();

        tenantRepository.Add(tenant);
        await dataContext.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }
}