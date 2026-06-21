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
    private readonly AppointmentMapper _mapper = new();
    private readonly AppointmentService _sut;

    public AppointmentServiceTests()
    {
        _unitOfWork.SetupGet(u => u.Appointments).Returns(_appointmentRepository.Object);
        _sut = new AppointmentService(_unitOfWork.Object, _mapper);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedAppointments()
    {
        var appointment = TestData.SampleAppointment();
        _appointmentRepository
            .Setup(r => r.GetAllAsync(AppointmentStatus.Pending, It.IsAny<CancellationToken>()))
            .ReturnsAsync([appointment]);

        var result = await _sut.GetAllAsync(AppointmentStatus.Pending);

        result.ShouldHaveSingleItem();
        result[0].Id.ShouldBe(appointment.Id);
        result[0].PatientName.ShouldBe(appointment.PatientName);
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
}
