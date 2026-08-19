using Mediator;
using PostForge.Application.Tenants.DTOs;

namespace PostForge.Application.Tenants.Queries.GetTenantById;

public record GetTenantByIdQuery(Guid Id) : IRequest<TenantDto?>;