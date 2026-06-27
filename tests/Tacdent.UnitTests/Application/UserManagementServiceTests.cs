using Moq;
using Tacdent.Application.Errors;
using Tacdent.Application.Mapping;
using Tacdent.Application.Services;
using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;
using Tacdent.Data.Repositories.Interfaces;
using Tacdent.UnitTests.Infrastructure;

namespace Tacdent.UnitTests.Application;

public class UserManagementServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Pbkdf2PasswordHasher _passwordHasher = new();
    private readonly UserMapper _mapper = new();
    private readonly UserManagementService _sut;

    public UserManagementServiceTests()
    {
        _unitOfWork.SetupGet(u => u.Users).Returns(_userRepository.Object);
        _sut = new UserManagementService(_unitOfWork.Object, _passwordHasher, _mapper);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailIsUnique_CreatesActiveUser()
    {
        _userRepository
            .Setup(r => r.EmailExistsAsync("new@tacdent.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.CreateAsync(
            new CreateUserDto("New@Tacdent.Local", "password123", UserRole.Staff));

        result.IsSuccess.ShouldBeTrue();
        result.Value.Email.ShouldBe("new@tacdent.local");
        result.Value.Role.ShouldBe(UserRole.Staff);
        result.Value.IsActive.ShouldBeTrue();
        _userRepository.Verify(
            r => r.AddAsync(It.Is<User>(u => u.Email == "new@tacdent.local"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailExists_ReturnsConflict()
    {
        _userRepository
            .Setup(r => r.EmailExistsAsync("existing@tacdent.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.CreateAsync(
            new CreateUserDto("existing@tacdent.local", "password123", UserRole.Staff));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(UserErrors.EmailAlreadyExists.Code);
    }

    [Fact]
    public async Task UpdateRoleAsync_WhenLastActiveAdmin_ReturnsConflict()
    {
        var admin = TestData.SampleUser();
        _userRepository
            .Setup(r => r.GetByIdAsync(admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);
        _userRepository
            .Setup(r => r.CountActiveAdminsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.UpdateRoleAsync(
            admin.Id,
            new UpdateUserRoleDto(UserRole.Staff));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(UserErrors.CannotModifyLastAdmin.Code);
    }

    [Fact]
    public async Task SetActiveAsync_WhenLastActiveAdmin_ReturnsConflict()
    {
        var admin = TestData.SampleUser();
        _userRepository
            .Setup(r => r.GetByIdAsync(admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);
        _userRepository
            .Setup(r => r.CountActiveAdminsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.SetActiveAsync(
            admin.Id,
            new UpdateUserStatusDto(false));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(UserErrors.CannotModifyLastAdmin.Code);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenFound_RehashesPassword()
    {
        var user = TestData.SampleUser();
        var originalHash = user.PasswordHash;
        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.ResetPasswordAsync(
            user.Id,
            new ResetPasswordDto("new-password-123"));

        result.IsSuccess.ShouldBeTrue();
        user.PasswordHash.ShouldNotBe(originalHash);
        _passwordHasher.Verify("new-password-123", user.PasswordHash).ShouldBeTrue();
        _userRepository.Verify(r => r.Update(user), Times.Once);
    }
}
