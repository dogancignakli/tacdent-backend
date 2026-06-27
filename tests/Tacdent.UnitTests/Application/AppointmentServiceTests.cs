using Moq;
using Tacdent.Application.Errors;
using Tacdent.Application.Mapping;
using Tacdent.Application.Services;
using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;
using Tacdent.Data.Repositories.Interfaces;
using Tacdent.UnitTests.Infrastructure;

namespace Tacdent.UnitTests.Application;

public class AppointmentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly AppointmentMapper _mapper = new();
    private readonly AppointmentService _sut;

    public AppointmentServiceTests()
    {
        _unitOfWork.SetupGet(u => u.Appointments).Returns(_appointmentRepository.Object);
        _unitOfWork.SetupGet(u => u.Users).Returns(_userRepository.Object);
        _sut = new AppointmentService(_unitOfWork.Object, _mapper);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsMappedAppointmentsWithMetadata()
    {
        var appointment = TestData.SampleAppointment();
        var query = new AppointmentQuery(AppointmentStatus.Pending, 1, 20, AppointmentSortField.PreferredDate, Tacdent.Core.DTOs.SortDirection.Desc);
        var paged = new PagedResult<Appointment>([appointment], 1, 20, 1);

        _appointmentRepository
            .Setup(r => r.GetPagedAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _sut.GetPagedAsync(query);

        result.Items.ShouldHaveSingleItem();
        result.Items[0].Id.ShouldBe(appointment.Id);
        result.Items[0].PatientName.ShouldBe(appointment.PatientName);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(20);
        result.TotalCount.ShouldBe(1);
        result.TotalPages.ShouldBe(1);
        result.HasNextPage.ShouldBeFalse();
        result.HasPreviousPage.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        _appointmentRepository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        var result = await _sut.GetByIdAsync(id);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(AppointmentErrors.NotFound(id).Code);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsMappedDto()
    {
        var appointment = TestData.SampleAppointment();
        _appointmentRepository
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var result = await _sut.GetByIdAsync(appointment.Id);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(appointment.Id);
        result.Value.Email.ShouldBe(appointment.Email);
    }

    [Fact]
    public async Task CreateAsync_WhenDateIsInPast_ReturnsFailureWithoutSaving()
    {
        var dto = TestData.ValidCreateDto() with
        {
            PreferredDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)),
        };

        var result = await _sut.CreateAsync(dto);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(AppointmentErrors.PastDate.Code);
        _appointmentRepository.Verify(
            r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenValid_PersistsPendingAppointment()
    {
        var dto = TestData.ValidCreateDto();
        Appointment? captured = null;
        _appointmentRepository
            .Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Callback<Appointment, CancellationToken>((entity, _) => captured = entity)
            .Returns(Task.CompletedTask);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.ShouldBeTrue();
        captured.ShouldNotBeNull();
        captured!.Id.ShouldNotBe(Guid.Empty);
        captured.Status.ShouldBe(AppointmentStatus.Pending);
        captured.PatientName.ShouldBe(dto.PatientName);
        _appointmentRepository.Verify(
            r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenNotFound_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        _appointmentRepository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        var result = await _sut.UpdateStatusAsync(id, new UpdateAppointmentStatusDto(AppointmentStatus.Confirmed));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(AppointmentErrors.NotFound(id).Code);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenFound_UpdatesStatusAndSaves()
    {
        var appointment = TestData.SampleAppointment();
        _appointmentRepository
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.UpdateStatusAsync(
            appointment.Id,
            new UpdateAppointmentStatusDto(AppointmentStatus.Confirmed));

        result.IsSuccess.ShouldBeTrue();
        appointment.Status.ShouldBe(AppointmentStatus.Confirmed);
        result.Value.Status.ShouldBe(AppointmentStatus.Confirmed);
        _appointmentRepository.Verify(r => r.Update(appointment), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        _appointmentRepository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        var result = await _sut.DeleteAsync(id);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(AppointmentErrors.NotFound(id).Code);
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_RemovesAndSaves()
    {
        var appointment = TestData.SampleAppointment();
        _appointmentRepository
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.DeleteAsync(appointment.Id);

        result.IsSuccess.ShouldBeTrue();
        _appointmentRepository.Verify(r => r.Remove(appointment), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignAsync_WhenAssigneeIsActive_SetsAssignee()
    {
        var appointment = TestData.SampleAppointment();
        var assignee = TestData.SampleUser("staff@tacdent.local", "hash", UserRole.Staff);
        _appointmentRepository
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _userRepository
            .Setup(r => r.GetByIdAsync(assignee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignee);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.AssignAsync(
            appointment.Id,
            new AssignAppointmentDto(assignee.Id));

        result.IsSuccess.ShouldBeTrue();
        appointment.AssignedUserId.ShouldBe(assignee.Id);
        result.Value.AssignedUserEmail.ShouldBe(assignee.Email);
    }

    [Fact]
    public async Task AssignAsync_WhenAssigneeIsInactive_ReturnsValidationFailure()
    {
        var appointment = TestData.SampleAppointment();
        var assignee = TestData.SampleUser("staff@tacdent.local", "hash", UserRole.Staff, isActive: false);
        _appointmentRepository
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _userRepository
            .Setup(r => r.GetByIdAsync(assignee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignee);

        var result = await _sut.AssignAsync(
            appointment.Id,
            new AssignAppointmentDto(assignee.Id));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(UserErrors.InactiveAssignee.Code);
    }

    [Fact]
    public async Task AssignAsync_WhenUnassigning_ClearsAssignee()
    {
        var assignee = TestData.SampleUser("staff@tacdent.local", "hash", UserRole.Staff);
        var appointment = TestData.SampleAppointment();
        appointment.AssignedUserId = assignee.Id;
        appointment.AssignedUser = assignee;
        _appointmentRepository
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.AssignAsync(
            appointment.Id,
            new AssignAppointmentDto(null));

        result.IsSuccess.ShouldBeTrue();
        appointment.AssignedUserId.ShouldBeNull();
        appointment.AssignedUser.ShouldBeNull();
        result.Value.AssignedUserEmail.ShouldBeNull();
    }
}
