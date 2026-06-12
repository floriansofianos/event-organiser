using EventOrganizer.Domain.Enums;

namespace EventOrganizer.Domain.Entities;

public class Event
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public string? Description { get; private set; }
    public string? Location { get; private set; }
    public EventStatus Status { get; private set; }
    public Guid CreatorUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Event() { }

    public static Event Create(string name, DateTime date, string? description, string? location, Guid creatorUserId)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Name = name,
            Date = date,
            Description = description,
            Location = location,
            Status = EventStatus.Active,
            CreatorUserId = creatorUserId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, DateTime date, string? description, string? location)
    {
        Name = name;
        Date = date;
        Description = description;
        Location = location;
    }

    public void Cancel()
    {
        Status = EventStatus.Cancelled;
    }
}
