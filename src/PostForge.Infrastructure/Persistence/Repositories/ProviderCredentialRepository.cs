using ECO.Data;
using ECO.Providers.EntityFramework;
using PostForge.Domain.Interfaces;
using PostForge.Domain.Entities;

namespace PostForge.Infrastructure.Persistence.Repositories;

public class ProviderCredentialRepository : EntityFrameworkRepository<ProviderCredential, Guid>, IProviderCredentialRepository
{
    public ProviderCredentialRepository(IDataContext dataContext) : base(dataContext) { }
}
