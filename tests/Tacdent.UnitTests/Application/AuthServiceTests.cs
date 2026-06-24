using Microsoft.Extensions.Options;
using Moq;
using Tacdent.Application.Errors;
using Tacdent.Application.Options;
using Tacdent.Application.Services;
using Tacdent.Core.Entities;
using Tacdent.Data.Repositories.Interfaces;
using Tacdent.UnitTests.Infrastructure;

namespace Tacdent.UnitTests.Application;

public class AuthServiceTests
{
    private const string Password = "TacDent-Dev-Admin-2026!";
    private const string Email = "admin@tacdent.local";

    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Pbkdf2PasswordHasher _passwordHasher = new();
    private readonly AuthOptions _options = new() { MaxFailedAttempts = 3, LockoutMinutes = 15 };

    public AuthServiceTests()
    {
        _unitOfWork.SetupGet(u => u.Users).Returns(_userRepository.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private AuthService CreateSut() =>
        new(_unitOfWork.Object, _passwordHasher, Options.Create(_options));

    private User UserWithPassword(string password, bool isActive = true)
    {
        var user = TestData.SampleUser(Email, _passwordHasher.Hash(password), UserRole.Admin, isActive);
        return user;
    }

    [Fact]
    public async Task AuthenticateAsync_WhenCredentialsValid_ReturnsAuthenticatedUser()
    {
        var user = UserWithPassword(Password);
        _userRepository
            .Setup(r => r.GetByEmailAsync(Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await CreateSut().AuthenticateAsync(Email, Password);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(user.Id);
        result.Value.Email.ShouldBe(user.Email);
        result.Value.Role.ShouldBe(UserRole.Admin);
    }

    [Fact]
    public async Task AuthenticateAsync_NormalizesEmailBeforeLookup()
    {
        var user = UserWithPassword(Password);
        _userRepository
            .Setup(r => r.GetByEmailAsync(Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await CreateSut().AuthenticateAsync("  ADMIN@Tacdent.Local  ", Password);

        result.IsSuccess.ShouldBeTrue();
        _userRepository.Verify(r => r.GetByEmailAsync(Email, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenUserNotFound_ReturnsInvalidCredentials()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await CreateSut().AuthenticateAsync(Email, Password);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(AuthErrors.InvalidCredentials.Code);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenUserInactive_ReturnsInvalidCredentials()
    {
        var user = UserWithPassword(Password, isActive: false);
        _userRepository
            .Setup(r => r.GetByEmailAsync(Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await CreateSut().AuthenticateAsync(Email, Password);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(AuthErrors.InvalidCredentials.Code);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenPasswordWrong_ReturnsInvalidCredentialsAndIncrementsCounter()
    {
        var user = UserWithPassword(Password);
        _userRepository
            .Setup(r => r.GetByEmailAsync(Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await CreateSut().AuthenticateAsync(Email, "wrong-password");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(AuthErrors.InvalidCredentials.Code);
        user.AccessFailedCount.ShouldBe(1);
        _userRepository.Verify(r => r.Update(user), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenFailuresReachThreshold_LocksAccount()
    {
        var user = UserWithPassword(Password);
        user.AccessFailedCount = _options.MaxFailedAttempts - 1;
        _userRepository
            .Setup(r => r.GetByEmailAsync(Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await CreateSut().AuthenticateAsync(Email, "wrong-password");

        result.IsFailure.ShouldBeTrue();
        user.LockoutEndUtc.ShouldNotBeNull();
        user.LockoutEndUtc!.Value.ShouldBeGreaterThan(DateTime.UtcNow);
        user.AccessFailedCount.ShouldBe(0);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenAccountLocked_ReturnsAccountLocked()
    {
        var user = UserWithPassword(Password);
        user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(10);
        _userRepository
            .Setup(r => r.GetByEmailAsync(Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await CreateSut().AuthenticateAsync(Email, Password);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(AuthErrors.AccountLocked.Code);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenSuccessAfterFailures_ResetsCounters()
    {
        var user = UserWithPassword(Password);
        user.AccessFailedCount = 2;
        _userRepository
            .Setup(r => r.GetByEmailAsync(Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await CreateSut().AuthenticateAsync(Email, Password);

        result.IsSuccess.ShouldBeTrue();
        user.AccessFailedCount.ShouldBe(0);
        user.LockoutEndUtc.ShouldBeNull();
        _userRepository.Verify(r => r.Update(user), Times.Once);
    }
}
