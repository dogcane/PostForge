using FluentAssertions;
using PostForge.Domain.Entities;

namespace PostForge.UnitTests.Domain;

public class TenantMembershipTests
{
    [Fact]
    public void CreatingMembership_ShouldSetProperties()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var result = TenantMembership.Create(tenantId, userId);

        result.Success.Should().BeTrue();
        result.Value!.TenantId.Should().Be(tenantId);
        result.Value.UserId.Should().Be(userId);
    }

    [Fact]
    public void CreatingMembership_WithEmptyTenantId_ShouldReturnFailure()
    {
        var result = TenantMembership.Create(Guid.Empty, Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "TenantId");
    }

    [Fact]
    public void CreatingMembership_WithEmptyUserId_ShouldReturnFailure()
    {
        var result = TenantMembership.Create(Guid.NewGuid(), Guid.Empty);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Context == "UserId");
    }
}