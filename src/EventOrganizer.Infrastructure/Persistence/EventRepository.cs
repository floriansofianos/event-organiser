using EventOrganizer.Domain.Entities;
using EventOrganizer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventOrganizer.Infrastructure.Persistence;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Event>> GetByCreatorIdAsync(Guid creatorId, CancellationToken cancellationToken = default)
        => await _context.Events
            .Where(e => e.CreatorUserId == creatorId)
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        await _context.Events.AddAsync(@event, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Event @event, CancellationToken cancellationToken = default)
    {
        _context.Events.Update(@event);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
