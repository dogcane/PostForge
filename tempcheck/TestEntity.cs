using ECO;
using Microsoft.EntityFrameworkCore;

public class TestEntity : AggregateRoot<Guid>
{
    public string Name { get; set; }
    public TestEntity() : base(Guid.NewGuid()) { }
    public TestEntity(string name) : base(Guid.NewGuid()) { Name = name; }
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
