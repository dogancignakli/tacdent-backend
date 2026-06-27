using Tacdent.Api.Factories;
using Tacdent.Api.ViewModels;
using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;
using Tacdent.UnitTests.Infrastructure;

namespace Tacdent.UnitTests.Api;

public class AppointmentFactoryTests
{
    private readonly AppointmentFactory _sut = new();

    [Fact]
    public void ToCreateDto_TrimsFieldsAndNullsEmptyNotes()
    {
        var request = new CreateAppointmentRequest
        {
            PatientName = "  Jane Doe  ",
            Email = "  jane@example.com ",
            Phone = "  +15551234567 ",
            PreferredDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(3)),
            PreferredTime = new TimeOnly(10, 30),
            ServiceType = "  General Checkup ",
            Notes = "   ",
        };

        var dto = _sut.ToCreateDto(request);

        dto.PatientName.ShouldBe("Jane Doe");
        dto.Email.ShouldBe("jane@example.com");
        dto.Phone.ShouldBe("+15551234567");
        dto.ServiceType.ShouldBe("General Checkup");
        dto.Notes.ShouldBeNull();
    }

    [Fact]
    public void ToCreateDto_PreservesNonEmptyNotes()
    {
        var request = new CreateAppointmentRequest
        {
            PatientName = "Jane Doe",
            Email = "jane@example.com",
            Phone = "+15551234567",
            PreferredDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(3)),
            PreferredTime = new TimeOnly(10, 30),
            ServiceType = "General Checkup",
            Notes = "  First visit ",
        };

        var dto = _sut.ToCreateDto(request);

        dto.Notes.ShouldBe("First visit");
    }

    [Fact]
    public void ToUpdateStatusDto_MapsStatus()
    {
        var dto = _sut.ToUpdateStatusDto(new UpdateAppointmentStatusRequest
        {
            Status = AppointmentStatus.Confirmed,
        });

        dto.Status.ShouldBe(AppointmentStatus.Confirmed);
    }

    [Fact]
    public void ToQuery_MapsRequestFields()
    {
        var request = new AppointmentQueryRequest(
            AppointmentStatus.Pending,
            Page: 2,
            PageSize: 10,
            SortBy: AppointmentSortField.CreatedAt,
            SortDirection: Tacdent.Core.DTOs.SortDirection.Asc);

        var query = _sut.ToQuery(request);

        query.Status.ShouldBe(AppointmentStatus.Pending);
        query.Page.ShouldBe(2);
        query.PageSize.ShouldBe(10);
        query.SortBy.ShouldBe(AppointmentSortField.CreatedAt);
        query.SortDirection.ShouldBe(Tacdent.Core.DTOs.SortDirection.Asc);
    }

    [Fact]
    public void ToResponse_MapsAllFields()
    {
        var dto = new AppointmentDto(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Jane Doe",
            "jane@example.com",
            "+15551234567",
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(3)),
            new TimeOnly(10, 30),
            "General Checkup",
            "First visit",
            AppointmentStatus.Pending,
            TestData.AuditTimestamp,
            TestData.AuditTimestamp,
            null,
            null);

        var response = _sut.ToResponse(dto);

        response.Id.ShouldBe(dto.Id);
        response.PatientName.ShouldBe(dto.PatientName);
        response.Email.ShouldBe(dto.Email);
        response.Phone.ShouldBe(dto.Phone);
        response.PreferredDate.ShouldBe(dto.PreferredDate);
        response.PreferredTime.ShouldBe(dto.PreferredTime);
        response.ServiceType.ShouldBe(dto.ServiceType);
        response.Notes.ShouldBe(dto.Notes);
        response.Status.ShouldBe(dto.Status);
        response.CreatedAt.ShouldBe(dto.CreatedAt);
        response.UpdatedAt.ShouldBe(dto.UpdatedAt);
    }
}
