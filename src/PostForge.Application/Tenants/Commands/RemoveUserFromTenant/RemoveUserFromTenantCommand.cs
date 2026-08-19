using Mediator;

namespace PostForge.Application.Tenants.Commands.RemoveUserFromTenant;

public record RemoveUserFromTenantCommand(Guid TenantId, Guid UserId) : IRequest<Unit>;