namespace EventOrganizer.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsEmailConfirmed { get; private set; }
    public string? EmailConfirmationToken { get; private set; }
    public DateTime? EmailConfirmationTokenExpiry { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User() { }

    public static User Create(string firstName, string lastName, string email, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            IsEmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetEmailConfirmationToken(string token, DateTime expiry)
    {
        EmailConfirmationToken = token;
        EmailConfirmationTokenExpiry = expiry;
    }

    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
        EmailConfirmationToken = null;
        EmailConfirmationTokenExpiry = null;
    }
}
