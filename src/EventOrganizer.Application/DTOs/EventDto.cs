namespace EventOrganizer.Application.DTOs;

public record EventDto(
    Guid Id,
    string Name,
    DateTime Date,
    string? Description,
    string? Location,
    string Status,
    DateTime CreatedAt
);
