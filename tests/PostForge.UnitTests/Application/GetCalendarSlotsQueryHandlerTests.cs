using FluentAssertions;
using PostForge.Application.Scheduling.Queries.GetCalendarSlots;
using PostForge.Domain.Entities;
using PostForge.Infrastructure.DAL.Repositories;

namespace PostForge.UnitTests.Application;

public class GetCalendarSlotsQueryHandlerTests : HandlerTestBase
{
    private async Task<ScheduleSlot> AddSlot(Guid postId, DateTime scheduledAtUtc, Guid? tenantId = null)
    {
        var slot = ScheduleSlot.Create(tenantId ?? TenantId, postId, "FACEBOOK", scheduledAtUtc).Value!;
        var ctx = ((ScheduleSlotRepository)ScheduleSlotRepository).DbContext;
        ctx.Set<ScheduleSlot>().Add(slot);
        await ctx.SaveChangesAsync(CancellationToken.None);
        return slot;
    }

    [Fact]
    public async Task Handle_ShouldReturnSlotsWithinRangeOrderedByDate()
    {
        var start = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var first = await AddSlot(Guid.NewGuid(), new DateTime(2025, 1, 5, 10, 0, 0, DateTimeKind.Utc));
        var second = await AddSlot(Guid.NewGuid(), new DateTime(2025, 1, 3, 10, 0, 0, DateTimeKind.Utc));

        var handler = new GetCalendarSlotsHandler(ScheduleSlotRepository);
        var result = await handler.Handle(new GetCalendarSlotsQuery(start, end), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(second.Id);
        result[1].Id.Should().Be(first.Id);
    }

    [Fact]
    public async Task Handle_ShouldExcludeSlotsOutsideRange()
    {
        var start = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        await AddSlot(Guid.NewGuid(), new DateTime(2025, 1, 10, 10, 0, 0, DateTimeKind.Utc));
        await AddSlot(Guid.NewGuid(), new DateTime(2024, 12, 31, 23, 0, 0, DateTimeKind.Utc));
        await AddSlot(Guid.NewGuid(), new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc));

        var handler = new GetCalendarSlotsHandler(ScheduleSlotRepository);
        var result = await handler.Handle(new GetCalendarSlotsQuery(start, end), CancellationToken.None);

        result.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ShouldNotReturnSlotsFromOtherTenants()
    {
        var start = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        await AddSlot(Guid.NewGuid(), new DateTime(2025, 1, 10, 10, 0, 0, DateTimeKind.Utc));
        await AddSlot(Guid.NewGuid(), new DateTime(2025, 1, 11, 10, 0, 0, DateTimeKind.Utc), Guid.NewGuid());

        var handler = new GetCalendarSlotsHandler(ScheduleSlotRepository);
        var result = await handler.Handle(new GetCalendarSlotsQuery(start, end), CancellationToken.None);

        result.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoMatches()
    {
        var start = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        var handler = new GetCalendarSlotsHandler(ScheduleSlotRepository);
        var result = await handler.Handle(new GetCalendarSlotsQuery(start, end), CancellationToken.None);

        result.Should().BeEmpty();
    }
}