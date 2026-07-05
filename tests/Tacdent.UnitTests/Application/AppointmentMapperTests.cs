using Tacdent.Application.Mapping;
using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;
using Tacdent.UnitTests.Infrastructure;

namespace Tacdent.UnitTests.Application;

public class AppointmentMapperTests
{
    private readonly AppointmentMapper _sut = new();

    [Fact]
    public void ToDto_MapsAllFields()
    {
        var entity = TestData.SampleAppointment();

        var dto = _sut.ToDto(entity);

        dto.Id.ShouldBe(entity.Id);
        dto.PatientName.ShouldBe(entity.PatientName);
        dto.Email.ShouldBe(entity.Email);
        dto.Phone.ShouldBe(entity.Phone);
        dto.PreferredDate.ShouldBe(entity.PreferredDate);
        dto.PreferredTime.ShouldBe(entity.PreferredTime);
        dto.ServiceType.ShouldBe(entity.ServiceType);
        dto.Notes.ShouldBe(entity.Notes);
        dto.Status.ShouldBe(entity.Status);
        dto.CreatedAt.ShouldBe(entity.CreatedAt);
        dto.UpdatedAt.ShouldBe(entity.UpdatedAt);
        dto.AssignedUserId.ShouldBeNull();
        dto.AssignedUserEmail.ShouldBeNull();
    }

    [Fact]
    public void ToDto_MapsAssigneeFields()
    {
        var assignee = TestData.SampleUser("staff@tacdent.local", "hash", UserRole.Staff);
        var entity = TestData.SampleAppointment();
        entity.AssignedUserId = assignee.Id;
        entity.AssignedUser = assignee;

        var dto = _sut.ToDto(entity);

        dto.AssignedUserId.ShouldBe(assignee.Id);
        dto.AssignedUserEmail.ShouldBe(assignee.Email);
    }

    [Fact]
    public void ToDtoList_MapsAllEntities()
    {
        var entities = new List<Appointment> { TestData.SampleAppointment() }.AsReadOnly();

        var dtos = _sut.ToDtoList(entities);

        dtos.ShouldHaveSingleItem();
        dtos[0].Id.ShouldBe(entities[0].Id);
    }
}
