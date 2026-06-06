using EventOrganizer.Domain.Entities;

namespace EventOrganizer.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
