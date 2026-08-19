using Mediator;

namespace PostForge.Application.Tenants.Commands.AddUserToTenant;

public record AddUserToTenantCommand(Guid TenantId, string Email, string Password) : IRequest<Guid>;