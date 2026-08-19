using ECO;
using ECO.Data;
using ECO.Providers.EntityFramework;
using Microsoft.EntityFrameworkCore;
using PostForge.Domain.Interfaces;
using PostForge.Domain.Entities;
using PostForge.Infrastructure.DAL;
using PostForge.Infrastructure.DAL.Repositories;

namespace PostForge.UnitTests.Application;

public abstract class HandlerTestBase : IDisposable
{
    private readonly string _dbName;
    private readonly IPersistenceUnitFactory _factory;

    protected HandlerTestBase()
    {
        _dbName = Guid.NewGuid().ToString();
        TenantId = Guid.NewGuid();
        TenantContext = new TestTenantContext(TenantId);

        var options = new DbContextOptionsBuilder<PostForgeDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;

        _factory = new PersistenceUnitFactory();
        var unit = new EntityFrameworkPersistenceUnit<PostForgeDbContext>(
            "TestUnit",
            options,
            null);

        unit.AddClass<Post, Guid>();
        unit.AddClass<Campaign, Guid>();
        unit.AddClass<ScheduleSlot, Guid>();
        unit.AddClass<SocialAccount, Guid>();
        unit.AddClass<ProviderCredential, Guid>();
        unit.AddClass<Tenant, Guid>();
        unit.AddClass<TenantMembership, Guid>();

        _factory.AddPersistenceUnit(unit);
        DataContext = _factory.OpenDataContext();

        PostRepository = new PostRepository(DataContext, TenantContext);
        CampaignRepository = new CampaignRepository(DataContext, TenantContext);
        ScheduleSlotRepository = new ScheduleSlotRepository(DataContext, TenantContext);
        SocialAccountRepository = new SocialAccountRepository(DataContext, TenantContext);
        ProviderCredentialRepository = new ProviderCredentialRepository(DataContext, TenantContext);
        TenantRepository = new TenantRepository(DataContext);
        TenantMembershipRepository = new TenantMembershipRepository(DataContext, TenantContext);
    }

    protected Guid TenantId { get; }
    protected ITenantContext TenantContext { get; }
    protected IDataContext DataContext { get; }
    protected IPostRepository PostRepository { get; }
    protected ICampaignRepository CampaignRepository { get; }
    protected IScheduleSlotRepository ScheduleSlotRepository { get; }
    protected ISocialAccountRepository SocialAccountRepository { get; }
    protected IProviderCredentialRepository ProviderCredentialRepository { get; }
    protected ITenantRepository TenantRepository { get; }
    protected ITenantMembershipRepository TenantMembershipRepository { get; }

    public void Dispose()
    {
        DataContext.Dispose();
    }

    private sealed class TestTenantContext(Guid tenantId) : ITenantContext
    {
        public Guid? TenantId => tenantId;
        public Guid? UserId => Guid.NewGuid();
        public bool IsSuperUser => false;
    }
}