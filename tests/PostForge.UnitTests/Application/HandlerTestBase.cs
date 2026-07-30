using ECO;
using ECO.Data;
using ECO.Providers.EntityFramework;
using Microsoft.EntityFrameworkCore;
using PostForge.Domain.Interfaces;
using PostForge.Domain.Entities;
using PostForge.Infrastructure.Persistence;
using PostForge.Infrastructure.Persistence.Repositories;

namespace PostForge.UnitTests.Application;

public abstract class HandlerTestBase : IDisposable
{
    private readonly string _dbName;
    private readonly IPersistenceUnitFactory _factory;

    protected HandlerTestBase()
    {
        _dbName = Guid.NewGuid().ToString();
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

        _factory.AddPersistenceUnit(unit);
        DataContext = _factory.OpenDataContext();

        PostRepository = new PostRepository(DataContext);
        CampaignRepository = new CampaignRepository(DataContext);
        ScheduleSlotRepository = new ScheduleSlotRepository(DataContext);
        SocialAccountRepository = new SocialAccountRepository(DataContext);
        ProviderCredentialRepository = new ProviderCredentialRepository(DataContext);
    }

    protected IDataContext DataContext { get; }
    protected IPostRepository PostRepository { get; }
    protected ICampaignRepository CampaignRepository { get; }
    protected IScheduleSlotRepository ScheduleSlotRepository { get; }
    protected ISocialAccountRepository SocialAccountRepository { get; }
    protected IProviderCredentialRepository ProviderCredentialRepository { get; }

    public void Dispose()
    {
        DataContext.Dispose();
    }
}
