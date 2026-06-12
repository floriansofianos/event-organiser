using EventOrganizer.Application.DTOs;
using EventOrganizer.Application.Exceptions;
using EventOrganizer.Application.Interfaces;
using EventOrganizer.Domain.Entities;
using EventOrganizer.Domain.Enums;
using EventOrganizer.Domain.Interfaces;

namespace EventOrganizer.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<EventDto> CreateAsync(Guid userId, CreateEventRequest request, CancellationToken cancellationToken = default)
    {
        var @event = Event.Create(request.Name, request.Date, request.Description, request.Location, userId);
        await _eventRepository.AddAsync(@event, cancellationToken);
        return ToDto(@event);
    }

    public async Task<IReadOnlyList<EventDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var events = await _eventRepository.GetByCreatorIdAsync(userId, cancellationToken);
        return events.Select(ToDto).ToList();
    }

    public async Task<EventDto> UpdateAsync(Guid userId, Guid eventId, UpdateEventRequest request, CancellationToken cancellationToken = default)
    {
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken)
            ?? throw new NotFoundException("Event not found.");

        if (@event.CreatorUserId != userId)
            throw new ForbiddenException("You do not have permission to edit this event.");

        if (@event.Status == EventStatus.Cancelled)
            throw new Exceptions.ApplicationException("Cannot edit a cancelled event.", 400);

        @event.Update(request.Name, request.Date, request.Description, request.Location);
        await _eventRepository.UpdateAsync(@event, cancellationToken);
        return ToDto(@event);
    }

    public async Task CancelAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken)
            ?? throw new NotFoundException("Event not found.");

        if (@event.CreatorUserId != userId)
            throw new ForbiddenException("You do not have permission to cancel this event.");

        if (@event.Status == EventStatus.Cancelled)
            throw new Exceptions.ApplicationException("Event is already cancelled.", 400);

        @event.Cancel();
        await _eventRepository.UpdateAsync(@event, cancellationToken);
    }

    private static EventDto ToDto(Event @event) => new(
        @event.Id,
        @event.Name,
        @event.Date,
        @event.Description,
        @event.Location,
        @event.Status.ToString(),
        @event.CreatedAt
    );
}
