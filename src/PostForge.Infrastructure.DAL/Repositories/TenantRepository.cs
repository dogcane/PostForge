using ECO.Data;
using ECO.Providers.EntityFramework;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.DAL.Repositories;

public class TenantRepository(IDataContext dataContext) : EntityFrameworkRepository<Tenant, Guid>(dataContext), ITenantRepository;