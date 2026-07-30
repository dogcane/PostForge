using ECO.Data;
using ECO.Providers.EntityFramework;
using PostForge.Domain.Interfaces;
using PostForge.Domain.Entities;

namespace PostForge.Infrastructure.Persistence.Repositories;

public class SocialAccountRepository : EntityFrameworkRepository<SocialAccount, Guid>, ISocialAccountRepository
{
    public SocialAccountRepository(IDataContext dataContext) : base(dataContext) { }
}
