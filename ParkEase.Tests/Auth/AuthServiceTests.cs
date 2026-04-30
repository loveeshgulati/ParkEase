using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using ParkEase.Auth.DTOs;
using ParkEase.Auth.Entities;
using ParkEase.Auth.Interfaces;
using ParkEase.Auth.Services;

namespace ParkEase.Tests.Auth;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IUserRepository>  _userRepo = null!;
    private Mock<IPublishEndpoint> _bus      = null!;
    private IConfiguration         _config   = null!;
    private AuthService            _sut      = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepo = new Mock<IUserRepository>();
        _bus      = new Mock<IPublishEndpoint>();

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"]   = "SuperSecretKey_MustBe32CharsLong!!",
                ["Jwt:Issuer"]   = "ParkEase",
                ["Jwt:Audience"] = "ParkEaseUsers"
            })
            .Build();

        _bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new AuthService(
            _userRepo.Object, _bus.Object,
            _config, NullLogger<AuthService>.Instance);
    }

    // ── Register ──────────────────────────────────────────────────────────────

    [Test]
    public async Task Register_NewDriver_ReturnsActiveStatus()
    {
        _userRepo.Setup(r => r.ExistsByEmailAsync("driver@test.com")).ReturnsAsync(false);
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.UserId = 1; return u; });

        var result = await _sut.RegisterAsync(new RegisterRequestDto
        {
            FullName = "Test Driver", Email = "driver@test.com",
            Password = "pass123", Phone = "9999999999", Role = "DRIVER"
        });

        Assert.That(result.Status, Is.EqualTo("ACTIVE"));
    }

    [Test]
    public async Task Register_NewManager_ReturnsPendingApprovalStatus()
    {
        _userRepo.Setup(r => r.ExistsByEmailAsync("mgr@test.com")).ReturnsAsync(false);
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.UserId = 2; return u; });

        var result = await _sut.RegisterAsync(new RegisterRequestDto
        {
            FullName = "Mgr User", Email = "mgr@test.com",
            Password = "pass123", Phone = "8888888888", Role = "MANAGER"
        });

        Assert.That(result.Status, Is.EqualTo("PENDING_APPROVAL"));
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Test]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("pass123");
        var user = new User { UserId = 1, Email = "u@u.com", PasswordHash = hash,
            Role = "DRIVER", Status = "ACTIVE", IsActive = true, FullName = "U" };

        _userRepo.Setup(r => r.FindByEmailAsync("u@u.com")).ReturnsAsync(user);
        _userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);

        var result = await _sut.LoginAsync(new LoginRequestDto { Email = "u@u.com", Password = "pass123" });

        Assert.That(result.AccessToken,  Is.Not.Null.And.Not.Empty);
        Assert.That(result.RefreshToken, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void Login_WrongPassword_ThrowsUnauthorized()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("correct");
        var user = new User { Email = "u@u.com", PasswordHash = hash, Status = "ACTIVE", IsActive = true };
        _userRepo.Setup(r => r.FindByEmailAsync("u@u.com")).ReturnsAsync(user);

        Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.LoginAsync(new LoginRequestDto { Email = "u@u.com", Password = "wrong" }));
    }
}
