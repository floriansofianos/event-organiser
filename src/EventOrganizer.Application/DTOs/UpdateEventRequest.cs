namespace EventOrganizer.Application.DTOs;

public record UpdateEventRequest(
    string Name,
    DateTime Date,
    string? Description,
    string? Location
);
