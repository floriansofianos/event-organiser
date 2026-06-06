using EventOrganizer.Application.DTOs;
using EventOrganizer.Application.Interfaces;
using EventOrganizer.Application.Services;
using EventOrganizer.Domain.Entities;
using EventOrganizer.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace EventOrganizer.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private const string FrontendUrl = "http://localhost:3000";

    private AuthService CreateSut() => new(
        _userRepoMock.Object,
        _hasherMock.Object,
        _tokenServiceMock.Object,
        _emailServiceMock.Object,
        FrontendUrl);

    // ── Register ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidData_CreatesUserAndSendsEmail()
    {
        var request = new RegisterRequest("Jane", "Doe", "jane@example.com", "Secret1!", "Secret1!");

        _userRepoMock.Setup(r => r.GetByEmailAsync("jane@example.com", default)).ReturnsAsync((User?)null);
        _hasherMock.Setup(h => h.Hash("Secret1!")).Returns("hashed");

        await CreateSut().RegisterAsync(request);

        _userRepoMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Email == "jane@example.com" &&
            u.FirstName == "Jane" &&
            u.LastName == "Doe" &&
            !u.IsEmailConfirmed), default), Times.Once);

        _emailServiceMock.Verify(e => e.SendConfirmationEmailAsync(
            "jane@example.com",
            "Jane Doe",
            It.Is<string>(s => s.Contains("/confirm-email?token=")),
            default), Times.Once);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ThrowsConflictException()
    {
        var existing = User.Create("Existing", "User", "jane@example.com", "hash");
        _userRepoMock.Setup(r => r.GetByEmailAsync("jane@example.com", default)).ReturnsAsync(existing);

        var request = new RegisterRequest("Jane", "Doe", "jane@example.com", "Secret1!", "Secret1!");

        var act = async () => await CreateSut().RegisterAsync(request);

        await act.Should().ThrowAsync<Exceptions.ConflictException>()
            .WithMessage("*already exists*");
    }

    // ── Login ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var user = User.Create("Jane", "Doe", "jane@example.com", "hashed");
        user.ConfirmEmail();

        _userRepoMock.Setup(r => r.GetByEmailAsync("jane@example.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("Secret1!", "hashed")).Returns(true);
        _tokenServiceMock.Setup(t => t.GenerateToken(user)).Returns("jwt-token");

        var result = await CreateSut().LoginAsync(new LoginRequest("jane@example.com", "Secret1!"));

        result.Token.Should().Be("jwt-token");
        result.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorizedException()
    {
        var user = User.Create("Jane", "Doe", "jane@example.com", "hashed");
        _userRepoMock.Setup(r => r.GetByEmailAsync("jane@example.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("wrong", "hashed")).Returns(false);

        var act = async () => await CreateSut().LoginAsync(new LoginRequest("jane@example.com", "wrong"));

        await act.Should().ThrowAsync<Exceptions.UnauthorizedException>();
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ThrowsUnauthorizedException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((User?)null);

        var act = async () => await CreateSut().LoginAsync(new LoginRequest("unknown@example.com", "pass"));

        await act.Should().ThrowAsync<Exceptions.UnauthorizedException>();
    }

    [Fact]
    public async Task Login_WithUnconfirmedEmail_ThrowsForbiddenException()
    {
        var user = User.Create("Jane", "Doe", "jane@example.com", "hashed");
        // email NOT confirmed
        _userRepoMock.Setup(r => r.GetByEmailAsync("jane@example.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("Secret1!", "hashed")).Returns(true);

        var act = async () => await CreateSut().LoginAsync(new LoginRequest("jane@example.com", "Secret1!"));

        await act.Should().ThrowAsync<Exceptions.ForbiddenException>()
            .WithMessage("*confirm your email*");
    }

    // ── ConfirmEmail ────────────────────────────────────────────────────────

    [Fact]
    public async Task ConfirmEmail_WithValidToken_SetsConfirmed()
    {
        var user = User.Create("Jane", "Doe", "jane@example.com", "hash");
        user.SetEmailConfirmationToken("valid-token", DateTime.UtcNow.AddHours(24));

        _userRepoMock.Setup(r => r.GetByEmailConfirmationTokenAsync("valid-token", default)).ReturnsAsync(user);

        await CreateSut().ConfirmEmailAsync(new ConfirmEmailRequest("valid-token"));

        user.IsEmailConfirmed.Should().BeTrue();
        user.EmailConfirmationToken.Should().BeNull();
        _userRepoMock.Verify(r => r.UpdateAsync(user, default), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmail_WithExpiredToken_ThrowsApplicationException()
    {
        var user = User.Create("Jane", "Doe", "jane@example.com", "hash");
        user.SetEmailConfirmationToken("expired-token", DateTime.UtcNow.AddHours(-1));

        _userRepoMock.Setup(r => r.GetByEmailConfirmationTokenAsync("expired-token", default)).ReturnsAsync(user);

        var act = async () => await CreateSut().ConfirmEmailAsync(new ConfirmEmailRequest("expired-token"));

        await act.Should().ThrowAsync<Exceptions.ApplicationException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task ConfirmEmail_WithInvalidToken_ThrowsNotFoundException()
    {
        _userRepoMock.Setup(r => r.GetByEmailConfirmationTokenAsync("bad-token", default)).ReturnsAsync((User?)null);

        var act = async () => await CreateSut().ConfirmEmailAsync(new ConfirmEmailRequest("bad-token"));

        await act.Should().ThrowAsync<Exceptions.NotFoundException>();
    }
}
