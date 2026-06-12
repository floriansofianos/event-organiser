using EventOrganizer.Application.DTOs;
using EventOrganizer.Application.Services;
using EventOrganizer.Domain.Entities;
using EventOrganizer.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace EventOrganizer.Application.Tests.Services;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _repoMock = new();
    private EventService CreateSut() => new(_repoMock.Object);

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime FutureDate = DateTime.UtcNow.AddDays(30);

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WithValidRequest_AddsEventAndReturnsDto()
    {
        var request = new CreateEventRequest("Birthday Party", FutureDate, "Fun times", "123 Main St");

        var result = await CreateSut().CreateAsync(UserId, request);

        _repoMock.Verify(r => r.AddAsync(It.Is<Event>(e =>
            e.Name == "Birthday Party" &&
            e.Date == FutureDate &&
            e.Description == "Fun times" &&
            e.Location == "123 Main St" &&
            e.CreatorUserId == UserId), default), Times.Once);

        result.Name.Should().Be("Birthday Party");
        result.Date.Should().Be(FutureDate);
        result.Description.Should().Be("Fun times");
        result.Location.Should().Be("123 Main St");
        result.Status.Should().Be("Active");
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WithNullOptionalFields_AddsEventSuccessfully()
    {
        var request = new CreateEventRequest("Dinner", FutureDate, null, null);

        var result = await CreateSut().CreateAsync(UserId, request);

        result.Description.Should().BeNull();
        result.Location.Should().BeNull();
    }

    // ── GetAllAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyUserEvents()
    {
        var events = new List<Event>
        {
            Event.Create("Event A", FutureDate, null, null, UserId),
            Event.Create("Event B", FutureDate, null, null, UserId),
        };
        _repoMock.Setup(r => r.GetByCreatorIdAsync(UserId, default)).ReturnsAsync(events);

        var result = await CreateSut().GetAllAsync(UserId);

        result.Should().HaveCount(2);
        result.Select(e => e.Name).Should().BeEquivalentTo("Event A", "Event B");
    }

    [Fact]
    public async Task GetAllAsync_WhenNoEvents_ReturnsEmptyList()
    {
        _repoMock.Setup(r => r.GetByCreatorIdAsync(UserId, default))
            .ReturnsAsync(new List<Event>());

        var result = await CreateSut().GetAllAsync(UserId);

        result.Should().BeEmpty();
    }

    // ── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_WithValidOwner_UpdatesAndReturnsDto()
    {
        var @event = Event.Create("Old Name", FutureDate, null, null, UserId);
        var eventId = @event.Id;
        _repoMock.Setup(r => r.GetByIdAsync(eventId, default)).ReturnsAsync(@event);

        var request = new UpdateEventRequest("New Name", FutureDate.AddDays(1), "New desc", "New location");

        var result = await CreateSut().UpdateAsync(UserId, eventId, request);

        result.Name.Should().Be("New Name");
        result.Description.Should().Be("New desc");
        result.Location.Should().Be("New location");
        _repoMock.Verify(r => r.UpdateAsync(@event, default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenEventNotFound_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Event?)null);

        var act = async () => await CreateSut().UpdateAsync(UserId, Guid.NewGuid(),
            new UpdateEventRequest("X", FutureDate, null, null));

        await act.Should().ThrowAsync<Exceptions.NotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task UpdateAsync_WhenCalledByNonOwner_ThrowsForbiddenException()
    {
        var @event = Event.Create("Party", FutureDate, null, null, Guid.NewGuid()); // different owner
        _repoMock.Setup(r => r.GetByIdAsync(@event.Id, default)).ReturnsAsync(@event);

        var act = async () => await CreateSut().UpdateAsync(UserId, @event.Id,
            new UpdateEventRequest("X", FutureDate, null, null));

        await act.Should().ThrowAsync<Exceptions.ForbiddenException>()
            .WithMessage("*permission*");
    }

    [Fact]
    public async Task UpdateAsync_OnCancelledEvent_ThrowsApplicationException()
    {
        var @event = Event.Create("Party", FutureDate, null, null, UserId);
        @event.Cancel();
        _repoMock.Setup(r => r.GetByIdAsync(@event.Id, default)).ReturnsAsync(@event);

        var act = async () => await CreateSut().UpdateAsync(UserId, @event.Id,
            new UpdateEventRequest("X", FutureDate, null, null));

        await act.Should().ThrowAsync<Exceptions.ApplicationException>()
            .WithMessage("*cancelled*");
    }

    // ── CancelAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelAsync_WithValidOwner_CancelsEvent()
    {
        var @event = Event.Create("Party", FutureDate, null, null, UserId);
        _repoMock.Setup(r => r.GetByIdAsync(@event.Id, default)).ReturnsAsync(@event);

        await CreateSut().CancelAsync(UserId, @event.Id);

        @event.Status.Should().Be(Domain.Enums.EventStatus.Cancelled);
        _repoMock.Verify(r => r.UpdateAsync(@event, default), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_WhenEventNotFound_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Event?)null);

        var act = async () => await CreateSut().CancelAsync(UserId, Guid.NewGuid());

        await act.Should().ThrowAsync<Exceptions.NotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task CancelAsync_WhenCalledByNonOwner_ThrowsForbiddenException()
    {
        var @event = Event.Create("Party", FutureDate, null, null, Guid.NewGuid()); // different owner
        _repoMock.Setup(r => r.GetByIdAsync(@event.Id, default)).ReturnsAsync(@event);

        var act = async () => await CreateSut().CancelAsync(UserId, @event.Id);

        await act.Should().ThrowAsync<Exceptions.ForbiddenException>()
            .WithMessage("*permission*");
    }

    [Fact]
    public async Task CancelAsync_WhenAlreadyCancelled_ThrowsApplicationException()
    {
        var @event = Event.Create("Party", FutureDate, null, null, UserId);
        @event.Cancel();
        _repoMock.Setup(r => r.GetByIdAsync(@event.Id, default)).ReturnsAsync(@event);

        var act = async () => await CreateSut().CancelAsync(UserId, @event.Id);

        await act.Should().ThrowAsync<Exceptions.ApplicationException>()
            .WithMessage("*already cancelled*");
    }
}
