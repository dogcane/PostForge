using Mediator;

namespace PostForge.Application.Tenants.Commands.CreateTenant;

public record CreateTenantCommand(string Name, string Slug) : IRequest<Guid>;