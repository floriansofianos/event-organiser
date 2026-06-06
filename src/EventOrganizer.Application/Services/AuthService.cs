using EventOrganizer.Application.DTOs;
using EventOrganizer.Application.Exceptions;
using EventOrganizer.Application.Interfaces;
using EventOrganizer.Domain.Entities;
using EventOrganizer.Domain.Interfaces;

namespace EventOrganizer.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly string _frontendUrl;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailService emailService,
        string frontendUrl)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailService = emailService;
        _frontendUrl = frontendUrl;
    }

    public async Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new ConflictException("An account with this email already exists.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.FirstName, request.LastName, request.Email, passwordHash);

        var token = Guid.NewGuid().ToString("N");
        user.SetEmailConfirmationToken(token, DateTime.UtcNow.AddHours(24));

        await _userRepository.AddAsync(user, cancellationToken);

        var confirmationLink = $"{_frontendUrl}/confirm-email?token={token}";
        await _emailService.SendConfirmationEmailAsync(
            user.Email,
            $"{user.FirstName} {user.LastName}",
            confirmationLink,
            cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsEmailConfirmed)
            throw new ForbiddenException("Please confirm your email address before logging in.");

        var token = _tokenService.GenerateToken(user);
        return new AuthResponse(token, user.Email, user.FirstName, user.LastName);
    }

    public async Task ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailConfirmationTokenAsync(request.Token, cancellationToken)
            ?? throw new NotFoundException("Invalid or expired confirmation token.");

        if (user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
            throw new Exceptions.ApplicationException("The confirmation token has expired. Please register again.", 400);

        user.ConfirmEmail();
        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return new UserDto(user.Id, user.FirstName, user.LastName, user.Email, user.IsEmailConfirmed);
    }
}
