using EventOrganizer.Application.DTOs;

namespace EventOrganizer.Application.Interfaces;

public interface IEventService
{
    Task<EventDto> CreateAsync(Guid userId, CreateEventRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<EventDto> UpdateAsync(Guid userId, Guid eventId, UpdateEventRequest request, CancellationToken cancellationToken = default);
    Task CancelAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default);
}
