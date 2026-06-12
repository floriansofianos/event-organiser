namespace EventOrganizer.Application.DTOs;

public record CreateEventRequest(
    string Name,
    DateTime Date,
    string? Description,
    string? Location
);
