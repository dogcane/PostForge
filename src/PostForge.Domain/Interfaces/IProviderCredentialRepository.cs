using ECO;
using PostForge.Domain.Entities;

namespace PostForge.Domain.Interfaces;

public interface IProviderCredentialRepository : IRepository<ProviderCredential, Guid>
{
}
