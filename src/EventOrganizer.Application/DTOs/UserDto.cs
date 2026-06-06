namespace EventOrganizer.Application.DTOs;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsEmailConfirmed
);
